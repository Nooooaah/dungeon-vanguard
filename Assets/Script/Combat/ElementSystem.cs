using System.Collections.Generic;

/// <summary>
/// 元素上下文 —— 记录本回合已打出的元素，传给ICardEffect.Execute()
/// </summary>
public class ElementContext
{
    /// <summary>本回合已打出的元素列表（按出牌顺序）</summary>
    public List<Element> PlayedElementsThisTurn = new List<Element>();

    /// <summary>当前攻击目标的元素（敌人的元素，用于牌×敌人反应）</summary>
    public Element TargetElement;

    /// <summary>记录一张牌打出时的元素</summary>
    public void RecordPlayed(Element e)
    {
        PlayedElementsThisTurn.Add(e);
    }

    /// <summary>回合结束时清空</summary>
    public void Clear()
    {
        PlayedElementsThisTurn.Clear();
        TargetElement = Element.None; // 无敌人时默认无元素，不会触发任何克制反应
    }
}

/// <summary>
/// 元素反应数据
/// </summary>
[System.Serializable]
public class ReactionData
{
    public string reactionName;          // "蒸汽爆发"
    public ReactionModifier modifier;    // 加成方式
    public int value;                    // 加成数值
    public bool freezeTarget;            // 是否冰冻目标

    public string GetDescription() => modifier switch
    {
        ReactionModifier.FlatBonus => $"{reactionName}：伤害 +{value}",
        ReactionModifier.PercentBonus => $"{reactionName}：伤害 ×{value / 100f:F1}",
        ReactionModifier.Freeze => $"{reactionName}：目标跳过下回合",
        _ => reactionName
    };
}

/// <summary>反应加成方式</summary>
public enum ReactionModifier
{
    FlatBonus,      // 固定加成（如 +4 伤害）
    PercentBonus,   // 百分比加成（如 ×1.5 = 150%）
    Freeze          // 冰冻效果
}

/// <summary>
/// 元素反应系统 —— 查表触发 Combo
/// 组长负责，写在 Scripts/Combat/ElementSystem.cs
///
/// 支持两种反应触发方式：
///   ① 牌 × 牌：同一回合内打出不同元素的牌（手牌 combo）
///   ② 牌 × 敌人：玩家牌的元素克制敌人元素（属性克制）
/// </summary>
public class ElementSystem
{
    // ===== 牌 × 牌 反应表 =====
    private Dictionary<(Element, Element), ReactionData> _cardReactionTable;

    // ===== 牌 × 敌人 反应表 =====
    private Dictionary<(Element, Element), ReactionData> _enemyReactionTable;

    public ElementSystem()
    {
        BuildCardReactionTable();
        BuildEnemyReactionTable();
    }

    // ==========================================
    //  牌 × 牌 反应（原有）
    // ==========================================
    private void BuildCardReactionTable()
    {
        _cardReactionTable = new Dictionary<(Element, Element), ReactionData>
        {
            { (Element.Fire, Element.Water), new ReactionData
                { reactionName = "蒸汽爆发", modifier = ReactionModifier.FlatBonus, value = 4 } },
            { (Element.Fire, Element.Wind), new ReactionData
                { reactionName = "烈焰风暴", modifier = ReactionModifier.PercentBonus, value = 150 } },
            { (Element.Water, Element.Fire), new ReactionData
                { reactionName = "蒸汽爆发", modifier = ReactionModifier.FlatBonus, value = 4 } },
            { (Element.Water, Element.Wind), new ReactionData
                { reactionName = "冰刺", modifier = ReactionModifier.Freeze, freezeTarget = true } },
            { (Element.Wind, Element.Fire), new ReactionData
                { reactionName = "烈焰风暴", modifier = ReactionModifier.PercentBonus, value = 150 } },
            { (Element.Wind, Element.Water), new ReactionData
                { reactionName = "冰刺", modifier = ReactionModifier.Freeze, freezeTarget = true } },
        };
    }

    // ==========================================
    //  牌 × 敌人 反应（新增，克制关系）
    // ==========================================
    private void BuildEnemyReactionTable()
    {
        _enemyReactionTable = new Dictionary<(Element, Element), ReactionData>
        {
            // 火牌打水敌人 → 蒸汽爆发
            { (Element.Fire, Element.Water), new ReactionData
                { reactionName = "蒸汽爆发", modifier = ReactionModifier.FlatBonus, value = 4 } },

            // 水牌打火敌人 → 蒸汽爆发
            { (Element.Water, Element.Fire), new ReactionData
                { reactionName = "蒸汽爆发", modifier = ReactionModifier.FlatBonus, value = 4 } },

            // 风牌打水敌人 → 冰刺（冰冻敌人）
            { (Element.Wind, Element.Water), new ReactionData
                { reactionName = "冰刺", modifier = ReactionModifier.Freeze, freezeTarget = true } },

            // 风牌打火敌人 → 烈焰风暴
            { (Element.Wind, Element.Fire), new ReactionData
                { reactionName = "烈焰风暴", modifier = ReactionModifier.PercentBonus, value = 150 } },

            // 火牌打风敌人 → 烈焰风暴
            { (Element.Fire, Element.Wind), new ReactionData
                { reactionName = "烈焰风暴", modifier = ReactionModifier.PercentBonus, value = 150 } },

            // 水牌打风敌人 → 冰刺
            { (Element.Water, Element.Wind), new ReactionData
                { reactionName = "冰刺", modifier = ReactionModifier.Freeze, freezeTarget = true } },
        };
    }

    // ==========================================
    //  公开方法
    // ==========================================

    /// <summary>
    /// 检查牌×牌反应（同一回合内打出多张牌）
    /// </summary>
    public ReactionData CheckCardReaction(Element current, ElementContext context)
    {
        foreach (var played in context.PlayedElementsThisTurn)
        {
            if (_cardReactionTable.TryGetValue((current, played), out var reaction))
                return reaction;
        }
        return null;
    }

    /// <summary>
    /// 检查牌×敌人反应（属性克制，每张牌打出时自动检查）
    /// </summary>
    public ReactionData CheckEnemyReaction(Element cardElement, Element enemyElement)
    {
        if (_enemyReactionTable.TryGetValue((cardElement, enemyElement), out var reaction))
            return reaction;
        return null;
    }

    /// <summary>
    /// 根据反应计算最终伤害
    /// </summary>
    public int CalculateReactionDamage(int baseDamage, ReactionData reaction)
    {
        if (reaction == null) return baseDamage;

        return reaction.modifier switch
        {
            ReactionModifier.FlatBonus => baseDamage + reaction.value,
            ReactionModifier.PercentBonus => (int)(baseDamage * reaction.value / 100f),
            ReactionModifier.Freeze => baseDamage,
            _ => baseDamage
        };
    }

    /// <summary>获取反应描述（UI显示用）</summary>
    public string GetReactionText(Element e1, Element e2)
    {
        if (_cardReactionTable.TryGetValue((e1, e2), out var reaction))
            return $"{Emoji(e1)} + {Emoji(e2)} {reaction.reactionName}！";
        return "";
    }

    public string Emoji(Element e) => e switch
    {
        Element.Fire => "[火]",
        Element.Water => "[水]",
        Element.Wind => "[风]",
        _ => "?"
    };
}
