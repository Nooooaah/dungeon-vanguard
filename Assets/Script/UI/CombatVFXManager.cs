using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 战斗特效管理器 —— 程序化生成战斗VFX
/// 攻击：玩家身上出现多道狰狞抓痕
/// 防御：目标身上出现蓝色古罗马护盾
/// 治疗：目标身上出现绿色十字架 + 淡淡绿光环
/// </summary>
public class CombatVFXManager : MonoBehaviour
{
    public static CombatVFXManager Instance { get; private set; }

    private RectTransform _particleLayer;
    private RectTransform _enemyArea;
    private RectTransform _playerArea;
    private TMP_FontAsset _font;

    private Sprite _clawSprite;
    private Sprite _shieldSprite;
    private Sprite _glowSprite;

    void Awake()
    {
        Instance = this;
        GenerateSprites();
    }

    public void Initialize(RectTransform particleLayer, RectTransform enemyArea,
                           RectTransform playerArea, TMP_FontAsset font)
    {
        _particleLayer = particleLayer;
        _enemyArea = enemyArea;
        _playerArea = playerArea;
        _font = font;
    }

    /// <summary>设置玩家区域（BattleUI 在 playerArea 为空时调用）</summary>
    public void SetPlayerArea(RectTransform area) => _playerArea = area;

    // ==========================================
    //  公开接口
    // ==========================================

    /// <summary>攻击特效：在玩家身上生成多道狰狞抓痕</summary>
    public void PlayClawMarks(Vector2 position)
    {
        if (_particleLayer == null) return;
        StartCoroutine(ClawMarksRoutine(position));
    }

    /// <summary>防御特效：在目标身上出现蓝色古罗马护盾</summary>
    public void PlayShield(Vector2 position)
    {
        if (_particleLayer == null) return;
        StartCoroutine(ShieldRoutine(position));
    }

    /// <summary>治疗特效：在目标身上出现绿色十字架 + 绿光环</summary>
    public void PlayHeal(Vector2 position)
    {
        if (_particleLayer == null) return;
        StartCoroutine(HealRoutine(position));
    }

    // ==========================================
    //  位置辅助 —— 将目标的 worldPosition 转换到 _particleLayer 的局部坐标
    // ==========================================

    /// <summary>获取敌人在 particleLayer 局部坐标系中的位置</summary>
    public Vector2 GetEnemyPosition()
    {
        return WorldToParticleLocal(_enemyArea);
    }

    /// <summary>获取玩家在 particleLayer 局部坐标系中的位置</summary>
    public Vector2 GetPlayerPosition()
    {
        return WorldToParticleLocal(_playerArea);
    }

