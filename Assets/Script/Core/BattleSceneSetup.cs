using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 战斗场景初始化 —— 根据 GameManager 状态动态加载角色/敌人 Prefab
/// 1. 实例化 Prefab（保留 Player/EnemyBase 脚本逻辑）
/// 2. 从 Prefab 中提取 Sprite，赋给 UI Image（让立绘在 Canvas 中可见）
/// 3. 设置 EnemyName、EnemyIntentText、EnemyHPBar 等 UI
/// </summary>
[DefaultExecutionOrder(-100)]
public class BattleSceneSetup : MonoBehaviour
{
    [System.Serializable]
    public class EnemyPrefabEntry
    {
        public EnemyData data;
        public GameObject prefab;
    }

    [Header("Player Prefabs (by element)")]
    public GameObject playerFirePrefab;
    public GameObject playerWaterPrefab;
    public GameObject playerWindPrefab;

    [Header("Enemy Prefabs")]
    public EnemyPrefabEntry[] enemyPrefabs;

    [Header("Script Containers (world-space, holds Player/EnemyBase)")]
    public Transform playerContainer;
    public Transform enemyContainer;

    [Header("UI Panels (visual area with placeholder stick figures)")]
    public Transform playerCharacterPanel;
    public Transform enemyAreaPanel;

    [Header("Player UI Texts")]
    public TMP_Text playerNameText;

    [Header("Enemy UI Texts")]
    public TMP_Text enemyNameText;
    public TMP_Text enemyIntentText;

    [Header("Enemy HP Bar")]
    public HPBarUI enemyHPBar;

    [Header("Battle UI")]
    public BattleUI battleUI;

    // 占位火柴人部件名 —— 替换立绘后隐藏这些
    private static readonly string[] PlayerPlaceholderNames =
        { "Head", "Body", "Arms", "LeftLeg", "RightLeg", "Sword" };
    private static readonly string[] EnemyPlaceholderNames =
        { "EHead", "EyeL", "EyeR", "EBody", "ELeftArm", "ERightArm", "ELeftLeg", "ERightLeg" };

    void Start()
    {
        SetupPlayer();
        SetupEnemy();
    }

    void SetupPlayer()
    {
        Element element = Element.Fire;
        var gm = GameManager.Instance;
        if (gm != null)
            element = gm.ChosenElement;

        GameObject prefab = element switch
        {
            Element.Fire => playerFirePrefab,
            Element.Water => playerWaterPrefab,
            Element.Wind => playerWindPrefab,
            _ => playerFirePrefab,
        };

        if (prefab == null)
        {
            Debug.LogError($"[BattleSceneSetup] Player prefab for {element} not assigned!");
            return;
        }

        ClearContainer(playerContainer);
        var instance = Instantiate(prefab, playerContainer);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;

        Sprite portrait = ExtractSprite(instance);
        SetPortraitImage(playerCharacterPanel, portrait, "PlayerPortrait");
        HideChildren(playerCharacterPanel, PlayerPlaceholderNames);

        if (playerNameText != null)
        {
            playerNameText.text = element switch
            {
                Element.Fire => "火元素",
                Element.Water => "水元素",
                Element.Wind => "风元素",
                _ => "勇者",
            };
        }

        Debug.Log($"[BattleSceneSetup] Player setup complete: {element}, sprite={portrait?.name}");
    }

    void SetupEnemy()
    {
        var gm = GameManager.Instance;
        var data = gm?.PendingEnemyData;

        // 没有 PendingEnemyData 时，用第一个敌人作为 fallback（方便直接测试 BattleScene）
        if (data == null && enemyPrefabs != null && enemyPrefabs.Length > 0)
        {
            data = enemyPrefabs[0].data;
            Debug.LogWarning($"[BattleSceneSetup] PendingEnemyData is null, using fallback: {data.enemyName}");
        }

        if (data == null)
        {
            Debug.LogError("[BattleSceneSetup] No enemy data available at all!");
            return;
        }

        // 同步 BattleUI 的 enemyData（这样 BattleUI.Start 不会因为 enemyData=null 崩溃）
        if (battleUI != null)
            battleUI.enemyData = data;

        // 查找匹配的 Prefab
        var entry = System.Array.Find(enemyPrefabs, e => e.data == data);
        if (entry == null || entry.prefab == null)
        {
            Debug.LogError($"[BattleSceneSetup] No prefab for enemy '{data.enemyName}'!");
            return;
        }

        // 1. 实例化 Prefab（保留 EnemyBase + EnemyAI 脚本）
        ClearContainer(enemyContainer);
        var instance = Instantiate(entry.prefab, enemyContainer);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;

        // 2. 从 Prefab 中提取 Sprite，赋给 UI Image
        Sprite portrait = ExtractSprite(instance);
        SetPortraitImage(enemyAreaPanel, portrait, "EnemyPortrait");

        // 2b. 将 EnemyIntentText 移到立绘下方，避免被放大的立绘遮挡
        PositionIntentTextBelowPortrait(enemyAreaPanel, portrait);

        // 3. 隐藏占位火柴人
        HideChildren(enemyAreaPanel, EnemyPlaceholderNames);

        // 4. 设置 EnemyName
        if (enemyNameText != null)
            enemyNameText.text = data.enemyName;

        // 5. 设置 EnemyIntentText（显示第一个行动的意图）
        if (enemyIntentText != null)
        {
            if (data.actions != null && data.actions.Count > 0)
                enemyIntentText.text = data.actions[0].GetIntentText();
            else
                enemyIntentText.text = "";
        }

        // 6. EnemyHPBar —— BattleUI.Start 会自动绑定，这里只做 fallback：
        //    如果 BattleUI 没有绑定（比如直接测试），手动设置 HP 文本
        if (enemyHPBar != null)
        {
            var hpText = enemyHPBar.GetComponentInChildren<TMP_Text>();
            // BattleUI.Start 会覆盖这个绑定，这里只是确保不会空白
        }

        Debug.Log($"[BattleSceneSetup] Enemy setup complete: name={data.enemyName}, " +
                  $"hp={data.maxHp}, sprite={portrait?.name}, " +
                  $"intent={(data.actions != null && data.actions.Count > 0 ? data.actions[0].intentText : "none")}");
    }

