using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Battle UI — 纯 UI/视觉层，战斗逻辑委托给 BattleManager。
/// 职责：卡牌渲染、手牌管理、VFX、HP条绑定、回合UI显示。
/// 战斗逻辑（伤害/反应/Buff/敌人AI）由 BattleManager + 子系统处理。
/// </summary>
public class BattleUI : MonoBehaviour
{
    [Header("Enemy")]
    public EnemyData enemyData;
    public Transform enemyArea;

    [Header("Player Area (VFX)")]
    public RectTransform playerArea;

    [Header("Player HPBarUI")]
    public HPBarUI playerHPBar;

    [Header("Enemy HPBarUI")]
    public HPBarUI enemyHPBar;
    public Image enemyPortrait;
    public TMP_Text enemyNameText;
    public TMP_Text enemyIntentText;

    [Header("Hand")]
    public Transform handArea;

    [Header("Controls")]
    public Button endTurnButton;
    public TMP_Text turnText;

    [Header("Combo Effects")]
    public Image flashOverlay;
    public RectTransform particleLayer;
    public RectTransform comboTextLayer;

    [Header("Damage Text Layer")]
    public RectTransform damageTextLayer;

    [Header("Deck Info")]
    public TMP_Text deckCountText;
    public TMP_Text discardCountText;

    // ── Combat system references ──
    private BattleManager _bm;
    private Player _player;
    private EnemyBase _enemy;

    // ── Deck state ──
    private List<CardData> _deck = new List<CardData>();
    private List<CardData> _discard = new List<CardData>();
    private List<GameObject> _handCards = new List<GameObject>();
    private int _deckCount = 0;
    private int _discardCount = 0;

    // ── VFX ──
    private ComboEffectSystem _comboSystem;

    // ── Card font (msyhl SDF for CJK) ──
    private TMP_FontAsset _cardFont;

    void Start()
    {
        // Load card font for CJK text
        _cardFont = Resources.Load<TMP_FontAsset>("msyhl SDF");
#if UNITY_EDITOR
        if (_cardFont == null)
            _cardFont = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/msyhl SDF.asset");
#endif

        // Clean up any stale card objects left in HandArea from a previous session
        if (handArea != null)
        {
            for (int i = handArea.childCount - 1; i >= 0; i--)
            {
                var child = handArea.GetChild(i);
                if (child.name.StartsWith("Card_"))
                    Destroy(child.gameObject);
            }
        }

        _comboSystem = ComboEffectSystem.Instance;
        if (flashOverlay != null)
            _comboSystem.Initialize(flashOverlay, particleLayer, comboTextLayer);

        // 初始化战斗特效管理器
        var vfxGo = new GameObject("CombatVFXManager");
        var vfx = vfxGo.AddComponent<CombatVFXManager>();
        var playerAreaRT = playerArea;
        if (playerAreaRT == null)
        {
            // 查找场景中的 PlayerCharacter 对象（玩家身体所在位置）
            var playerChar = GameObject.Find("PlayerCharacter");
            if (playerChar != null)
                playerAreaRT = playerChar.GetComponent<RectTransform>();
        }
        if (playerAreaRT == null && playerHPBar != null)
            playerAreaRT = playerHPBar.GetComponent<RectTransform>();
        vfx.Initialize(particleLayer,
            enemyArea != null ? enemyArea.GetComponent<RectTransform>() : null,
            playerAreaRT,
            _cardFont);

        // 获取 BattleManager（场景中的）
        _bm = FindObjectOfType<BattleManager>();
        if (_bm == null)
        {
            var go = new GameObject("BattleManager");
            _bm = go.AddComponent<BattleManager>();
        }

        // 获取 Player 和 Enemy（场景中的）
        _player = FindObjectOfType<Player>();
        if (_player == null)
        {
            var go = new GameObject("Player");
            _player = go.AddComponent<Player>();
        }

        _enemy = FindObjectOfType<EnemyBase>();
        if (_enemy == null)
        {
            var go = new GameObject("Enemy");
            _enemy = go.AddComponent<EnemyBase>();
            go.AddComponent<EnemyAI>();
        }

        // 确保 Player 已初始化（Player.Start() 可能还没跑）
        if (_player.Stats == null)
        {
            var gm = GameManager.Instance;
            Element elem = gm != null && gm.ChosenElement != Element.None
                ? gm.ChosenElement
                : Element.Fire;
            var stats = new CharacterStats(
                gm != null ? gm.PlayerMaxHp : GameConstants.PLAYER_MAX_HP,
                GameConstants.PLAYER_MAX_ENERGY,
                elem
            );
            if (gm != null) stats.currentHp = gm.PlayerCurrentHp;
            // 通过反射设置 Stats
            var statsField = typeof(Player).GetProperty("Stats");
            if (statsField != null) statsField.SetValue(_player, stats);
        }

        // 设置敌人数据
        if (enemyData == null)
            enemyData = PickRandomEnemy();

        // Initialize enemy with data (handles both inspector-assigned and runtime-assigned cases)
        if (enemyData != null && (_enemy.Data == null || _enemy.CurrentHP == 0))
        {
            _enemy.Initialize(enemyData);
        }

        // 注册到 BattleManager
        _bm.RegisterPlayer(_player);
        _bm.RegisterEnemy(_enemy);

        // 绑定 HP 条
        if (playerHPBar != null) playerHPBar.BindTo(_player);
        if (enemyHPBar != null) enemyHPBar.BindTo(_enemy);

        // 订阅 BattleManager 回调（抽牌/弃牌/回能）
        _bm.OnDrawCard += DrawCard;
        _bm.OnDiscardHand += DiscardHand;
        _bm.OnRestoreEnergy += RestoreEnergy;

        // 订阅 TurnManager 事件
        var tm = _bm.TurnManager;
        tm.OnPlayerTurnStart += HandlePlayerTurnStart;
        tm.OnEnemyTurnStart += HandleEnemyTurnStart;
        tm.OnStateChanged += HandleStateChanged;

        if (endTurnButton != null)
            endTurnButton.onClick.AddListener(OnEndTurn);

        // 创建左上角返回主菜单按钮
        CreateReturnButton();

        // 构建牌组
        BuildInitialDeck();
        ShuffleDeck();

        // 更新 UI
        SetTurn(0);
        UpdateDeckInfo();

        // 开始战斗（StartBattle 会触发 OnPlayerTurnStart，
        // HandlePlayerTurnStart 中会预决策敌人行动并更新意图显示）
        _bm.StartBattle();

        // HandlePlayerTurnStart 已经抽满 INITIAL_HAND 张，不需要额外补抽

        // 创建右下角牌库面板
        CreateDeckPanel();
    }

