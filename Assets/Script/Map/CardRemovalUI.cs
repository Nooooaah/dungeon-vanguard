using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 选牌移除界面 —— 纯代码构建，不依赖预制体
/// 玩家从当前牌组中选择一张牌移除
/// </summary>
public class CardRemovalUI : MonoBehaviour
{
    private TMP_FontAsset _font;
    private GameObject _panel;
    private Transform _cardListContainer;
    private System.Action _onComplete;

    void Awake()
    {
        _font = Resources.Load<TMP_FontAsset>("msyhl SDF");
#if UNITY_EDITOR
        if (_font == null)
            _font = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/msyhl SDF.asset");
#endif
        if (_font == null)
            _font = TMP_Settings.defaultFontAsset;

        BuildUI();
    }

    void BuildUI()
    {
        var rt = transform as RectTransform;
        if (rt == null) rt = gameObject.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

        // Panel
        _panel = new GameObject("RemovalPanel");
        _panel.transform.SetParent(transform, false);
        var panelRt = _panel.AddComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 0.5f);
        panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.sizeDelta = new Vector2(700, 500);
        panelRt.anchoredPosition = Vector2.zero;
        var panelImg = _panel.AddComponent<Image>();
        panelImg.color = new Color(0.08f, 0.08f, 0.14f, 0.96f);

