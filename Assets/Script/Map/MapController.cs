using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 地图控制器 负责 地图界面交互 + 场景切换
/// 每个地图场景挂载一个 MapController，只显示当前层的节点
/// </summary>
public class MapController : MonoBehaviour
{
    [Header("UI 引用")]
    public Transform floorContainer;           // 垂直容器
    public GameObject floorLabelPrefab;        // "第 X 层" 标签预制件
    public GameObject nodeButtonPrefab;        // 节点按钮预制体

    [Header("事件系统")]
    public EventPopupUI eventPopup;            // 事件弹窗引用
<<<<<<< HEAD
=======
    public CardRemovalUI cardRemovalUI;        // 移除卡牌界面引用
>>>>>>> dd15dc953bda76878a619e06d67d3f5c5cb965a7

    // ===== 运行时数据 =====
    private List<MapNode> _floor;              // 当前层的节点列表
    private MapGenerator _generator = new MapGenerator();
<<<<<<< HEAD
=======
    private TextMeshProUGUI _floorTitle;      // 顶部固定层标题
>>>>>>> dd15dc953bda76878a619e06d67d3f5c5cb965a7

    // 跨场景保存的当前层地图数据（static 不会被场景销毁回收）
    private static List<MapNode> _savedFloor;

    void Start()
    {
        if (_savedFloor != null)
        {
            // 从战斗/事件场景返回，恢复地图数据
            _floor = _savedFloor;
            var gm = GameManager.Instance;
            if (gm != null && gm.PendingNode != null)
            {
                OnBattleVictory(gm.PendingNode);
                gm.PendingNode = null;
            }
            else
            {
                RefreshDisplay();
            }
        }
        else
        {
            GenerateAndDisplay();
        }
    }

    /// <summary>新游戏开始时清除旧地图数据</summary>
    public static void ClearSavedMap()
    {
        _savedFloor = null;
    }

    /// <summary>生成当前层地图并刷新UI</summary>
    public void GenerateAndDisplay()
    {
        var gm = GameManager.Instance;
        int floor = (gm != null && gm.CurrentFloor > 0) ? gm.CurrentFloor : 1;
        _floor = _generator.GenerateSingleFloor(floor);
        _savedFloor = _floor;
        RefreshDisplay();
    }

