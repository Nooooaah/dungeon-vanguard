using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 地图生成器 负责生成每层节点
/// </summary>
public class MapGenerator
{
    /// <summary>生成单层节点（用于每层独立场景）</summary>
    public List<MapNode> GenerateSingleFloor(int floor)
    {
        int nodeCount = Random.Range(GameConstants.NODES_PER_FLOOR_MIN,
                                     GameConstants.NODES_PER_FLOOR_MAX + 1);
        if (floor == GameConstants.TOTAL_FLOORS)
            nodeCount = 1;

        var nodes = GenerateFloor(floor, nodeCount);

        for (int i = 0; i < nodes.Count; i++)
            nodes[i].isUnlocked = (i == 0);

        return nodes;
    }

    /// <summary>生成完整地图（5层，每层3-4个节点）</summary>
    public List<List<MapNode>> GenerateMap()
    {
        List<List<MapNode>> map = new List<List<MapNode>>();

        for (int floor = 1; floor <= GameConstants.TOTAL_FLOORS; floor++)
        {
            int nodeCount = Random.Range(GameConstants.NODES_PER_FLOOR_MIN,
                                         GameConstants.NODES_PER_FLOOR_MAX + 1);
            List<MapNode> floorNodes = GenerateFloor(floor, nodeCount);

            for (int i = 0; i < floorNodes.Count; i++)
                floorNodes[i].isUnlocked = (floor == 1 && i == 0);

            map.Add(floorNodes);
        }

        return map;
    }

    /// <summary>生成单层节点</summary>
    private List<MapNode> GenerateFloor(int floor, int count)
    {
        List<MapNode> nodes = new List<MapNode>();

        List<NodeType> types = DetermineFloorTypes(floor, count);

        for (int i = 0; i < types.Count; i++)
        {
            var node = new MapNode
            {
                floor = floor,
                nodeType = types[i],
                isCompleted = false
            };

            if (types[i] == NodeType.Battle)
                node.enemyData = GetRandomNormalEnemy();
            else if (types[i] == NodeType.Elite)
                node.enemyData = GetEliteEnemy();
            else if (types[i] == NodeType.Boss)
                node.enemyData = GetBossEnemy();

            nodes.Add(node);
        }

        return nodes;
    }

    /// <summary>决定该层每节点类型</summary>
    private List<NodeType> DetermineFloorTypes(int floor, int count)
    {
        List<NodeType> types = new List<NodeType>();

        switch (floor)
        {
            case 1:
            case 2:
                for (int i = 0; i < count; i++)
                {
                    float roll = Random.value;
                    types.Add(roll < 0.3f ? NodeType.Event : NodeType.Battle);
                }
                if (!types.Contains(NodeType.Battle))
                    types[0] = NodeType.Battle;
                break;

            case 3:
                types.Add(NodeType.Elite);
                for (int i = 1; i < count; i++)
                    types.Add(Random.value < 0.3f ? NodeType.Event : NodeType.Battle);
                break;

            case 4:
                types.Add(NodeType.Rest);
                for (int i = 1; i < count; i++)
                    types.Add(Random.value < 0.3f ? NodeType.Event : NodeType.Battle);
                break;

            case 5:
                types.Add(NodeType.Boss);
                break;
        }

        for (int i = types.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (types[i], types[j]) = (types[j], types[i]);
        }

        return types;
    }

    // ===== 敌人数据获取（从Resources/Data目录加载）=====

    private EnemyData GetRandomNormalEnemy()
    {
        var all = Resources.LoadAll<EnemyData>("Enemies");
        var normals = new List<EnemyData>();
        foreach (var e in all)
            if (e.type == EnemyType.Normal)
                normals.Add(e);
        return normals.Count > 0 ? normals[Random.Range(0, normals.Count)] : null;
    }

    private EnemyData GetEliteEnemy()
    {
        var all = Resources.LoadAll<EnemyData>("Enemies");
        foreach (var e in all)
            if (e.type == EnemyType.Elite)
                return e;
        return null;
    }

    private EnemyData GetBossEnemy()
    {
        var all = Resources.LoadAll<EnemyData>("Enemies");
        foreach (var e in all)
            if (e.type == EnemyType.Boss)
                return e;
        return null;
    }
}
