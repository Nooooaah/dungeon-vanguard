using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// RewardScene initializer: generates 3 random card choices and shows them via RewardPopupUI.
/// Listens for card selection to add it to the player's persistent deck.
/// </summary>
public class RewardSceneInit : MonoBehaviour
{
    private RewardPopupUI _rewardUI;

    void Start()
    {
        _rewardUI = FindObjectOfType<RewardPopupUI>();
        if (_rewardUI == null)
        {
            Debug.LogError("[RewardSceneInit] RewardPopupUI not found!");
            return;
        }

        // Wire all UI references that weren't connected in Inspector
        WireUIReferences();

        // Generate 3 random cards
        var allCards = Resources.LoadAll<CardData>("");
        if (allCards.Length == 0)
        {
            Debug.LogError("[RewardSceneInit] No CardData found in Resources!");
            return;
        }

        var rewardCards = new List<CardData>();
        var pool = new List<CardData>(allCards);
        for (int i = 0; i < 3 && pool.Count > 0; i++)
        {
            int idx = Random.Range(0, pool.Count);
            rewardCards.Add(pool[idx]);
            pool.RemoveAt(idx);
        }

        int gold = Random.Range(15, 30);

<<<<<<< HEAD
=======
        if (GameManager.Instance != null)
            GameManager.Instance.TotalGold += gold;

>>>>>>> dd15dc953bda76878a619e06d67d3f5c5cb965a7
        // Subscribe to card selection
        _rewardUI.OnCardSelectedEvent += OnCardSelected;

        _rewardUI.ShowCardReward(rewardCards, gold);

        Debug.Log("[RewardSceneInit] ShowCardReward called with " + rewardCards.Count + " cards");
    }

    void WireUIReferences()
    {
        var panel = _rewardUI.transform;

        _rewardUI.titleText = FindTMPInChildren(panel, "Title");
        _rewardUI.goldText = FindTMPInChildren(panel, "GoldText");
        _rewardUI.cardContainer = panel.Find("Panel/CardContainer");
        _rewardUI.skipButton = FindButtonInChildren(panel, "SkipBtn");
        _rewardUI.eventPanel = panel.Find("Panel/EventPanel")?.gameObject;

        // Wire skip button onClick (RewardPopupUI.Start ran before us, skipButton was null then)
        if (_rewardUI.skipButton != null)
        {
            _rewardUI.skipButton.onClick.AddListener(() => _rewardUI.Skip());
        }

        Debug.Log("[RewardSceneInit] Wired: title=" + (_rewardUI.titleText != null) +
                  " gold=" + (_rewardUI.goldText != null) +
                  " cardContainer=" + (_rewardUI.cardContainer != null) +
                  " skip=" + (_rewardUI.skipButton != null));
    }

    void OnCardSelected(CardData card)
    {
        if (card == null) return;

        var gm = GameManager.Instance;
        if (gm != null)
        {
            gm.AddCardToDeck(card);
            Debug.Log("[RewardSceneInit] Added card to deck: " + card.cardName);
        }
        else
        {
            Debug.LogError("[RewardSceneInit] GameManager.Instance is null!");
        }
    }

    TMP_Text FindTMPInChildren(Transform parent, string name)
    {
        var t = parent.Find(name);
        if (t != null) return t.GetComponent<TMP_Text>();
        foreach (Transform child in parent)
        {
            var found = FindTMPInChildren(child, name);
            if (found != null) return found;
        }
        return null;
    }

    Button FindButtonInChildren(Transform parent, string name)
    {
        var t = parent.Find(name);
        if (t != null) return t.GetComponent<Button>();
        foreach (Transform child in parent)
        {
            var found = FindButtonInChildren(child, name);
            if (found != null) return found;
        }
        return null;
    }

    void OnDestroy()
    {
        if (_rewardUI != null)
            _rewardUI.OnCardSelectedEvent -= OnCardSelected;
    }
}
