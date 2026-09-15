using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Reward popup: 3-choice card reward, skip option.
/// Also handles event popups with choices.
/// After selection, triggers GameManager to return to MapScene.
/// </summary>
public class RewardPopupUI : MonoBehaviour
{
    [Header("Title")]
    public TMP_Text titleText;

    [Header("Card Choices")]
    public Transform cardContainer;
    public GameObject cardRewardPrefab;

    [Header("Buttons")]
    public Button skipButton;

    [Header("Event Mode")]
    public GameObject eventPanel;
    public TMP_Text eventDescriptionText;
    public Transform eventChoiceContainer;
    public GameObject eventChoiceButtonPrefab;

    [Header("Gold Display")]
    public TMP_Text goldText;

    private List<CardData> _rewardCards = new List<CardData>();

    private bool _rewardProcessed = false;

    void Start()
    {
        if (skipButton != null)
            skipButton.onClick.AddListener(OnSkipClicked);
    }

    private void OnSkipClicked()
    {
        if (_rewardProcessed) return;
        _rewardProcessed = true;
        gameObject.SetActive(false);
        ReturnToMap();
    }

    /// <summary>Public method to trigger skip (called by RewardSceneInit).</summary>
    public void Skip()
    {
        OnSkipClicked();
    }

    /// <summary>
    /// Shows a 3-choice card reward.
    /// </summary>
    public void ShowCardReward(List<CardData> cards, int gold)
    {
        _rewardCards = cards;
        _rewardProcessed = false;

        if (titleText != null)
            titleText.text = "选择一张卡牌奖励";

        if (goldText != null)
            goldText.text = "金币 +" + gold;

        // Clear old cards
        if (cardContainer != null)
        {
            for (int i = cardContainer.childCount - 1; i >= 0; i--)
                Destroy(cardContainer.GetChild(i).gameObject);
        }

        // Spawn card choices
        for (int i = 0; i < cards.Count; i++)
        {
            var card = cards[i];
            GameObject go;

            if (cardRewardPrefab != null)
            {
                go = Instantiate(cardRewardPrefab, cardContainer);
            }
            else
            {
                // No prefab — build a simple card from code
                go = CreateSimpleCard(card, i, cards.Count);
            }

            var display = go.GetComponent<CardDisplay>();
            if (display != null)
                display.Setup(card);

            var button = go.GetComponent<Button>();
            if (button == null)
                button = go.AddComponent<Button>();
            var captured = card;
            var capturedRT = go.GetComponent<RectTransform>();
            button.onClick.AddListener(() =>
            {
                if (_rewardProcessed) return;
                _rewardProcessed = true;
                StartCoroutine(PlaySelectEffect(capturedRT, () =>
                {
                    OnCardSelected?.Invoke(captured);
                    gameObject.SetActive(false);
                    ReturnToMap();
                }));
            });

            // Add hover effect
            if (go.GetComponent<CardHoverEffect>() == null)
                go.AddComponent<CardHoverEffect>();
        }

        gameObject.SetActive(true);
    }

