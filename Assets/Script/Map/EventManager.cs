using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 事件数据模型
/// </summary>
[System.Serializable]
public class EventData
{
    public string title;
    [TextArea(2, 4)] public string description;
    public List<EventChoice> choices;
}

/// <summary>事件选项</summary>
[System.Serializable]
public class EventChoice
{
    public string text;
    public EventEffect[] effects;
}

/// <summary>事件效果</summary>
[System.Serializable]
public class EventEffect
{
    public EventEffectType type;
    public int value;
}

public enum EventEffectType
{
    Heal,           // 回复HP
    Damage,         // 扣HP
    GainCard,       // 获得卡牌
    RemoveCard,     // 移除卡牌
    GainShield,     // 获得护盾（下届战开始时生效）
    Nothing,        // 继续探索
    Gamble          // 赌博：50%回复30HP / 50%失去10HP
}

/// <summary>
/// 事件管理器 —— 负责事件模板的生成与执行
/// </summary>
public static class EventManager
{
    private static List<EventData> _eventTemplates;

    /// <summary>当 ExecuteChoice 包含 RemoveCard 时设为 true，MapController 据此延迟 CompleteNode</summary>
    public static bool PendingCardRemoval { get; set; }

    /// <summary>随机获取一个事件</summary>
    public static EventData GetRandomEvent()
    {
        InitIfNeeded();
        return _eventTemplates[Random.Range(0, _eventTemplates.Count)];
    }

    /// <summary>执行选项效果</summary>
    public static void ExecuteChoice(EventChoice choice)
    {
        if (choice?.effects == null) return;

        var gm = GameManager.Instance;
        if (gm == null) return;

        PendingCardRemoval = false;

        foreach (var effect in choice.effects)
        {
            switch (effect.type)
            {
                case EventEffectType.Heal:
                    gm.PlayerCurrentHp = Mathf.Min(gm.PlayerCurrentHp + effect.value, gm.PlayerMaxHp);
                    Debug.Log($"[事件] 回复 {effect.value} HP");
                    break;

                case EventEffectType.Damage:
                    gm.PlayerCurrentHp = Mathf.Max(1, gm.PlayerCurrentHp - effect.value);
                    Debug.Log($"[事件] 失去 {effect.value} HP");
                    break;

                case EventEffectType.GainCard:
                    var allCards = Resources.LoadAll<CardData>("");
                    if (allCards.Length > 0)
                    {
                        for (int i = 0; i < effect.value; i++)
                        {
                            var card = allCards[Random.Range(0, allCards.Length)];
                            gm.AddCardToDeck(card);
                            Debug.Log($"[事件] 获得卡牌：{card.cardName}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("[事件] Resources 中未找到 CardData");
                    }
                    break;

                case EventEffectType.RemoveCard:
                    PendingCardRemoval = true;
                    Debug.Log("[事件] 等待玩家选择要移除的卡牌");
                    break;

                case EventEffectType.GainShield:
                    gm.NextBattleStartShield = effect.value;
                    Debug.Log($"[事件] 下届战开始 {effect.value} 护盾");
                    break;

                case EventEffectType.Gamble:
                    // 在玩家选择时才随机决定结果
                    if (Random.value < 0.5f)
                    {
                        gm.PlayerCurrentHp = Mathf.Min(gm.PlayerCurrentHp + 30, gm.PlayerMaxHp);
                        Debug.Log("[事件] 赌博成功！回复 30 HP");
                    }
                    else
                    {
                        gm.PlayerCurrentHp = Mathf.Max(1, gm.PlayerCurrentHp - 10);
                        Debug.Log("[事件] 赌博失败！失去 10 HP");
                    }
                    break;
            }
        }
    }

    private static void InitIfNeeded()
    {
        if (_eventTemplates != null) return;

        _eventTemplates = new List<EventData>
        {
            // 事件1：神秘药水
            new EventData
            {
                title = "神秘药水",
                description = "路边石台上放着一瓶散发微光的药水，瓶身上刻着古老的符文。",
                choices = new List<EventChoice>
                {
                    new EventChoice
                    {
                        text = "喝下去（回复15HP）",
                        effects = new EventEffect[] { new() { type = EventEffectType.Heal, value = 15 } }
                    },
                    new EventChoice
                    {
                        text = "放下来，继续探索",
                        effects = new EventEffect[] { new() { type = EventEffectType.Nothing } }
                    },
                    new EventChoice
                    {
                        text = "一搏！（50%：+30HP / 50%：-10HP）",
                        effects = new EventEffect[] { new() { type = EventEffectType.Gamble } }
                    }
                }
            },

            // 事件2：遭遇宝箱
            new EventData
            {
                title = "遭遇宝箱",
                description = "你在角落的箱子上发现了红色锈迹的锁。",
                choices = new List<EventChoice>
                {
                    new EventChoice
                    {
                        text = "打开（获得一张随机牌）",
                        effects = new EventEffect[] { new() { type = EventEffectType.GainCard, value = 1 } }
                    },
                    new EventChoice
                    {
                        text = "离开，继续探索",
                        effects = new EventEffect[] { new() { type = EventEffectType.Nothing } }
                    },
                    new EventChoice
                    {
                        text = "强行撬开，获得两张随机牌（-10HP）",
                        effects = new EventEffect[]
                        {
                            new() { type = EventEffectType.GainCard, value = 2 },
                            new() { type = EventEffectType.Damage, value = 10 }
                        }
                    }
                }
            },

            // 事件3：休憩营地
            new EventData
            {
                title = "休憩营地",
                description = "你发现了一个简易的营地，篝火仍在燃烧。",
                choices = new List<EventChoice>
                {
                    new EventChoice
                    {
                        text = "休息片刻（回复10HP）",
                        effects = new EventEffect[] { new() { type = EventEffectType.Heal, value = 10 } }
                    },
                    new EventChoice
                    {
                        text = "整理行囊（移除一张牌）",
                        effects = new EventEffect[] { new() { type = EventEffectType.RemoveCard, value = 1 } }
                    }
                }
            },

            // 事件4：邪恶法师拦路
            new EventData
            {
                title = "邪恶法师",
                description = "一名邪恶法师拦住去路，法杖闪烁着不祥的光芒：'留下买路财，否则别想通过！'",
                choices = new List<EventChoice>
                {
                    new EventChoice
                    {
                        text = "唯唯诺诺，卑微屈服（-3HP）",
                        effects = new EventEffect[] { new() { type = EventEffectType.Damage, value = 3 } }
                    },
                    new EventChoice
                    {
                        text = "奋起反击，战！（获得药水，回复10HP）",
                        effects = new EventEffect[] { new() { type = EventEffectType.Heal, value = 10 } }
                    }
                }
            },

            // 事件5：地牢求救女子
            new EventData
            {
                title = "求救女子",
                description = "在地牢深处，一位楚楚可怜的美丽女子向你求救，恳请你送她回家。",
                choices = new List<EventChoice>
                {
                    new EventChoice
                    {
                        text = "视若无睹，一走了之",
                        effects = new EventEffect[] { new() { type = EventEffectType.Nothing } }
                    },
                    new EventChoice
                    {
                        text = "上前搭救，助人为乐",
                        effects = new EventEffect[] { new() { type = EventEffectType.Damage, value = 3 } }
                    },
                    new EventChoice
                    {
                        text = "恶贯满盈，杀（获得武器，+1张卡牌）",
                        effects = new EventEffect[] { new() { type = EventEffectType.GainCard, value = 1 } }
                    }
                }
            }
        };
    }
}
