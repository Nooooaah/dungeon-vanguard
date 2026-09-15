using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum ComboType
{
    Steam,      // Fire + Water → white smoke
    FireStorm,  // Fire + Wind  → fire vortex
    IceSpikes   // Water + Wind → ice crystal burst
}

/// <summary>
/// Triggers screen-wide combo visual effects when element reactions occur.
/// Each combo plays a screen flash + particle burst + text popup.
/// </summary>
public class ComboEffectSystem : MonoBehaviour
{
    [Header("Screen Flash")]
    public float flashDuration = 0.4f;
    public float flashMaxAlpha = 0.5f;

    [Header("Particles")]
    public int particleCount = 60;
    public float particleLifetime = 1.5f;
    public float particleSpread = 300f;

    [Header("Text Popup")]
    public float popupDuration = 1.2f;
    public float popupRise = 80f;

    private Image _flashOverlay;
    private RectTransform _particleLayer;
    private RectTransform _textLayer;
    private static ComboEffectSystem _instance;

    public static ComboEffectSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("ComboEffectSystem");
                _instance = go.AddComponent<ComboEffectSystem>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    /// <summary>
    /// Initializes the effect system with required UI layers.
    /// Call this from the BattleUI setup.
    /// </summary>
    public void Initialize(Image flashOverlay, RectTransform particleLayer, RectTransform textLayer)
    {
        _flashOverlay = flashOverlay;
        _particleLayer = particleLayer;
        _textLayer = textLayer;
        if (_flashOverlay != null)
            _flashOverlay.color = Color.clear;
    }

    /// <summary>
    /// Triggers a combo effect based on the two element types.
    /// </summary>
    public void TriggerCombo(Element e1, Element e2)
    {
        ComboType combo = GetComboType(e1, e2);
        TriggerCombo(combo);
    }

    /// <summary>
    /// Triggers a specific combo effect directly.
    /// </summary>
    public void TriggerCombo(ComboType combo)
    {
        Color flashColor;
        Color particleColor;
        string label;

        switch (combo)
        {
            case ComboType.Steam:
                flashColor = new Color(0.9f, 0.9f, 0.95f, 1f);
                particleColor = new Color(0.85f, 0.85f, 0.9f, 0.6f);
                label = "蒸汽爆发!";
                break;
            case ComboType.FireStorm:
                flashColor = new Color(1f, 0.3f, 0.0f, 1f);
                particleColor = new Color(1f, 0.45f, 0.0f, 0.8f);
                label = "烈焰风暴!";
                break;
            case ComboType.IceSpikes:
                flashColor = new Color(0.6f, 0.85f, 1f, 1f);
                particleColor = new Color(0.7f, 0.9f, 1f, 0.8f);
                label = "冰刺炸裂!";
                break;
            default:
                return;
        }

        StartCoroutine(PlayComboEffect(flashColor, particleColor, label, combo));
    }

    ComboType GetComboType(Element e1, Element e2)
    {
        var set = new HashSet<Element> { e1, e2 };
        if (set.Contains(Element.Fire) && set.Contains(Element.Water))
            return ComboType.Steam;
        if (set.Contains(Element.Fire) && set.Contains(Element.Wind))
            return ComboType.FireStorm;
        if (set.Contains(Element.Water) && set.Contains(Element.Wind))
            return ComboType.IceSpikes;
        return ComboType.Steam; // fallback
    }

    IEnumerator PlayComboEffect(Color flashCol, Color particleCol, string label, ComboType combo)
    {
        // 1. Screen flash
        if (_flashOverlay != null)
        {
            yield return StartCoroutine(FlashScreen(flashCol));
        }

        // 2. Particles + text popup (parallel)
        if (_particleLayer != null)
            StartCoroutine(SpawnParticles(particleCol, combo));

        if (_textLayer != null)
            StartCoroutine(ShowComboText(label, flashCol));

        yield return null;
    }

