using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 卡牌 ScriptableObject 配置 —— 在 Inspector 里填数据即可
/// 创建方式：右键 Project 窗口 → Create → Game → Card
/// 组员A负责
/// </summary>
[CreateAssetMenu(fileName = "Card_", menuName = "Game/Card")]
public class CardData : ScriptableObject
{
    [Header("基本信息")]
    public string cardName;         // "火球术"
    public Element element;         // Fire / Water / Wind
    public CardType cardType;       // Attack / Skill
    [Range(0, 3)] public int cost; // 能耗

    [Header("攻击效果（Attack牌用）")]
    [FormerlySerializedAs("damageAmount")]
    public int damage;              // 伤害值

    [Header("技能效果（Skill牌用）")]
    [FormerlySerializedAs("shieldAmount")]
    public int shield;              // 护盾值
    [FormerlySerializedAs("healAmount")]
    public int heal;                // 治疗值
    [FormerlySerializedAs("drawAmount")]
    public int draw;                // 额外抽牌数

    [Header("Buff效果（可选）")]
    public BuffType buffType;       // None / Burn / Freeze
    public int buffValue;           // Buff数值（灼烧=每回合伤害）
    public int buffDuration;        // Buff持续回合数

    [Header("特殊效果")]
    public bool targetSkipTurn;     // 目标跳过下回合
    public int selfDamage;          // 自伤值（对自身造成伤害）
    public bool dotDamage;          // 是否有持续伤害（灼烧）
    public int dotAmount;           // 持续伤害值

    [Header("外观")]
    public Sprite artwork;              // 卡牌插画（UI显示用）

    [Header("描述")]
    [TextArea(2, 4)] public string description; // 卡牌描述文本

    /// <summary>生成卡牌描述（自动拼接效果文本）</summary>
    public string GetDescription()
    {
        if (!string.IsNullOrEmpty(description)) return description;

        // 自动生成描述
        string desc = "";
        if (damage > 0) desc += $"造成 {damage} 点伤害";
        if (shield > 0) desc += (desc.Length > 0 ? "，" : "") + $"获得 {shield} 点护盾";
        if (heal > 0) desc += (desc.Length > 0 ? "，" : "") + $"回复 {heal} 点生命";
        if (draw > 0) desc += (desc.Length > 0 ? "，" : "") + $"抽 {draw} 张牌";
        if (buffType == BuffType.Burn) desc += (desc.Length > 0 ? "，" : "") + $"灼烧 {buffValue}/回合（{buffDuration}回合）";
        if (buffType == BuffType.Freeze) desc += (desc.Length > 0 ? "，" : "") + $"冰冻 {buffDuration} 回合";
        return desc;
    }

    /// <summary>元素对应中文名</summary>
    public static string GetElementName(Element e) => e switch
    {
        Element.Fire => "火",
        Element.Water => "水",
        Element.Wind => "风",
        _ => ""
    };

    /// <summary>元素对应颜色</summary>
    public static Color GetElementColor(Element e) => e switch
    {
        Element.Fire => new Color(1.0f, 0.27f, 0.0f),
        Element.Water => new Color(0.0f, 0.45f, 0.85f),
        Element.Wind => new Color(0.13f, 0.60f, 0.20f),
        _ => Color.white
    };

    /// <summary>元素对应浅色</summary>
    public static Color GetElementColorLight(Element e) => e switch
    {
        Element.Fire => new Color(1.0f, 0.65f, 0.0f),
        Element.Water => new Color(0.0f, 0.81f, 0.82f),
        Element.Wind => new Color(0.0f, 0.81f, 0.82f),
        _ => Color.gray
    };
}
