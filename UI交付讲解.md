# 地牢·先锋 — UI交付讲解

> 2026/7/15 | UI交互与视觉美术 | Unity UGUI + TMP + C#

---

## 一、项目概述

中文roguelike卡牌构筑游戏。选火/水/风元素 → 组卡组 → 闯5层地牢 → 击败Boss。核心：**元素连击反应**。

```
主菜单 → 元素选择 → 地图 → 战斗 → 奖励 → … → Boss → 胜负
```

---

## 二、UI界面层架构

**纯代码程序化UI**，零Prefab依赖。设计模式：单例 + 接口驱动(`IDamageable`) + 事件驱动(`TurnManager`) + 静态工厂(`DamageText.Show()`)

### 战斗核心

| 脚本 | 类型 | 职责 |
|------|------|------|
| **BattleUI** | MB | 战斗HUD核心。管理牌堆/手牌/弃牌堆，`CreateCardUI`/`PlayCard`/`RefreshHandCosts`。订阅`BattleManager`+`TurnManager`事件 |
| **CardUI** | MB | `Setup(CardData)`, `SetEnergySufficient(bool)`。卡牌视觉(背景/框架/名称/费用/描述) |
| **CardDragHandler** | MB | `IBeginDragHandler/IDragHandler/IEndDragHandler`。`OnDragEnded=Action<bool,Vector2>` → BattleUI.PlayCard |
| **CardHoverEffect** | MB | `IPointerEnterHandler/IPointerExitHandler`。悬停放大/灰化 |
| **CardPlayAnimation** | MB | `PlayArc(target, onComplete)`。出牌贝塞尔弧线动画 |
| **ComboEffectSystem** | MB | Singleton。`Initialize(flashOverlay, particleLayer, textLayer)`。元素反应VFX |
| **HPBarUI** | MB | 血条/护盾/能量。`SetEnergy(current, max)` |
| **DamageText** | MB | 浮动伤害数字。静态工厂`Show()` |
| **DamageableEntity** | MB | `: IDamageable`。UI实体桥接 |

### 场景UI

| 脚本 | 类型 | 职责 |
|------|------|------|
| **DeckViewPopup** | MB | `Initialize(deck, discard, allCards, font)`。牌组/弃牌/全部标签页 |
| **ElementSelectionUI** | MB | 元素选择 → `GameManager.StartNewGame(Element)` |
| **MainMenuUI** | MB | 开始/继续 → 加载ElementSelection场景 |
| **MapUI** | MB | 地图节点导航(战斗/精英/休息/事件/Boss) |
| **RewardPopupUI** | MB | `ShowCardReward(cards, gold)`。选卡 → `GameManager.AddCardToDeck` |
| **RewardSceneInit** | MB | 奖励场景引导 → 驱动`RewardPopupUI` |
| **GameOverUI** | MB | `ShowResult(victory, floor, kills, gold)` |
| **CardDisplay** | MB | `Setup(CardData)` 卡牌视觉设置 |

### 音频与样式

| 脚本 | 类型 | 职责 |
|------|------|------|
| **SoundManager** | MB | Singleton。程序化音频合成：`PlayCardHover/Draw/Play`, `PlayDamage/Heal/Shield/Combo/EndTurn` |
| **BGMManager** | MB | 背景音乐控制 |
| **UISoundHandler** | MB | UI悬停+点击音效，挂载到UI元素 |
| **UIStyleGuide** | Static | 共享样式常量(配色/字号/尺寸) + 元素颜色查找 |

### 视频

| 脚本 | 类型 | 职责 |
|------|------|------|
| **IntroVideoController** | MB | 开场视频流程控制 |
| **IntroVideoPlayer** | MB | 开场视频播放 |
| **GifPlayer** | MB | GIF帧序列播放 |

---

## 四、视觉美术

- **样式系统**(`UIStyleGuide`)：`static class` + `readonly`(颜色)/`const`(字号尺寸)，一处修改全局生效
- **配色**：暗背景`#1a1a2e` / 火`#e74c3c` / 水`#3498db` / 风`#2ecc71` / 金`#d9b34d`
- **字体**：`msyhl SDF`(中文) / `msyhbd SDF`(加粗)
- **资源**：卡牌UI + 元素图标 + 地图图标 + 533张敌人素材

---

## 五、特效系统

**元素连击**（火+水=蒸汽 / 火+风=火风暴 / 水+风=冰刺）：屏幕闪光 + 60粒子 + 文字弹窗并行播放，每种连击粒子运动不同（螺旋/上冲/扩散）。

**三重反馈**：微(卡牌发光) → 中(出牌动画/飘字) → 宏(屏幕震动/全屏闪光)。

---

## 六、重要代码讲解

### 6.1 程序化卡牌构建

