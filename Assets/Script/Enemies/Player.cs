using UnityEngine;

/// <summary>
/// 玩家运行时 —— 实现 IDamageable
/// 组员B负责，挂载在 BattleScene 的 Player GameObject 上
/// </summary>
public class Player : MonoBehaviour, IDamageable
{
    public CharacterStats Stats { get; private set; }

    // ===== IDamageable =====
    public int CurrentHP => Stats?.currentHp ?? 0;
    public int MaxHP => Stats?.maxHp ?? 0;
    public int Shield => Stats?.shield ?? 0;
    public bool IsAlive => Stats != null && Stats.IsAlive;
    public Element ElementType => Stats?.element ?? Element.None;

    // ===== 事件（UI绑定用）=====
    public System.Action<int, int> OnHpChanged;     // (current, max)
    public System.Action<int> OnShieldChanged;       // (shield)
    public System.Action<int, int> OnEnergyChanged;  // (current, max)
    public System.Action OnDeath;

    void Start()
    {
        // 从 GameManager 获取玩家的持久数据初始化 Stats
        var gm = GameManager.Instance;
        Element elem = gm != null && gm.ChosenElement != Element.None
            ? gm.ChosenElement
            : Element.Fire;
        Stats = new CharacterStats(
            gm != null ? gm.PlayerMaxHp : GameConstants.PLAYER_MAX_HP,
            GameConstants.PLAYER_MAX_ENERGY,
            elem
        );
        if (gm != null) Stats.currentHp = gm.PlayerCurrentHp;

        // 注册到 BattleManager
        var bm = gm != null ? gm.BattleManager : FindObjectOfType<BattleManager>();
        bm?.RegisterPlayer(this);

        Debug.Log($"[Player] 初始化完成，HP={Stats.currentHp}/{Stats.maxHp}，元素={Stats.element}");
    }

    // ==========================================
    //  IDamageable 实现
    // ==========================================

    public void TakeDamage(int amount, Element attackerElement)
    {
        if (!IsAlive) return;
        if (amount <= 0) return;

        // 护盾先吸收
        if (Stats.shield > 0)
        {
            int absorbed = Mathf.Min(Stats.shield, amount);
            Stats.shield -= absorbed;
            amount -= absorbed;
            OnShieldChanged?.Invoke(Stats.shield);
        }

        // 剩余扣HP
        Stats.currentHp -= amount;
        if (Stats.currentHp < 0) Stats.currentHp = 0;

        OnHpChanged?.Invoke(Stats.currentHp, Stats.maxHp);
        Debug.Log($"[Player] 受到 {amount} 点伤害，剩余HP={Stats.currentHp}/{Stats.maxHp}");

        if (Stats.currentHp <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (!IsAlive) return;
        // 上限检查
        int actualHeal = Mathf.Min(amount, Stats.maxHp - Stats.currentHp);
        Stats.currentHp += actualHeal;
        OnHpChanged?.Invoke(Stats.currentHp, Stats.maxHp);
    }

    public void AddShield(int amount)
    {
        if (!IsAlive) return;
        Stats.shield = Mathf.Min(Stats.shield + amount, GameConstants.SHIELD_CAP);
        OnShieldChanged?.Invoke(Stats.shield);
    }

    public void ClearShield()
    {
        Stats.ClearShield();
        OnShieldChanged?.Invoke(Stats.shield);
    }

    // ==========================================
    //  能量管理
    // ==========================================

    /// <summary>恢复满能量</summary>
    public void RestoreEnergy()
    {
        Stats.RestoreEnergy();
        OnEnergyChanged?.Invoke(Stats.currentEnergy, Stats.maxEnergy);
    }

    /// <summary>消耗能量，返回是否成功</summary>
    public bool SpendEnergy(int amount)
    {
        bool success = Stats.SpendEnergy(amount);
        if (success)
            OnEnergyChanged?.Invoke(Stats.currentEnergy, Stats.maxEnergy);
        return success;
    }

    /// <summary>当前能量是否足够</summary>
    public bool HasEnoughEnergy(int cost) => Stats.currentEnergy >= cost;

    // ==========================================
    //  死亡
    // ==========================================

    private void Die()
    {
        OnDeath?.Invoke();
        Debug.Log("[Player] 玩家死亡！");

        // 同步到 GameManager
        if (GameManager.Instance != null)
            GameManager.Instance.PlayerCurrentHp = 0;
    }

    // ==========================================
    //  跨战斗持久化
    // ==========================================

    /// <summary>战斗结束时保存玩家状态到 GameManager</summary>
    public void SaveToGameManager()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.PlayerCurrentHp = Stats.currentHp;
        GameManager.Instance.PlayerMaxHp = Stats.maxHp;
    }
}
