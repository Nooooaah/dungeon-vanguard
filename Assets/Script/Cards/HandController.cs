using UnityEngine;

/// <summary>
/// 手牌显示控制器 —— 管理手牌排列布局
/// 组员A负责，挂在 BattleScene 的手牌区 GameObject 上
/// </summary>
public class HandController : MonoBehaviour
{
    [Header("布局参数")]
    [Range(0, 200)] public float cardSpacing = 100f;     // 卡牌间距
    [Range(0, 20)] public float hoverLift = 10f;         // 悬停上移距离
    [Range(0.5f, 2f)] public float hoverScale = 1.1f;    // 悬停放大倍数
    public float arrangeSpeed = 8f;                       // 排列动画速度

    [Header("引用")]
    public CardManager cardManager;                       // 拖入 CardManager

    private RectTransform _rectTransform;
    private Card _hoveredCard;

    void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        if (cardManager != null)
            cardManager.OnHandChanged += RefreshLayout;
    }

    void Update()
    {
        // 平滑排列（Lerp）
        ArrangeCards();
        HandleHover();
        HandleClick();
    }

    /// <summary>排列所有手牌到正确位置</summary>
    private void ArrangeCards()
    {
        var hand = GetCardsFromManager();
        if (hand == null || hand.Count == 0) return;

        float totalWidth = (hand.Count - 1) * cardSpacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < hand.Count; i++)
        {
            var card = hand[i];
            if (card == null) continue;

            Vector3 targetPos = new Vector3(startX + i * cardSpacing, 0, 0);
            Vector3 targetScale = Vector3.one;

            // 悬停的卡牌稍微上移+放大
            if (card == _hoveredCard)
            {
                targetPos.y += hoverLift;
                targetScale = Vector3.one * hoverScale;
            }

            // 平滑过渡
            card.transform.localPosition = Vector3.Lerp(
                card.transform.localPosition, targetPos, Time.deltaTime * arrangeSpeed);
            card.transform.localScale = Vector3.Lerp(
                card.transform.localScale, targetScale, Time.deltaTime * arrangeSpeed);

            // 层级：悬停卡牌在最上面
            card.transform.SetSiblingIndex(card == _hoveredCard ? hand.Count - 1 : i);
        }
    }

    /// <summary>处理鼠标悬停检测</summary>
    private void HandleHover()
    {
        Card newHover = null;

        // 简单的射线检测：找到鼠标下的卡牌
        var hand = GetCardsFromManager();
        foreach (var card in hand)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(
                card.transform as RectTransform, Input.mousePosition))
            {
                newHover = card;
                break;
            }
        }

        _hoveredCard = newHover;
    }

    /// <summary>处理点击出牌</summary>
    private void HandleClick()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (_hoveredCard == null) return;
        if (cardManager == null) return;

        cardManager.PlayCard(_hoveredCard);
    }

    /// <summary>手牌变化时刷新布局</summary>
    public void RefreshLayout()
    {
        // 由 CardManager.OnHandChanged 触发
    }

    private System.Collections.Generic.List<Card> GetCardsFromManager()
    {
        return cardManager != null ? cardManager.GetHand() : new System.Collections.Generic.List<Card>();
    }

    void OnDestroy()
    {
        if (cardManager != null)
            cardManager.OnHandChanged -= RefreshLayout;
    }
}
