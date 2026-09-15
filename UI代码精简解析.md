# 地牢·先锋 — UI代码精简解析

> 按界面层分类，每类提炼核心代码与设计要点

---

## 一、战斗核心

### BattleUI — 战斗HUD核心

唯一牌堆管理器，~1184行，全代码构建。

```csharp
// 程序化卡牌构建：10+层UI层级零Prefab
public GameObject CreateCardUI(CardData data) {
    var go = new GameObject("Card_" + data.cardName);
    // 圆角遮罩(程序化生成Sprite) → 元素背景 → 边框 → 费用水晶 → 卡名 → 描述
    bgImg.sprite = LoadSprite(data.element == Element.Fire ? "Fire_BG" : ...);
    descText.text = data.GetDescription();  // CardData自动生成
}

// 扇形手牌布局：每帧Lerp插值
float offset = i - (count - 1) * 0.5f;
float yArc = Mathf.Abs(offset) * fanArcHeight;   // 越靠边越低
float angle = -offset * fanAngleStep;               // 越靠边越倾斜
if (isHovered) { targetPos.y += hoverLift; }        // 悬停上浮180px
rt.anchoredPosition = Vector2.Lerp(/*当前*/, targetPos, Time.deltaTime * 12f);

// 事件订阅：BattleManager + TurnManager
_bm.OnDrawCard += DrawCard;
_bm.OnDiscardHand += DiscardHand;
tm.OnPlayerTurnStart += HandlePlayerTurnStart;
```

要点：`CardData`是唯一数据源，`LoadSprite()`双重回退(`Resources.Load`→`AssetDatabase`)，扇形=偏移+倾斜+弧形下沉。

### CardUI — 卡牌视觉绑定

```csharp
public void Setup(CardData data) { /* 绑定背景/框架/名称/费用/描述 */ }
public void SetEnergySufficient(bool sufficient) {
    // 不足时降低alpha+灰化，恢复时还原
}
```

### CardDragHandler — 拖拽出牌

```csharp
// 关键：拖拽时关闭射线，让DropZone能接收
_canvasGroup.blocksRaycasts = false;

// 放置检测：RaycastAll匹配Tag或名称
foreach (var r in results)
    if (r.gameObject.CompareTag("DropZone") || r.gameObject.name.Contains("Enemy"))
        { validDrop = true; break; }
if (!validDrop) _rt.anchoredPosition = _originalPosition;  // 无效→回弹

// 拖尾粒子：每0.03s生成，随机偏移+漂移+缩小淡出
```

### CardHoverEffect — 悬停发光与灰化

```csharp
// 悬停：Outline切换为金色发光
_outline.effectColor = glowColor;  // (1f, 0.85f, 0.3f)

// 灰化：延迟缓存策略，首次灰化才遍历子Image
public void SetGrayed(bool grayed) {
    if (grayed) foreach (img) img.color = new Color(0.45f, 0.45f, 0.45f, img.color.a);
    else        foreach (kv)   img.color = kv.Value;  // 逐个还原
}
// _isGrayed优先于_isHovered
```

### CardPlayAnimation — 贝塞尔弧线出牌

```csharp
// 二次贝塞尔：P = (1-t)²·P₀ + 2(1-t)t·P₁ + t²·P₂
Vector2 mid = (startPos + targetPos) * 0.5f + Vector2.up * 80f;
// 正弦摇摆：中点倾斜最大±8°
rt.localEulerAngles = new Vector3(0, 0, Mathf.Sin(t * Mathf.PI) * 8f);
// 两阶段：飞行0.5s → 淡出0.2s → onComplete执行伤害逻辑
```

### ComboEffectSystem — 元素连击VFX (Singleton)

```csharp
// 三种连击粒子运动模式完全不同：
if (FireStorm)  dir = 螺旋(角度递增+半径递增, 转2圈);  // 火+风
if (IceSpikes)  dir = 上冲(X随机, Y强制向上);           // 水+风
if (Steam)      dir = 缓慢上升扩散;                      // 火+水
// 每次 = 屏幕闪光 + 60粒子 + 文字弹窗，三者并行协程
```

### HPBarUI — 血条/护盾/能量

```csharp
private IDamageable _boundEntity;  // 接口绑定，Player/Enemy共用
// Lerp追赶：显示值平滑趋向实际血量
_displayedHP = Mathf.Lerp(_displayedHP, _boundEntity.CurrentHP, Time.deltaTime * 5f);
// 三段变色
hpFill.color = ratio > 0.5f ? hpGreen : ratio > 0.25f ? hpYellow : hpRed;
// 能量图标：i < currentEnergy 则亮
```

### DamageText — 浮动伤害数字 (静态工厂)

