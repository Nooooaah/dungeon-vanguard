using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 事件弹窗UI —— 纯代码构建，不依赖预制体
/// </summary>
public class EventPopupUI : MonoBehaviour
{
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public Transform choicesContainer;
    public GameObject choiceButtonPrefab;

    private System.Action<EventChoice> _onChoiceCallback;
    private TMP_FontAsset _font;
    private GameObject _overlay;
    private GameObject _card;

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
        // Full-screen overlay
        var rt = transform as RectTransform;
        if (rt == null) { rt = gameObject.AddComponent<RectTransform>(); }
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

        // Card (center panel)
        _card = new GameObject("EventCard");
        _card.transform.SetParent(transform, false);
        var cardRt = _card.AddComponent<RectTransform>();
        cardRt.anchorMin = new Vector2(0.5f, 0.5f);
        cardRt.anchorMax = new Vector2(0.5f, 0.5f);
        cardRt.sizeDelta = new Vector2(600, 450);
        cardRt.anchoredPosition = Vector2.zero;
        var cardImg = _card.AddComponent<Image>();
        cardImg.color = new Color(0.08f, 0.08f, 0.14f, 0.95f);

        // Title
        var titleGo = new GameObject("TitleText");
        titleGo.transform.SetParent(_card.transform, false);
        var titleRt = titleGo.AddComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0, 1); titleRt.anchorMax = new Vector2(1, 1);
        titleRt.pivot = new Vector2(0.5f, 1);
        titleRt.sizeDelta = new Vector2(-40, 60);
        titleRt.anchoredPosition = new Vector2(0, -20);
        titleText = titleGo.AddComponent<TextMeshProUGUI>();
        if (_font != null) titleText.font = _font;
        titleText.fontSize = 32; titleText.color = new Color(1f, 0.9f, 0.5f);
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.raycastTarget = false;

        // Description
        var descGo = new GameObject("DescriptionText");
        descGo.transform.SetParent(_card.transform, false);
        var descRt = descGo.AddComponent<RectTransform>();
        descRt.anchorMin = new Vector2(0, 1); descRt.anchorMax = new Vector2(1, 1);
        descRt.pivot = new Vector2(0.5f, 1);
        descRt.sizeDelta = new Vector2(-40, 120);
        descRt.anchoredPosition = new Vector2(0, -90);
        descriptionText = descGo.AddComponent<TextMeshProUGUI>();
        if (_font != null) descriptionText.font = _font;
        descriptionText.fontSize = 20; descriptionText.color = new Color(0.88f, 0.88f, 0.88f);
        descriptionText.alignment = TextAlignmentOptions.Center;
        descriptionText.overflowMode = TextOverflowModes.Overflow;
        descriptionText.raycastTarget = false;

        // Choices container
        var choicesGo = new GameObject("ChoicesContainer");
        choicesGo.transform.SetParent(_card.transform, false);
        var choicesRt = choicesGo.AddComponent<RectTransform>();
        choicesRt.anchorMin = new Vector2(0, 0); choicesRt.anchorMax = new Vector2(1, 0);
        choicesRt.pivot = new Vector2(0.5f, 0);
        choicesRt.sizeDelta = new Vector2(-40, 200);
        choicesRt.anchoredPosition = new Vector2(0, 20);
        choicesContainer = choicesGo.transform;

<<<<<<< HEAD
        gameObject.SetActive(false);
=======
>>>>>>> dd15dc953bda76878a619e06d67d3f5c5cb965a7
    }

    TextMeshProUGUI CreateTMP(string name, Transform parent, float y, float height, int fontSize, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.sizeDelta = new Vector2(-40, height);
        rt.anchoredPosition = new Vector2(0, y);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        if (_font != null) tmp.font = _font;
        tmp.fontSize = fontSize; tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        return tmp;
    }

    public void Show(EventData eventData, System.Action<EventChoice> onChoiceCallback)
    {
        _onChoiceCallback = onChoiceCallback;
        gameObject.SetActive(true);

        if (titleText != null) titleText.text = eventData.title;
        if (descriptionText != null) descriptionText.text = eventData.description;

        // Clear old choices
        if (choicesContainer != null)
        {
            foreach (Transform child in choicesContainer)
                Destroy(child.gameObject);

            float yPos = 0f;
            foreach (var choice in eventData.choices)
            {
                var btnGo = new GameObject("Choice_" + choice.text.Substring(0, Mathf.Min(4, choice.text.Length)));
                btnGo.transform.SetParent(choicesContainer, false);
                var btnRt = btnGo.AddComponent<RectTransform>();
                btnRt.anchorMin = new Vector2(0, 1); btnRt.anchorMax = new Vector2(1, 1);
                btnRt.pivot = new Vector2(0.5f, 1);
                btnRt.sizeDelta = new Vector2(0, 50);
                btnRt.anchoredPosition = new Vector2(0, -yPos);
                var img = btnGo.AddComponent<Image>();
                img.color = new Color(0.2f, 0.3f, 0.5f, 1f);
                var btn = btnGo.AddComponent<Button>();

                var textGo = new GameObject("Text");
                textGo.transform.SetParent(btnGo.transform, false);
                var textRt = textGo.AddComponent<RectTransform>();
                textRt.anchorMin = Vector2.zero; textRt.anchorMax = Vector2.one;
                textRt.offsetMin = new Vector2(10, 0); textRt.offsetMax = new Vector2(-10, 0);
                var tmp = textGo.AddComponent<TextMeshProUGUI>();
                if (_font != null) tmp.font = _font;
                tmp.fontSize = 20; tmp.color = Color.white;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.text = choice.text;
                tmp.raycastTarget = false;

                var captured = choice;
                btn.onClick.AddListener(() => OnChoiceSelected(captured));

                yPos += 60f;
            }
        }
    }

    private void OnChoiceSelected(EventChoice choice)
    {
        _onChoiceCallback?.Invoke(choice);
        gameObject.SetActive(false);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