    GameObject CreateSimpleCard(CardData data, int index, int total)
    {
        var go = new GameObject("RewardCard_" + data.cardName);
        go.transform.SetParent(cardContainer, false);
        var rt = go.AddComponent<RectTransform>();
        float spacing = 320f;
        float startX = -(total - 1) * spacing * 0.5f;
        rt.anchoredPosition = new Vector2(startX + index * spacing, 0);
        rt.sizeDelta = new Vector2(240, 340);

        var fontAsset = TMP_Settings.defaultFontAsset;
        var msyhl = Resources.Load<TMP_FontAsset>("msyhl SDF");
#if UNITY_EDITOR
        if (msyhl == null)
            msyhl = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/msyhl SDF.asset");
#endif
        if (msyhl != null) fontAsset = msyhl;

        // CardBorder
        var borderGo = new GameObject("CardBorder");
        borderGo.transform.SetParent(go.transform, false);
        var borderRt = borderGo.AddComponent<RectTransform>();
        borderRt.anchorMin = Vector2.zero; borderRt.anchorMax = Vector2.one;
        borderRt.offsetMin = new Vector2(-3, -3); borderRt.offsetMax = new Vector2(3, 3);
        var borderImg = borderGo.AddComponent<Image>();
        borderImg.color = new Color(0.75f, 0.58f, 0.15f, 1f);
        borderImg.raycastTarget = false;

        // BG
        var bgGo = new GameObject("BG");
        bgGo.transform.SetParent(go.transform, false);
        var bgRt = bgGo.AddComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;
        var bgImg = bgGo.AddComponent<Image>();
        string bgName = data.element == Element.Fire ? "Fire_BG" : data.element == Element.Water ? "Water_BG" : "Wind_BG";
        bgImg.sprite = LoadRewardSprite(bgName);
        bgImg.color = Color.white;

        // Frame
        var frameGo = new GameObject("Frame");
        frameGo.transform.SetParent(go.transform, false);
        var frameRt = frameGo.AddComponent<RectTransform>();
        frameRt.anchorMin = Vector2.zero; frameRt.anchorMax = Vector2.one;
        frameRt.offsetMin = new Vector2(2, 2); frameRt.offsetMax = new Vector2(-2, -2);
        var frameImg = frameGo.AddComponent<Image>();
        frameImg.sprite = LoadRewardSprite("CardFrame");
        frameImg.color = Color.white;
        frameImg.raycastTarget = false;

        // Art
        string artName = data.element == Element.Fire ? "Fire_Art" : data.element == Element.Water ? "Water_Art" : "Wind_Art";
        var artGo = new GameObject("Art");
        artGo.transform.SetParent(go.transform, false);
        var artRt = artGo.AddComponent<RectTransform>();
        artRt.anchoredPosition = new Vector2(0, 30);
        artRt.sizeDelta = new Vector2(200, 110);
        var artImg = artGo.AddComponent<Image>();
        artImg.sprite = LoadRewardSprite(artName);
        artImg.color = new Color(1, 1, 1, 0.5f);
        artImg.raycastTarget = false;

        // ManaCrystal
        var crystalGo = new GameObject("ManaCrystal");
        crystalGo.transform.SetParent(go.transform, false);
        var crystalRt = crystalGo.AddComponent<RectTransform>();
        crystalRt.anchoredPosition = new Vector2(-88, 140);
        crystalRt.sizeDelta = new Vector2(50, 50);
        var crystalImg = crystalGo.AddComponent<Image>();
        crystalImg.sprite = LoadRewardSprite("ManaCrystal");
        crystalImg.color = Color.white;
        crystalImg.raycastTarget = false;

        var costGo = new GameObject("CostText");
        costGo.transform.SetParent(crystalGo.transform, false);
        var costRt = costGo.AddComponent<RectTransform>();
        costRt.anchorMin = Vector2.zero; costRt.anchorMax = Vector2.one;
        costRt.offsetMin = Vector2.zero; costRt.offsetMax = Vector2.zero;
        var costText = costGo.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) costText.font = fontAsset;
        costText.alignment = TextAlignmentOptions.Center; costText.text = data.cost.ToString();
        costText.fontSize = 26; costText.color = Color.white;
        costText.raycastTarget = false;

        // ElementBadge — 已移除