```csharp
DamageText.Show(parent, pos, 15, DamageType.Damage);  // 一行调用，自动创建+销毁
// 三段缩放：0→1.2x(弹入)→1.0x(回弹)→0.9x(淡出) + 上浮50px
// 红=伤害/绿=治疗/蓝=护盾/金=暴击(1.5倍字号)
```

### DamageableEntity — IDamageable桥接

```csharp
public class DamageableEntity : MonoBehaviour, IDamageable
// HP/护盾吸收/治疗/重置，事件OnHPChanged/OnShieldChanged
```

---

## 二、场景UI

### DeckViewPopup — 牌库查看弹窗

```csharp
public void Initialize(List<CardData> deck, List<CardData> discard, TMP_FontAsset font) {
    BuildUI();  // 全代码构建：遮罩+面板+标签页+滚动列表+关闭按钮
    ShowDeck();
}
// 标签页切换：牌库/弃牌
// 每条目：费用(蓝色26pt) + 卡名+描述(金色20pt)
// ESC键关闭
```

### ElementSelectionUI — 元素选择

```csharp
// 点击元素 → PlayerPrefs保存 → GameManager.StartNewGame(element)
```

### MainMenuUI — 主菜单

```csharp
// 新游戏/继续(PlayerPrefs检测存档)/退出
// → 加载ElementSelection场景
```

### MapUI — 地图导航

```csharp
// 垂直节点图：战斗/精英/休息/事件/Boss
// 节点类型×(独立sprite+颜色)，已完成显示✓，点击可达节点前进
```

### RewardPopupUI — 奖励选择

```csharp
public void ShowCardReward(List<CardData> cards, int gold) {
    // 无Prefab时 → CreateSimpleCard() 全代码构建卡牌
    // 选中效果协程：
    //   1. punch-zoom: 1.0x→1.35x→1.15x
    //   2. 金色发光描边(Outline effectDistance→8px)
    //   3. 24粒子向外爆发(360°均匀分布)
    //   4. 其他卡牌CanvasGroup淡出+缩小
}
// OnCardSelectedEvent → RewardSceneInit → GameManager.AddCardToDeck
// 也可ShowEvent()显示事件弹窗
```

### RewardSceneInit — 奖励场景引导

```csharp
void Start() {
    // 1. WireUIReferences(): 递归查找绑定titleText/goldText/cardContainer/skipButton
    // 2. Resources.LoadAll<CardData> → 随机抽3张
    // 3. 订阅OnCardSelectedEvent → gm.AddCardToDeck(card)
}
// 递归查找工具：FindTMPInChildren / FindButtonInChildren
```

### GameOverUI — 结算界面

```csharp
public void ShowResult(bool victory, int floor, int kills, int gold)
// 胜利=金色标题(#d9b34d) / 失败=暗红(#b32626)
// 重新开始 / 返回主菜单
```

### CardDisplay — 卡牌视觉设置

```csharp
public void Setup(CardData data) {
    cardData = data;
    RefreshDisplay();  // 绑定name/cost/damage/description/element/artwork
}
// Inspector或运行时赋值均可，Start()自动刷新
```

---

## 三、音频与样式

### SoundManager — 程序化音频合成 (Singleton)

```csharp
public void PlayTone(float freq, float duration, float volume, int waveType) {
    float envelope = Mathf.Exp(-t * (3f / duration));  // 指数衰减包络
    // waveType: 0=正弦(柔和) 1=方波(打击) 2=锯齿(粗糙)
    sample = Mathf.Sin(2f * Mathf.PI * freq * t);
    data[i] = sample * envelope * volume;
}
// 预设：悬停=800Hz正弦 / 伤害=150Hz方波 / 治疗=C5-E5-G5琶音
// 外部MP3优先，回退到程序化合成
```

### UIStyleGuide — 全局样式 (Static)

```csharp
public static class UIStyleGuide {
    public static readonly Color Fire = new Color(1.0f, 0.27f, 0.0f);  // readonly=运行时常量
    public const int TitleSize = 48;                                    // const=编译期常量
    public const float CardWidth = 220f;
    public static Color GetElementColor(Element e) { /* Fire/Water/Wind映射 */ }
}
// 一处修改全局生效，新增面板引用UIStyleGuide.Gold而非硬编码
```

### BGMManager / UISoundHandler

> ⏳ 待实现。设计：BGMManager控制背景音乐播放/切换；UISoundHandler挂载到UI元素，自动播放悬停+点击音效。

---

## 四、视频

### IntroVideoController / IntroVideoPlayer / GifPlayer

> ⏳ 待实现。设计：开场视频流程控制+播放；GIF帧序列播放器。
