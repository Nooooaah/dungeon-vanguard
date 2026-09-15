using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 敌人 ScriptableObject 配置 —— 在 Inspector 里填数据即可
/// 创建方式：右键 Project 窗口 → Create → Game → Enemy
/// 组员B负责
/// </summary>
[CreateAssetMenu(fileName = "Enemy_", menuName = "Game/Enemy")]
public class EnemyData : ScriptableObject
{
    [Header("基本信息")]
    public string enemyName;        // "火元素"
    public Element element;         // Fire / Water / Wind
    public EnemyType type;          // Normal / Elite / Boss
    [FormerlySerializedAs("maxHP")]
    public int maxHp;               // 最大生命值

    [Header("行动列表")]
    public List<EnemyAction> actions; // 该敌人可执行的行动

    [Header("Boss专用")]
    [Tooltip("HP低于此百分比触发狂暴（0=不触发）")]
    [Range(0, 100)] public int rageThreshold;   // 如 50 表示HP<50%狂暴
    public List<EnemyAction> rageActions;        // 狂暴后新增的行动（可选）
}

/// <summary>单个敌人行动</summary>
[System.Serializable]
public class EnemyAction
{
    [FormerlySerializedAs("displayText")]
    public string intentText;       // 意图预览："攻击 8 点"
    [FormerlySerializedAs("type")]
    public ActionType actionType;   // 攻击/防御/蓄力
    [FormerlySerializedAs("value")]
    public int damage;              // 伤害值
    [FormerlySerializedAs("value2")]
    public int shield;              // 护盾值（给自己加）
    public int heal;                // 治疗值
    [Range(0, 100)] public int weight; // AI选择权重（越大越容易被选中）

    /// <summary>生成意图描述（UI显示用）</summary>
    public string GetIntentText()
    {
        return actionType switch
        {
            ActionType.Attack => $"[攻] {intentText}",
            ActionType.Defend => $"[防] {intentText}",
            ActionType.Charge => $"[蓄] {intentText}",
            ActionType.AttackAndDefend => $"[攻防] {intentText}",
            ActionType.Cure => $"[治] {intentText}",
            _ => intentText
        };
    }
}

/// <summary>敌人类型</summary>
public enum EnemyType { Normal, Elite, Boss }

/// <summary>行动类型</summary>
public enum ActionType { Attack, Defend, Charge,AttackAndDefend,Cure}