        // NameText
        var nameGo = new GameObject("NameText");
        nameGo.transform.SetParent(go.transform, false);
        var nameRt = nameGo.AddComponent<RectTransform>();
        nameRt.anchoredPosition = new Vector2(0, 115);
        nameRt.sizeDelta = new Vector2(180, 36);
        var nameText = nameGo.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) nameText.font = fontAsset;
        nameText.fontSize = 24; nameText.color = new Color(1f, 0.92f, 0.7f);
        nameText.alignment = TextAlignmentOptions.Center; nameText.text = data.cardName;
        nameText.raycastTarget = false;

        // DescPanel
        var descPanelGo = new GameObject("DescPanel");
        descPanelGo.transform.SetParent(go.transform, false);
        var dpRt = descPanelGo.AddComponent<RectTransform>();
        dpRt.anchoredPosition = new Vector2(0, -80);
        dpRt.sizeDelta = new Vector2(220, 100);
        var dpImg = descPanelGo.AddComponent<Image>();
        dpImg.sprite = LoadRewardSprite("DescPanel");
        dpImg.color = new Color(0.05f, 0.05f, 0.08f, 0.9f);
        dpImg.raycastTarget = false;

        // DescText
        var descGo = new GameObject("DescText");
        descGo.transform.SetParent(go.transform, false);
        var descRt = descGo.AddComponent<RectTransform>();
        descRt.anchoredPosition = new Vector2(0, -80);
        descRt.sizeDelta = new Vector2(210, 90);
        var descText = descGo.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) descText.font = fontAsset;
        descText.fontSize = 18; descText.color = new Color(0.95f, 0.95f, 0.95f, 1f);
        descText.alignment = TextAlignmentOptions.Center; descText.text = data.GetDescription();
        descText.overflowMode = TextOverflowModes.Ellipsis;
        descText.raycastTarget = false;

        // AttackIcon (右上角)
        var atkIconGo = new GameObject("AttackIcon");
        atkIconGo.transform.SetParent(go.transform, false);
        var atkIconRt = atkIconGo.AddComponent<RectTransform>();
        atkIconRt.anchoredPosition = new Vector2(82, 140);
        atkIconRt.sizeDelta = new Vector2(48, 48);
        var atkIconImg = atkIconGo.AddComponent<Image>();
        atkIconImg.sprite = LoadRewardSprite("AttackIcon");
        atkIconImg.color = Color.white;
        atkIconImg.raycastTarget = false;
        atkIconGo.SetActive(data.damage > 0);

        var atkTextGo = new GameObject("AttackText");
        atkTextGo.transform.SetParent(atkIconGo.transform, false);
        var atkTextRt = atkTextGo.AddComponent<RectTransform>();
        atkTextRt.anchorMin = Vector2.zero; atkTextRt.anchorMax = Vector2.one;
        atkTextRt.offsetMin = Vector2.zero; atkTextRt.offsetMax = Vector2.zero;
        var attackText = atkTextGo.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) attackText.font = fontAsset;
        attackText.fontSize = 24; attackText.color = Color.white;
        attackText.alignment = TextAlignmentOptions.Center; attackText.text = data.damage.ToString();
        attackText.raycastTarget = false;

        // HealthIcon (右上角，与攻击图标同位置，不会同时出现)
        int healOrShield = Mathf.Max(data.heal, data.shield);
        var hpIconGo = new GameObject("HealthIcon");
        hpIconGo.transform.SetParent(go.transform, false);
        var hpIconRt = hpIconGo.AddComponent<RectTransform>();
        hpIconRt.anchoredPosition = new Vector2(82, 140);
        hpIconRt.sizeDelta = new Vector2(48, 48);
        var hpIconImg = hpIconGo.AddComponent<Image>();
        hpIconImg.sprite = LoadRewardSprite("HealthIcon");
        hpIconImg.color = Color.white;
        hpIconImg.raycastTarget = false;
        hpIconGo.SetActive(healOrShield > 0);

        var hpTextGo = new GameObject("HealthText");
        hpTextGo.transform.SetParent(hpIconGo.transform, false);
        var hpTextRt = hpTextGo.AddComponent<RectTransform>();
        hpTextRt.anchorMin = Vector2.zero; hpTextRt.anchorMax = Vector2.one;
        hpTextRt.offsetMin = Vector2.zero; hpTextRt.offsetMax = Vector2.zero;
        var healText = hpTextGo.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) healText.font = fontAsset;
        healText.fontSize = 26; healText.color = Color.white;
        healText.alignment = TextAlignmentOptions.Center; healText.text = healOrShield.ToString();
        healText.raycastTarget = false;

        // Hint
        var hintGo = new GameObject("HintText");
        hintGo.transform.SetParent(go.transform, false);
        var hintRt = hintGo.AddComponent<RectTransform>();
        hintRt.anchoredPosition = new Vector2(0, -155);
        hintRt.sizeDelta = new Vector2(200, 25);
        var hintText = hintGo.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) hintText.font = fontAsset;
        hintText.fontSize = 14; hintText.color = new Color(0.6f, 0.6f, 0.6f);
        hintText.alignment = TextAlignmentOptions.Center; hintText.text = "点击选择";
        hintText.raycastTarget = false;

        // Outline
        var outline = go.AddComponent<Outline>();
        outline.effectColor = new Color(0, 0, 0, 0.85f);
        outline.effectDistance = new Vector2(2, -2);

        return go;
    }

    /// <summary>
    /// Selection effect: flash glow, punch scale, particle burst, fade other cards.
    /// </summary>
    IEnumerator PlaySelectEffect(RectTransform selectedCard, System.Action onComplete)
    {
        var selectedGO = selectedCard.gameObject;
        var siblingCards = new List<RectTransform>();

        foreach (Transform child in cardContainer)
        {
            if (child != selectedCard)
                siblingCards.Add(child as RectTransform);
        }

        // ── Glow flash ──
        var outline = selectedGO.GetComponent<Outline>();
        Color origOutlineColor = outline != null ? outline.effectColor : new Color(0, 0, 0, 0.85f);
        Vector2 origOutlineDist = outline != null ? outline.effectDistance : new Vector2(2, -2);

        // ── Particle burst (pool of small Image dots) ──
        var particles = new List<RectTransform>();
        int particleCount = 24;
        var particleParent = selectedCard.parent;
        for (int i = 0; i < particleCount; i++)
        {
            var pGo = new GameObject("Particle_" + i);
            pGo.transform.SetParent(particleParent, false);
            var pRt = pGo.AddComponent<RectTransform>();
            pRt.anchoredPosition = selectedCard.anchoredPosition;
            pRt.sizeDelta = new Vector2(8, 8);
            var pImg = pGo.AddComponent<Image>();
            pImg.color = new Color(1f, 0.85f, 0.3f, 1f);
            pImg.raycastTarget = false;
            particles.Add(pRt);
        }

        // ── Animate ──
        float duration = 0.6f;
        float elapsed = 0f;
        Vector3 startScale = selectedCard.localScale;
        Vector3 peakScale = startScale * 1.35f;
        Vector3 endScale = startScale * 1.15f;

        // Random directions for particles
        var directions = new Vector2[particleCount];
        for (int i = 0; i < particleCount; i++)
        {
            float angle = (360f / particleCount) * i * Mathf.Deg2Rad;
            float dist = Random.Range(120f, 220f);
            directions[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;
        }

        while (elapsed < duration)
        {
            float t = elapsed / duration;

            // Selected card: punch scale (overshoot then settle)
            if (t < 0.4f)
            {
                float k = t / 0.4f;
                selectedCard.localScale = Vector3.Lerp(startScale, peakScale, k * k);
            }
            else
            {
                float k = (t - 0.4f) / 0.6f;
                selectedCard.localScale = Vector3.Lerp(peakScale, endScale, k);
            }

            // Glow outline: bright → fade
            if (outline != null)
            {
                float glowT = t < 0.5f ? 1f : 1f - (t - 0.5f) / 0.5f;
                outline.effectColor = new Color(1f, 0.85f, 0.3f, glowT);
                outline.effectDistance = Vector2.Lerp(origOutlineDist, Vector2.one * 8f, glowT);
            }

            // Other cards: fade out + shrink
            foreach (var sib in siblingCards)
            {
                var cg = sib.GetComponent<CanvasGroup>();
                if (cg == null) cg = sib.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = 1f - t;
                sib.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.8f, t);
            }

            // Particles: fly outward + fade
            for (int i = 0; i < particles.Count; i++)
            {
                if (particles[i] == null) continue;
                Vector2 startPos = selectedCard.anchoredPosition;
                Vector2 endPos = startPos + directions[i];
                particles[i].anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                var pImg = particles[i].GetComponent<Image>();
                if (pImg != null)
                    pImg.color = new Color(1f, 0.85f, 0.3f, 1f - t);

                float pSize = Mathf.Lerp(8f, 2f, t);
                particles[i].sizeDelta = new Vector2(pSize, pSize);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Cleanup particles
        foreach (var p in particles)
        {
            if (p != null) Destroy(p.gameObject);
        }

        // Restore outline
        if (outline != null)
        {
            outline.effectColor = origOutlineColor;
            outline.effectDistance = origOutlineDist;
        }

        onComplete?.Invoke();
    }

    Sprite LoadRewardSprite(string name)
    {
        var sprite = Resources.Load<Sprite>("CardArt/" + name);
        if (sprite != null) return sprite;
#if UNITY_EDITOR
        string[] paths = {
            "Assets/Art/Cards/" + name + ".png",
            "Assets/Art/Effects/" + name + ".png",
            "Assets/Art/UI/" + name + ".png",
        };
        foreach (var path in paths)
        {
            sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null) return sprite;
            var tex = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex != null)
                return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f, 100f);
        }
