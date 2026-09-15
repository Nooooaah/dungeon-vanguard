using UnityEngine;

/// <summary>
/// 敌人运行时基类 —— 实现 IDamageable + IElementEntity
/// 组员B负责，挂载在敌人 Prefab 上
/// </summary>
public class EnemyBase : MonoBehaviour, IDamageable, IElementEntity
{
    [Header("配置")]
    public EnemyData Data;          // 在 Inspector 拖入对应的 .asset

    // ===== 运行时状态 =====
    public int CurrentHP => CurrentHp;
    public int MaxHP => Data != null ? Data.maxHp : 0;
    public int CurrentHp { get; private set; }
    public int Shield { get; private set; }
    public bool IsAlive => CurrentHp > 0;

    // ===== 接口实现 =====
    public Element ElementType => Data != null ? Data.element : Element.None;

    // ===== 事件 =====
    public System.Action<EnemyBase> OnDeath;
    public System.Action<EnemyBase, int> OnHpChanged;  // (敌人, 新HP)
    public System.Action<EnemyBase, int> OnShieldChanged;

    void Start()
    {
        if (Data == null)
        {
            // BattleUI will call Initialize() later with the correct EnemyData
            return;
        }

        Initialize(Data);
    }

    /// <summary>Initialize enemy with data. Called by BattleUI if Data wasn't set in inspector.</summary>
    public void Initialize(EnemyData data)
    {
        Data = data;
        CurrentHp = Data.maxHp;
        Shield = 0;

        var bm = GameManager.Instance != null ? GameManager.Instance.BattleManager : FindObjectOfType<BattleManager>();
        if (bm != null)
        {
            bm.RegisterEnemy(this);
            bm.ElementContext.TargetElement = Data.element;
        }

        OnHpChanged?.Invoke(this, CurrentHp);
    }

    // ==========================================
    //  IDamageable 实现
    // ==========================================

    public void TakeDamage(int amount, Element attackerElement)
    {
        if (!IsAlive) return;
        if (amount <= 0) return;

        // 元素抗性：同元素伤害减免25%
        if (attackerElement != Element.None && attackerElement == Data.element)
        {
            amount = Mathf.Max(1, amount * 3 / 4);
            Debug.Log($"[{Data.enemyName}] 元素抗性：伤害减免25% → {amount}");
        }

        // 护盾先吸收
        if (Shield > 0)
        {
            int absorbed = Mathf.Min(Shield, amount);
            Shield -= absorbed;
            amount -= absorbed;
            OnShieldChanged?.Invoke(this, Shield);
        }

        // 剩余扣HP
        CurrentHp -= amount;
        OnHpChanged?.Invoke(this, CurrentHp);

        if (CurrentHp <= 0)
        {
            CurrentHp = 0;
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (!IsAlive) return;
        // 敌人一般不治疗；如果某些敌人会治疗，限制不超过maxHP
        int actualHeal = Mathf.Min(amount, Data.maxHp - CurrentHp);
        CurrentHp += actualHeal;
        OnHpChanged?.Invoke(this, CurrentHp);
    }

    public void AddShield(int amount)
    {
        if (!IsAlive) return;
        Shield = Mathf.Min(Shield + amount, GameConstants.SHIELD_CAP);
        OnShieldChanged?.Invoke(this, Shield);
    }

    public void ClearShield()
    {
        Shield = 0;
        OnShieldChanged?.Invoke(this, Shield);
    }

    // ==========================================
    //  IElementEntity 实现
    // ==========================================

    public void ApplyReaction(ReactionData reaction)
    {
        if (reaction == null) return;
        Debug.Log($"[{Data.enemyName}] 受到元素反应：{reaction.GetDescription()}");
        // 敌人可根据反应类型做特殊处理（如Boss抵抗冰冻）
        // 默认什么都不做，冰冻由BuffSystem处理
    }

    // ==========================================
    //  死亡
    // ==========================================

    private void Die()
    {
        OnDeath?.Invoke(this);

        var bm = GameManager.Instance != null ? GameManager.Instance.BattleManager : FindObjectOfType<BattleManager>();
        bm?.UnregisterEnemy(this);

        if (GameManager.Instance != null)
            GameManager.Instance.TotalKills++;

        Debug.Log($"[{Data.enemyName}] 被击败！");
        
        // Disable instead of Destroy to avoid GC spike
        gameObject.SetActive(false);
        Destroy(gameObject, 1f);
    }

    // ==========================================
    //  敌人行动（由EnemyAI调用）
    // ==========================================

    /// <summary>对玩家执行一个行动</summary>
    public void ExecuteAction(EnemyAction action)
    {
        var bm = GameManager.Instance != null ? GameManager.Instance.BattleManager : FindObjectOfType<BattleManager>();
        var player = bm?.Player;

        switch (action.actionType)
        {
            case ActionType.Attack:
                if (player != null && player.IsAlive)
                    player.TakeDamage(action.damage, Data.element);
                break;

            case ActionType.Defend:
                AddShield(action.shield);
                if (action.heal > 0)
                    Heal(action.heal);
                break;

            case ActionType.Charge:
                // 蓄力：本回合不行动，下回合释放大伤害
                // 实际伤害在 EnemyAI 中处理（记录蓄力状态，下回合自动释放）
                Debug.Log($"[{Data.enemyName}] 蓄力中…");
                break;
        }
    }

    void OnDestroy()
    {
        var bm = GameManager.Instance != null ? GameManager.Instance.BattleManager : FindObjectOfType<BattleManager>();
        bm?.UnregisterEnemy(this);
    }
}