    /// <summary>刷新地图显示</summary>
    public void RefreshDisplay()
    {
        // 清空旧UI
        foreach (Transform child in floorContainer)
            Destroy(child.gameObject);

        var gm = GameManager.Instance;
        int floor = (gm != null && gm.CurrentFloor > 0) ? gm.CurrentFloor : 1;

<<<<<<< HEAD
=======
        // 顶部固定层标题（不随滚动移动）
        CreateOrUpdateFloorTitle(floor);

>>>>>>> dd15dc953bda76878a619e06d67d3f5c5cb965a7
        var label = Instantiate(floorLabelPrefab, floorContainer);
        label.name = $"Floor_{floor}";

        // 隐藏层背景方框
        var floorImg = label.GetComponent<Image>();
        if (floorImg != null) floorImg.enabled = false;

        // 禁用布局组，手动定位节点以实现错开效果
        var hlg = label.GetComponent<HorizontalLayoutGroup>();
        if (hlg != null) hlg.enabled = false;
        var csf = label.GetComponent<ContentSizeFitter>();
        if (csf != null) csf.enabled = false;

        var labelRT = (RectTransform)label.transform;
        labelRT.pivot = new Vector2(0.5f, 0.5f);

        // 判断是否为 Boss 层（仅一个 Boss 节点）
        bool isBossFloor = _floor.Count == 1 && _floor[0].nodeType == NodeType.Boss;

        // 禁用 Content 的布局组件，手动控制标签位置
        var contentVLG = floorContainer.GetComponent<VerticalLayoutGroup>();
        var contentCSF = floorContainer.GetComponent<ContentSizeFitter>();
        if (contentVLG != null) contentVLG.enabled = false;
        if (contentCSF != null) contentCSF.enabled = false;

        // Content 覆盖整个 Viewport（高度 700）
        var contentRT = (RectTransform)floorContainer;
        contentRT.sizeDelta = new Vector2(0, 700);

        // 所有层：标签中心对齐 Viewport 中心，节点以 y=0（地图纵向居中）为基准上下错开
        labelRT.anchorMin = new Vector2(0.5f, 0.5f);
        labelRT.anchorMax = new Vector2(0.5f, 0.5f);
        labelRT.sizeDelta = new Vector2(700, 700);
        labelRT.anchoredPosition = new Vector2(0, 0);

<<<<<<< HEAD
        // 层标题：顶部居中
        var labelText = label.GetComponentInChildren<TMP_Text>();
        if (labelText != null)
        {
            labelText.SetText($"第 {floor} 层");
            labelText.alignment = TMPro.TextAlignmentOptions.Center;
            var titleRT = (RectTransform)labelText.transform;
            titleRT.anchorMin = new Vector2(0.5f, 1f);
            titleRT.anchorMax = new Vector2(0.5f, 1f);
            titleRT.anchoredPosition = new Vector2(0, -30);
            titleRT.sizeDelta = new Vector2(200, 40);
        }
=======
        // 隐藏预制件自带的层标题文本（改用顶部固定的 FloorTitle）
        var labelText = label.GetComponentInChildren<TMP_Text>();
        if (labelText != null)
            labelText.gameObject.SetActive(false);
>>>>>>> dd15dc953bda76878a619e06d67d3f5c5cb965a7

        int nodeCount = _floor.Count;
        float spacing = 220f;
        float startX = -(nodeCount - 1) * spacing * 0.5f;
        float defaultNodeSize = 120f;

        for (int i = 0; i < _floor.Count; i++)
        {
            var node = _floor[i];
            var btn = Instantiate(nodeButtonPrefab, label.transform);
            var btnUI = btn.GetComponent<NodeButtonUI>();
            if (btnUI != null)
                btnUI.Init(node, OnNodeClicked);

            var btnRT = (RectTransform)btn.transform;

            // Boss 节点：更大、居中
            if (node.nodeType == NodeType.Boss)
            {
                btnRT.sizeDelta = new Vector2(220, 220);
                if (btnUI != null && btnUI.iconImage != null)
                {
                    var iconRT = (RectTransform)btnUI.iconImage.transform;
                    iconRT.sizeDelta = new Vector2(-10, -25);
                }
                btnRT.anchoredPosition = new Vector2(0, 0);
                continue;
            }

            // 普通节点放大
            btnRT.sizeDelta = new Vector2(defaultNodeSize, defaultNodeSize);
            if (btnUI != null && btnUI.iconImage != null)
            {
                var iconRT = (RectTransform)btnUI.iconImage.transform;
<<<<<<< HEAD
                iconRT.sizeDelta = new Vector2(-10, -25);
=======
                // Event nodes: slightly larger icon
                if (node.nodeType == NodeType.Event)
                    iconRT.sizeDelta = new Vector2(5, -10);
                else
                    iconRT.sizeDelta = new Vector2(-10, -25);
>>>>>>> dd15dc953bda76878a619e06d67d3f5c5cb965a7
            }

            float xOffset = startX + i * spacing;
            float yOffset = (i % 2 == 0) ? 40f : -40f;
            btnRT.anchoredPosition = new Vector2(xOffset, yOffset);
        }
    }

<<<<<<< HEAD
=======
    /// <summary>创建或更新场景顶部居中的层标题（加粗）</summary>
    private void CreateOrUpdateFloorTitle(int floor)
    {
        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;

        if (_floorTitle == null)
        {
            var go = new GameObject("FloorTitle", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(canvas.transform, false);
            go.transform.SetAsLastSibling();

            _floorTitle = go.GetComponent<TextMeshProUGUI>();

            var font = Resources.Load<TMP_FontAsset>("msyhl SDF");
#if UNITY_EDITOR
            if (font == null)
                font = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/msyhl SDF.asset");
#endif
            if (font == null)
                font = TMP_Settings.defaultFontAsset;
            _floorTitle.font = font;

            _floorTitle.fontSize = 52;
            _floorTitle.fontStyle = FontStyles.Bold;
            _floorTitle.alignment = TextAlignmentOptions.Center;
            _floorTitle.color = new Color(1f, 0.93f, 0.65f, 1f);

            var outline = go.AddComponent<UnityEngine.UI.Outline>();
            outline.effectColor = new Color(0.08f, 0.04f, 0f, 0.85f);
            outline.effectDistance = new Vector2(2.5f, -2.5f);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0, -20);
            rt.sizeDelta = new Vector2(600, 80);
        }

        _floorTitle.SetText($"第 {floor} 层");
    }

>>>>>>> dd15dc953bda76878a619e06d67d3f5c5cb965a7
    /// <summary>节点点击回调</summary>
    private void OnNodeClicked(MapNode node)
    {
        if (!node.isUnlocked || node.isCompleted) return;

        switch (node.nodeType)
        {
            case NodeType.Battle:
            case NodeType.Elite:
            case NodeType.Boss:
                EnterBattle(node);
                break;

            case NodeType.Event:
                TriggerEvent(node);
                break;

            case NodeType.Rest:
                TriggerRest(node);
                break;
        }
    }

