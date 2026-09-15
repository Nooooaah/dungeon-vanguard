using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 牌库查看弹窗 — 点击牌库区域弹出，显示牌库和弃牌堆的所有卡牌。
/// </summary>
public class DeckViewPopup : MonoBehaviour
{
    private GameObject _overlay;
    private GameObject _panel;
    private ScrollRect _scrollRect;
    private TMP_Text _titleLabel;
<<<<<<< HEAD
    private bool _showingDeck = true;
    private List<CardData> _deckRef;
    private List<CardData> _discardRef;
=======
    private int _viewMode = 0; // 0=deck, 1=discard, 2=all
    private List<CardData> _deckRef;
    private List<CardData> _discardRef;
    private List<CardData> _allCardsRef;
>>>>>>> dd15dc953bda76878a619e06d67d3f5c5cb965a7
    private TMP_FontAsset _font;

    public void Initialize(List<CardData> deck, List<CardData> discard, TMP_FontAsset font)
    {
        _deckRef = deck;
        _discardRef = discard;
        _font = font;
        BuildUI();
        ShowDeck();
    }

<<<<<<< HEAD
=======
    public void Initialize(List<CardData> deck, List<CardData> discard, List<CardData> allCards, TMP_FontAsset font)
    {
        _deckRef = deck;
        _discardRef = discard;
        _allCardsRef = allCards;
        _font = font;
        BuildUI();
        ShowDeck();
    }

>>>>>>> dd15dc953bda76878a619e06d67d3f5c5cb965a7
    void BuildUI()
    {
        // Overlay (semi-transparent background)
        _overlay = new GameObject("DeckViewOverlay");
        _overlay.transform.SetParent(transform, false);
        var overlayRt = _overlay.AddComponent<RectTransform>();
        overlayRt.anchorMin = Vector2.zero; overlayRt.anchorMax = Vector2.one;
        overlayRt.offsetMin = Vector2.zero; overlayRt.offsetMax = Vector2.zero;
        var overlayImg = _overlay.AddComponent<Image>();
        overlayImg.color = new Color(0, 0, 0, 0.7f);
        overlayImg.raycastTarget = true;

        // Panel
        _panel = new GameObject("DeckViewPanel");
        _panel.transform.SetParent(_overlay.transform, false);
        var panelRt = _panel.AddComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 0.5f);
        panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.pivot = new Vector2(0.5f, 0.5f);
        panelRt.sizeDelta = new Vector2(600f, 700f);
        var panelImg = _panel.AddComponent<Image>();
        panelImg.color = new Color(0.06f, 0.05f, 0.10f, 0.98f);

        // Title
        var titleGo = new GameObject("Title");
        titleGo.transform.SetParent(_panel.transform, false);
        var titleRt = titleGo.AddComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.5f, 1f);
        titleRt.anchorMax = new Vector2(0.5f, 1f);
        titleRt.pivot = new Vector2(0.5f, 1f);
        titleRt.anchoredPosition = new Vector2(0, -20f);
        titleRt.sizeDelta = new Vector2(500f, 50f);
        _titleLabel = titleGo.AddComponent<TextMeshProUGUI>();
        if (_font != null) _titleLabel.font = _font;
        _titleLabel.fontSize = 32;
        _titleLabel.color = new Color(1f, 0.85f, 0.35f, 1f);
        _titleLabel.alignment = TextAlignmentOptions.Center;
        _titleLabel.raycastTarget = false;

<<<<<<< HEAD
        // Tab buttons container
=======
        // Tab buttons container (3 tabs)
>>>>>>> dd15dc953bda76878a619e06d67d3f5c5cb965a7
        var tabGo = new GameObject("Tabs");
        tabGo.transform.SetParent(_panel.transform, false);
        var tabRt = tabGo.AddComponent<RectTransform>();
        tabRt.anchorMin = new Vector2(0.5f, 1f);
        tabRt.anchorMax = new Vector2(0.5f, 1f);
        tabRt.pivot = new Vector2(0.5f, 1f);
        tabRt.anchoredPosition = new Vector2(0, -80f);
