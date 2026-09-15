/// <summary>
/// 地图节点数据
/// 组员C负责，Scripts/Map/MapNode.cs
/// </summary>
[System.Serializable]
public class MapNode
{
    public NodeType nodeType;       // 战斗 / 精英 / 事件 / 回复 / Boss
    public int floor;               // 所在层数 (1-5)
    public bool isCompleted;        // 是否已完成
    public bool isUnlocked;         // 是否解锁（只有当前层的节点可点击）

    // 战斗/精英/Boss节点：指定敌人数据
    public EnemyData enemyData;     // 对应的敌人配置

    public string GetDisplayName() => nodeType switch
    {
        NodeType.Battle => "战斗",
        NodeType.Elite => "精英",
        NodeType.Event => "事件",
        NodeType.Rest => "回复",
        NodeType.Boss => "Boss",
        _ => "?"
    };
}
