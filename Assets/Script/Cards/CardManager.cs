using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 卡牌总管 —— 管理抽牌堆、手牌、弃牌堆
/// 组员A负责
///
/// 挂在 BattleScene 的 GameObject 上（与 BattleManager 同一物体或独立）
/// </summary>
public class CardManager : MonoBehaviour
{
    // ===== 三个牌堆 =====
    private List<CardData> _drawPile = new List<CardData>();     // 抽牌堆
    private List<Card> _hand = new List<Card>();                  // 手牌（Card实例）
    private List<CardData> _discardPile = new List<CardData>();   // 弃牌堆

    // ===== 运行时数据 =====
    public List<CardData> FullDeck { get; private set; } = new List<CardData>(); // 完整牌组（跨战斗）
    // ===== 预制体 =====
    [Header("Prefab")]
    public GameObject cardPrefab;           // Card.prefab
    public Transform handParent;            // 手牌区父节点

    // ===== 回调（UI监听）=====
    public System.Action OnHandChanged;     // 手牌变化时通知UI刷新

    void Start()
    {
        // 订阅 BattleManager 事件
        var bm = GameManager.Instance?.BattleManager;
        if (bm != null)
        {
            bm.OnDrawCard += DrawCard;
            bm.OnDiscardHand += DiscardHand;
            bm.OnRestoreEnergy += RestorePlayerEnergy;
        }
    }

    // ==========================================
    //  牌组初始化
    // ==========================================

    /// <summary>用一组CardData初始化牌组（每场战斗开始时调用）</summary>
    public void InitializeDeck(List<CardData> deck)
    {
        FullDeck = new List<CardData>(deck);
        ResetForNewBattle();
    }

    /// <summary>重置牌堆，准备新战斗</summary>
    public void ResetForNewBattle()
    {
        _drawPile = new List<CardData>(FullDeck);
        _discardPile.Clear();
        ClearHand();

        // 洗牌
        Shuffle(_drawPile);
    }

    // ==========================================
    //  抽牌 / 洗牌
    // ==========================================

    /// <summary>从抽牌堆抽一张牌到手上</summary>
    public void DrawCard()
    {
        // 抽牌堆空了 → 弃牌堆洗入
        if (_drawPile.Count == 0)
        {
            if (_discardPile.Count == 0) return; // 完全没牌了
            _drawPile = new List<CardData>(_discardPile);
            _discardPile.Clear();
            Shuffle(_drawPile);
        }

        // 手牌满了 → 抽到的牌直接丢弃
        if (_hand.Count >= GameConstants.HAND_LIMIT)
        {
            var overflow = _drawPile[0];
            _drawPile.RemoveAt(0);
            _discardPile.Add(overflow);
            return;
        }

        // 正常抽牌
        var cardData = _drawPile[0];
        _drawPile.RemoveAt(0);

        // 实例化Card预制体
        var cardObj = Instantiate(cardPrefab, handParent);
        var card = cardObj.GetComponent<Card>();
        if (card != null)
        {
            card.Init(cardData);
            card.OnPlayed += OnCardPlayed;
            _hand.Add(card);
        }

        OnHandChanged?.Invoke();
        UpdateAffordableStates();
    }

    /// <summary>一次抽N张</summary>
    public void DrawCards(int count)
    {
        for (int i = 0; i < count; i++)
            DrawCard();
    }

    // ==========================================
    //  出牌
    // ==========================================

    /// <summary>玩家点击一张牌——执行效果</summary>
    public bool PlayCard(Card card)
    {
        if (card == null || !_hand.Contains(card)) return false;

        var bm = GameManager.Instance?.BattleManager;
        if (bm == null) return false;

        // 检查能量
        int actualCost = card.Cost;

        var player = bm.Player as Player;
        if (player == null) return false;

        if (!player.HasEnoughEnergy(actualCost)) return false;

        // 扣能量
        player.SpendEnergy(actualCost);

        // 通过BattleManager打出（会记录元素上下文+检查反应）
        bm.PlayCard(card);

        return true;
    }

    /// <summary>卡牌打出后的回调：从手牌移除 → 加入弃牌堆</summary>
    private void OnCardPlayed(Card card)
    {
        if (!_hand.Contains(card)) return;

        _hand.Remove(card);
        _discardPile.Add(card.Data);

        // 销毁卡牌GameObject（或归还对象池）
        card.OnPlayed -= OnCardPlayed;
        Destroy(card.gameObject);

        OnHandChanged?.Invoke();
        UpdateAffordableStates();
    }

    // ==========================================
    //  弃牌 / 清空手牌
    // ==========================================

    /// <summary>获取手牌列表（HandController读取用）</summary>
    public List<Card> GetHand() => _hand;

    /// <summary>弃掉所有手牌（回合结束时调用）</summary>
    public void DiscardHand()
    {
        foreach (var card in _hand)
        {
            _discardPile.Add(card.Data);
            card.OnPlayed -= OnCardPlayed;
            Destroy(card.gameObject);
        }
        _hand.Clear();
        OnHandChanged?.Invoke();
    }

    // ==========================================
    //  牌组扩充（奖励选牌）
    // ==========================================

    /// <summary>给完整牌组添加一张牌（奖励选牌后调用）</summary>
    public void AddCardToDeck(CardData cardData)
    {
        FullDeck.Add(cardData);
    }

    /// <summary>从牌组移除一张牌</summary>
    public void RemoveCardFromDeck(CardData cardData)
    {
        FullDeck.Remove(cardData);
    }

    // ==========================================
    //  辅助
    // ==========================================

    private void ClearHand()
    {
        foreach (var card in _hand)
        {
            card.OnPlayed -= OnCardPlayed;
            Destroy(card.gameObject);
        }
        _hand.Clear();
    }

    private void RestorePlayerEnergy()
    {
        var player = GameManager.Instance?.BattleManager?.Player as Player;
        player?.RestoreEnergy();
    }

    /// <summary>根据当前能量更新所有手牌的可打出状态</summary>
    public void UpdateAffordableStates()
    {
        var player = GameManager.Instance?.BattleManager?.Player as Player;
        if (player == null) return;

        foreach (var card in _hand)
        {
            int actualCost = card.Cost;
            card.SetAffordable(player.HasEnoughEnergy(actualCost));
        }
    }

    /// <summary>Fisher-Yates 洗牌算法</summary>
    private void Shuffle(List<CardData> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    public int DrawPileCount => _drawPile.Count;
    public int HandCount => _hand.Count;
    public int DiscardPileCount => _discardPile.Count;

    void OnDestroy()
    {
        var bm = GameManager.Instance?.BattleManager;
        if (bm != null)
        {
            bm.OnDrawCard -= DrawCard;
            bm.OnDiscardHand -= DiscardHand;
            bm.OnRestoreEnergy -= RestorePlayerEnergy;
        }
    }
}
