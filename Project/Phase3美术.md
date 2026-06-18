# Phase3 美术接入方案

## 目标

将 Phase3 当前偏原型的 UI 面板替换为可交付的美术 UI。第一轮只处理订单面板 `Panel_Order`，完成后再用同样方式推进 `Panel_Shape`、`Panel_Glaze`、`Panel_Firing`、`Panel_Result`。

本任务不重写 Phase3 的订单、器型、釉料、烧制、结算业务逻辑，只替换 UI 表现层，并补齐必要的数据绑定。

## 当前代码依据

订单面板相关代码：

- `Assets/Phase3/Scripts/UI/OrderPanelController.cs`
- `Assets/Phase3/Scripts/Core/GameManager.cs`
- `Assets/Phase3/Scripts/Systems/Order/OrderManager.cs`
- `Assets/Phase3/Scripts/Data/OrderData.cs`
- `Assets/Phase3/Editor/Phase3SceneBuilder.cs`

现有 `OrderPanelController` 字段：

```csharp
[SerializeField] private OrderManager orderManager;
[SerializeField] private GameManager gameManager;
[SerializeField] private Button acceptButton;
[SerializeField] private Text orderNameText;
[SerializeField] private Text targetShapeText;
[SerializeField] private Text rewardSilverText;
[SerializeField] private Text rewardReputationText;
```

重要约束：第一轮改造应尽量保留这些字段名，避免 Unity 已有序列化引用因为重命名而丢失。

如确实需要重命名字段，必须通过 Unity Inspector 重新挂接引用，并在提交说明里明确列出。

## GameManager 关键引用

替换 `Panel_Order` 或改成美术 Prefab 后，必须重新检查 `GameManager` 上的两个引用：

```csharp
[SerializeField] private GameObject panelOrder;
[SerializeField] private OrderPanelController orderPanelController;
```

原因：

- `panelOrder` 会被 `UpdatePanels()` 用于 `SetActive()` 显隐切换。
- `orderPanelController` 会在 `BeginOrder()` 中被直接调用：

```csharp
orderPanelController.ShowOrder(currentOrder);
```

因此，替换订单面板 Prefab 后，仅挂好 `OrderPanelController` 自己的字段还不够，还必须确保 `GameManager.panelOrder` 指向新的 `Panel_Order`，`GameManager.orderPanelController` 指向新面板上的 `OrderPanelController`。

## 当前订单显示流程

现有流程有两个入口：

```text
GameManager.BeginOrder()
└── orderPanelController.ShowOrder(currentOrder)

OrderPanelController.OnEnable()
└── Refresh()
    └── orderManager.GetCurrentOrder()
        └── ShowOrder(order)
```

当前这两个入口数据同源，所以暂时不会冲突。但后续如果订单数据来自桥接层、任务系统、剧情系统或临时预览数据，`OnEnable -> Refresh()` 可能覆盖外部传入的订单。

第一轮建议保持现状，但在方案和代码注释中明确：

- `ShowOrder(OrderData order)` 是外部注入显示入口。
- `Refresh()` 是从 `OrderManager` 自驱刷新入口。
- 若后续出现非 `OrderManager` 数据源，应增加 `currentDisplayedOrder` 缓存或关闭 `OnEnable` 自动刷新。

## 订单面板 Prefab 方案

建议新增目录：

```text
Assets/Phase3/Art/UI/
Assets/Phase3/Art/UI/Sprites/
Assets/Phase3/Art/UI/Fonts/
Assets/Phase3/Prefabs/UI/
```

命名统一使用 PascalCase，贴近现有 C#、Prefab 命名风格：

```text
Assets/Phase3/Prefabs/UI/OrderPanelArt.prefab
Assets/Phase3/Art/UI/Sprites/OrderPanelBg.png
Assets/Phase3/Art/UI/Sprites/OrderSeal.png
Assets/Phase3/Art/UI/Sprites/IconSilver.png
Assets/Phase3/Art/UI/Sprites/IconReputation.png
Assets/Phase3/Art/UI/Sprites/StarOn.png
Assets/Phase3/Art/UI/Sprites/StarOff.png
```

## 推荐 UI 层级

第一轮可使用以下结构：

```text
Panel_Order
├── Img_Background
├── Img_TitleRibbon
├── Img_OrderSeal
├── Text_OrderName
├── Group_Requirement
│   ├── Text_RequirementTitle
│   ├── Text_Shape
│   ├── Text_Glaze
│   └── Group_Difficulty
│       ├── Star_1
│       ├── Star_2
│       ├── Star_3
│       ├── Star_4
│       └── Star_5
├── Group_Reward
│   ├── Icon_Silver
│   ├── Text_Silver
│   ├── Icon_Reputation
│   └── Text_Reputation
└── Btn_Accept
    └── Text_Label
```

说明：

