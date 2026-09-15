using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Card rendering component: binds CardData to UI (name, cost, description,
/// element background color). Handles energy-insufficient gray state.
/// Works alongside CardHoverEffect and CardDragHandler.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class CardUI : MonoBehaviour
{
    [Header("Card Data")]
    public CardData cardData;

    [Header("UI Elements")]
    public Image backgroundImage;
    public Image frameImage;
    public Image elementBadge;
    public Image artImage;
    public TMP_Text nameText;
    public TMP_Text costText;
    public TMP_Text descriptionText;
    public TMP_Text elementText;
    public TMP_Text attackText;
    public TMP_Text healText;
    public GameObject attackIconGroup;
    public GameObject healIconGroup;

    [Header("State")]
    public bool interactable = true;
    public bool energySufficient = true;

    // Cached original colors for gray-out restore
    private Color _bgOriginalColor = Color.clear;
    private bool _wasGrayed;

    /// <summary>
    /// Binds cardData to all UI elements.
    /// </summary>
    public void Setup(CardData data)
    {
        cardData = data;
        RefreshDisplay();
    }

    /// <summary>
    /// Sets whether the card can be played (energy check).
    /// When insufficient: card darkens + semi-transparent.
    /// </summary>
    public void SetEnergySufficient(bool sufficient)
    {
        energySufficient = sufficient;
        interactable = sufficient;

        if (backgroundImage != null)
        {
            if (_bgOriginalColor == Color.clear)
                _bgOriginalColor = backgroundImage.color;

            if (sufficient)
            {
                // Restore
                backgroundImage.color = _bgOriginalColor;
            }
            else
            {
                // Darken
                Color c = _bgOriginalColor;
                backgroundImage.color = new Color(c.r * 0.35f, c.g * 0.35f, c.b * 0.35f, 0.7f);
            }
        }

        // Gray out text
        if (!sufficient)
        {
            if (nameText != null) nameText.color = new Color(0.5f, 0.5f, 0.5f, 0.8f);
            if (costText != null) costText.color = new Color(0.5f, 0.5f, 0.5f, 0.8f);
            if (descriptionText != null) descriptionText.color = new Color(0.4f, 0.4f, 0.4f, 0.7f);
        }
        else
        {
            if (nameText != null) nameText.color = new Color(1f, 0.92f, 0.7f);
            if (costText != null) costText.color = Color.white;
            if (descriptionText != null) descriptionText.color = new Color(0.92f, 0.92f, 0.92f);
        }

        _wasGrayed = !sufficient;
    }

    void RefreshDisplay()
    {
        if (cardData == null) return;

        if (nameText != null)
            nameText.text = cardData.cardName;

        if (costText != null)
            costText.text = cardData.cost.ToString();

        if (descriptionText != null)
            descriptionText.text = cardData.GetDescription();

        if (elementText != null)
            elementText.text = CardData.GetElementName(cardData.element);

        if (backgroundImage != null)
        {
            backgroundImage.color = Color.white;
            _bgOriginalColor = Color.white;
        }

        if (elementBadge != null)
            elementBadge.color = CardData.GetElementColor(cardData.element);

        if (artImage != null && cardData.artwork != null)
            artImage.sprite = cardData.artwork;

        // Attack icon
        if (attackIconGroup != null)
            attackIconGroup.SetActive(cardData.damage > 0);
        if (attackText != null && cardData.damage > 0)
            attackText.text = cardData.damage.ToString();

        // Heal / Shield icon
        int healOrShield = Mathf.Max(cardData.heal, cardData.shield);
        if (healIconGroup != null)
            healIconGroup.SetActive(healOrShield > 0);
        if (healText != null && healOrShield > 0)
            healText.text = healOrShield.ToString();
    }

    void Start()
    {
        if (cardData != null)
            RefreshDisplay();
    }
}
