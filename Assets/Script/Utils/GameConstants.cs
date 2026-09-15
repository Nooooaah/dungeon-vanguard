/// <summary>
/// 全局常量 —— 全组共用，统一修改数值的地方
/// </summary>
public static class GameConstants
{
    // ===== 玩家 =====
    public const int PLAYER_MAX_HP = 60;// 玩家初始生命
    public const int PLAYER_MAX_ENERGY = 3; // 玩家初始能量
    public const int PLAYER_INITIAL_HAND = 5;    // 手牌初始数
    public const int PLAYER_DRAW_PER_TURN = 1; //每回合抽牌数
    public const int HAND_LIMIT = 10;       // 手牌上限
    public const int SHIELD_CAP = 99;        // 最大护盾

    // ===== 初始卡组 =====
    public const int INITIAL_DECK_BASIC = 5;    // 初始元素基础牌
    public const int INITIAL_DECK_ATTACK = 2;   // 通用攻击牌
    public const int INITIAL_DECK_DEFEND = 1;   // 通用防御牌

    // ===== 冒险 =====
    public const int TOTAL_FLOORS = 5;           // 总层数
    public const int NODES_PER_FLOOR_MIN = 3;    // 每层最小节点数
    public const int NODES_PER_FLOOR_MAX = 4;     // 每层最大节点数
    public const float REST_HEAL_PERCENT = 0.3f; // 回复点回复30%

    // ===== 场景名 =====
    public const string SCENE_MAIN_MENU = "MainMenu";
    public const string SCENE_ELEMENT_SELECTION = "ElementSelection";
    public const string SCENE_MAP = "MapScenes";
    public const string SCENE_BATTLE = "BattleScene";
    public const string SCENE_REWARD = "RewardScene";
    public const string SCENE_GAME_OVER = "GameOverScene";

    /// <summary>获取第 floor 层对应的地图场景名</summary>
    public static string GetMapSceneName(int floor) => $"MapScene_{System.Math.Max(1, floor)}";

    // ===== 存档 =====
    public const string SAVE_FILE_NAME = "save.json";
}

/// <summary>三种元素</summary>
public enum Element
{
    None=0,
    Fire,   // 火 — 高伤害
    Water,  // 水 — 治疗/控制
    Wind    // 风 — 抽牌/连击
}

/// <summary>卡牌类型</summary>
public enum CardType
{
    Attack, // 攻击牌：对敌人造成伤害
    Skill   // 技能牌：护盾/治疗/抽牌
}

/// <summary>Buff类型</summary>
public enum BuffType
{
    None,
    Burn,   // 灼烧：每回合扣血，持续N回合
    Freeze  // 冰冻：跳过下回合行动
}

/// <summary>伤害类型（用于跳字颜色）</summary>
public enum DamageType
{
    Damage,   // 红色
    Heal,     // 绿色
    Shield,   // 蓝色
    Critical  // 金色（暴击）
}

/// <summary>节点类型</summary>
public enum NodeType
{
    Battle,
    Elite,
    Event,
        Rest,
    Boss
}