- `Text_OrderName` 绑定现有 `orderNameText`。
- `Text_Shape` 绑定现有 `targetShapeText`。
- `Text_Silver` 绑定现有 `rewardSilverText`。
- `Text_Reputation` 绑定现有 `rewardReputationText`。
- `Text_Glaze` 第一轮可新增字段，但要明确它的数据源目前只是 `OrderData.requiredGlazeID`。
- 不建议第一轮加入 `Text_Description`，因为当前 `OrderData` 没有描述字段。若一定要做描述区域，应先决定描述来自 `ShapeConfigSO.description`、`GlazeConfigSO.description`，还是新增 `OrderData.displayDescription`。

## OrderPanelController 改造建议

第一轮建议保留现有字段名，并只追加字段：

```csharp
[SerializeField] private Text targetGlazeText;
[SerializeField] private Image[] difficultyStars;
[SerializeField] private Sprite starOnSprite;
[SerializeField] private Sprite starOffSprite;
```

不建议直接把现有字段改成：

```csharp
private Text shapeText;
private Text silverText;
private Text reputationText;
```

因为这会让现有 `targetShapeText`、`rewardSilverText`、`rewardReputationText` 的序列化引用失效。

推荐显示逻辑：

```csharp
SetText(orderNameText, $"订单：{order.orderName}");
SetText(targetShapeText, $"目标器型：{ResolveShapeName(order.requiredShapeID)}");
SetText(targetGlazeText, $"目标釉色：{ResolveGlazeName(order.requiredGlazeID)}");
SetText(rewardSilverText, $"银两奖励：{order.baseGold}");
SetText(rewardReputationText, $"声望奖励：{order.baseReputation}");
SetDifficulty(order.difficulty);
```

如果暂时不做配置解析，第一轮可先显示 ID：

```csharp
SetText(targetShapeText, $"目标器型：{order.requiredShapeID}");
SetText(targetGlazeText, $"目标釉色：{order.requiredGlazeID}");
```

但 UI 文案或验收说明中要写清楚：这是临时 ID 显示，不是最终中文名显示。

## 中文名解析方案

项目中已有显示名字段：

- `ShapeConfigSO.shapeID`
- `ShapeConfigSO.nameCN`
- `ShapeConfigSO.nameEN`
- `GlazeConfigSO.glazeID`
- `GlazeConfigSO.nameCN`
- `GlazeConfigSO.nameEN`

因此不建议在 `OrderPanelController` 中写死 ID 到中文名的映射。

可选方案：

### 方案 A：Controller 直接引用配置数组

在 `OrderPanelController` 中新增：

```csharp
[SerializeField] private ShapeConfigSO[] shapeConfigs;
[SerializeField] private GlazeConfigSO[] glazeConfigs;
```

刷新时通过 ID 查找：

```csharp
private string ResolveShapeName(string shapeID)
{
    foreach (var config in shapeConfigs)
    {
        if (config != null && config.shapeID == shapeID)
        {
            return string.IsNullOrEmpty(config.nameCN) ? shapeID : config.nameCN;
        }
    }

    return shapeID;
}
```

优点：实现快，适合第一轮。

缺点：每个面板可能重复写查找逻辑。

### 方案 B：新增 Phase3ConfigLookup

新增一个配置查询组件或 ScriptableObject，集中保存：

```csharp
public ShapeConfigSO[] shapeConfigs;
public GlazeConfigSO[] glazeConfigs;
```

提供：

```csharp
public string GetShapeNameCN(string shapeID);
public string GetGlazeNameCN(string glazeID);
```

优点：后续 Shape、Glaze、Result 面板都能复用。

缺点：第一轮需要多加一个资产和挂接点。

推荐第一轮使用方案 A，第二轮抽成方案 B。

## 中文乱码修复

当前需要重点检查：

- `Assets/Phase3/Scripts/UI/OrderPanelController.cs`
- `Assets/Phase3/Scripts/Data/OrderData.cs`
- `Assets/Phase3/Scripts/Data/ShapeConfigSO.cs`
- `Assets/Phase3/Scripts/Data/GlazeConfigSO.cs`
- `Assets/Phase3/Editor/Phase3SceneBuilder.cs`

第一轮必须先修 `OrderPanelController.cs` 中面向玩家显示的硬编码文案：

```csharp
SetText(orderNameText, "订单：未选择");
SetText(targetShapeText, "目标器型：-");
SetText(rewardSilverText, "银两奖励：0");
SetText(rewardReputationText, "声望奖励：0");
```

修复方式：

1. 用支持 UTF-8 的编辑器打开脚本。
2. 将文件保存为 UTF-8。
3. 不要用错误编码重新打开后再保存。
4. 修复后让 Unity 重新编译。
5. 检查 Console 是否有编译错误。

如果团队编辑器环境不统一，可以选择把 UI 文案从脚本中迁移到配置资产，减少脚本内中文硬编码。

## Phase3SceneBuilder 防护修正

当前 `EnsureOrderPanelControls()` 的真实行为是：

```csharp
Transform panel = canvasTf.Find("Panel_Order");
if (panel == null) return created;

if (panel.Find("Btn_Accept") == null)
{
    var btn = CreateButtonChild(panel, "Btn_Accept", "...", 20);
    SetStretchAnchors(btn, new Vector2(0.7f, 0.1f), new Vector2(0.98f, 0.9f));
    created.Add("Btn_Accept in Panel_Order");
}
```