    IEnumerator FlashScreen(Color color)
    {
        float elapsed = 0f;
        float half = flashDuration * 0.5f;

        // Fade in
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float a = (elapsed / half) * flashMaxAlpha;
            _flashOverlay.color = new Color(color.r, color.g, color.b, a);
            yield return null;
        }

        // Fade out
        elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float a = flashMaxAlpha * (1f - elapsed / half);
            _flashOverlay.color = new Color(color.r, color.g, color.b, a);
            yield return null;
        }

        _flashOverlay.color = Color.clear;
    }

    IEnumerator SpawnParticles(Color color, ComboType combo)
    {
        var particles = new List<RectTransform>();
        var startPos = Vector2.zero;

        for (int i = 0; i < particleCount; i++)
        {
            var go = new GameObject("Particle_" + i);
            go.transform.SetParent(_particleLayer, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = startPos;
            rt.sizeDelta = new Vector2(12, 12);

            var img = go.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;

            // Random direction based on combo type
            Vector2 dir;
            if (combo == ComboType.FireStorm)
            {
                // Spiral outward
                float angle = (float)i / particleCount * Mathf.PI * 4f;
                float radius = 20f + (float)i / particleCount * particleSpread;
                dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            }
            else if (combo == ComboType.IceSpikes)
            {
                // Sharp upward spikes
                dir = new Vector2(Random.Range(-particleSpread, particleSpread), Random.Range(50f, particleSpread));
            }
            else
            {
                // Steam: slow rising puff
                dir = new Vector2(Random.Range(-particleSpread * 0.5f, particleSpread * 0.5f), Random.Range(80f, particleSpread));
            }

            particles.Add(rt);
            StartCoroutine(MoveParticle(rt, dir, color));
        }

        yield return new WaitForSeconds(particleLifetime);

        foreach (var p in particles)
        {
            if (p != null) Destroy(p.gameObject);
        }
    }

    IEnumerator MoveParticle(RectTransform rt, Vector2 direction, Color baseColor)
    {
        Vector2 start = rt.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < particleLifetime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / particleLifetime;
            rt.anchoredPosition = start + direction * t;

            // Fade + shrink
            var img = rt.GetComponent<Image>();
            if (img != null)
            {
                img.color = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * (1f - t));
                rt.sizeDelta = Vector2.Lerp(new Vector2(12, 12), new Vector2(4, 4), t);
            }

            yield return null;
        }
    }

    IEnumerator ShowComboText(string text, Color color)
    {
        var go = new GameObject("ComboText");
        go.transform.SetParent(_textLayer, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(900, 100);

        var textComp = go.AddComponent<TextMeshProUGUI>();
        var fontAsset = TMPro.TMP_Settings.defaultFontAsset;
        // Try msyhl SDF for CJK
        var msyhl = UnityEngine.Resources.Load<TMPro.TMP_FontAsset>("msyhl SDF");
        if (msyhl != null) fontAsset = msyhl;
#if UNITY_EDITOR
        if (fontAsset == null || fontAsset.name == "LiberationSans SDF")
            fontAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>("Assets/msyhl SDF.asset");
#endif
        if (fontAsset != null)
            textComp.font = fontAsset;
        textComp.fontSize = 48;
        textComp.alignment = TextAlignmentOptions.Center;
        textComp.color = color;
        textComp.text = text;
        textComp.overflowMode = TextOverflowModes.Overflow;
        textComp.raycastTarget = false;

        var outline = go.AddComponent<Outline>();
        outline.effectColor = new Color(0, 0, 0, 0.8f);
        outline.effectDistance = new Vector2(3, -3);

        Vector2 startPos = rt.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < popupDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / popupDuration;

            // Rise + fade
            rt.anchoredPosition = startPos + Vector2.up * (popupRise * t);
            float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.2f;
            rt.localScale = Vector3.one * scale;

            if (t > 0.5f)
                textComp.color = new Color(color.r, color.g, color.b, 1f - (t - 0.5f) * 2f);

            yield return null;
        }

        Destroy(go);
    }
}