#endif
        return null;
    }

    /// <summary>
    /// Shows an event popup with description and choices.
    /// </summary>
    public void ShowEvent(string title, string description, List<string> choices)
    {
        if (eventPanel != null)
            eventPanel.SetActive(true);

        if (titleText != null)
            titleText.text = title;

        if (eventDescriptionText != null)
            eventDescriptionText.text = description;

        // Clear old choices
        if (eventChoiceContainer != null)
        {
            for (int i = eventChoiceContainer.childCount - 1; i >= 0; i--)
                Destroy(eventChoiceContainer.GetChild(i).gameObject);
        }

        // Spawn choice buttons
        foreach (var choice in choices)
        {
            if (eventChoiceButtonPrefab != null)
            {
                var go = Instantiate(eventChoiceButtonPrefab, eventChoiceContainer);
                var text = go.GetComponentInChildren<TMP_Text>();
                if (text != null)
                    text.text = choice;

                var button = go.GetComponent<Button>();
                var captured = choice;
                if (button != null)
                    button.onClick.AddListener(() =>
                    {
                        Debug.Log("[Event] Choice: " + captured);
                        gameObject.SetActive(false);
                        ReturnToMap();
                    });
            }
        }

        gameObject.SetActive(true);
    }

    private void ReturnToMap()
    {
        var gm = GameManager.Instance;
        if (gm != null)
        {
            gm.OnRewardComplete();
        }
        else
        {
            Debug.LogWarning("[RewardPopup] GameManager.Instance is null, returning to MainMenu.");
            UnityEngine.SceneManagement.SceneManager.LoadScene(GameConstants.SCENE_MAIN_MENU);
        }
    }

    private System.Action<CardData> OnCardSelected;
    public event System.Action<CardData> OnCardSelectedEvent
    {
        add { OnCardSelected += value; }
        remove { OnCardSelected -= value; }
    }
}
