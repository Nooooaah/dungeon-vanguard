/// <summary>
/// 可受伤实体接口 —— 所有能"被打"的对象都实现此接口（Player、EnemyBase）
/// 定义者：组长
/// 实现者：组员B（EnemyBase、Player）
/// 调用者：组长（DamageSystem）、组员A（Card.Execute）、组员D（HPBarUI）
/// </summary>
public interface IDamageable
{
    /// <summary>当前生命值</summary>
    int CurrentHP { get; }
    /// <summary>最大生命值</summary>
    int MaxHP { get; }
    /// <summary>当前护盾值</summary>
    int Shield { get; }
    /// <summary>是否存活</summary>
    bool IsAlive { get; }

    void TakeDamage(int amount, Element element);
    void Heal(int amount);
    void AddShield(int amount);
    void ClearShield();
}
