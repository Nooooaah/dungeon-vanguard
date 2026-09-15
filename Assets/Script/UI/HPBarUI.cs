using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// HP bar UI with smooth lerp animation, dynamic color (green/yellow/red),
/// shield overlay, and 3 energy icons that gray out when consumed.
/// Binds to any IDamageable entity via BindTo().
/// </summary>
public class HPBarUI : MonoBehaviour
{
    [Header("HP")]
    public Image hpFill;
    public TMP_Text hpText;
    public Color hpGreen = new Color(0.15f, 0.70f, 0.20f);
    public Color hpYellow = new Color(0.90f, 0.75f, 0.15f);
    public Color hpRed = new Color(0.80f, 0.15f, 0.15f);

    [Header("Shield")]
    public Image shieldFill;
    public TMP_Text shieldText;
    public GameObject shieldContainer;

    [Header("Energy Icons (3 circles)")]
    public Image[] energyIcons = new Image[3];
    public Color energyActive = new Color(0.15f, 0.40f, 0.85f);
    public Color energyDepleted = new Color(0.20f, 0.20f, 0.22f, 1f);

    [Header("Animation")]
    public float lerpSpeed = 5f;

    private IDamageable _boundEntity;
    private float _displayedHP;
    private int _displayedShield;
    private int _displayedEnergy;

<<<<<<< HEAD
=======
    void Start()
    {
        // 确保用 localScale.x 控制宽度，不依赖 Filled Image type
        if (hpFill != null)
        {
            hpFill.type = Image.Type.Simple;
            hpFill.rectTransform.pivot = new Vector2(0f, 0.5f);
        }
        if (shieldFill != null)
        {
            shieldFill.type = Image.Type.Simple;
            shieldFill.rectTransform.pivot = new Vector2(0f, 0.5f);
        }
    }

>>>>>>> dd15dc953bda76878a619e06d67d3f5c5cb965a7
    void Update()
    {
        if (_boundEntity == null) return;

        // Smooth HP lerp
        float targetHP = _boundEntity.CurrentHP;
        if (Mathf.Abs(_displayedHP - targetHP) > 0.1f)
        {
            _displayedHP = Mathf.Lerp(_displayedHP, targetHP, Time.deltaTime * lerpSpeed);
        }
        else
        {
            _displayedHP = targetHP;
        }

<<<<<<< HEAD
        // Update HP fill
        if (hpFill != null)
        {
            float ratio = _boundEntity.MaxHP > 0 ? _displayedHP / _boundEntity.MaxHP : 0f;
            hpFill.fillAmount = Mathf.Clamp01(ratio);
=======
        // Update HP fill — 用 localScale.x 控制血条长度
        if (hpFill != null)
        {
            float ratio = _boundEntity.MaxHP > 0 ? _displayedHP / _boundEntity.MaxHP : 0f;
            float clamped = Mathf.Clamp01(ratio);
            hpFill.rectTransform.localScale = new Vector3(clamped, 1f, 1f);
>>>>>>> dd15dc953bda76878a619e06d67d3f5c5cb965a7

            // Dynamic color
            if (ratio > 0.5f)
                hpFill.color = hpGreen;
            else if (ratio > 0.25f)
                hpFill.color = hpYellow;
            else
                hpFill.color = hpRed;
        }

        // HP text
        if (hpText != null)
            hpText.text = Mathf.RoundToInt(_boundEntity.CurrentHP) + "/" + _boundEntity.MaxHP;

        // Shield
        if (_displayedShield != _boundEntity.Shield)
            _displayedShield = _boundEntity.Shield;

        if (shieldContainer != null)
            shieldContainer.SetActive(_displayedShield > 0);

        if (shieldFill != null)
<<<<<<< HEAD
            shieldFill.fillAmount = Mathf.Clamp01((float)_displayedShield / Mathf.Max(1, _boundEntity.MaxHP) * 0.5f);
=======
        {
            float shieldRatio = Mathf.Clamp01((float)_displayedShield / Mathf.Max(1, _boundEntity.MaxHP) * 0.5f);
            shieldFill.rectTransform.localScale = new Vector3(shieldRatio, 1f, 1f);
        }
>>>>>>> dd15dc953bda76878a619e06d67d3f5c5cb965a7

        if (shieldText != null)
            shieldText.text = _displayedShield > 0 ? _displayedShield.ToString() : "";
    }

    /// <summary>
    /// Binds this HP bar to an IDamageable entity for automatic sync.
    /// </summary>
    public void BindTo(IDamageable entity)
    {
        _boundEntity = entity;
        _displayedHP = entity.CurrentHP;
        _displayedShield = entity.Shield;
        UpdateDisplay();
    }

    /// <summary>
    /// Updates energy display: grays out icons beyond currentEnergy.
    /// </summary>
    public void SetEnergy(int currentEnergy, int maxEnergy)
    {
        _displayedEnergy = currentEnergy;
        for (int i = 0; i < energyIcons.Length; i++)
        {
            if (energyIcons[i] == null) continue;
            if (i < currentEnergy)
            {
                energyIcons[i].color = energyActive;
            }
            else
            {
                energyIcons[i].color = energyDepleted;
            }
        }
    }

    void UpdateDisplay()
    {
        if (_boundEntity == null) return;
        _displayedHP = _boundEntity.CurrentHP;
        _displayedShield = _boundEntity.Shield;

        if (hpFill != null)
        {
            float ratio = _boundEntity.MaxHP > 0 ? (float)_boundEntity.CurrentHP / _boundEntity.MaxHP : 0f;
<<<<<<< HEAD
            hpFill.fillAmount = Mathf.Clamp01(ratio);
=======
            float clamped = Mathf.Clamp01(ratio);
            hpFill.rectTransform.localScale = new Vector3(clamped, 1f, 1f);
>>>>>>> dd15dc953bda76878a619e06d67d3f5c5cb965a7
            hpFill.color = ratio > 0.5f ? hpGreen : ratio > 0.25f ? hpYellow : hpRed;
        }
        if (hpText != null)
            hpText.text = _boundEntity.CurrentHP + "/" + _boundEntity.MaxHP;
    }
}