    void Update()
    {
        // Only layout when there are cards and not dragging
        if (_handCards.Count > 0)
            LayoutHand();
    }

    EnemyData PickRandomEnemy()
    {
        var enemies = Resources.LoadAll<EnemyData>("Enemies");
#if UNITY_EDITOR
        if (enemies.Length == 0)
        {
            var guids = UnityEditor.AssetDatabase.FindAssets("t:EnemyData");
            var list = new System.Collections.Generic.List<EnemyData>();
            foreach (var g in guids)
            {
                var ed = UnityEditor.AssetDatabase.LoadAssetAtPath<EnemyData>(UnityEditor.AssetDatabase.GUIDToAssetPath(g));
                if (ed != null) list.Add(ed);
            }
            enemies = list.ToArray();
        }
#endif
        if (enemies.Length == 0) return null;
        var normalEnemies = enemies.Where(e => e.type != EnemyType.Boss && e.type != EnemyType.Elite).ToArray();
        if (normalEnemies.Length > 0)
            return normalEnemies[Random.Range(0, normalEnemies.Length)];
        return enemies[0];
    }

    void BuildInitialDeck()
    {
        _deck.Clear();
        var gm = GameManager.Instance;

        // 如果 GameManager 有持久牌组，优先使用
        if (gm != null && gm.HasPersistentDeck)
        {
            _deck = gm.GetPlayerDeck();
            _deckCount = _deck.Count;
            Debug.Log($"[BattleUI] Using persistent deck: {_deck.Count} cards");
            return;
        }

        // Fallback: 随机生成初始牌组
        var allCards = Resources.LoadAll<CardData>("");

        Element playerElement = gm != null && gm.ChosenElement != Element.None
            ? gm.ChosenElement
            : Element.Fire;

        var elementCards = allCards.Where(c => c.element == playerElement).ToList();
        var otherCards = allCards.Where(c => c.element != playerElement).ToList();

        var chosen = new List<CardData>();

        // 5张本元素牌
        chosen.AddRange(elementCards.OrderBy(c => Random.value).Take(5));

        // 2张其他元素攻击牌
        chosen.AddRange(otherCards.Where(c => c.cardType == CardType.Attack)
            .OrderBy(c => Random.value).Take(2));

        // 1张防御牌
        var defense = allCards.Where(c => c.cardType == CardType.Skill && c.shield > 0)
            .OrderBy(c => Random.value).FirstOrDefault();
        if (defense != null) chosen.Add(defense);

        _deck = new List<CardData>(chosen);
        _deckCount = _deck.Count;
        Debug.Log($"[BattleUI] Deck built: {_deck.Count} cards (element={playerElement})");

        // 保存为持久牌组
        if (gm != null)
            gm.SetInitialDeck(_deck);
    }

    void ShuffleDeck()
    {
        for (int i = _deck.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var temp = _deck[i]; _deck[i] = _deck[j]; _deck[j] = temp;
        }
    }

    // ── 卡牌管理（由 BattleManager 回调驱动）──

    void DrawCard()
    {
        if (_deck.Count == 0 && _discard.Count > 0)
        {
            _deck = new List<CardData>(_discard);
            _discard.Clear();
            _discardCount = 0;
            ShuffleDeck();
            UpdateDeckInfo();
        }
        if (_deck.Count == 0) return;
        if (_handCards.Count >= GameConstants.HAND_LIMIT) return;

        var data = _deck[0];
        _deck.RemoveAt(0);
        _deckCount = _deck.Count;
        SpawnCardInHand(data);
        UpdateDeckInfo();
    }

    void DiscardHand()
    {
        foreach (var cardObj in _handCards)
        {
            var card = cardObj.GetComponent<Card>();
            if (card != null) _discard.Add(card.Data);
            Destroy(cardObj);
        }
        _handCards.Clear();
        _discardCount = _discard.Count;
        UpdateDeckInfo();
    }

    void RestoreEnergy()
    {
        _player?.RestoreEnergy();
        if (playerHPBar != null)
            playerHPBar.SetEnergy(_player?.Stats?.currentEnergy ?? 0, GameConstants.PLAYER_MAX_ENERGY);
    }