<<<<<<< HEAD
        tabRt.sizeDelta = new Vector2(500f, 50f);

        // Deck tab button
        var deckTabGo = new GameObject("DeckTab");
        deckTabGo.transform.SetParent(tabGo.transform, false);
        var deckTabRt = deckTabGo.AddComponent<RectTransform>();
        deckTabRt.anchorMin = new Vector2(0f, 0f);
        deckTabRt.anchorMax = new Vector2(0.5f, 1f);
        deckTabRt.offsetMin = new Vector2(10f, 0f);
        deckTabRt.offsetMax = new Vector2(-5f, 0f);
        var deckTabImg = deckTabGo.AddComponent<Image>();
        deckTabImg.color = new Color(0.2f, 0.35f, 0.55f, 1f);
        var deckTabBtn = deckTabGo.AddComponent<Button>();
        var deckTabText = CreateLabel(deckTabGo, "牌库");
        deckTabText.fontSize = 24;
        deckTabBtn.onClick.AddListener(() => { _showingDeck = true; RefreshContent(); });

        // Discard tab button
        var discardTabGo = new GameObject("DiscardTab");
        discardTabGo.transform.SetParent(tabGo.transform, false);
        var discardTabRt = discardTabGo.AddComponent<RectTransform>();
        discardTabRt.anchorMin = new Vector2(0.5f, 0f);
        discardTabRt.anchorMax = new Vector2(1f, 1f);
        discardTabRt.offsetMin = new Vector2(5f, 0f);
        discardTabRt.offsetMax = new Vector2(-10f, 0f);
        var discardTabImg = discardTabGo.AddComponent<Image>();
        discardTabImg.color = new Color(0.3f, 0.2f, 0.15f, 1f);
        var discardTabBtn = discardTabGo.AddComponent<Button>();
        var discardTabText = CreateLabel(discardTabGo, "弃牌");
        discardTabText.fontSize = 24;
        discardTabBtn.onClick.AddListener(() => { _showingDeck = false; RefreshContent(); });
=======
        tabRt.sizeDelta = new Vector2(560f, 50f);

        var tabHlg = tabGo.AddComponent<HorizontalLayoutGroup>();
        tabHlg.spacing = 6f;
        tabHlg.padding = new RectOffset(4, 4, 0, 0);
        tabHlg.childForceExpandWidth = true;
        tabHlg.childForceExpandHeight = true;

        // Deck tab
        var deckTabGo = CreateTabButton(tabGo.transform, "牌库", new Color(0.2f, 0.35f, 0.55f, 1f));
        deckTabGo.GetComponent<Button>().onClick.AddListener(() => { _viewMode = 0; RefreshContent(); });

        // Discard tab
        var discardTabGo = CreateTabButton(tabGo.transform, "弃牌", new Color(0.3f, 0.2f, 0.15f, 1f));
        discardTabGo.GetComponent<Button>().onClick.AddListener(() => { _viewMode = 1; RefreshContent(); });

        // All cards tab (my deck)
        var allTabGo = CreateTabButton(tabGo.transform, "我的牌组", new Color(0.15f, 0.35f, 0.2f, 1f));
        allTabGo.GetComponent<Button>().onClick.AddListener(() => { _viewMode = 2; RefreshContent(); });
>>>>>>> dd15dc953bda76878a619e06d67d3f5c5cb965a7

        // Scroll view
        var scrollGo = new GameObject("ScrollView");
        scrollGo.transform.SetParent(_panel.transform, false);
        var scrollRt = scrollGo.AddComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0f, 0f);
        scrollRt.anchorMax = new Vector2(1f, 1f);
        scrollRt.offsetMin = new Vector2(20f, 70f);
        scrollRt.offsetMax = new Vector2(-20f, -140f);
        var scrollImg = scrollGo.AddComponent<Image>();
        scrollImg.color = new Color(0.02f, 0.02f, 0.05f, 0.5f);
<<<<<<< HEAD
        _scrollRect = scrollGo.AddComponent<ScrollRect>();
        _scrollRect.horizontal = false;
        _scrollRect.vertical = true;

        // Content
        var contentGo = new GameObject("Content");
        contentGo.transform.SetParent(scrollGo.transform, false);
