/// <summary>
/// 卡牌效果接口 —— 所有卡牌出牌时通过此接口执行
/// 定义者：组长
/// 实现者：组员A（Card）
<<<<<<< HEAD
/// 调用者：组长（BattleManager）、组员A（HandController）
=======
/// 调用者：BattleManager、BattleUI
>>>>>>> dd15dc953bda76878a619e06d67d3f5c5cb965a7
/// </summary>

public interface ICardEffect
{
    void Execute(IDamageable target, ElementContext context);
    Element ElementType { get; }
    int Cost { get; }
}