    /// <summary>把任意 RectTransform 的世界坐标转换为 _particleLayer 下的局部坐标</summary>
    Vector2 WorldToParticleLocal(RectTransform target)
    {
        if (target == null || _particleLayer == null)
            return Vector2.zero;

        // 获取目标在世界空间中的位置
        Vector3 worldPos = target.position;

        // 获取 _particleLayer 所在 Canvas 的渲染相机（Overlay 模式下为 null）
        Canvas canvas = _particleLayer.GetComponentInParent<Canvas>();
        Camera cam = canvas != null ? canvas.worldCamera : null;

        // world → screen → particleLayer local
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, worldPos);
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _particleLayer, screenPos, cam, out localPos);
        return localPos;
    }

    // ==========================================
    //  程序化纹理生成
    // ==========================================

    void GenerateSprites()
    {
        _clawSprite = GenerateClawSprite();
        _shieldSprite = GenerateShieldSprite();
        _glowSprite = GenerateGlowSprite();
    }

    /// <summary>生成抓痕纹理：中间粗两端尖的弧形条</summary>
    Sprite GenerateClawSprite()
    {
        int w = 32, h = 160;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        var pixels = new Color32[w * h];
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                // vertical taper: 0 at edges, 1 at center
                float vy = (float)y / (h - 1);
                float taperY = Mathf.Sin(vy * Mathf.PI); // 0 at ends, 1 at middle
                // horizontal taper
                float vx = (float)x / (w - 1);
                float taperX = 1f - Mathf.Abs(vx - 0.5f) * 2f;
                // slight curve
                float curveOffset = Mathf.Sin(vy * Mathf.PI * 0.5f) * 3f;
                float distX = Mathf.Abs(x - w * 0.5f - curveOffset);
                float halfWidth = (w * 0.5f) * taperY;
                float alpha = Mathf.Clamp01(1f - distX / Mathf.Max(1f, halfWidth));
                alpha *= taperY;
                byte a = (byte)(alpha * 255);
                pixels[y * w + x] = new Color32(255, 40, 30, a);
            }
        }
        tex.SetPixels32(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
    }

    /// <summary>生成护盾纹理：椭圆形带渐变边缘</summary>
    Sprite GenerateShieldSprite()
    {
        int size = 200;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var pixels = new Color32[size * size];
        float cx = size * 0.5f, cy = size * 0.5f;
        float rx = size * 0.42f, ry = size * 0.48f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x - cx) / rx;
                float dy = (y - cy) / ry;
                float dist = dx * dx + dy * dy;
                float alpha;
                if (dist < 0.7f)
                    alpha = 0.6f;
                else if (dist < 1f)
                    alpha = Mathf.Lerp(0.6f, 0f, (dist - 0.7f) / 0.3f);
                else
                    alpha = 0f;
                // border ring
                float borderAlpha = 0f;
                if (dist > 0.82f && dist < 0.98f)
                    borderAlpha = Mathf.Lerp(1f, 0f, Mathf.Abs(dist - 0.9f) / 0.08f);
                byte a = (byte)(Mathf.Max(alpha, borderAlpha) * 255);
                byte r = (byte)(30 + borderAlpha * 120);
                byte g = (byte)(80 + borderAlpha * 100);
                byte b = (byte)(220 + borderAlpha * 35);
                pixels[y * size + x] = new Color32(r, g, b, a);
            }
        }
        tex.SetPixels32(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    /// <summary>生成圆形光晕纹理</summary>
    Sprite GenerateGlowSprite()
    {
        int size = 256;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var pixels = new Color32[size * size];
        float cx = size * 0.5f, cy = size * 0.5f;
        float radius = size * 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - cx, dy = y - cy;
                float dist = Mathf.Sqrt(dx * dx + dy * dy) / radius;
                float alpha = Mathf.Clamp01(1f - dist);
                alpha = alpha * alpha * 0.5f; // soft falloff
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)(alpha * 255));
            }
        }
        tex.SetPixels32(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    // ==========================================
    //  攻击特效：多道狰狞抓痕
    // ==========================================

    IEnumerator ClawMarksRoutine(Vector2 position)
    {
        int clawCount = 5;
        float spread = 120f;
        var claws = new List<GameObject>();

        for (int i = 0; i < clawCount; i++)
        {
            var go = new GameObject("VFX_ClawMark_" + i);
            go.transform.SetParent(_particleLayer, false);
            var rt = go.AddComponent<RectTransform>();

            float xOffset = Mathf.Lerp(-spread, spread, (float)i / (clawCount - 1));
            rt.anchoredPosition = position + new Vector2(xOffset, Random.Range(-15f, 15f));
            rt.sizeDelta = new Vector2(70f, 380f + Random.Range(-20f, 20f));

            float angle = -25f + (float)i / (clawCount - 1) * 50f + Random.Range(-5f, 5f);
            rt.localRotation = Quaternion.Euler(0, 0, angle);
            rt.localScale = Vector3.zero;

            var img = go.AddComponent<Image>();
            img.sprite = _clawSprite;
            img.color = new Color(1f, 0.15f, 0.1f, 1f);
            img.raycastTarget = false;

            // glow layer
            var glowGo = new GameObject("Glow");
            glowGo.transform.SetParent(go.transform, false);
            var glowRt = glowGo.AddComponent<RectTransform>();
            glowRt.anchorMin = Vector2.zero; glowRt.anchorMax = Vector2.one;
            glowRt.offsetMin = new Vector2(-12, -12); glowRt.offsetMax = new Vector2(12, 12);
            var glowImg = glowGo.AddComponent<Image>();
            glowImg.sprite = _clawSprite;
            glowImg.color = new Color(1f, 0.5f, 0.2f, 0.5f);
            glowImg.raycastTarget = false;

            claws.Add(go);
        }

        // slash-in animation
        float inDuration = 0.12f;
        float elapsed = 0f;
        while (elapsed < inDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / inDuration);
            foreach (var go in claws)
            {
                if (go == null) continue;
                var rt = go.GetComponent<RectTransform>();
                rt.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t * t);
            }
            yield return null;
        }

        // hold
        yield return new WaitForSeconds(0.25f);

        // fade out
        float outDuration = 0.5f;
        elapsed = 0f;
        while (elapsed < outDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / outDuration);
            float alpha = 1f - t;
            foreach (var go in claws)
            {
                if (go == null) continue;
                var img = go.GetComponent<Image>();
                if (img != null)
                {
                    var c = img.color;
                    img.color = new Color(c.r, c.g, c.b, alpha);
                }
                // fade glow children
                for (int c = 0; c < go.transform.childCount; c++)
                {
                    var childImg = go.transform.GetChild(c).GetComponent<Image>();
                    if (childImg != null)
                    {
                        var cc = childImg.color;
                        childImg.color = new Color(cc.r, cc.g, cc.b, alpha * 0.5f);
                    }
                }
            }
            yield return null;
        }

        foreach (var go in claws) { if (go != null) Destroy(go); }
    }

    // ==========================================
    //  防御特效：蓝色古罗马护盾
    // ==========================================

    IEnumerator ShieldRoutine(Vector2 position)
    {
        // Main shield
        var go = new GameObject("VFX_Shield");
        go.transform.SetParent(_particleLayer, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(360f, 440f);
        rt.localScale = new Vector3(1.2f, 1.2f, 1f);

        var img = go.AddComponent<Image>();
        img.sprite = _shieldSprite;
        img.color = new Color(0.4f, 0.6f, 1f, 0.8f);
        img.raycastTarget = false;

        // Roman cross (vertical bar)
        var vBarGo = new GameObject("CrossV");
        vBarGo.transform.SetParent(go.transform, false);
        var vBarRt = vBarGo.AddComponent<RectTransform>();
        vBarRt.anchoredPosition = Vector2.zero;
        vBarRt.sizeDelta = new Vector2(22f, 280f);
        var vBarImg = vBarGo.AddComponent<Image>();
        vBarImg.color = new Color(1f, 0.85f, 0.3f, 0.9f);
        vBarImg.raycastTarget = false;

        // Roman cross (horizontal bar)
        var hBarGo = new GameObject("CrossH");
        hBarGo.transform.SetParent(go.transform, false);
        var hBarRt = hBarGo.AddComponent<RectTransform>();
        hBarRt.anchoredPosition = new Vector2(0, 20f);
        hBarRt.sizeDelta = new Vector2(180f, 22f);
        var hBarImg = hBarGo.AddComponent<Image>();
        hBarImg.color = new Color(1f, 0.85f, 0.3f, 0.9f);
        hBarImg.raycastTarget = false;

        // Outer glow
        var glowGo = new GameObject("ShieldGlow");
        glowGo.transform.SetParent(_particleLayer, false);
        var glowRt = glowGo.AddComponent<RectTransform>();
        glowRt.anchoredPosition = position;
        glowRt.sizeDelta = new Vector2(520f, 520f);
        glowRt.SetAsFirstSibling();
        var glowImg = glowGo.AddComponent<Image>();
        glowImg.sprite = _glowSprite;
        glowImg.color = new Color(0.3f, 0.5f, 1f, 0.4f);
        glowImg.raycastTarget = false;

        // Bounce-in animation
        float inDuration = 0.2f;
        float elapsed = 0f;
        while (elapsed < inDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / inDuration);
            float scale = Mathf.Lerp(1.2f, 1f, EaseOutBack(t));
            rt.localScale = new Vector3(scale, scale, 1f);
            // pulse glow
            float glowAlpha = 0.4f * (1f - t * 0.3f);
            glowImg.color = new Color(0.3f, 0.5f, 1f, glowAlpha);
            yield return null;
        }
        rt.localScale = Vector3.one;

        // hold
        yield return new WaitForSeconds(0.8f);

        // fade out
        float outDuration = 0.5f;
        elapsed = 0f;
        while (elapsed < outDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / outDuration);
            float alpha = 1f - t;
            img.color = new Color(0.4f, 0.6f, 1f, 0.8f * alpha);
            vBarImg.color = new Color(1f, 0.85f, 0.3f, 0.9f * alpha);
            hBarImg.color = new Color(1f, 0.85f, 0.3f, 0.9f * alpha);
            glowImg.color = new Color(0.3f, 0.5f, 1f, 0.4f * alpha);
            yield return null;
        }

        if (go != null) Destroy(go);
        if (glowGo != null) Destroy(glowGo);
    }

    // ==========================================
    //  治疗特效：绿色十字架 + 绿光环
    // ==========================================

    IEnumerator HealRoutine(Vector2 position)
    {
        // Green glow halo (behind)
        var glowGo = new GameObject("VFX_HealGlow");
        glowGo.transform.SetParent(_particleLayer, false);
        var glowRt = glowGo.AddComponent<RectTransform>();
        glowRt.anchoredPosition = position;
        glowRt.sizeDelta = new Vector2(300f, 300f);
        var glowImg = glowGo.AddComponent<Image>();
        glowImg.sprite = _glowSprite;
        glowImg.color = new Color(0.1f, 1f, 0.3f, 0.35f);
        glowImg.raycastTarget = false;
        glowImg.SetNativeSize();

        // Green cross
        var crossGo = new GameObject("VFX_HealCross");
        crossGo.transform.SetParent(_particleLayer, false);
        var crossRt = crossGo.AddComponent<RectTransform>();
        crossRt.anchoredPosition = position;
        crossRt.localScale = Vector3.zero;

        // cross vertical bar
        var cvGo = new GameObject("CV");
        cvGo.transform.SetParent(crossGo.transform, false);
        var cvRt = cvGo.AddComponent<RectTransform>();
        cvRt.anchoredPosition = Vector2.zero;
        cvRt.sizeDelta = new Vector2(28f, 120f);
        var cvImg = cvGo.AddComponent<Image>();
        cvImg.color = new Color(0.2f, 1f, 0.3f, 1f);
        cvImg.raycastTarget = false;

        // cross horizontal bar
        var chGo = new GameObject("CH");
        chGo.transform.SetParent(crossGo.transform, false);
        var chRt = chGo.AddComponent<RectTransform>();
        chRt.anchoredPosition = new Vector2(0, 10f);
        chRt.sizeDelta = new Vector2(90f, 28f);
        var chImg = chGo.AddComponent<Image>();
        chImg.color = new Color(0.2f, 1f, 0.3f, 1f);
        chImg.raycastTarget = false;

        // cross glow (slightly larger, behind cross)
        var crossGlowGo = new GameObject("CrossGlow");
        crossGlowGo.transform.SetParent(crossGo.transform, false);
        crossGlowGo.transform.SetAsFirstSibling();
        var crossGlowRt = crossGlowGo.AddComponent<RectTransform>();
        crossGlowRt.anchoredPosition = Vector2.zero;
        crossGlowRt.sizeDelta = new Vector2(140f, 180f);
        var crossGlowImg = crossGlowGo.AddComponent<Image>();
        crossGlowImg.sprite = _glowSprite;
        crossGlowImg.color = new Color(0.3f, 1f, 0.4f, 0.4f);
        crossGlowImg.raycastTarget = false;
        crossGlowImg.SetNativeSize();

        // Sparkle particles
        var sparkles = new List<GameObject>();
        int sparkleCount = 8;
        for (int i = 0; i < sparkleCount; i++)
        {
            var spGo = new GameObject("Sparkle_" + i);
            spGo.transform.SetParent(_particleLayer, false);
            var spRt = spGo.AddComponent<RectTransform>();
            float angle = (float)i / sparkleCount * Mathf.PI * 2f;
            float radius = Random.Range(60f, 100f);
            spRt.anchoredPosition = position + new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
            spRt.sizeDelta = new Vector2(10f, 10f);
            var spImg = spGo.AddComponent<Image>();
            spImg.color = new Color(0.4f, 1f, 0.5f, 0.9f);
            spImg.raycastTarget = false;
            sparkles.Add(spGo);
        }

        // Bounce-in animation
        float inDuration = 0.25f;
        float elapsed = 0f;
        while (elapsed < inDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / inDuration);
            float scale = EaseOutBack(t);
            crossRt.localScale = new Vector3(scale, scale, 1f);
            // pulse glow
            float glowPulse = 0.35f + Mathf.Sin(t * Mathf.PI * 2f) * 0.1f;
            glowImg.color = new Color(0.1f, 1f, 0.3f, glowPulse);
            // sparkles float inward
            foreach (var spGo in sparkles)
            {
                if (spGo == null) continue;
                var spRt = spGo.GetComponent<RectTransform>();
                float spAngle = (float)sparkles.IndexOf(spGo) / sparkleCount * Mathf.PI * 2f;
                float spRadius = Mathf.Lerp(100f, 50f, t);
                spRt.anchoredPosition = position + new Vector2(
                    Mathf.Cos(spAngle) * spRadius,
                    Mathf.Sin(spAngle) * spRadius + t * 30f);
                spRt.localScale = Vector3.one * (1f - t * 0.5f);
            }
            yield return null;
        }
        crossRt.localScale = Vector3.one;

        // hold with gentle pulse
        float holdDuration = 0.6f;
        elapsed = 0f;
        while (elapsed < holdDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / holdDuration;
            float pulse = 0.35f + Mathf.Sin(t * Mathf.PI * 3f) * 0.08f;
            glowImg.color = new Color(0.1f, 1f, 0.3f, pulse);
            yield return null;
        }

        // fade out
        float outDuration = 0.6f;
        elapsed = 0f;
        while (elapsed < outDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / outDuration);
            float alpha = 1f - t;
            cvImg.color = new Color(0.2f, 1f, 0.3f, alpha);
            chImg.color = new Color(0.2f, 1f, 0.3f, alpha);
            crossGlowImg.color = new Color(0.3f, 1f, 0.4f, 0.4f * alpha);
            glowImg.color = new Color(0.1f, 1f, 0.3f, 0.35f * alpha);
            crossRt.localScale = Vector3.one * (1f + t * 0.3f);
            foreach (var spGo in sparkles)
            {
                if (spGo == null) continue;
                var spImg = spGo.GetComponent<Image>();
                if (spImg != null)
                {
                    var c = spImg.color;
                    spImg.color = new Color(c.r, c.g, c.b, alpha * 0.5f);
                }
                var spRt = spGo.GetComponent<RectTransform>();
                spRt.anchoredPosition += Vector2.up * (Time.deltaTime * 40f);
            }
            yield return null;
        }

        if (crossGo != null) Destroy(crossGo);
        if (glowGo != null) Destroy(glowGo);
        foreach (var spGo in sparkles) { if (spGo != null) Destroy(spGo); }
    }

    // ==========================================
    //  缓动函数
    // ==========================================

    float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}
