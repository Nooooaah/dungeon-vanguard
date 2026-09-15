using UnityEngine;
using TMPro;

/// <summary>
/// Centralized UI style definitions: colors, fonts, sizes, spacing.
/// Used by all 6 UI panels for visual consistency.
/// </summary>
public static class UIStyleGuide
{
    // ── Color Palette ──
    public static readonly Color BgDark       = new Color(0.06f, 0.06f, 0.10f, 1f);
    public static readonly Color BgPanel      = new Color(0.10f, 0.10f, 0.16f, 0.92f);
    public static readonly Color BgPanelLight = new Color(0.16f, 0.16f, 0.24f, 0.92f);

    public static readonly Color Fire          = new Color(1.0f, 0.27f, 0.0f);
    public static readonly Color FireLight     = new Color(1.0f, 0.55f, 0.0f);
    public static readonly Color Water         = new Color(0.0f, 0.45f, 0.85f);
    public static readonly Color WaterLight    = new Color(0.0f, 0.75f, 0.95f);
    public static readonly Color Wind          = new Color(0.13f, 0.60f, 0.20f);
    public static readonly Color WindLight     = new Color(0.25f, 0.75f, 0.40f);

    public static readonly Color Gold          = new Color(0.85f, 0.70f, 0.30f);
    public static readonly Color GoldLight     = new Color(1.0f, 0.90f, 0.55f);
    public static readonly Color TextWhite     = new Color(0.95f, 0.95f, 0.95f);
    public static readonly Color TextDim       = new Color(0.60f, 0.60f, 0.65f);
    public static readonly Color TextGold      = new Color(1f, 0.92f, 0.70f);
    public static readonly Color HPRed         = new Color(0.80f, 0.15f, 0.15f);
    public static readonly Color HPGreen       = new Color(0.15f, 0.70f, 0.20f);
    public static readonly Color EnergyBlue    = new Color(0.15f, 0.40f, 0.85f);
    public static readonly Color ShieldGray    = new Color(0.55f, 0.60f, 0.70f);
    public static readonly Color ButtonNormal  = new Color(0.18f, 0.18f, 0.26f);
    public static readonly Color ButtonHover   = new Color(0.28f, 0.28f, 0.38f);
    public static readonly Color ButtonPressed = new Color(0.12f, 0.12f, 0.18f);

    // ── Combo Colors ──
    public static readonly Color SteamColor    = new Color(0.85f, 0.85f, 0.90f);
    public static readonly Color FireStormCol  = new Color(1.0f, 0.35f, 0.0f);
    public static readonly Color IceSpikeCol   = new Color(0.6f, 0.85f, 1.0f);

    // ── Typography ──
    public const int TitleSize   = 48;
    public const int HeaderSize  = 32;
    public const int BodySize    = 20;
    public const int SmallSize   = 16;
    public const int CardNameSize = 22;
    public const int CardDescSize = 14;
    public const int StatSize    = 26;

    // ── Layout Spacing ──
    public const float MarginSmall  = 8f;
    public const float MarginMedium = 16f;
    public const float MarginLarge  = 24f;
    public const float MarginXL     = 40f;
    public const float CardWidth    = 220f;
    public const float CardHeight   = 320f;
    public const float ButtonHeight = 56f;
    public const float ButtonWidth  = 280f;
    public const float CornerRadii   = 12f;

    // ── Font Asset Path ──
    public const string TMPFontPath = "Fonts & Materials/LiberationSans SDF";

    /// <summary>
    /// Loads the TMP font asset. For CJK characters, a fallback or dynamic
    /// font asset should be assigned via TMP Settings.
    /// </summary>
    public static TMP_FontAsset GetFont()
    {
        return TMP_Settings.defaultFontAsset;
    }

    public static Color GetElementColor(Element e)
    {
        switch (e)
        {
            case Element.Fire:  return Fire;
            case Element.Water: return Water;
            case Element.Wind:  return Wind;
            default: return Color.white;
        }
    }

    public static Color GetElementLight(Element e)
    {
        switch (e)
        {
            case Element.Fire:  return FireLight;
            case Element.Water: return WaterLight;
            case Element.Wind:  return WindLight;
            default: return Color.white;
        }
    }
}
