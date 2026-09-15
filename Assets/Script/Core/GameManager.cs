using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    // ===== 单例 =====
    public static GameManager Instance { get; private set; }

    // ===== 子系统引用（BattleManager在BattleScene中，这里存引用） =====
    public BattleManager BattleManager { get; set; }

    // ===== 游戏状态 =====
    public Element ChosenElement { get; set; }        // 玩家选择的元素
    public int CurrentFloor { get; set; } = 0;        // 当前层数
    public bool IsGameRunning { get; private set; }   // 是否在游戏中

    // ===== 玩家持久数据（跨战斗保留） =====
    public int PlayerMaxHp { get; set; } = GameConstants.PLAYER_MAX_HP;
    public int PlayerCurrentHp { get; set; } = GameConstants.PLAYER_MAX_HP;
    public int NextBattleStartShield { get; set; } = 0;

    // ===== GameOverScene 显示模式 =====
    public bool LastGameResultIsVictory { get; private set; }  // true=通关胜利, false=战斗失败
    public int TotalKills { get; set; } = 0;     // 本局击败敌人数
    public int TotalGold { get; set; } = 0;      // 本局获得金币总数

    // 战斗场景需要的数据（MapController 设置，BattleScene 读取）
    public EnemyData PendingEnemyData { get; set; }
    public MapNode PendingNode { get; set; }

    // 卡组数据统一由 GameManager._playerDeck 管理

    // ===== 玩家持久牌组 =====
    private List<CardData> _playerDeck = new List<CardData>();
    public List<CardData> PlayerDeck => _playerDeck;
    public bool HasPersistentDeck => _playerDeck.Count > 0;

    public void AddCardToDeck(CardData card)
    {
        if (card != null)
            _playerDeck.Add(card);
    }

    public void RemoveCardFromDeck(CardData card)
    {
        if (card != null)
            _playerDeck.Remove(card);
    }

    public void SetInitialDeck(List<CardData> deck)
    {
        _playerDeck = new List<CardData>(deck);
    }

    public List<CardData> GetPlayerDeck()
    {
        return new List<CardData>(_playerDeck);
    }

    public void ClearDeck()
    {
        _playerDeck.Clear();
    }

    void Awake()
    {
        // 单例模式
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        //自动管理
        gameObject.AddComponent<PoolManager>();
    }

    void Start()
    {
        Debug.Log("[GameManager] 游戏管理器初始化完成");
    }

    // ===== 场景管理 =====

    /// <summary>加载场景</summary>
    public void LoadScene(string sceneName)
    {
        PlayerState.Instance?.SavePlayerState();
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>开始新游戏</summary>
    public void StartNewGame(Element element)
    {
        ChosenElement = element;
        CurrentFloor = 1;
        ClearDeck();
        TotalKills = 0;
        TotalGold = 0;
        PlayerCurrentHp = PlayerMaxHp;
        IsGameRunning = true;
        MapController.ClearSavedMap();

        Debug.Log($"[GameManager] 新游戏开始！选择元素：{element}");
        LoadScene(GameConstants.GetMapSceneName(CurrentFloor));
    }

    /// <summary>继续游戏（从存档读取）</summary>
    public void ContinueGame()
    {
        // TODO: 从JSON读档恢复状态
        IsGameRunning = true;
        LoadScene(GameConstants.GetMapSceneName(CurrentFloor));
    }

    /// <summary>进入战斗场景</summary>
    public void EnterBattle()
    {
        LoadScene(GameConstants.SCENE_BATTLE);
    }

    /// <summary>战斗胜利 → 进入奖励场景</summary>
    public void OnBattleWin()
    {
        LoadScene(GameConstants.SCENE_REWARD);
    }

    /// <summary>奖励选完 → 返回当前层地图</summary>
    public void OnRewardComplete()
    {
        int floor = CurrentFloor > 0 ? CurrentFloor : 1;
        LoadScene(GameConstants.GetMapSceneName(floor));
    }

    /// <summary>通关 → 显示胜利画面（复用 GameOverScene，由 GameOverUI 判断胜利/失败）</summary>
    public void OnGameClear()
    {
        IsGameRunning = false;
        LastGameResultIsVictory = true;
        Debug.Log("🎉 恭喜通关！");
        LoadScene(GameConstants.SCENE_GAME_OVER);
    }

    /// <summary>游戏结束→进入 GameOverScene</summary>
    public void OnGameOver()
    {
        IsGameRunning = false;
        LastGameResultIsVictory = false;
        Debug.Log("💀 游戏结束");
        LoadScene(GameConstants.SCENE_GAME_OVER);
    }

    /// <summary>GameOver 重新开始（重新打本场战斗）</summary>
    public void OnRestartFromGameOver()
    {
        PlayerCurrentHp = PlayerMaxHp;
        NextBattleStartShield = 0;
        LoadScene(GameConstants.SCENE_BATTLE);
    }

    /// <summary>返回主菜单</summary>
    public void ReturnToMainMenu()
    {
        IsGameRunning = false;
        LoadScene(GameConstants.SCENE_MAIN_MENU);
    }

    /// <summary>重置游戏状态</summary>
    private void ResetGameState()
    {
        ChosenElement = Element.None;
        CurrentFloor = 0;
        PlayerCurrentHp = PlayerMaxHp;
        NextBattleStartShield = 0;
        PendingEnemyData = null;
        PendingNode = null;
        MapController.ClearSavedMap();
    }

    /// <summary>退出游戏</summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