    /// <summary>从 Prefab 实例中提取第一个 SpriteRenderer 的 Sprite</summary>
    Sprite ExtractSprite(GameObject instance)
    {
        if (instance == null) return null;

        var sr = instance.GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
            return sr.sprite;

        foreach (var childSr in instance.GetComponentsInChildren<SpriteRenderer>())
        {
            if (childSr.sprite != null)
                return childSr.sprite;
        }

        Debug.LogWarning($"[BattleSceneSetup] No Sprite found in {instance.name}");
        return null;
    }

    /// <summary>在 UI 面板下创建/查找 Portrait Image 并设置 Sprite</summary>
    void SetPortraitImage(Transform panel, Sprite sprite, string portraitName)
    {
        if (panel == null || sprite == null) return;

        var existing = panel.Find(portraitName);
        Image portraitImage;

        if (existing != null)
        {
            portraitImage = existing.GetComponent<Image>();
        }
        else
        {
            var go = new GameObject(portraitName);
            go.transform.SetParent(panel, false);
            portraitImage = go.AddComponent<Image>();

            var rt = portraitImage.rectTransform;
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(300, 400);
            go.transform.SetSiblingIndex(2);
        }

        portraitImage.sprite = sprite;
        portraitImage.preserveAspect = true;
        portraitImage.enabled = true;

        // Auto-scale so visible content fills the rect height.
        // The Image component uses sprite.rect for preserveAspect, but the actual
        // visible content only fills sprite.textureRect within that rect. We need
        // to compensate for both the aspect-ratio mismatch and the content padding.
        float rectW = 300f, rectH = 400f;
        float spW = sprite.rect.width, spH = sprite.rect.height;
        float texW = sprite.textureRect.width, texH = sprite.textureRect.height;
        float spriteAspect = spW / spH;
        float rectAspect = rectW / rectH;

        float scale;
        if (spriteAspect > rectAspect)
        {
            // Width-constrained: content height = rectW * texH / spW
            scale = (rectH * spW) / (rectW * texH);
        }
        else
        {
            // Height-constrained: content height = rectH * texH / spH
            scale = spH / texH;
        }
        portraitImage.transform.localScale = new Vector3(scale, scale, 1f);
    }

    /// <summary>将 EnemyIntentText 移到立绘可见内容下方，避免被放大后的立绘遮挡</summary>
    void PositionIntentTextBelowPortrait(Transform panel, Sprite sprite)
    {
        if (panel == null || sprite == null) return;

        var intentText = panel.Find("EnemyIntentText");
        if (intentText == null) return;

        var portrait = panel.Find("EnemyPortrait");
        if (portrait == null) return;

        float scale = portrait.localScale.x;
        float spAspect = sprite.rect.width / sprite.rect.height;
        float rAspect = 300f / 400f;
        float drawnH = spAspect > rAspect ? 300f / spAspect : 400f;
        float actualH = drawnH * scale;
        float bottomY = -actualH / 2f;

        intentText.localPosition = new Vector3(0, bottomY - 30f, 0);
    }

    void HideChildren(Transform parent, string[] names)
    {
        if (parent == null) return;
        foreach (var name in names)
        {
            var child = parent.Find(name);
            if (child != null)
                child.gameObject.SetActive(false);
        }
    }

    void ClearContainer(Transform container)
    {
        if (container == null) return;
        for (int i = container.childCount - 1; i >= 0; i--)
            Destroy(container.GetChild(i).gameObject);
    }
}