=======
        scrollImg.raycastTarget = true;
        _scrollRect = scrollGo.AddComponent<ScrollRect>();
        _scrollRect.horizontal = false;
        _scrollRect.vertical = true;
        _scrollRect.scrollSensitivity = 20f;

        // Viewport (required for scrolling to work)
        var viewportGo = new GameObject("Viewport");
        viewportGo.transform.SetParent(scrollGo.transform, false);
        var viewportRt = viewportGo.AddComponent<RectTransform>();
        viewportRt.anchorMin = Vector2.zero;
        viewportRt.anchorMax = Vector2.one;
        viewportRt.pivot = new Vector2(0.5f, 0.5f);
        viewportRt.offsetMin = Vector2.zero;
        viewportRt.offsetMax = Vector2.zero;
        var viewportImg = viewportGo.AddComponent<Image>();
        viewportImg.color = new Color(1, 1, 1, 0);
        viewportImg.raycastTarget = true;
        var viewportMask = viewportGo.AddComponent<RectMask2D>();
        _scrollRect.viewport = viewportRt;

        // Content
        var contentGo = new GameObject("Content");
        contentGo.transform.SetParent(viewportGo.transform, false);
>>>>>>> dd15dc953bda76878a619e06d67d3f5c5cb965a7
        var contentRt = contentGo.AddComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0f, 1f);
        contentRt.anchorMax = new Vector2(1f, 1f);
        contentRt.pivot = new Vector2(0.5f, 1f);
<<<<<<< HEAD
=======
        contentRt.anchoredPosition = Vector2.zero;
>>>>>>> dd15dc953bda76878a619e06d67d3f5c5cb965a7
        contentRt.sizeDelta = new Vector2(0f, 0f);
        var vlg = contentGo.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 8f;
        vlg.padding = new RectOffset(10, 10, 10, 10);
        vlg.childAlignment = TextAnchor.UpperCenter;
<<<<<<< HEAD
        var fitter = contentGo.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
=======
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        var fitter = contentGo.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
>>>>>>> dd15dc953bda76878a619e06d67d3f5c5cb965a7
        _scrollRect.content = contentRt;

        // Close button
        var closeGo = new GameObject("CloseBtn");
        closeGo.transform.SetParent(_panel.transform, false);
        var closeRt = closeGo.AddComponent<RectTransform>();
        closeRt.anchorMin = new Vector2(0.5f, 0f);
        closeRt.anchorMax = new Vector2(0.5f, 0f);
        closeRt.pivot = new Vector2(0.5f, 0f);
        closeRt.anchoredPosition = new Vector2(0, 15f);
        closeRt.sizeDelta = new Vector2(200f, 45f);
        var closeImg = closeGo.AddComponent<Image>();
        closeImg.color = new Color(0.3f, 0.2f, 0.06f, 0.85f);
        var closeBtn = closeGo.AddComponent<Button>();
        var closeLabel = CreateLabel(closeGo, "关闭");
        closeLabel.fontSize = 22;
        closeLabel.color = new Color(0.85f, 0.85f, 0.85f, 1f);
        closeBtn.onClick.AddListener(Close);
    }

<<<<<<< HEAD
=======
    GameObject CreateTabButton(Transform parent, string label, Color color)
    {
        var go = new GameObject("Tab_" + label);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        var btn = go.AddComponent<Button>();
        var tmp = CreateLabel(go, label);
        tmp.fontSize = 22;
        return go;
    }

