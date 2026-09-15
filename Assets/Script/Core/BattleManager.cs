using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 战斗管理器 —— 战斗流程总调度
/// 组长负责，写在 Scripts/Core/BattleManager.cs
///
/// 挂载在 BattleScene 的 GameObject 上
/// </summary>
public class BattleManager : MonoBehaviour
{
    // ===== 子系统引用 =====
    public TurnManager TurnManager { get; private set; }
    public DamageSystem DamageSystem { get; private set; }
    public ElementSystem ElementSystem { get; private set; }
    public BuffSystem BuffSystem { get; private set; }

    // ===== 战斗实体 =====
    // 敌人列表（由组员B的代码注册进来）
    private List<IDamageable> _enemies = new List<IDamageable>();
    // 玩家引用（由组员B的Player注册）
    public IDamageable Player { get; private set; }

    // ===== 元素上下文 =====
    public ElementContext ElementContext { get; private set; } = new ElementContext();

    // ===== 卡牌系统引用（由组员A注册） =====
    // BattleUI 的 DrawCard / DiscardHand 等回调
    public System.Action OnDrawCard;
    public System.Action OnDiscardHand;
    public System.Action OnRestoreEnergy;

    void Awake()
    {
        // 初始化子系统
        BuffSystem = new BuffSystem();
        ElementSystem = new ElementSystem();
        DamageSystem = new DamageSystem(ElementSystem, BuffSystem);
        TurnManager = new TurnManager();

        // 注册到 GameManager（让全组能通过 GameManager.Instance.BattleManager 访问）
        if (GameManager.Instance != null)
            GameManager.Instance.BattleManager = this;

        // 订阅回合事件
        TurnManager.OnPlayerTurnStart += HandlePlayerTurnStart;
        TurnManager.OnEnemyTurnStart += HandleEnemyTurnStart;
        TurnManager.OnStateChanged += HandleStateChanged;
    }

    /// <summary>注册玩家</summary>
    public void RegisterPlayer(IDamageable player)
    {
        Player = player;
    }

    /// <summary>注册敌人</summary>
    public void RegisterEnemy(IDamageable enemy)
    {
        if (!_enemies.Contains(enemy))
            _enemies.Add(enemy);
    }

    /// <summary>注销敌人（死亡时）</summary>
    public void UnregisterEnemy(IDamageable enemy)
    {
        _enemies.Remove(enemy);
    }

    /// <summary>获取存活敌人列表</summary>
    public List<IDamageable> GetAliveEnemies()
    {
        return _enemies.FindAll(e => e.IsAlive);
    }

    /// <summary>获取最前面的存活敌人（自动选目标）</summary>
    public IDamageable GetFirstAliveEnemy()
    {
        return _enemies.Find(e => e.IsAlive);
    }

    /// <summary>开始战斗</summary>
    public void StartBattle()
    {
        ElementContext.Clear();
        TurnManager.StartBattle();
    }

    // ===== 回合事件处理 =====

    private void HandlePlayerTurnStart()
    {
        // 0. 结算玩家身上的Buff效果（灼烧扣血、冰冻检查）
        if (Player != null)
        {
            var (events, frozenTargets) = BuffSystem.ProcessBuffsForTargets(
                new List<IDamageable> { Player });
            foreach (var evt in events)
            {
                Debug.Log($"[Buff] {evt}");
            }

            if (frozenTargets.Contains(Player))
            {
                Debug.Log("[BattleManager] 玩家被冰冻，跳过回合");
                TurnManager.EndPlayerTurn();
                return;
            }
        }

        // 1. 回满能量（由Player自己处理，BattleManager通知）
        OnRestoreEnergy?.Invoke();

        // 2. 抽5张牌（弃牌制：每回合重新抽满手牌）
        int drawCount = GameConstants.PLAYER_INITIAL_HAND;
        for (int i = 0; i < drawCount; i++)
            OnDrawCard?.Invoke();

        // 3. 清空本回合元素记录
        ElementContext.Clear();

        // 4. 清空护盾 —— 参考《杀戮尖塔》设计：护盾不跨回合保留
        foreach (var enemy in GetAliveEnemies())
        {
            enemy.ClearShield();
        }
        Player?.ClearShield();
    }

