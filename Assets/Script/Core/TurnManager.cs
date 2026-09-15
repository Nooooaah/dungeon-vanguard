using System;

/// <summary>
/// 回合状态枚举
/// </summary>
public enum TurnState
{
    BattleStart,    // 战斗开始（抽初始手牌）
    PlayerTurn,     // 玩家回合（回能+抽牌+等待操作）
    EnemyTurn,      // 敌人回合（AI决策+执行）
    CheckResult,    // 检查胜负
    Win,            // 胜利
    Lose            // 失败
}

<<<<<<< HEAD
/// <summary>
/// 回合管理器 —— 控制回合流转
/// 组长负责，写在 Scripts/Core/TurnManager.cs
/// </summary>
=======

>>>>>>> dd15dc953bda76878a619e06d67d3f5c5cb965a7
public class TurnManager
{
    public TurnState CurrentState { get; private set; } = TurnState.BattleStart;
    public int TurnNumber { get; private set; } = 0;              // 当前回合数

    public event Action<TurnState> OnStateChanged;                 // 状态变化事件
    public event Action OnPlayerTurnStart;                         // 玩家回合开始
    public event Action OnEnemyTurnStart;                          // 敌人回合开始

    /// <summary>战斗开始</summary>
    public void StartBattle()
    {
        TurnNumber = 0;
        TransitionTo(TurnState.BattleStart);
        // BattleStart → 抽初始手牌后自动进入 PlayerTurn
        StartPlayerTurn();
    }

    /// <summary>开始玩家回合</summary>
    public void StartPlayerTurn()
    {
        TurnNumber++;
        TransitionTo(TurnState.PlayerTurn);
        OnPlayerTurnStart?.Invoke();
    }

    /// <summary>玩家点击"结束回合"</summary>
    public void EndPlayerTurn()
    {
        if (CurrentState != TurnState.PlayerTurn) return;
        StartEnemyTurn();
    }

    /// <summary>开始敌人回合</summary>
    private void StartEnemyTurn()
    {
        TransitionTo(TurnState.EnemyTurn);
        OnEnemyTurnStart?.Invoke();
    }

    /// <summary>敌人回合结束 → 检查胜负</summary>
    public void EndEnemyTurn()
    {
        TransitionTo(TurnState.CheckResult);
        // BattleManager 检查胜负后决定下一步
    }

    /// <summary>判定为胜利</summary>
    public void DeclareWin()
    {
        if (CurrentState == TurnState.Win || CurrentState == TurnState.Lose) return;
        TransitionTo(TurnState.Win);
    }

    /// <summary>判定为失败</summary>
    public void DeclareLose()
    {
        if (CurrentState == TurnState.Win || CurrentState == TurnState.Lose) return;
        TransitionTo(TurnState.Lose);
    }

    /// <summary>胜负未分 → 开始下一回合</summary>
    public void ContinueToNextTurn()
    {
        StartPlayerTurn();
    }

    private void TransitionTo(TurnState newState)
    {
        CurrentState = newState;
        OnStateChanged?.Invoke(newState);
    }
}