>>>>>>> dd15dc953bda76878a619e06d67d3f5c5cb965a7
    TextMeshProUGUI CreateLabel(GameObject parent, string text)
    {
        var go = new GameObject("Label");
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        if (_font != null) tmp.font = _font;
        tmp.text = text;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        tmp.color = Color.white;
        return tmp;
    }

    void ShowDeck()
    {
<<<<<<< HEAD
        _showingDeck = true;
=======
        _viewMode = 0;
>>>>>>> dd15dc953bda76878a619e06d67d3f5c5cb965a7
        RefreshContent();
    }

    void RefreshContent()
    {
<<<<<<< HEAD
        var list = _showingDeck ? _deckRef : _discardRef;
        _titleLabel.text = (_showingDeck ? "牌库" : "弃牌堆") + " (" + (list?.Count ?? 0) + ")";
=======
        // Determine list and title based on view mode
        List<CardData> list;
        string titleName;
        bool groupByType = false;

        if (_viewMode == 0)
        {
            list = new List<CardData>(_deckRef);
            // 随机打乱，防止预测下一张
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                var tmp = list[i]; list[i] = list[j]; list[j] = tmp;
            }
            titleName = "牌库";
        }
        else if (_viewMode == 1)
        {
            list = _discardRef;
            titleName = "弃牌堆";
        }
        else
        {
            // All cards: deck + discard + hand
            list = new List<CardData>();
            if (_allCardsRef != null) list.AddRange(_allCardsRef);
            titleName = "我的牌组";
            groupByType = true;
        }

        _titleLabel.text = titleName + " (" + (list?.Count ?? 0) + ")";
>>>>>>> dd15dc953bda76878a619e06d67d3f5c5cb965a7

        // Clear old entries
        for (int i = _scrollRect.content.childCount - 1; i >= 0; i--)
            Destroy(_scrollRect.content.GetChild(i).gameObject);

        if (list == null || list.Count == 0)
        {
<<<<<<< HEAD
            var emptyGo = new GameObject("Empty");
            emptyGo.transform.SetParent(_scrollRect.content, false);
            var emptyRt = emptyGo.AddComponent<RectTransform>();
            emptyRt.sizeDelta = new Vector2(0f, 60f);
            var emptyText = emptyGo.AddComponent<TextMeshProUGUI>();
            if (_font != null) emptyText.font = _font;
            emptyText.text = "空";
            emptyText.fontSize = 24;
            emptyText.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            emptyText.alignment = TextAlignmentOptions.Center;
            emptyText.raycastTarget = false;
            return;
        }

        foreach (var card in list)
        {
            if (card == null) continue;

            // Entry row
            var entryGo = new GameObject("Entry_" + card.cardName);
            entryGo.transform.SetParent(_scrollRect.content, false);
            var entryImg = entryGo.AddComponent<Image>();
            entryImg.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);
            var entryHlg = entryGo.AddComponent<HorizontalLayoutGroup>();
            entryHlg.padding = new RectOffset(8, 8, 4, 4);
            entryHlg.spacing = 8f;
            entryHlg.childForceExpandWidth = false;
            entryHlg.childForceExpandHeight = true;
            entryHlg.childControlWidth = true;
            entryHlg.childControlHeight = true;
            var entryFitter = entryGo.AddComponent<ContentSizeFitter>();
            entryFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Cost
            var costGo = new GameObject("Cost");
            costGo.transform.SetParent(entryGo.transform, false);
            var costTmp = costGo.AddComponent<TextMeshProUGUI>();
            if (_font != null) costTmp.font = _font;
            costTmp.text = card.cost.ToString();
            costTmp.fontSize = 26;
            costTmp.color = new Color(0.4f, 0.7f, 1f, 1f);
            costTmp.alignment = TextAlignmentOptions.Center;
            costTmp.raycastTarget = false;
            var costFitter = costGo.AddComponent<ContentSizeFitter>();
            costFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Name + description
            var nameGo = new GameObject("Name");
            nameGo.transform.SetParent(entryGo.transform, false);
            var nameTmp = nameGo.AddComponent<TextMeshProUGUI>();
            if (_font != null) nameTmp.font = _font;
            nameTmp.text = card.cardName + "  —  " + card.GetDescription();
            nameTmp.fontSize = 20;
            nameTmp.color = new Color(1f, 0.92f, 0.7f, 1f);
            nameTmp.alignment = TextAlignmentOptions.Left;
            nameTmp.raycastTarget = false;
            nameTmp.enableWordWrapping = true;
            nameTmp.overflowMode = TextOverflowModes.Overflow;