    private void HandleEnemyTurnStart()
    {
        // 结算敌人身上的Buff效果（灼烧扣血、冰冻检查）
        var aliveEnemies = GetAliveEnemies();
        if (aliveEnemies.Count > 0)
        {
            var (events, frozenTargets) = BuffSystem.ProcessBuffsForTargets(aliveEnemies);
            foreach (var evt in events)
            {
                Debug.Log($"[Buff] {evt}");
            }
        }

        // 灼烧可能击杀敌人
        if (GetAliveEnemies().Count == 0)
        {
            TurnManager.DeclareWin();
            return;
        }

        // 敌人行动由组员B的EnemyAI驱动
        // EnemyAI监听OnEnemyTurnStart，遍历敌人执行行动
        // EnemyAI应通过 BuffSystem.ShouldSkipTurn(enemy) 检查冰冻
        // 所有敌人行动完毕后，调用 EndEnemyPhase()
    }

    /// <summary>所有敌人行动完毕后调用此方法</summary>
    public void EndEnemyPhase()
    {
        // 减少所有Buff持续时间，清理过期Buff
        BuffSystem.TickAllDurations();

        TurnManager.EndEnemyTurn();
        CheckBattleResult();
    }

    // ===== 玩家操作接口 =====

    /// <summary>玩家点击"结束回合"</summary>
    public void OnEndTurnClicked()
    {
        OnDiscardHand?.Invoke();
        TurnManager.EndPlayerTurn();
    }

    /// <summary>
    /// 玩家出牌（BattleUI 调用此方法）
    /// </summary>
    /// <param name="card">卡牌（实现了ICardEffect）</param>
    /// <returns>是否成功打出</returns>
    public bool PlayCard(ICardEffect card)
    {
        if (TurnManager.CurrentState != TurnState.PlayerTurn) return false;

        // 自动选目标：攻击牌打敌人，技能牌目标在 Execute 内部自行处理
        IDamageable target = GetFirstAliveEnemy();

        // 设置目标元素（用于牌×敌人元素反应）
        if (target is IElementEntity entity)
            ElementContext.TargetElement = entity.ElementType;
        else
            ElementContext.TargetElement = Element.None;

        // 记录元素
        ElementContext.RecordPlayed(card.ElementType);

        // 执行卡牌效果
        card.Execute(target, ElementContext);

        // 检查是否所有敌人已死
        if (GetAliveEnemies().Count == 0)
        {
            // Delay win declaration to let VFX play
            StartCoroutine(DelayedWin());
        }

        return true;
    }

    System.Collections.IEnumerator DelayedWin()
    {
        yield return new WaitForSeconds(0.3f);
        TurnManager.DeclareWin();
    }

    // ===== 胜负判定 =====

    public void CheckBattleResult()
    {
        if (Player != null && !Player.IsAlive)
        {
            TurnManager.DeclareLose();
            return;
        }

        if (GetAliveEnemies().Count == 0)
        {
            TurnManager.DeclareWin();
            return;
        }

        // 胜负未分，继续下一回合
        TurnManager.ContinueToNextTurn();
    }

    // ===== 战斗结束 =====

    public void EndBattle(bool isWin)
    {
        BuffSystem.ClearAllBuffs();
        ElementContext.Clear();

        var gm = GameManager.Instance;

        if (isWin)
        {
            Debug.Log("🎉 战斗胜利！");

            // 将玩家战斗后血量同步到 GameManager（跨战斗保留）
            if (Player is Player playerComp)
                playerComp.SaveToGameManager();

            if (gm != null)
            {
                // Boss 战斗胜利 → 直接通关
                if (gm.PendingNode != null && gm.PendingNode.nodeType == NodeType.Boss)
                {
                    gm.OnGameClear();
                    return;
                }

                // 普通战斗胜利 → 进入奖励场景
                gm.OnBattleWin();
            }
        }
        else
        {
            Debug.Log("💀 战斗失败…");

            if (gm != null)
            {
                gm.OnGameOver();
            }
        }
    }

    private void HandleStateChanged(TurnState state)
    {
        Debug.Log($"[BattleManager] 回合状态 → {state}");

        if (state == TurnState.Win)
            EndBattle(true);
        else if (state == TurnState.Lose)
            EndBattle(false);
    }

    void OnDestroy()
    {
        TurnManager.OnPlayerTurnStart -= HandlePlayerTurnStart;
        TurnManager.OnEnemyTurnStart -= HandleEnemyTurnStart;
        TurnManager.OnStateChanged -= HandleStateChanged;
    }
}
