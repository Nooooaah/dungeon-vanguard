using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Renders a CardData instance on UI elements (Canvas-based card prefab).
/// Assign via the Inspector or call Setup() at runtime.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class CardDisplay : MonoBehaviour
{
    [Header("Card Data")]
    public CardData cardData;

    [Header("UI References")]
    public Image backgroundImage;
    public Image artImage;
    public Image elementBadge;
    public TMP_Text nameText;
    public TMP_Text costText;
    public TMP_Text damageText;
    public TMP_Text descriptionText;
    public TMP_Text elementText;

    /// <summary>
    /// Applies the cardData to the UI elements.
    /// Call this after assigning cardData, or it runs on Start().
    /// </summary>
    public void Setup(CardData data)
    {
        cardData = data;
        RefreshDisplay();
    }

    void Start()
    {
        if (cardData != null)
            RefreshDisplay();
    }

    void RefreshDisplay()
    {
        if (cardData == null) return;

        if (nameText != null)
            nameText.text = cardData.cardName;

        if (costText != null)
            costText.text = cardData.cost.ToString();

        if (damageText != null)
            damageText.text = cardData.damage.ToString();

        if (descriptionText != null)
            descriptionText.text = cardData.description;

        if (elementText != null)
            elementText.text = CardData.GetElementName(cardData.element);

        if (artImage != null && cardData.artwork != null)
            artImage.sprite = cardData.artwork;

        if (backgroundImage != null)
            backgroundImage.color = Color.white;

        if (elementBadge != null)
            elementBadge.color = CardData.GetElementColor(cardData.element);
    }
}