=======
            CreateEmptyEntry();
            return;
        }

        if (groupByType)
        {
            // Group by card name, show count
            var groups = new Dictionary<string, (CardData card, int count)>();
            foreach (var card in list)
            {
                if (card == null) continue;
                if (groups.ContainsKey(card.cardName))
                {
                    var g = groups[card.cardName];
                    g.count++;
                    groups[card.cardName] = g;
                }
                else
                {
                    groups[card.cardName] = (card, 1);
                }
            }

            foreach (var kvp in groups)
            {
                CreateCardEntry(kvp.Value.card, kvp.Value.count);
            }
        }
        else
        {
            foreach (var card in list)
            {
                if (card == null) continue;
                CreateCardEntry(card, -1);
            }
        }
    }

    void CreateEmptyEntry()
    {
        var emptyGo = new GameObject("Empty");
        emptyGo.transform.SetParent(_scrollRect.content, false);
        var emptyRt = emptyGo.AddComponent<RectTransform>();
        emptyRt.sizeDelta = new Vector2(0f, 60f);
        var emptyText = emptyGo.AddComponent<TextMeshProUGUI>();
        if (_font != null) emptyText.font = _font;
        emptyText.text = "空";
        emptyText.fontSize = 24;
        emptyText.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        emptyText.alignment = TextAlignmentOptions.Center;
        emptyText.raycastTarget = false;
    }

    void CreateCardEntry(CardData card, int count)
    {
        // Entry row
        var entryGo = new GameObject("Entry_" + card.cardName);
        entryGo.transform.SetParent(_scrollRect.content, false);
        var entryImg = entryGo.AddComponent<Image>();
        entryImg.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);
        var entryHlg = entryGo.AddComponent<HorizontalLayoutGroup>();
        entryHlg.padding = new RectOffset(8, 8, 4, 4);
        entryHlg.spacing = 8f;
        entryHlg.childForceExpandWidth = false;
        entryHlg.childForceExpandHeight = true;
        entryHlg.childControlWidth = true;
        entryHlg.childControlHeight = true;
        var entryFitter = entryGo.AddComponent<ContentSizeFitter>();
        entryFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Cost
        var costGo = new GameObject("Cost");
        costGo.transform.SetParent(entryGo.transform, false);
        var costTmp = costGo.AddComponent<TextMeshProUGUI>();
        if (_font != null) costTmp.font = _font;
        costTmp.text = card.cost.ToString();
        costTmp.fontSize = 26;
        costTmp.color = new Color(0.4f, 0.7f, 1f, 1f);
        costTmp.alignment = TextAlignmentOptions.Center;
        costTmp.raycastTarget = false;
        var costFitter = costGo.AddComponent<ContentSizeFitter>();
        costFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Name + description
        var nameGo = new GameObject("Name");
        nameGo.transform.SetParent(entryGo.transform, false);
        var nameTmp = nameGo.AddComponent<TextMeshProUGUI>();
        if (_font != null) nameTmp.font = _font;
        nameTmp.text = card.cardName + "  —  " + card.GetDescription();
        nameTmp.fontSize = 20;
        nameTmp.color = new Color(1f, 0.92f, 0.7f, 1f);
        nameTmp.alignment = TextAlignmentOptions.Left;
        nameTmp.raycastTarget = false;
        nameTmp.enableWordWrapping = true;
        nameTmp.overflowMode = TextOverflowModes.Overflow;

        // Count (only for "all cards" view)
        if (count > 0)
        {
            var countGo = new GameObject("Count");
            countGo.transform.SetParent(entryGo.transform, false);
            var countTmp = countGo.AddComponent<TextMeshProUGUI>();
            if (_font != null) countTmp.font = _font;
            countTmp.text = "x" + count;
            countTmp.fontSize = 24;
            countTmp.color = new Color(0.3f, 1f, 0.4f, 1f);
            countTmp.alignment = TextAlignmentOptions.Center;
            countTmp.raycastTarget = false;
            var countFitter = countGo.AddComponent<ContentSizeFitter>();
            countFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
>>>>>>> dd15dc953bda76878a619e06d67d3f5c5cb965a7
        }
    }

    void Close()
    {
        Destroy(_overlay);
        Destroy(gameObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && _overlay != null)
            Close();
    }
}
