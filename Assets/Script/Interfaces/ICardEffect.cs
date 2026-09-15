/// <summary>
/// 卡牌效果接口 —— 所有卡牌出牌时通过此接口执行
/// 定义者：组长
/// 实现者：组员A（Card）
/// 调用者：BattleManager、BattleUI
/// </summary>

public interface ICardEffect
{
    void Execute(IDamageable target, ElementContext context);
    Element ElementType { get; }
    int Cost { get; }
}
