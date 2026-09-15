using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 地图节点按钮UI —— 挂载在节点按钮Prefab上
/// 组员C负责（或组员D协作）
/// </summary>
public class NodeButtonUI : MonoBehaviour
{
    public TMP_Text labelText;
    public Image bgImage;
    public Image iconImage;              // 节点类型图标
    public Sprite[] nodeTypeIcons;      // 按 NodeType 枚举顺序: Battle, Elite, Event, Rest, Boss
    public Button button;
    public GameObject completedMark;    // ✓ 已完成标记
    public GameObject lockedMark;       // 🔒 未解锁标记

    private MapNode _node;
    private System.Action<MapNode> _onClick;

    // 节点状态颜色
    private static readonly Color ColorUnlocked = new Color(0.2f, 0.6f, 0.86f);  // 蓝
    private static readonly Color ColorCompleted = new Color(0.3f, 0.3f, 0.3f);   // 灰
    private static readonly Color ColorLocked = new Color(0.15f, 0.15f, 0.15f);   // 暗
    private static readonly Color ColorBoss = new Color(0.91f, 0.30f, 0.24f);     // 红

    public void Init(MapNode node, System.Action<MapNode> onClick)
    {
        _node = node;
        _onClick = onClick;

        labelText?.SetText(node.GetDisplayName());

        // 设置节点类型图标
        if (iconImage != null && nodeTypeIcons != null && nodeTypeIcons.Length > (int)node.nodeType)
        {
            iconImage.sprite = nodeTypeIcons[(int)node.nodeType];
            iconImage.gameObject.SetActive(nodeTypeIcons[(int)node.nodeType] != null);
        }

        // 隐藏背景方框，用图标作为按钮点击目标
        if (bgImage != null) bgImage.enabled = false;
        if (button != null && iconImage != null)
        {
            button.targetGraphic = iconImage;
            iconImage.raycastTarget = true;
        }

        // 将完成标记从 LockMask 下移出，使其独立显示
        if (completedMark != null && lockedMark != null &&
            completedMark.transform.parent == lockedMark.transform)
        {
            completedMark.transform.SetParent(iconImage.transform, false);
        }
        // 永久隐藏 LockMask 暗色方框
        if (lockedMark != null) lockedMark.SetActive(false);

        RefreshState();

        button?.onClick.AddListener(() => _onClick?.Invoke(_node));
    }

    public void RefreshState()
    {
        if (_node == null) return;

        // 完成标记（已从 LockMask 下移出，可独立显示）
        if (completedMark != null) completedMark.SetActive(_node.isCompleted);

        // 用图标透明度区分状态，不再用暗色方框
        Color c;
        if (_node.isCompleted)
            c = new Color(0.3f, 0.3f, 0.3f, 0.4f);   // 已完成：淡灰
        else if (!_node.isUnlocked)
            c = new Color(0.15f, 0.15f, 0.15f, 0.35f); // 未解锁：暗淡
        else if (_node.nodeType == NodeType.Boss)
            c = ColorBoss;                              // Boss：红色
        else
            c = ColorUnlocked;                         // 可用：蓝色

        if (iconImage != null) iconImage.color = c;
        if (button != null) button.interactable = _node.isUnlocked && !_node.isCompleted;
    }
}
