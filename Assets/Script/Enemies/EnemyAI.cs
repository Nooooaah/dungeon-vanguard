using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人AI —— 决策 + 行动执行
/// 组员B负责，挂载在敌人 Prefab 上（与 EnemyBase 同一个 GameObject）
/// </summary>
public class EnemyAI : MonoBehaviour
{
    private EnemyBase _enemy;

    // 蓄力状态
    private bool _isCharged;
    private EnemyAction _chargedAction;  // 蓄力完成后释放的行动

    // 预决策：在玩家回合开始时就决定下一步行动，供UI显示意图
    private EnemyAction _nextAction;
    public EnemyAction NextAction => _isCharged ? _chargedAction : _nextAction;

    void Start()
    {
        _enemy = GetComponent<EnemyBase>();
        if (_enemy == null)
        {
            Debug.LogError("[EnemyAI] 需要挂在与 EnemyBase 同一个 GameObject 上！");
            return;
        }

        // 订阅敌人回合事件
        var bm = GameManager.Instance != null ? GameManager.Instance.BattleManager : FindObjectOfType<BattleManager>();
        var tm = bm?.TurnManager;
        if (tm != null)
            tm.OnEnemyTurnStart += OnEnemyTurn;
    }

    void OnDestroy()
    {
        var bm = GameManager.Instance != null ? GameManager.Instance.BattleManager : FindObjectOfType<BattleManager>();
        var tm = bm?.TurnManager;
        if (tm != null)
            tm.OnEnemyTurnStart -= OnEnemyTurn;
    }

    /// <summary>敌人回合到来时，BattleManager 会触发此方法</summary>
    private void OnEnemyTurn()
    {
        if (_enemy == null || !_enemy.IsAlive) return;

        var bm = GameManager.Instance.BattleManager;

        // 检查是否被冰冻
        if (bm.BuffSystem.ShouldSkipTurn(_enemy))
        {
            Debug.Log($"[{_enemy.Data.enemyName}] 被冰冻，跳过本回合");
            bm.EndEnemyPhase();
            return;
        }

        // 蓄力释放
        if (_isCharged && _chargedAction != null)
        {
            Debug.Log($"[{_enemy.Data.enemyName}] 蓄力释放！");
            _enemy.ExecuteAction(_chargedAction);
            _isCharged = false;
            _chargedAction = null;
            bm.EndEnemyPhase();
            return;
        }

        // 执行预决策的行动（如果没有则现场决策）
        EnemyAction chosen = _nextAction ?? DecideAction();
        if (chosen != null)
        {
            _enemy.ExecuteAction(chosen);
        }
        _nextAction = null;

        bm.EndEnemyPhase();
    }

    /// <summary>预决策下一回合行动，供UI显示意图。由BattleUI在玩家回合开始时调用。</summary>
    public void PredecideNextAction()
    {
        if (_isCharged && _chargedAction != null)
        {
            _nextAction = null;
            return;
        }
        _nextAction = DecideAction();
    }

    /// <summary>
    /// AI决策：根据权重加权随机选择行动
    /// </summary>
    public EnemyAction DecideAction()
    {
        var actions = GetAvailableActions();
        if (actions.Count == 0) return null;

        return WeightedRandom(actions);
    }

    /// <summary>获取当前可用行动列表（含Boss狂暴切换）</summary>
    private List<EnemyAction> GetAvailableActions()
    {
        List<EnemyAction> available = new List<EnemyAction>();

        // 基础行动
        if (_enemy.Data.actions != null)
            available.AddRange(_enemy.Data.actions);

        // Boss狂暴：HP低于阈值时解锁狂暴行动
        if (_enemy.Data.type == EnemyType.Boss && _enemy.Data.rageThreshold > 0)
        {
            float hpPercent = (float)_enemy.CurrentHp / _enemy.Data.maxHp * 100f;
            if (hpPercent < _enemy.Data.rageThreshold && _enemy.Data.rageActions != null)
            {
                available.AddRange(_enemy.Data.rageActions);
            }
        }

        return available;
    }

    /// <summary>加权随机算法</summary>
    private EnemyAction WeightedRandom(List<EnemyAction> actions)
    {
        int totalWeight = 0;
        foreach (var a in actions)
            totalWeight += a.weight;

        if (totalWeight <= 0) return actions[0]; // 全0权重则取第一个

        int roll = Random.Range(0, totalWeight);
        int cumulative = 0;

        foreach (var action in actions)
        {
            cumulative += action.weight;
            if (roll < cumulative)
                return action;
        }

        return actions[actions.Count - 1]; // 兜底
    }

    /// <summary>强制蓄力（由外部调用，如Boss的蓄力技能）</summary>
    public void Charge(EnemyAction releaseAction)
    {
        _isCharged = true;
        _chargedAction = releaseAction;
    }
}
