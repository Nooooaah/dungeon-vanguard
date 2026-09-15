using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Floating damage/heal/shield text that pops up at target position,
/// floats upward, and fades out. Auto-destroys after animation.
/// Colors: Damage=red, Heal=green, Shield=blue, Critical=gold (1.5x size).
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class DamageText : MonoBehaviour
{
    [Header("Visual")]
    public float floatDuration = 1.0f;
    public float floatDistance = 50f;
    public Vector2 startOffset = new Vector2(0, 40f);

    [Header("Colors")]
    public static Color DamageColor = new Color(0.91f, 0.30f, 0.24f);   // #e74c3c
    public static Color HealColor   = new Color(0.18f, 0.80f, 0.44f);   // #2ecc71
    public static Color ShieldColor = new Color(0.20f, 0.60f, 0.96f);   // #3498db
    public static Color CriticalColor = new Color(0.95f, 0.61f, 0.07f);  // #f39c12

    private TMP_Text _text;
    private RectTransform _rt;

    /// <summary>
    /// Shows a damage/heal/shield number at the given position.
    /// Spawns a temporary GO that auto-destroys.
    /// </summary>
    public static DamageText Show(Transform parent, Vector2 localPos, int amount, DamageType type)
    {
        var go = new GameObject("DamageText_" + type);
        go.transform.SetParent(parent, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = localPos;
        rt.sizeDelta = new Vector2(120, 40);

        var text = go.AddComponent<TextMeshProUGUI>();
        text.fontSize = 24;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        text.overflowMode = TextOverflowModes.Overflow;

        var outline = go.AddComponent<Outline>();
        outline.effectColor = new Color(0, 0, 0, 0.8f);
        outline.effectDistance = new Vector2(2, -2);

        var dt = go.AddComponent<DamageText>();
        dt._text = text;
        dt._rt = rt;
        dt.Init(amount, type);
        return dt;
    }

    void Init(int amount, DamageType type)
    {
        string prefix = "";
        Color color;

        switch (type)
        {
            case DamageType.Damage:
                prefix = "-";
                color = DamageColor;
                _text.fontSize = 24;
                break;
            case DamageType.Heal:
                prefix = "+";
                color = HealColor;
                break;
            case DamageType.Shield:
                prefix = "+";
                color = ShieldColor;
                break;
            case DamageType.Critical:
                prefix = "-";
                color = CriticalColor;
                _text.fontSize = 36; // 1.5x
                break;
            default:
                color = Color.white;
                break;
        }

        _text.text = prefix + Mathf.Abs(amount);
        _text.color = color;

        // Start offset
        var startPos = _rt.anchoredPosition + startOffset;
        _rt.anchoredPosition = startPos;

        StartCoroutine(FloatAndFade());
    }

    IEnumerator FloatAndFade()
    {
        Vector2 startPos = _rt.anchoredPosition;
        Vector2 endPos = startPos + Vector2.up * floatDistance;
        float elapsed = 0f;

        // Pop-in scale
        transform.localScale = Vector3.zero;

        while (elapsed < floatDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / floatDuration;

            // Position: float up
            _rt.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

            // Scale: pop in (0→1.2→1.0), then shrink slightly
            if (t < 0.15f)
                transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * 1.2f, t / 0.15f);
            else if (t < 0.25f)
                transform.localScale = Vector3.Lerp(Vector3.one * 1.2f, Vector3.one, (t - 0.15f) / 0.10f);
            else
                transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.9f, (t - 0.25f) / 0.75f);

            // Fade out in last 40%
            if (t > 0.6f)
            {
                float alpha = 1f - (t - 0.6f) / 0.4f;
                _text.color = new Color(_text.color.r, _text.color.g, _text.color.b, alpha);
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}
