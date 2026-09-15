using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Represents a single node on the map.
/// </summary>
public enum MapNodeType
{
    Battle,
    Elite,
    Rest,
    Shop,
    Event,
    Boss
}

/// <summary>
/// Map UI: floor title, nodes arranged vertically, completion markers, click to advance.
/// </summary>
public class MapUI : MonoBehaviour
{
    [Header("Header")]
    public TMP_Text floorTitleText;

    [Header("Map Content")]
    public Transform mapContent;
    public GameObject mapNodePrefab;

    [Header("Navigation")]
    public Button backButton;

    [Header("Node Visuals")]
    public Sprite battleSprite;
    public Sprite eliteSprite;
    public Sprite restSprite;
    public Sprite shopSprite;
    public Sprite eventSprite;
    public Sprite bossSprite;

    public System.Action<int> OnNodeSelected;

    private int _currentFloor = 1;
    private int _currentNodeIndex = -1;
    private List<MapNodeData> _nodes = new List<MapNodeData>();
    private List<Image> _nodeImages = new List<Image>();

    public struct MapNodeData
    {
        public int index;
        public MapNodeType type;
        public string label;
        public bool completed;
        public bool available;
    }

    void Start()
    {
        // 从 GameManager 获取当前楼层
        if (GameManager.Instance != null)
            _currentFloor = GameManager.Instance.CurrentFloor;

        if (floorTitleText != null)
            floorTitleText.text = "第 " + _currentFloor + " 层";

        // 绑定场景中预放置的节点按钮
        if (mapContent != null)
        {
            foreach (Transform child in mapContent)
            {
                if (!child.name.StartsWith("Node_"))
                    continue;

                var btn = child.GetComponent<Button>();
                if (btn == null)
                    continue;

                // 已完成的节点（有 Checkmark 子物体）不可点击
                bool completed = child.Find("Checkmark") != null;
                btn.interactable = !completed;

                if (!completed)
                    btn.onClick.AddListener(OnNodeClicked);
            }
        }

        // 返回按钮 → 返回主菜单
        if (backButton != null)
            backButton.onClick.AddListener(OnBack);
    }

    void OnNodeClicked()
    {
        Debug.Log("[MapUI] Node clicked → EnterBattle");

        if (OnNodeSelected != null)
        {
            OnNodeSelected.Invoke(0);
            return;
        }

        if (GameManager.Instance != null)
            GameManager.Instance.EnterBattle();
        else
        {
            Debug.LogError("[MapUI] GameManager.Instance is null!");
            SceneManager.LoadScene(GameConstants.SCENE_MAIN_MENU);
        }
    }

    void OnBack()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ReturnToMainMenu();
        else
            SceneManager.LoadScene(GameConstants.SCENE_MAIN_MENU);
    }

    /// <summary>
    /// Populates the map with the given node data.
    /// </summary>
    public void SetMapData(int floor, List<MapNodeData> nodes, int currentNodeIndex)
    {
        _currentFloor = floor;
        _currentNodeIndex = currentNodeIndex;
        _nodes = nodes;

        if (floorTitleText != null)
            floorTitleText.text = "第 " + floor + " 层";

        // Clear existing
        if (mapContent != null)
        {
            for (int i = mapContent.childCount - 1; i >= 0; i--)
                Destroy(mapContent.GetChild(i).gameObject);
        }
        _nodeImages.Clear();

        // Build nodes bottom-to-top (boss at top)
        for (int i = nodes.Count - 1; i >= 0; i--)
        {
            var node = nodes[i];
            var go = new GameObject("Node_" + i);
            go.transform.SetParent(mapContent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(80, 80);

            var img = go.AddComponent<Image>();
            img.sprite = GetNodeSprite(node.type);
            img.raycastTarget = true;

            // Node color based on state
            if (node.completed)
                img.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            else if (node.available)
                img.color = GetNodeTypeColor(node.type);
            else
                img.color = new Color(0.4f, 0.4f, 0.4f, 0.5f);

            // Node label
            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(go.transform, false);
            var labelRt = labelGo.AddComponent<RectTransform>();
            labelRt.anchoredPosition = new Vector2(0, -50);
            labelRt.sizeDelta = new Vector2(100, 24);
            var labelText = labelGo.AddComponent<TextMeshProUGUI>();
            labelText.fontSize = 14;
            labelText.color = Color.white;
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.text = node.label;

            // Completed checkmark
            if (node.completed)
            {
                var checkGo = new GameObject("Checkmark");
                checkGo.transform.SetParent(go.transform, false);
                var checkRt = checkGo.AddComponent<RectTransform>();
                checkRt.sizeDelta = new Vector2(30, 30);
                checkRt.anchoredPosition = new Vector2(25, 25);
                var checkImg = checkGo.AddComponent<Image>();
                checkImg.color = new Color(0.2f, 0.8f, 0.2f, 1f);
            }

            var button = go.AddComponent<Button>();
            button.interactable = node.available;
            int capturedIndex = i;
            button.onClick.AddListener(() => SelectNode(capturedIndex));

            _nodeImages.Add(img);
        }

        // Draw connection lines between nodes
        DrawConnections();
    }

    void SelectNode(int index)
    {
        Debug.Log("[Map] Selected node " + index);
        OnNodeSelected?.Invoke(index);
    }

    Sprite GetNodeSprite(MapNodeType type)
    {
        switch (type)
        {
            case MapNodeType.Battle: return battleSprite;
            case MapNodeType.Elite:  return eliteSprite;
            case MapNodeType.Rest:   return restSprite;
            case MapNodeType.Shop:   return shopSprite;
            case MapNodeType.Event:  return eventSprite;
            case MapNodeType.Boss:   return bossSprite;
            default: return null;
        }
    }

    Color GetNodeTypeColor(MapNodeType type)
    {
        switch (type)
        {
            case MapNodeType.Battle: return new Color(0.8f, 0.2f, 0.2f);
            case MapNodeType.Elite:  return new Color(0.8f, 0.4f, 0.1f);
            case MapNodeType.Rest:   return new Color(0.2f, 0.7f, 0.3f);
            case MapNodeType.Shop:   return new Color(0.1f, 0.5f, 0.8f);
            case MapNodeType.Event: return new Color(0.6f, 0.4f, 0.8f);
            case MapNodeType.Boss:   return new Color(0.9f, 0.1f, 0.1f);
            default: return Color.white;
        }
    }

    void DrawConnections()
    {
        // Simple vertical line between consecutive nodes
        // Full path rendering would require a UI line renderer
    }
}
