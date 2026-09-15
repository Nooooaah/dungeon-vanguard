/// <summary>
/// 元素实体接口 —— 参与元素反应的对象实现此接口
/// 定义者：组长
/// 实现者：组员A（Card）、组员B（EnemyBase）
/// 调用者：组长（ElementSystem）
/// </summary>
public interface IElementEntity
{
    Element ElementType { get; }
    void ApplyReaction(ReactionData reaction);
}