    // ==========================================
    //  进入战斗
    // ==========================================

    private void EnterBattle(MapNode node)
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        gm.PendingEnemyData = node.enemyData;
        gm.PendingNode = node;

        gm.EnterBattle();
    }

    // ==========================================
    //  事件
    // ==========================================

    private void TriggerEvent(MapNode node)
    {
        var eventData = EventManager.GetRandomEvent();

<<<<<<< HEAD
=======
        EnsureCardRemovalUI();

>>>>>>> dd15dc953bda76878a619e06d67d3f5c5cb965a7
        if (eventPopup != null)
        {
            eventPopup.Show(eventData, (choice) =>
            {
                EventManager.ExecuteChoice(choice);
<<<<<<< HEAD
                CompleteNode(node);
=======

                if (EventManager.PendingCardRemoval && cardRemovalUI != null)
                {
                    EventManager.PendingCardRemoval = false;
                    cardRemovalUI.Show(() => CompleteNode(node));
                }
                else
                {
                    CompleteNode(node);
                }
>>>>>>> dd15dc953bda76878a619e06d67d3f5c5cb965a7
            });
        }
    }

<<<<<<< HEAD
=======
    private void EnsureCardRemovalUI()
    {
        if (cardRemovalUI != null) return;
        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;
        var go = new GameObject("CardRemovalUI", typeof(RectTransform));
        go.transform.SetParent(canvas.transform, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        cardRemovalUI = go.AddComponent<CardRemovalUI>();
    }

>>>>>>> dd15dc953bda76878a619e06d67d3f5c5cb965a7
    // ==========================================
    //  回复点
    // ==========================================

    private void TriggerRest(MapNode node)
    {
        var player = GameManager.Instance;
        if (player == null) return;

        int healAmount = Mathf.RoundToInt(player.PlayerMaxHp * GameConstants.REST_HEAL_PERCENT);
        player.PlayerCurrentHp = Mathf.Min(player.PlayerCurrentHp + healAmount, player.PlayerMaxHp);

        Debug.Log($"[Map] 回复 {healAmount} HP，当前HP={player.PlayerCurrentHp}");
        CompleteNode(node);
    }

    // ==========================================
    //  节点完成 → 解锁同层下一节点 / 进入下一层
    // ==========================================

    private void CompleteNode(MapNode node)
    {
        node.isCompleted = true;

        int nodeIndex = _floor.IndexOf(node);

        // 还有下一个节点 → 解锁它
        if (nodeIndex + 1 < _floor.Count)
        {
            _floor[nodeIndex + 1].isUnlocked = true;
            _savedFloor = _floor;
            RefreshDisplay();
        }
        // 当前层最后一个节点完成 → 进入下一层场景
        else
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            _savedFloor = null;

            if (gm.CurrentFloor >= GameConstants.TOTAL_FLOORS)
            {
                gm.OnGameClear();
            }
            else
            {
                gm.CurrentFloor++;
                Debug.Log($"[Map] 进入第 {gm.CurrentFloor} 层");
                gm.LoadScene(GameConstants.GetMapSceneName(gm.CurrentFloor));
            }
        }
    }

    /// <summary>战斗胜利后由 MapController.Start 调用，标记节点完成</summary>
    public void OnBattleVictory(MapNode node)
    {
        if (node != null)
        {
            CompleteNode(node);
        }
    }
}
