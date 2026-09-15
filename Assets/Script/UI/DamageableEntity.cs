using UnityEngine;

/// <summary>
/// Simple MonoBehaviour implementation of IDamageable for battle entities.
/// Provides HP, shield, and damage/heal logic with event notifications.
/// </summary>
public class DamageableEntity : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public int maxHP = 30;
    public int currentHP = 30;
    public int shield = 0;

    public int CurrentHP => currentHP;
    public int MaxHP => maxHP;
    public int Shield => shield;
    public bool IsAlive => currentHP > 0;
    public bool IsDead => currentHP <= 0;

    /// <summary>
    /// Raised when HP changes: (currentHP, maxHP, damageAmount).
    /// damageAmount > 0 means damage taken, < 0 means healed.
    /// </summary>
    public event System.Action<int, int, int> OnHPChanged;

    /// <summary>
    /// Raised when shield changes: (newShield, delta).
    /// </summary>
    public event System.Action<int, int> OnShieldChanged;

    public void TakeDamage(int amount, Element attackerElement)
    {
        if (!IsAlive) return;

        int remaining = amount;

        // Shield absorbs first
        if (shield > 0)
        {
            int absorbed = Mathf.Min(shield, remaining);
            shield -= absorbed;
            remaining -= absorbed;
            OnShieldChanged?.Invoke(shield, -absorbed);
        }

        if (remaining > 0)
        {
            currentHP = Mathf.Max(0, currentHP - remaining);
            OnHPChanged?.Invoke(currentHP, maxHP, remaining);
        }
    }

    public void Heal(int amount)
    {
        if (!IsAlive) return;
        int healed = Mathf.Min(amount, maxHP - currentHP);
        currentHP += healed;
        OnHPChanged?.Invoke(currentHP, maxHP, -healed);
    }

    public void AddShield(int amount)
    {
        shield += amount;
        OnShieldChanged?.Invoke(shield, amount);
    }

    public void ClearShield()
    {
        shield = 0;
        OnShieldChanged?.Invoke(shield, 0);
    }

    public void ResetHP(int newMaxHP)
    {
        maxHP = newMaxHP;
        currentHP = newMaxHP;
        shield = 0;
        OnHPChanged?.Invoke(currentHP, maxHP, 0);
        OnShieldChanged?.Invoke(shield, 0);
    }
}
