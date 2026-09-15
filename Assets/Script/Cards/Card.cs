using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 卡牌运行时 —— 挂载在 Card.prefab 上
/// 实现 ICardEffect 接口，负责卡牌显示和效果执行
/// 组员A负责
/// </summary>
public class Card : MonoBehaviour, ICardEffect
{
    // ===== 数据 =====
    public CardData Data { get; private set; }

    // ===== ICardEffect =====
    public Element ElementType => Data?.element ?? Element.None;
    public int Cost => Data?.cost ?? 0;

    // ===== UI引用（由Card.prefab的子物体拖入）=====
    [Header("UI 引用")]
    public TMP_Text nameText;
    public TMP_Text costText;
    public TMP_Text descText;
    public TMP_Text attackText;      // 攻击力数值
    public TMP_Text healText;        // 治疗/护盾数值
    public Image elementIcon;
    public Image cardBackground;
    public Button button;
    public GameObject costTooHighOverlay;   // 能量不足时的灰色遮罩
    public GameObject attackIconGroup;      // AttackIcon 整组（无伤害时隐藏）
    public GameObject healIconGroup;        // HealthIcon 整组（无治疗/护盾时隐藏）

    // ===== 回调 =====
    public System.Action<Card> OnPlayed;    // 被打出时回调（BattleUI监听）

    /// <summary>用CardData初始化卡牌显示</summary>
    public void Init(CardData data)
    {
        Data = data;
        RefreshUI();
    }

    /// <summary>刷新卡牌显示</summary>
    public void RefreshUI()
    {
        if (Data == null) return;

        if (nameText != null) nameText.text = Data.cardName;
        if (costText != null) costText.text = Data.cost.ToString();
        if (descText != null) descText.text = Data.GetDescription();

        // Element badge color
        Color elementColor = GetElementColor(Data.element);
        if (elementIcon != null)
            elementIcon.color = elementColor;

        // Attack value
        if (attackIconGroup != null)
            attackIconGroup.SetActive(Data.damage > 0);
        if (attackText != null && Data.damage > 0)
            attackText.text = Data.damage.ToString();

        // Heal / Shield value (show whichever is non-zero)
        int healOrShield = Mathf.Max(Data.heal, Data.shield);
        if (healIconGroup != null)
            healIconGroup.SetActive(healOrShield > 0);
        if (healText != null && healOrShield > 0)
            healText.text = healOrShield.ToString();
    }

    /// <summary>更新能量不足的显示状态</summary>
    public void SetAffordable(bool canAfford)
    {
        if (costTooHighOverlay != null)
            costTooHighOverlay.SetActive(!canAfford);
        if (button != null)
            button.interactable = canAfford;
    }

    // ==========================================
    //  ICardEffect 实现 —— 卡牌效果执行
    // ==========================================

    public void Execute(IDamageable target, ElementContext context)
    {
        if (Data == null) return;

        var bm = GameManager.Instance?.BattleManager;
        if (bm == null) return;

        // 攻击效果 → DamageSystem.DealDamage（自动处理反应）
        if (Data.damage > 0 && target != null)
        {
            bm.DamageSystem.DealDamage(target, Data.damage, Data.element, context);
        }

        // 护盾 → DamageSystem.AddShield（加给自己）
        if (Data.shield > 0)
        {
            bm.DamageSystem.AddShield(bm.Player, Data.shield);
        }

        // 治疗 → DamageSystem.Heal
        if (Data.heal > 0)
        {
            bm.DamageSystem.Heal(bm.Player, Data.heal);
        }

        // 额外抽牌
        if (Data.draw > 0)
        {
            for (int i = 0; i < Data.draw; i++)
                bm.OnDrawCard?.Invoke();
        }

        // Buff效果 → BuffSystem
        if (Data.buffType != BuffType.None && target != null)
        {
            bm.BuffSystem.ApplyBuff(target, Data.buffType, Data.buffValue, Data.buffDuration);
        }

        // 通知监听者：这张牌已打出
        OnPlayed?.Invoke(this);
    }

    /// <summary>元素对应颜色</summary>
    public static Color GetElementColor(Element e) => e switch
    {
        Element.Fire => new Color(0.91f, 0.30f, 0.24f),  // #e94560 红
        Element.Water => new Color(0.20f, 0.60f, 0.86f), // #3498db 蓝
        Element.Wind => new Color(0.18f, 0.80f, 0.44f),  // #2ecc71 绿
        _ => Color.gray
    };
}