```csharp
public GameObject CreateCardUI(CardData data)
{
    var go = new GameObject("Card_" + data.cardName);
    // 圆角遮罩(程序化生成Sprite) + 元素背景 + 费用水晶 + 卡名 + 描述
    bgImg.sprite = LoadSprite(data.element == Element.Fire ? "Fire_BG" : ...);
    descText.text = data.GetDescription();  // CardData自动生成
}
```

要点：`CardData`是唯一数据源，新增卡牌零UI工作量。`LoadSprite()`双重回退：`Resources.Load` → `AssetDatabase`。

### 6.2 扇形手牌布局

```csharp
float offset = i - (count - 1) * 0.5f;
float yArc = Mathf.Abs(offset) * fanArcHeight;   // 越靠边越低
float angle = -offset * fanAngleStep;               // 越靠边越倾斜
if (isHovered) { targetPos.y += hoverLift; }        // 悬停上浮
rt.anchoredPosition = Vector2.Lerp(/*当前*/, targetPos, Time.deltaTime * 12f);  // 平滑
```

要点：扇形 = 位置偏移 + 角度倾斜 + 弧形下沉。Lerp产生丝滑滑动感。

### 6.3 拖拽与放置检测

```csharp
public void OnBeginDrag(PointerEventData eventData) {
    _canvasGroup.blocksRaycasts = false;  // 关键：关闭射线让DropZone能接收
}
public void OnEndDrag(PointerEventData eventData) {
    foreach (var r in results)
        if (r.gameObject.CompareTag("DropZone")) { validDrop = true; break; }
    if (!validDrop) _rt.anchoredPosition = _originalPosition;  // 无效→回弹
}
```

要点：`blocksRaycasts=false`是核心，否则卡牌挡住DropZone。拖尾粒子每0.03s生成。

### 6.4 贝塞尔出牌动画

```csharp
// 二次贝塞尔：P = (1-t)²·P₀ + 2(1-t)t·P₁ + t²·P₂
Vector2 mid = (startPos + targetPos) * 0.5f + Vector2.up * 80f;
rt.localEulerAngles = new Vector3(0, 0, Mathf.Sin(t * Mathf.PI) * 8f);  // 正弦摇摆
// 飞行0.5s → 淡出0.2s → onComplete回调执行伤害逻辑
```

### 6.5 连击粒子运动

```csharp
if (FireStorm)  dir = 螺旋(角度递增+半径递增, 转2圈);
if (IceSpikes)  dir = 上冲(X随机, Y强制向上);
if (Steam)      dir = 缓慢上升扩散;
```

### 6.6 伤害飘字

```csharp
DamageText.Show(parent, pos, 15, DamageType.Damage);  // 一行调用，自动创建+销毁
// 三段缩放：0→1.2x(弹入)→1.0x(回弹)→0.9x(淡出) + 上浮50px
// 红=伤害/绿=治疗/蓝=护盾/金=暴击(1.5倍字号)
```

### 6.7 卡牌灰化

```csharp
public void SetGrayed(bool grayed) {
    if (grayed) foreach (img) img.color = new Color(0.45f, 0.45f, 0.45f, img.color.a);
    else        foreach (kv)   img.color = kv.Value;  // 缓存还原
}
```

要点：延迟缓存(首次灰化才遍历)，`_isGrayed`优先于`_isHovered`。

### 6.8 血条接口绑定

```csharp
private IDamageable _boundEntity;  // Player和Enemy共用
_displayedHP = Mathf.Lerp(_displayedHP, _boundEntity.CurrentHP, Time.deltaTime * 5f);
hpFill.color = ratio > 0.5f ? hpGreen : ratio > 0.25f ? hpYellow : hpRed;
```

### 6.9 程序化音效

```csharp
float envelope = Mathf.Exp(-t * (3f / duration));  // 指数衰减包络
// 正弦=柔和 / 方波=打击 / 锯齿=粗糙，外部MP3优先，回退到合成
```

---

## 七、技术亮点

1. **全程序化UI** — 零Prefab，代码生成全部UI
2. **统一样式系统** — `UIStyleGuide`集中管理
3. **元素连击特效** — 3种连击×独立粒子运动
4. **三重视觉反馈** — 微/中/宏
5. **程序化音效** — 三种波形合成，无需音频文件
6. **贝塞尔出牌** — 弧线+正弦旋转+淡出
7. **数据驱动** — `CardData`自动生成描述

---

## 八、交付状态

| 交付物 | 状态 |
|--------|------|
| UI脚本(18个) | ✅ |
| 全局样式/卡牌交互/战斗UI | ✅ |
| 连击特效/飘字/血条/音效 | ✅ |
| 牌库/奖励/地图/结算 | ✅ |
| 美术资源/字体 | ✅ |
| 场景文件 | ⏳ 待创建 |

> 全部UI代码已完成，待创建场景文件后即可集成测试。