也就是说，它只检查 `Btn_Accept`，并不检查 `OrderPanelController`。如果美术面板按钮改名，或者按钮不在 `Panel_Order` 的直接子级下，SceneBuilder 会重新生成一个原型按钮。

建议改造：

```csharp
private static List<string> EnsureOrderPanelControls(Transform canvasTf)
{
    var created = new List<string>();
    Transform panel = canvasTf.Find("Panel_Order");
    if (panel == null) return created;

    if (panel.GetComponent<OrderPanelController>() != null)
    {
        return created;
    }

    if (panel.Find("Btn_Accept") == null)
    {
        var btn = CreateButtonChild(panel, "Btn_Accept", "接单", 20);
        SetStretchAnchors(btn, new Vector2(0.7f, 0.1f), new Vector2(0.98f, 0.9f));
        created.Add("Btn_Accept in Panel_Order");
    }

    return created;
}
```

更稳的做法是保留 `Btn_Accept` 这个节点名，避免修改 SceneBuilder 时影响原型同步流程。

如果美术按钮必须放在更深层级，例如：

```text
Panel_Order/Footer/Btn_Accept
```

则 SceneBuilder 不能继续用 `panel.Find("Btn_Accept")`，需要改为递归查找，或约定按钮仍放在直接子级。

第一轮推荐：保留 `Panel_Order/Btn_Accept` 这个直接子级节点。

## 实施步骤

### 1. 准备美术资源

1. 创建 `Assets/Phase3/Art/UI/Sprites/`。
2. 导入背景、按钮、图标、星级等 Sprite。
3. 设置 Texture Type 为 `Sprite (2D and UI)`。
4. 需要拉伸的背景图设置 Sprite Border。

### 2. 制作 OrderPanelArt.prefab

1. 复制当前场景中的 `Panel_Order`。
2. 保留根节点名 `Panel_Order`。
3. 保留或重新挂载 `OrderPanelController`。
4. 保留直接子节点 `Btn_Accept`。
5. 替换背景、装饰、文本和奖励图标。
6. 保存为 `Assets/Phase3/Prefabs/UI/OrderPanelArt.prefab`。

### 3. 调整 OrderPanelController

1. 保留现有序列化字段名。
2. 新增 `targetGlazeText`、`difficultyStars` 等字段。
3. 修复中文硬编码乱码。
4. 可选：接入 `ShapeConfigSO[]`、`GlazeConfigSO[]` 做中文名显示。
5. 明确 `ShowOrder()` 和 `Refresh()` 的职责。

### 4. 替换场景引用

1. 打开 `Assets/Phase3/Scenes/Phase3_Prototype.unity`。
2. 用 `OrderPanelArt.prefab` 替换旧 `Panel_Order`。
3. 检查 `OrderPanelController` 字段：
   - `orderManager`
   - `gameManager`
   - `acceptButton`
   - `orderNameText`
   - `targetShapeText`
   - `targetGlazeText`
   - `rewardSilverText`
   - `rewardReputationText`
   - `difficultyStars`
4. 检查 `GameManager` 字段：
   - `panelOrder`
   - `orderPanelController`

### 5. 调整 Phase3SceneBuilder

1. 避免在已有美术版 `OrderPanelController` 时补原型按钮。
2. 或者保留 `Panel_Order/Btn_Accept` 直接子级，降低改造成本。
3. 修复 SceneBuilder 中会显示到 UI 上的乱码按钮文案。

### 6. 验证

进入 Play Mode 后检查：

1. `GameManager` 初始进入 Order 状态时，只有 `Panel_Order` 显示。
2. 订单名、器型、釉色、银两、声望、难度显示正确。
3. 点击 `Btn_Accept` 能进入 Shape 状态。
4. `GameManager.panelOrder` 能正确控制新面板显隐。
5. `GameManager.orderPanelController` 指向新面板组件。
6. Console 无 Error。
7. 运行 `Phase3/Sync Scene (Ensure Mode)` 后不会额外生成一个原型接单按钮。

## 第一轮验收标准

- `OrderPanelArt.prefab` 已创建。
- `Panel_Order` 美术效果明显区别于原型面板。
- 现有序列化字段没有因重命名丢失。
- `GameManager.panelOrder` 和 `GameManager.orderPanelController` 已重新挂接。
- `ShowOrder()` 能显示当前订单。
- `Refresh()` 不会显示错误订单。
- 接单按钮流程不变，仍调用 `GameManager.GoToShape()`。
- 中文文案没有乱码。
- `Phase3SceneBuilder` 不会破坏美术面板。

## 暂不纳入第一轮

以下内容建议放到第二轮：

- 全量迁移到 TextMeshPro。
- 为 `OrderData` 新增客户头像、订单描述、订单图标。
- 抽象全局 `Phase3ConfigLookup`。
- 为所有 Phase3 面板统一建立 UI 皮肤系统。
- Addressables 化 UI 资源加载。

第一轮目标是把订单面板接稳，跑通“美术 Prefab + 现有业务逻辑 + 正确引用装配”的闭环。