        // Title
        var titleGo = new GameObject("TitleText");
        titleGo.transform.SetParent(_panel.transform, false);
        var titleRt = titleGo.AddComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0, 1); titleRt.anchorMax = new Vector2(1, 1);
        titleRt.pivot = new Vector2(0.5f, 1);
        titleRt.sizeDelta = new Vector2(-40, 60);
        titleRt.anchoredPosition = new Vector2(0, -20);
        var titleTmp = titleGo.AddComponent<TextMeshProUGUI>();
        if (_font != null) titleTmp.font = _font;
        titleTmp.fontSize = 30; titleTmp.color = new Color(1f, 0.9f, 0.5f);
        titleTmp.alignment = TextAlignmentOptions.Center;
        titleTmp.text = "选择要移除的卡牌";
        titleTmp.raycastTarget = false;

        // Scroll area for card list
        var scrollGo = new GameObject("ScrollView");
        scrollGo.transform.SetParent(_panel.transform, false);
        var scrollRt = scrollGo.AddComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0, 0); scrollRt.anchorMax = new Vector2(1, 1);
        scrollRt.pivot = new Vector2(0.5f, 0.5f);
        scrollRt.sizeDelta = new Vector2(-40, -140);
        scrollRt.anchoredPosition = new Vector2(0, 5);
        var scroll = scrollGo.AddComponent<ScrollRect>();
        scroll.horizontal = false; scroll.vertical = true;
        scroll.scrollSensitivity = 30f;

        // Viewport (required for Mask clipping)
        var viewportGo = new GameObject("Viewport");
        viewportGo.transform.SetParent(scrollGo.transform, false);
        var viewportRt = viewportGo.AddComponent<RectTransform>();
        viewportRt.anchorMin = Vector2.zero; viewportRt.anchorMax = Vector2.one;
        viewportRt.pivot = new Vector2(0.5f, 0.5f);
        viewportRt.sizeDelta = Vector2.zero;
        viewportRt.anchoredPosition = Vector2.zero;
        var viewportImg = viewportGo.AddComponent<Image>();
        viewportImg.color = new Color(0.08f, 0.08f, 0.14f, 0.96f);
        var viewportMask = viewportGo.AddComponent<Mask>();
        viewportMask.showMaskGraphic = false;
        scroll.viewport = viewportRt;

        // Content
        var contentGo = new GameObject("Content");
        contentGo.transform.SetParent(viewportGo.transform, false);
        var contentRt = contentGo.AddComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0, 1); contentRt.anchorMax = new Vector2(1, 1);
        contentRt.pivot = new Vector2(0.5f, 1);
        contentRt.sizeDelta = new Vector2(0, 0);
        var vlg = contentGo.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 8; vlg.padding = new RectOffset(10, 10, 10, 10);
        vlg.childControlWidth = true; vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;
        var csf = contentGo.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scroll.content = contentRt;
        _cardListContainer = contentGo.transform;

        // Cancel button
        var cancelGo = new GameObject("CancelButton");
        cancelGo.transform.SetParent(_panel.transform, false);
        var cancelRt = cancelGo.AddComponent<RectTransform>();
        cancelRt.anchorMin = new Vector2(0.5f, 0); cancelRt.anchorMax = new Vector2(0.5f, 0);
        cancelRt.pivot = new Vector2(0.5f, 0);
        cancelRt.sizeDelta = new Vector2(200, 45);
        cancelRt.anchoredPosition = new Vector2(0, 15);
        var cancelImg = cancelGo.AddComponent<Image>();
        cancelImg.color = new Color(0.5f, 0.2f, 0.2f, 1f);
        var cancelBtn = cancelGo.AddComponent<Button>();
        var cancelTextGo = new GameObject("Text");
        cancelTextGo.transform.SetParent(cancelGo.transform, false);
        var cancelTextRt = cancelTextGo.AddComponent<RectTransform>();
        cancelTextRt.anchorMin = Vector2.zero; cancelTextRt.anchorMax = Vector2.one;
        cancelTextRt.offsetMin = Vector2.zero; cancelTextRt.offsetMax = Vector2.zero;
        var cancelTmp = cancelTextGo.AddComponent<TextMeshProUGUI>();
        if (_font != null) cancelTmp.font = _font;
        cancelTmp.fontSize = 20; cancelTmp.color = Color.white;
        cancelTmp.alignment = TextAlignmentOptions.Center;
        cancelTmp.text = "取消";
        cancelTmp.raycastTarget = false;
        cancelBtn.onClick.AddListener(() => { gameObject.SetActive(false); _onComplete?.Invoke(); });

        gameObject.SetActive(false);
    }

    public void Show(System.Action onComplete)
    {
        _onComplete = onComplete;
        gameObject.SetActive(true);

        // Clear old entries
        foreach (Transform child in _cardListContainer)
            Destroy(child.gameObject);

        var gm = GameManager.Instance;
        if (gm == null || !gm.HasPersistentDeck)
        {
            Debug.LogWarning("[CardRemoval] 牌组为空，无法移除");
            gameObject.SetActive(false);
            _onComplete?.Invoke();
            return;
        }

        var deck = gm.GetPlayerDeck();
        // Deduplicate by CardData reference, count duplicates
        var seen = new Dictionary<CardData, int>();
        foreach (var card in deck)
        {
            if (card == null) continue;
            if (seen.ContainsKey(card)) seen[card]++;
            else seen[card] = 1;
        }

        foreach (var kvp in seen)
        {
            var card = kvp.Key;
            var count = kvp.Value;

            var entryGo = new GameObject("Card_" + card.cardName);
            entryGo.transform.SetParent(_cardListContainer, false);
            var entryRt = entryGo.AddComponent<RectTransform>();
            entryRt.sizeDelta = new Vector2(0, 60);
            var entryImg = entryGo.AddComponent<Image>();
            entryImg.color = card.element switch
            {
                Element.Fire  => new Color(1.0f, 0.50f, 0.0f, 1f),  // 橙色
                Element.Water => new Color(0.0f, 0.45f, 0.85f, 1f), // 蓝色
                Element.Wind  => new Color(0.13f, 0.60f, 0.20f, 1f),// 绿色
                _ => new Color(0.3f, 0.3f, 0.3f, 1f)
            };
            var entryBtn = entryGo.AddComponent<Button>();

            var nameGo = new GameObject("NameText");
            nameGo.transform.SetParent(entryGo.transform, false);
            var nameRt = nameGo.AddComponent<RectTransform>();
            nameRt.anchorMin = new Vector2(0, 0); nameRt.anchorMax = new Vector2(0.7f, 1);
            nameRt.offsetMin = new Vector2(15, 0); nameRt.offsetMax = new Vector2(0, 0);
            var nameTmp = nameGo.AddComponent<TextMeshProUGUI>();
            if (_font != null) nameTmp.font = _font;
            nameTmp.fontSize = 20; nameTmp.color = Color.white;
            nameTmp.alignment = TextAlignmentOptions.Left;
            nameTmp.text = $"[{CardData.GetElementName(card.element)}] {card.cardName}";
            if (count > 1) nameTmp.text += $" x{count}";
            nameTmp.raycastTarget = false;

            var descGo = new GameObject("DescText");
            descGo.transform.SetParent(entryGo.transform, false);
            var descRt = descGo.AddComponent<RectTransform>();
            descRt.anchorMin = new Vector2(0.7f, 0); descRt.anchorMax = new Vector2(1, 1);
            descRt.offsetMin = new Vector2(5, 0); descRt.offsetMax = new Vector2(-10, 0);
            var descTmp = descGo.AddComponent<TextMeshProUGUI>();
            if (_font != null) descTmp.font = _font;
            descTmp.fontSize = 16; descTmp.color = new Color(0.8f, 0.8f, 0.8f);
            descTmp.alignment = TextAlignmentOptions.Right;
            descTmp.text = card.GetDescription();
            descTmp.raycastTarget = false;

            var captured = card;
            entryBtn.onClick.AddListener(() => OnCardSelected(captured));
        }
    }

    private void OnCardSelected(CardData card)
    {
        var gm = GameManager.Instance;
        if (gm != null)
        {
            gm.RemoveCardFromDeck(card);
            Debug.Log($"[事件] 移除卡牌：{card.cardName}");
        }
        gameObject.SetActive(false);
        _onComplete?.Invoke();
    }
}