    void SpawnCardInHand(CardData data)
    {
        var go = CreateCardUI(data);
        go.transform.SetParent(handArea, false);

        // 添加 Card 组件（ICardEffect）供 BattleManager 使用
        var card = go.GetComponent<Card>();
        if (card == null) card = go.AddComponent<Card>();

        // 绑定 Card 组件的 UI 引用（CreateCardUI 创建的子物体）
        card.nameText = FindTMPInChildren(go, "NameText");
        card.costText = FindTMPInChildren(go, "CostText");
        card.descText = FindTMPInChildren(go, "DescText");
        card.attackText = FindTMPInChildren(go, "AttackText");
        card.healText = FindTMPInChildren(go, "HealthText");
        card.attackIconGroup = go.transform.Find("AttackIcon")?.gameObject;
        card.healIconGroup = go.transform.Find("HealthIcon")?.gameObject;
        var bgTr = go.transform.Find("BG");
        card.cardBackground = bgTr != null ? bgTr.GetComponent<Image>() : null;
        card.elementIcon = null;
        card.button = go.GetComponent<Button>();
        if (card.button == null) card.button = go.AddComponent<Button>();

        // Create cost-too-high overlay (gray semi-transparent)
        var overlayGo = new GameObject("CostOverlay");
        var maskTr = go.transform.Find("CardMask");
        overlayGo.transform.SetParent(maskTr != null ? maskTr : go.transform, false);
        var overlayRt = overlayGo.AddComponent<RectTransform>();
        overlayRt.anchorMin = Vector2.zero; overlayRt.anchorMax = Vector2.one;
        overlayRt.offsetMin = Vector2.zero; overlayRt.offsetMax = Vector2.zero;
        var overlayImg = overlayGo.AddComponent<Image>();
        overlayImg.color = new Color(0, 0, 0, 0.6f);
        overlayImg.raycastTarget = false;
        overlayGo.SetActive(false);
        card.costTooHighOverlay = overlayGo;

        card.Init(data);
        card.OnPlayed += OnCardPlayed;

        // UI 组件
        var cardUI = go.GetComponent<CardUI>();
        if (cardUI == null) cardUI = go.AddComponent<CardUI>();
        cardUI.attackText = FindTMPInChildren(go, "AttackText");
        cardUI.healText = FindTMPInChildren(go, "HealthText");
        cardUI.attackIconGroup = go.transform.Find("AttackIcon")?.gameObject;
        cardUI.healIconGroup = go.transform.Find("HealthIcon")?.gameObject;
        cardUI.Setup(data);

        var hover = go.GetComponent<CardHoverEffect>();
        if (hover == null) hover = go.AddComponent<CardHoverEffect>();
        if (bgTr != null)
            hover.SetBackgroundImage(bgTr.GetComponent<Image>());

        var drag = go.GetComponent<CardDragHandler>();
        if (drag == null) drag = go.AddComponent<CardDragHandler>();

        var playAnim = go.GetComponent<CardPlayAnimation>();
        if (playAnim == null) playAnim = go.AddComponent<CardPlayAnimation>();

        drag.OnDragEnded = (validDrop, dropPos) =>
        {
            if (validDrop) PlayCard(go, data);
        };

        _handCards.Add(go);
        SoundManager.Instance.PlayCardDraw();
        LayoutHand();
        RefreshHandCosts();
    }

    void OnCardPlayed(Card card)
    {
        if (card == null) return;
        var go = card.gameObject;
        _handCards.Remove(go);
        _discard.Add(card.Data);
        _discardCount = _discard.Count;
        LayoutHand();
        UpdateDeckInfo();
    }

    // ── 出牌 ──

    public void PlayCard(GameObject cardGo, CardData data)
    {
        var card = cardGo.GetComponent<Card>();
        if (card == null) return;

        // 检查能量是否足够
        var player = _bm.Player as Player;
        if (player == null) return;
        if (!player.HasEnoughEnergy(card.Cost))
        {
            Debug.Log("[BattleUI] Not enough energy for " + data.cardName);
            // 位置由 LayoutHand() 每帧平滑恢复
            return;
        }

        // 扣能量
        player.SpendEnergy(card.Cost);
        if (playerHPBar != null)
            playerHPBar.SetEnergy(player.Stats.currentEnergy, GameConstants.PLAYER_MAX_ENERGY);

        // 执行卡牌效果（BattleManager 会处理元素上下文 + 反应）
        _bm.PlayCard(card);

        // 播放出牌动画
        Vector2 targetPos = Vector2.up * 200f;
        if (enemyArea != null)
            targetPos = enemyArea.GetComponent<RectTransform>().anchoredPosition;

        var playAnim = cardGo.GetComponent<CardPlayAnimation>();
        if (playAnim != null)
        {
            playAnim.PlayArc(targetPos, () =>
            {
                card.OnPlayed -= OnCardPlayed;
                Destroy(cardGo);
            });
        }
        else
        {
            card.OnPlayed -= OnCardPlayed;
            Destroy(cardGo);
        }

        SoundManager.Instance.PlayCardPlay();

        // 触发攻击 VFX
        if (data.damage > 0)
            TriggerAttackVFX(data.element, data.damage);

        // 触发防御 VFX（玩家身上出现蓝色护盾）
        if (data.shield > 0 && CombatVFXManager.Instance != null)
            CombatVFXManager.Instance.PlayShield(CombatVFXManager.Instance.GetPlayerPosition());

        // 触发治疗 VFX（玩家身上出现绿色十字架）
        if (data.heal > 0 && CombatVFXManager.Instance != null)
            CombatVFXManager.Instance.PlayHeal(CombatVFXManager.Instance.GetPlayerPosition());

        // 显示反应文本
        var reactionText = DamageSystem.LastReactionText;
        if (!string.IsNullOrEmpty(reactionText))
        {
            if (comboTextLayer != null)
                StartCoroutine(ShowReactionText(reactionText));
        }

        RefreshHandCosts();
        UpdateEnemyDisplay();
    }

    // ── 回合事件处理 ──

    void HandlePlayerTurnStart()
    {
        SetTurn(_bm.TurnManager.TurnNumber);
        RestoreEnergy();

        // 预决策敌人下一步行动，确保意图显示与实际执行一致
        if (_enemy != null)
        {
            var ai = _enemy.GetComponent<EnemyAI>();
            if (ai != null) ai.PredecideNextAction();
        }

        UpdateEnemyDisplay();
        RefreshHandCosts();
    }

    void HandleEnemyTurnStart()
    {
        // EnemyAI 会自动执行，由 BattleManager 调度
        // 敌人行动完毕后 EnemyAI 调用 BattleManager.EndEnemyPhase()
    }

    void HandleStateChanged(TurnState state)
    {
        if (state == TurnState.Win)
            OnBattleWon();
        else if (state == TurnState.Lose)
            OnBattleLost();
    }

    void OnEndTurn()
    {
        _bm.OnEndTurnClicked();
        SoundManager.Instance.PlayEndTurn();
    }

