/// <summary>
/// 伤害计算系统 —— 伤害/护盾/治疗的统一计算管线
/// 组长负责，写在 Scripts/Combat/DamageSystem.cs
/// </summary>
public class DamageSystem
{
    private readonly ElementSystem _elementSystem;
    private readonly BuffSystem _buffSystem;

    public DamageSystem(ElementSystem elementSystem, BuffSystem buffSystem)
    {
        _elementSystem = elementSystem;
        _buffSystem = buffSystem;
    }

    /// <summary>
    /// 完整伤害计算管线：护盾吸收 → 剩余扣HP → 死亡判定
    /// </summary>
    /// <param name="target">目标（敌人或玩家）</param>
    /// <param name="rawDamage">原始伤害值（未含反应加成）</param>
    /// <param name="element">攻击元素</param>
    /// <param name="context">本回合元素上下文</param>
    /// <returns>实际造成的伤害量（用于跳字显示）</returns>
    public int DealDamage(IDamageable target, int rawDamage, Element element, ElementContext context)
    {
        if (rawDamage <= 0) return 0;
        if (!target.IsAlive) return 0;

        // 1. 检查元素反应（仅牌×牌 combo）
        int finalDamage = rawDamage;
        ReactionData reaction = null;
        string reactionText = "";

        if (context != null)
        {
            bool anyFreeze = false;

            // 牌 × 牌反应（同回合打出多张牌 combo）
            var cardReaction = _elementSystem.CheckCardReaction(element, context);
            if (cardReaction != null)
            {
                finalDamage = _elementSystem.CalculateReactionDamage(rawDamage, cardReaction);
                if (cardReaction.freezeTarget) anyFreeze = true;
                reactionText = _elementSystem.GetReactionText(element,
                    GetLastPlayedElement(context, element));
                reaction = cardReaction;
            }

            // 冰冻效果
            if (anyFreeze)
            {
                _buffSystem?.ApplyBuff(target, BuffType.Freeze, 0, 1);
            }

            if (reaction != null && target is IElementEntity entity)
            {
                entity.ApplyReaction(reaction);
            }
        }

        // 2. 造成伤害（target内部处理护盾→HP的管线）
        target.TakeDamage(finalDamage, element);

        // 4. 记录反应文本（供UI显示），通过简单的静态方式传递
        LastReactionText = reactionText;

        return finalDamage;
    }

    /// <summary>最近一次反应文本（UI读取用）</summary>
    public static string LastReactionText { get; private set; }

    /// <summary>
    /// 治疗：回复HP。
    /// ⚠️ 上限检查由 IDamageable 实现方负责（Heal内部应限制不超过maxHP）
    /// </summary>
    /// <returns>实际治疗量</returns>
    public int Heal(IDamageable target, int amount)
    {
        if (amount <= 0) return 0;
        if (!target.IsAlive) return 0;

        target.Heal(amount);
        return amount;
    }

    /// <summary>
    /// 添加护盾：可叠加。
    /// ⚠️ 上限 GameConstants.SHIELD_CAP (99) 由 IDamageable 实现方负责
    /// </summary>
    /// <returns>实际添加的护盾量</returns>
    public int AddShield(IDamageable target, int amount)
    {
        if (amount <= 0) return 0;
        if (!target.IsAlive) return 0;

        target.AddShield(amount);
        return amount;
    }

    // 辅助：从context里找到参与反应的另一个元素
    private Element GetLastPlayedElement(ElementContext context, Element current)
    {
        foreach (var e in context.PlayedElementsThisTurn)
        {
            if (e != current) return e;
        }
        return current;
    }
}