    void CreateReturnButton()
    {
        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;

        var go = new GameObject("ReturnButton");
        go.transform.SetParent(canvas.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(20f, -20f);
        rt.sizeDelta = new Vector2(130f, 40f);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.3f, 0.12f, 0.12f, 0.85f);
        var btn = go.AddComponent<Button>();
        btn.onClick.AddListener(() =>
        {
            var gm = GameManager.Instance;
            if (gm != null) gm.ReturnToMainMenu();
        });
        var label = CreatePanelLabel(go, "返回主菜单", 18, new Color(0.9f, 0.8f, 0.8f, 1f));
        label.alignment = TextAlignmentOptions.Center;
    }

    // ── 胜负 ──

    void OnBattleWon()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;
        if (gm.PendingNode != null && gm.PendingNode.nodeType == NodeType.Boss)
            gm.OnGameClear();
        else
            gm.OnBattleWin();
    }

    void OnBattleLost()
    {
        GameManager.Instance?.OnGameOver();
    }

    // ── UI 更新 ──

    void UpdateEnemyDisplay()
    {
        if (enemyData != null)
        {
            if (enemyNameText != null) enemyNameText.text = enemyData.enemyName;

            if (enemyIntentText != null)
            {
                bool frozen = _bm.BuffSystem.HasBuff(_enemy, BuffType.Freeze);
                if (frozen)
                    enemyIntentText.text = "[冰冻] 跳过回合";
                else
                {
                    // 从 EnemyAI 读取预决策的下一步行动
                    var ai = _enemy != null ? _enemy.GetComponent<EnemyAI>() : null;
                    var nextAction = ai != null ? ai.NextAction : null;
                    if (nextAction != null)
                        enemyIntentText.text = nextAction.GetIntentText();
                    else if (enemyData.actions.Count > 0)
                        enemyIntentText.text = enemyData.actions[0].GetIntentText();
                }
            }
        }
    }

    void SetTurn(int turn)
    {
        if (turnText != null) turnText.text = "第 " + turn + " 回合";
    }

    public void RefreshHandCosts()
    {
        var player = _bm?.Player as Player;
        if (player == null) return;

        foreach (var cardObj in _handCards)
        {
            var cardUI = cardObj.GetComponent<CardUI>();
            var hover = cardObj.GetComponent<CardHoverEffect>();
            var card = cardObj.GetComponent<Card>();
            if (card != null)
            {
                int cost = card.Cost;
                bool canAfford = player.HasEnoughEnergy(cost);
                card.SetAffordable(canAfford);
                if (cardUI != null) cardUI.SetEnergySufficient(canAfford);
                if (hover != null) hover.SetGrayed(!canAfford);
            }
        }
    }

    private GameObject _deckPanel;
    private Button _deckButton;

    void CreateDeckPanel()
    {
        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;

        _deckPanel = new GameObject("DeckPanel");
        _deckPanel.transform.SetParent(canvas.transform, false);
        var dpRt = _deckPanel.AddComponent<RectTransform>();
        dpRt.anchorMin = new Vector2(1f, 0f);
        dpRt.anchorMax = new Vector2(1f, 0f);
        dpRt.pivot = new Vector2(1f, 0f);
        dpRt.anchoredPosition = new Vector2(-20f, 20f);
        dpRt.sizeDelta = new Vector2(200f, 120f);
        var dpImg = _deckPanel.AddComponent<Image>();
        dpImg.color = new Color(0.06f, 0.05f, 0.10f, 0.88f);

        var vlg = _deckPanel.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 4f;
        vlg.padding = new RectOffset(10, 10, 8, 8);
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        // Discard row (top)
        var discardRow = new GameObject("DiscardRow");
        discardRow.transform.SetParent(_deckPanel.transform, false);
        var drRt = discardRow.AddComponent<RectTransform>();
        drRt.sizeDelta = new Vector2(0f, 40f);
        var drHlg = discardRow.AddComponent<HorizontalLayoutGroup>();
        drHlg.childForceExpandWidth = true;
        drHlg.childForceExpandHeight = true;
        drHlg.spacing = 8f;

        var discardLabel = CreatePanelLabel(discardRow, "弃牌", 22, new Color(0.7f, 0.7f, 0.7f, 1f));
        discardLabel.alignment = TextAlignmentOptions.Left;
        if (discardCountText == null)
            discardCountText = CreatePanelLabel(discardRow, "0", 22, new Color(0.85f, 0.85f, 0.85f, 1f));
        else
            discardCountText.alignment = TextAlignmentOptions.Right;
        discardCountText.alignment = TextAlignmentOptions.Right;

        // Divider
        var divGo = new GameObject("Divider");
        divGo.transform.SetParent(_deckPanel.transform, false);
        var divRt = divGo.AddComponent<RectTransform>();
        divRt.sizeDelta = new Vector2(0f, 2f);
        var divImg = divGo.AddComponent<Image>();
        divImg.color = new Color(0.3f, 0.25f, 0.1f, 0.5f);
        divImg.raycastTarget = false;

        // Deck row (bottom, clickable button)
        var deckBtnGo = new GameObject("DeckButton");
        deckBtnGo.transform.SetParent(_deckPanel.transform, false);
        var dbRt = deckBtnGo.AddComponent<RectTransform>();
        dbRt.sizeDelta = new Vector2(0f, 44f);
        var dbImg = deckBtnGo.AddComponent<Image>();
        dbImg.color = new Color(0.12f, 0.22f, 0.40f, 0.7f);
        _deckButton = deckBtnGo.AddComponent<Button>();
        var dbHlg = deckBtnGo.AddComponent<HorizontalLayoutGroup>();
        dbHlg.childForceExpandWidth = true;
        dbHlg.childForceExpandHeight = true;
        dbHlg.spacing = 8f;
        dbHlg.padding = new RectOffset(6, 6, 4, 4);

        var deckLabel = CreatePanelLabel(deckBtnGo, "牌库", 24, new Color(1f, 0.92f, 0.7f, 1f));
        deckLabel.alignment = TextAlignmentOptions.Left;
        if (deckCountText == null)
            deckCountText = CreatePanelLabel(deckBtnGo, "0", 24, new Color(1f, 0.92f, 0.7f, 1f));
        deckCountText.alignment = TextAlignmentOptions.Right;

        _deckButton.onClick.AddListener(ShowDeckView);
    }

    TextMeshProUGUI CreatePanelLabel(GameObject parent, string text, int fontSize, Color color)
    {
        var go = new GameObject("Label");
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0f, 0f);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        if (_cardFont != null) tmp.font = _cardFont;
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        tmp.enableWordWrapping = false;
        return tmp;
    }

    void ShowDeckView()
    {
        ShowPopup(0);
    }

    void ShowMyDeckView()
    {
        ShowPopup(2);
    }

    void ShowPopup(int viewMode)
    {
        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;

        // Build full deck list: deck + discard + hand
        var allCards = new List<CardData>();
        allCards.AddRange(_deck);
        allCards.AddRange(_discard);
        foreach (var cardGo in _handCards)
        {
            var card = cardGo.GetComponent<Card>();
            if (card != null && card.Data != null)
                allCards.Add(card.Data);
        }

        var popupGo = new GameObject("DeckViewPopup");
        popupGo.transform.SetParent(canvas.transform, false);
        var popupRt = popupGo.AddComponent<RectTransform>();
        popupRt.anchorMin = Vector2.zero; popupRt.anchorMax = Vector2.one;
        popupRt.offsetMin = Vector2.zero; popupRt.offsetMax = Vector2.zero;
        var popup = popupGo.AddComponent<DeckViewPopup>();
        popup.Initialize(_deck, _discard, allCards, _cardFont);

        // Set initial tab
        var type = typeof(DeckViewPopup);
        var viewModeField = type.GetField("_viewMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (viewModeField != null)
        {
            viewModeField.SetValue(popup, viewMode);
            var refreshMethod = type.GetMethod("RefreshContent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            refreshMethod?.Invoke(popup, null);
        }
    }

    void UpdateDeckInfo()
    {
        if (deckCountText != null) deckCountText.text = _deckCount.ToString();
        if (discardCountText != null) discardCountText.text = _discardCount.ToString();
    }

    [Header("扇形手牌参数")]
    public float fanCardSpacing = 130f;
    public float fanAngleStep = 4f;
    public float fanArcHeight = 3f;
    public float fanYLift = 60f;
    public float fanArrangeSpeed = 12f;
    public float hoverLift = 180f;
    public float hoverScaleMul = 1.15f;
    public float cardCornerRadius = 16f;

    private Sprite _roundedRectSprite;
    private bool _roundedRectCreated;

    Sprite GetRoundedRectSprite(float width, float height, float radius)
    {
        // Cache: only generate once
        if (_roundedRectCreated && _roundedRectSprite != null)
            return _roundedRectSprite;
        _roundedRectCreated = true;

        int w = Mathf.RoundToInt(width);
        int h = Mathf.RoundToInt(height);
        int r = Mathf.RoundToInt(radius);
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        var pixels = new Color32[w * h];
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int py = h - 1 - y;
                int cx, cy;
                if (py < r && x < r) { cx = r; cy = r; }
                else if (py >= h - r && x < r) { cx = r; cy = h - r - 1; }
                else if (py < r && x >= w - r) { cx = w - r - 1; cy = r; }
                else if (py >= h - r && x >= w - r) { cx = w - r - 1; cy = h - r - 1; }
                else { pixels[y * w + x] = new Color32(255, 255, 255, 255); continue; }
                float dist = Mathf.Sqrt((x - cx) * (x - cx) + (py - cy) * (py - cy)) - r;
                float alpha = Mathf.Clamp01(0.5f - dist);
                pixels[y * w + x] = new Color32(255, 255, 255, (byte)(alpha * 255));
            }
        }
        tex.SetPixels32(pixels);
        tex.Apply();
        _roundedRectSprite = Sprite.Create(tex, new Rect(0, 0, w, h), Vector2.one * 0.5f, 100f);
        return _roundedRectSprite;
    }

    void LayoutHand()
    {
        int count = _handCards.Count;
        if (count == 0) return;

        float cardScale = count > 7 ? 0.75f : count > 5 ? 0.85f : 0.95f;
        float startX = -(count - 1) * fanCardSpacing * 0.5f;

        for (int i = 0; i < count; i++)
        {
            var go = _handCards[i];
            if (go == null) continue;

            var drag = go.GetComponent<CardDragHandler>();
            if (drag != null && drag.IsDragging)
            {
                go.transform.SetAsLastSibling();
                continue;
            }

            var rt = go.GetComponent<RectTransform>();
            var hover = go.GetComponent<CardHoverEffect>();
            bool isHovered = hover != null && hover.IsHovered;

            float x = startX + i * fanCardSpacing;
            float offsetFromCenter = i - (count - 1) * 0.5f;
            float yArc = Mathf.Abs(offsetFromCenter) * fanArcHeight;
            float angle = -offsetFromCenter * fanAngleStep;

            Vector2 targetPos = new Vector2(x, yArc + fanYLift);
            Quaternion targetRot = Quaternion.Euler(0, 0, angle);
            Vector3 targetScale = Vector3.one * cardScale;

            if (isHovered)
            {
                targetPos.y += hoverLift;
                targetRot = Quaternion.identity;
                targetScale = Vector3.one * cardScale * hoverScaleMul;
                go.transform.SetAsLastSibling();
            }
            else
            {
                go.transform.SetSiblingIndex(i);
            }

            rt.anchoredPosition = Vector2.Lerp(rt.anchoredPosition, targetPos, Time.deltaTime * fanArrangeSpeed);
            rt.localRotation = Quaternion.Lerp(rt.localRotation, targetRot, Time.deltaTime * fanArrangeSpeed);
            rt.localScale = Vector3.Lerp(rt.localScale, targetScale, Time.deltaTime * fanArrangeSpeed);
        }
    }

    // ── VFX ──

    void TriggerAttackVFX(Element element, int damage)
    {
        if (flashOverlay != null)
            StartCoroutine(FlashScreen(element));
        if (enemyArea != null)
            StartCoroutine(ShakeTransform(enemyArea, 15f, 0.25f));
        
        // Simple damage number only — no particle burst
        if (particleLayer != null)
            StartCoroutine(ShowDamageNumber(element, damage));
    }

    IEnumerator ShowDamageNumber(Element element, int damage)
    {
        Vector2 centerPos = enemyArea != null
            ? enemyArea.GetComponent<RectTransform>().anchoredPosition
            : Vector2.zero;

        var dmgGo = new GameObject("VFX_DamageNumber");
        dmgGo.transform.SetParent(particleLayer, false);
        var dmgRt = dmgGo.AddComponent<RectTransform>();
        dmgRt.anchoredPosition = centerPos + new Vector2(0, 60f);
        dmgRt.sizeDelta = new Vector2(200, 60);
        var dmgText = dmgGo.AddComponent<TextMeshProUGUI>();
        if (_cardFont != null)
            dmgText.font = _cardFont;
        dmgText.fontSize = 42;
        dmgText.color = new Color(1f, 0.3f, 0.1f, 1f);
        dmgText.alignment = TextAlignmentOptions.Center;
        dmgText.text = "-" + damage;
        dmgText.raycastTarget = false;
        StartCoroutine(AnimateDamageNumber(dmgGo, dmgRt));
        yield return null;
    }

    IEnumerator FlashScreen(Element element)
    {
        Color flashColor = element == Element.Fire ? new Color(1f, 0.4f, 0.1f, 0.5f)
                         : element == Element.Water ? new Color(0.2f, 0.5f, 1f, 0.4f)
                         : new Color(0.3f, 0.8f, 0.3f, 0.4f);

        float elapsed = 0f;
        float duration = 0.3f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float alpha = flashColor.a * (1f - t);
            flashOverlay.color = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }
        flashOverlay.color = new Color(0, 0, 0, 0);
    }

    IEnumerator SpawnParticleBurst(Element element, int damage)
    {
        Color particleColor = element == Element.Fire ? new Color(1f, 0.5f, 0.1f, 1f)
                            : element == Element.Water ? new Color(0.3f, 0.6f, 1f, 1f)
                            : new Color(0.4f, 0.9f, 0.4f, 1f);

        int particleCount = Mathf.Min(12, 6 + damage);
        var particles = new List<GameObject>();

        Vector2 centerPos = enemyArea != null
            ? enemyArea.GetComponent<RectTransform>().anchoredPosition
            : Vector2.zero;

        for (int i = 0; i < particleCount; i++)
        {
            var go = new GameObject("VFX_Particle_" + i);
            go.transform.SetParent(particleLayer, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = centerPos;
            rt.sizeDelta = new Vector2(12, 12);
            var img = go.AddComponent<Image>();
            img.color = particleColor;
            img.raycastTarget = false;

            float angle = (float)i / particleCount * Mathf.PI * 2f;
            float speed = Random.Range(150f, 350f);
            float scale = Random.Range(0.5f, 1.5f);
            rt.localScale = Vector3.one * scale;
            particles.Add(go);
            StartCoroutine(AnimateParticle(go, rt, angle, speed, 0.6f));
        }

        var dmgGo = new GameObject("VFX_DamageNumber");
        dmgGo.transform.SetParent(particleLayer, false);
        var dmgRt = dmgGo.AddComponent<RectTransform>();
        dmgRt.anchoredPosition = centerPos + new Vector2(0, 60f);
        dmgRt.sizeDelta = new Vector2(200, 60);
        var dmgText = dmgGo.AddComponent<TextMeshProUGUI>();
        if (_cardFont != null)
            dmgText.font = _cardFont;
        dmgText.fontSize = 42;
        dmgText.color = new Color(1f, 0.3f, 0.1f, 1f);
        dmgText.alignment = TextAlignmentOptions.Center;
        dmgText.text = "-" + damage;
        dmgText.raycastTarget = false;
        StartCoroutine(AnimateDamageNumber(dmgGo, dmgRt));

        yield return new WaitForSeconds(0.7f);
        foreach (var p in particles) { if (p != null) Destroy(p); }
    }

    IEnumerator ShowReactionText(string text)
    {
        var go = new GameObject("ReactionText");
        go.transform.SetParent(comboTextLayer != null ? comboTextLayer : transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(900, 80);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        if (_cardFont != null)
            tmp.font = _cardFont;
        tmp.fontSize = 36;
        tmp.color = new Color(1f, 0.85f, 0.3f, 1f);
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.text = text;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.raycastTarget = false;

        float elapsed = 0f;
        float duration = 1.2f;
        Vector2 start = rt.anchoredPosition;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            rt.anchoredPosition = start + Vector2.up * (60f * t);
            if (t > 0.6f)
                tmp.color = new Color(1f, 0.85f, 0.3f, 1f - (t - 0.6f) / 0.4f);
            yield return null;
        }
        Destroy(go);
    }

    IEnumerator AnimateParticle(GameObject go, RectTransform rt, float angle, float speed, float duration)
    {
        Vector2 startPos = rt.anchoredPosition;
        Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            rt.anchoredPosition = startPos + direction * t;
            rt.localScale = Vector3.one * (1f - t);
            if (go.GetComponent<Image>() != null)
            {
                var c = go.GetComponent<Image>().color;
                go.GetComponent<Image>().color = new Color(c.r, c.g, c.b, 1f - t);
            }
            yield return null;
        }
        if (go != null) Destroy(go);
    }

    IEnumerator AnimateDamageNumber(GameObject go, RectTransform rt)
    {
        Vector2 startPos = rt.anchoredPosition;
        float elapsed = 0f;
        float duration = 0.8f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            rt.anchoredPosition = startPos + Vector2.up * (80f * t);
            if (go.GetComponent<TextMeshProUGUI>() != null)
            {
                var c = go.GetComponent<TextMeshProUGUI>().color;
                go.GetComponent<TextMeshProUGUI>().color = new Color(c.r, c.g, c.b, 1f - t);
            }
            yield return null;
        }
        if (go != null) Destroy(go);
    }

    IEnumerator ShakeTransform(Transform target, float intensity, float duration)
    {
        Vector3 originalPos = target.localPosition;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float shake = intensity * (1f - t);
            target.localPosition = originalPos + new Vector3(
                Random.Range(-shake, shake),
                Random.Range(-shake, shake),
                0f);
            yield return null;
        }
        target.localPosition = originalPos;
    }

    // ── 卡牌 UI 创建（纯视觉）──

    public GameObject CreateCardUI(CardData data)
    {
        var go = new GameObject("Card_" + data.cardName);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(240, 340);

        // 圆角遮罩
        var maskGo = new GameObject("CardMask");
        maskGo.transform.SetParent(go.transform, false);
        var maskRt = maskGo.AddComponent<RectTransform>();
        maskRt.anchorMin = Vector2.zero; maskRt.anchorMax = Vector2.one;
        maskRt.offsetMin = Vector2.zero; maskRt.offsetMax = Vector2.zero;
        var maskImg = maskGo.AddComponent<Image>();
        if (_roundedRectSprite == null)
            _roundedRectSprite = GetRoundedRectSprite(240, 340, cardCornerRadius);
        maskImg.sprite = _roundedRectSprite;
        maskImg.color = Color.white;
        maskImg.type = Image.Type.Sliced;
        maskImg.raycastTarget = false;
        var mask = maskGo.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        // ── CardBorder ──
        var borderGo = new GameObject("CardBorder");
        borderGo.transform.SetParent(maskGo.transform, false);
        var borderRt = borderGo.AddComponent<RectTransform>();
        borderRt.anchorMin = Vector2.zero; borderRt.anchorMax = Vector2.one;
        borderRt.offsetMin = new Vector2(-3, -3); borderRt.offsetMax = new Vector2(3, 3);
        var borderImg = borderGo.AddComponent<Image>();
        borderImg.color = new Color(0.75f, 0.58f, 0.15f, 1f);
        borderImg.raycastTarget = false;

        // ── BG (Background) ──
        var bgGo = new GameObject("BG");
        bgGo.transform.SetParent(maskGo.transform, false);
        var bgRt = bgGo.AddComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;
        var bgImg = bgGo.AddComponent<Image>();
        string bgName = data.element == Element.Fire ? "Fire_BG" : data.element == Element.Water ? "Water_BG" : "Wind_BG";
        bgImg.sprite = LoadSprite(bgName);
        bgImg.color = Color.white;

        // ── Frame ──
        var frameGo = new GameObject("Frame");
        frameGo.transform.SetParent(maskGo.transform, false);
        var frameRt = frameGo.AddComponent<RectTransform>();
        frameRt.anchorMin = Vector2.zero; frameRt.anchorMax = Vector2.one;
        frameRt.offsetMin = new Vector2(2, 2); frameRt.offsetMax = new Vector2(-2, -2);
        var frameImg = frameGo.AddComponent<Image>();
        frameImg.sprite = LoadSprite("CardFrame");
        frameImg.color = Color.white;
        frameImg.raycastTarget = false;

        // ── Art ──
        string artName = data.element == Element.Fire ? "Fire_Art" : data.element == Element.Water ? "Water_Art" : "Wind_Art";
        var artGo = new GameObject("Art");
        artGo.transform.SetParent(maskGo.transform, false);
        var artRt = artGo.AddComponent<RectTransform>();
        artRt.anchoredPosition = new Vector2(0, 30);
        artRt.sizeDelta = new Vector2(200, 110);
        var artImg = artGo.AddComponent<Image>();
        artImg.sprite = LoadSprite(artName);
        artImg.color = new Color(1, 1, 1, 0.5f);
        artImg.raycastTarget = false;

        // ── ManaCrystal / CostText ──
        var crystalGo = new GameObject("ManaCrystal");
        crystalGo.transform.SetParent(go.transform, false);
        var crystalRt = crystalGo.AddComponent<RectTransform>();
        crystalRt.anchoredPosition = new Vector2(-88, 140);
        crystalRt.sizeDelta = new Vector2(50, 50);
        var crystalImg = crystalGo.AddComponent<Image>();
        crystalImg.sprite = LoadSprite("ManaCrystal");
        crystalImg.color = Color.white;
        crystalImg.raycastTarget = false;

        var costGo = new GameObject("CostText");
        costGo.transform.SetParent(crystalGo.transform, false);
        var costRt = costGo.AddComponent<RectTransform>();
        costRt.anchorMin = Vector2.zero; costRt.anchorMax = Vector2.one;
        costRt.offsetMin = Vector2.zero; costRt.offsetMax = Vector2.zero;
        var costText = CreateTMP(costGo);
        costText.alignment = TextAlignmentOptions.Center; costText.text = data.cost.ToString();
        costText.fontSize = 26; costText.color = Color.white;
        costText.raycastTarget = false;

        // ── ElementBadge / ElementText ── (已移除)

        // ── NameText ──
        var nameGo = new GameObject("NameText");
        nameGo.transform.SetParent(go.transform, false);
        var nameRt = nameGo.AddComponent<RectTransform>();
        nameRt.anchoredPosition = new Vector2(0, 115);
        nameRt.sizeDelta = new Vector2(180, 36);
        var nameText = CreateTMP(nameGo);
        nameText.fontSize = 24; nameText.color = new Color(1f, 0.92f, 0.7f);
        nameText.alignment = TextAlignmentOptions.Center; nameText.text = data.cardName;
        nameText.raycastTarget = false;

        // ── DescPanel ──
        var descPanelGo = new GameObject("DescPanel");
        descPanelGo.transform.SetParent(maskGo.transform, false);
        var dpRt = descPanelGo.AddComponent<RectTransform>();
        dpRt.anchoredPosition = new Vector2(0, -80);
        dpRt.sizeDelta = new Vector2(220, 100);
        var dpImg = descPanelGo.AddComponent<Image>();
        dpImg.sprite = LoadSprite("DescPanel");
        dpImg.color = new Color(0.05f, 0.05f, 0.08f, 0.9f);
        dpImg.raycastTarget = false;

        // ── DescText ──
        var descGo = new GameObject("DescText");
        descGo.transform.SetParent(go.transform, false);
        var descRt = descGo.AddComponent<RectTransform>();
        descRt.anchoredPosition = new Vector2(0, -80);
        descRt.sizeDelta = new Vector2(210, 90);
        var descText = CreateTMP(descGo);
        descText.fontSize = 18; descText.color = new Color(0.95f, 0.95f, 0.95f, 1f);
        descText.alignment = TextAlignmentOptions.Center; descText.text = data.GetDescription();
        descText.overflowMode = TextOverflowModes.Ellipsis;
        descText.raycastTarget = false;

        // ── AttackIcon / AttackText (右上角) ──
        var atkIconGo = new GameObject("AttackIcon");
        atkIconGo.transform.SetParent(go.transform, false);
        var atkIconRt = atkIconGo.AddComponent<RectTransform>();
        atkIconRt.anchoredPosition = new Vector2(82, 140);
        atkIconRt.sizeDelta = new Vector2(48, 48);
        var atkIconImg = atkIconGo.AddComponent<Image>();
        atkIconImg.sprite = LoadSprite("AttackIcon");
        atkIconImg.color = Color.white;
        atkIconImg.raycastTarget = false;
        atkIconGo.SetActive(data.damage > 0);

        var atkTextGo = new GameObject("AttackText");
        atkTextGo.transform.SetParent(atkIconGo.transform, false);
        var atkTextRt = atkTextGo.AddComponent<RectTransform>();
        atkTextRt.anchorMin = Vector2.zero; atkTextRt.anchorMax = Vector2.one;
        atkTextRt.offsetMin = Vector2.zero; atkTextRt.offsetMax = Vector2.zero;
        var attackText = CreateTMP(atkTextGo);
        attackText.fontSize = 24; attackText.color = Color.white;
        attackText.alignment = TextAlignmentOptions.Center; attackText.text = data.damage.ToString();
        attackText.raycastTarget = false;

        // ── HealthIcon / HealthText (右上角，与攻击图标同位置，不会同时出现) ──
        int healOrShield = Mathf.Max(data.heal, data.shield);
        var hpIconGo = new GameObject("HealthIcon");
        hpIconGo.transform.SetParent(go.transform, false);
        var hpIconRt = hpIconGo.AddComponent<RectTransform>();
        hpIconRt.anchoredPosition = new Vector2(82, 140);
        hpIconRt.sizeDelta = new Vector2(48, 48);
        var hpIconImg = hpIconGo.AddComponent<Image>();
        hpIconImg.sprite = LoadSprite("HealthIcon");
        hpIconImg.color = Color.white;
        hpIconImg.raycastTarget = false;
        hpIconGo.SetActive(healOrShield > 0);

        var hpTextGo = new GameObject("HealthText");
        hpTextGo.transform.SetParent(hpIconGo.transform, false);
        var hpTextRt = hpTextGo.AddComponent<RectTransform>();
        hpTextRt.anchorMin = Vector2.zero; hpTextRt.anchorMax = Vector2.one;
        hpTextRt.offsetMin = Vector2.zero; hpTextRt.offsetMax = Vector2.zero;
        var healText = CreateTMP(hpTextGo);
        healText.fontSize = 24; healText.color = Color.white;
        healText.alignment = TextAlignmentOptions.Center; healText.text = healOrShield.ToString();
        healText.raycastTarget = false;

        // ── Outline ──
        var outline = go.AddComponent<Outline>();
        outline.effectColor = new Color(0, 0, 0, 0.85f);
        outline.effectDistance = new Vector2(2, -2);

        // ── CardUI component ──
        var cardUI = go.AddComponent<CardUI>();
        cardUI.cardData = data;
        cardUI.backgroundImage = bgImg;
        cardUI.frameImage = frameImg;
        cardUI.artImage = artImg;
        cardUI.nameText = nameText;
        cardUI.costText = costText;
        cardUI.descriptionText = descText;

        return go;
    }

    /// <summary>Recursively find a TMP_Text component in children by name.</summary>
    TMP_Text FindTMPInChildren(GameObject parent, string name)
    {
        var t = parent.transform.Find(name);
        if (t != null) return t.GetComponent<TMP_Text>();
        // Deep search
        foreach (Transform child in parent.transform)
        {
            var found = FindTMPInChildren(child.gameObject, name);
            if (found != null) return found;
        }
        return null;
    }

    /// <summary>Create a TextMeshProUGUI with the card font assigned.</summary>
    TextMeshProUGUI CreateTMP(GameObject parent)
    {
        var tmp = parent.AddComponent<TextMeshProUGUI>();
        if (_cardFont != null)
            tmp.font = _cardFont;
        return tmp;
    }

    Sprite LoadSprite(string name)
    {
        // Try Resources.Load first (works in both editor and builds)
        var sprite = Resources.Load<Sprite>("CardArt/" + name);
        if (sprite != null) return sprite;

#if UNITY_EDITOR
        // Editor-only fallback: load directly from AssetDatabase
        string[] paths = {
            "Assets/Art/Cards/" + name + ".png",
            "Assets/Art/Effects/" + name + ".png",
            "Assets/Art/UI/" + name + ".png",
            "Assets/Textures/" + name + ".png",
        };
        foreach (var path in paths)
        {
            sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null) return sprite;
            var tex = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex != null)
                return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f, 100f);
        }
#endif
        return null;
    }

    void OnDestroy()
    {
        if (_bm != null)
        {
            _bm.OnDrawCard -= DrawCard;
            _bm.OnDiscardHand -= DiscardHand;
            _bm.OnRestoreEnergy -= RestoreEnergy;

            var tm = _bm.TurnManager;
            if (tm != null)
            {
                tm.OnPlayerTurnStart -= HandlePlayerTurnStart;
                tm.OnEnemyTurnStart -= HandleEnemyTurnStart;
                tm.OnStateChanged -= HandleStateChanged;
            }
        }
    }
}
