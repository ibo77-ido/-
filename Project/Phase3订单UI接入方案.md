# Phase3 订单 UI 接入方案

## 目标

将 Phase3 订单面板 `Panel_Order` 接入现有美术素材，形成可复用的订单 UI Prefab，同时保持 Phase3 原有订单流程、按钮行为、GameManager 面板切换逻辑不变。

本方案以 `Project/Phase3美术.md` 为参考，但范围更窄，只覆盖订单 UI 第一轮接入。

## 当前素材

素材位置：

```text
Assets/Phase3/UI/订单面板/订单面板 文字.png
Assets/Phase3/UI/订单面板/订单面板 空.png
Assets/Phase3/UI/订单面板/接单按钮.png
```

素材判断：

- `订单面板 文字.png`：适合作为第一轮背景。它已经包含“订单 / 目标器型 / 银两奖励 / 声望奖励”固定标签，正好匹配当前 `OrderPanelController` 字段。
- `订单面板 空.png`：适合作为第二轮扩展背景。后续要加入釉色、难度、客户说明等更多字段时再使用。
- `接单按钮.png`：适合作为 `Btn_Accept` 的按钮图。

第一轮推荐使用：

```text
背景：订单面板 文字.png
按钮：接单按钮.png
```

## 当前代码绑定点

订单 UI 当前由 `OrderPanelController` 控制：

```csharp
[SerializeField] private OrderManager orderManager;
[SerializeField] private GameManager gameManager;
[SerializeField] private Button acceptButton;
[SerializeField] private Text orderNameText;
[SerializeField] private Text targetShapeText;
[SerializeField] private Text rewardSilverText;
[SerializeField] private Text rewardReputationText;
```

第一轮必须保留这些字段名，避免 Unity Inspector 中已有序列化引用丢失。

`GameManager` 中与订单面板相关的字段：

```csharp
[SerializeField] private GameObject panelOrder;
[SerializeField] private OrderPanelController orderPanelController;
```

替换面板后必须同步检查这两个引用。

## 接入边界

### 可以改

- 可以替换 `Panel_Order` 的视觉层级。
- 可以新增 `Image` 作为订单面板背景。
- 可以用 `接单按钮.png` 替换 `Btn_Accept` 的 `Image.sprite`。
- 可以调整 `Text` 的位置、字号、颜色、对齐方式。
- 可以修复 `OrderPanelController` 中的中文乱码。
- 可以让动态文本只显示值，不再重复显示标签。

例如：

```text
订单：青玉碗      ->  青玉碗
目标器型：SHAPE_001 -> SHAPE_001
银两奖励：120     ->  120
声望奖励：8       ->  8
```

因为背景图已经自带标签，动态文本不需要再重复显示“订单：”“目标器型：”。

### 暂不改

- 不改 `OrderManager` 的订单选择逻辑。
- 不改 `GameManager.GoToShape()` 流程。
- 不改 `GameManager.UpdatePanels()` 面板切换逻辑。
- 不重命名 `Panel_Order`。
- 不重命名 `Btn_Accept`。
- 不重命名 `OrderPanelController` 现有序列化字段。
- 不在第一轮加入 `targetGlazeText`、难度星级、订单描述等新功能。
- 不在第一轮迁移到 TextMeshPro。

### 必须避免

- 不要直接删除 `OrderPanelController`。
- 不要让背景图 `Image` 开启 `raycastTarget`。
- 不要让按钮透明边距形成超大点击区域。
- 不要把 `Btn_Accept` 放到 SceneBuilder 找不到的位置。
- 不要只替换 Phase3 场景，忘记验证 Phase9 桥接场景。

## 推荐 Prefab 层级

第一轮推荐层级：

```text
Panel_Order
├── Img_OrderPanelBg
├── Text_OrderNameValue
├── Text_TargetShapeValue
├── Text_RewardSilverValue
├── Text_RewardReputationValue
└── Btn_Accept
    └── Image_ButtonArt
```

绑定关系：

```text
acceptButton             -> Panel_Order/Btn_Accept
orderNameText            -> Text_OrderNameValue
targetShapeText          -> Text_TargetShapeValue
rewardSilverText         -> Text_RewardSilverValue
rewardReputationText     -> Text_RewardReputationValue
```

背景设置：

```text
Img_OrderPanelBg.sprite = 订单面板 文字.png
Img_OrderPanelBg.raycastTarget = false
```

按钮设置：

```text
Btn_Accept.Image.sprite = 接单按钮.png
Btn_Accept 保持 Button 组件
Btn_Accept 节点名保持不变
```

## 尺寸与布局建议

素材原始尺寸均为 `2048x2048`，透明边距较大。实际 UI 中不建议按原始尺寸直接铺满。

建议：

- `Panel_Order` 参考尺寸：`900 x 720` 或按当前 Canvas 视觉比例微调。
- 背景图使用 `Set Native Size` 后再等比缩放。
- 动态文本放在背景图横线右侧。
- `Btn_Accept` 的 RectTransform 只覆盖真实按钮视觉区域，不覆盖整张 2048 图透明区。
- 背景 `Image.raycastTarget = false`。
- 只有 `Btn_Accept` 接收点击。

建议文本颜色：

```text
深青绿：#0B4A3A 或接近背景标题色
```

建议文本对齐：

```text
Horizontal Overflow: Overflow 或 Wrap
Vertical Overflow: Truncate
Alignment: MiddleLeft
```

## OrderPanelController 调整

第一轮只做轻量调整。

空订单：

```csharp
SetText(orderNameText, "未选择");
SetText(targetShapeText, "-");
SetText(rewardSilverText, "0");
SetText(rewardReputationText, "0");
```

正常订单：

```csharp
SetText(orderNameText, order.orderName);
SetText(targetShapeText, order.requiredShapeID);
SetText(rewardSilverText, order.baseGold.ToString());
SetText(rewardReputationText, order.baseReputation.ToString());
```

说明：

- 第一轮继续显示 `requiredShapeID`，不做中文名解析。
- 因为背景图已经带标签，脚本不再拼接“订单：”“目标器型：”等标签。
- 按钮点击仍然调用 `gameManager.GoToShape()`。

## Phase3SceneBuilder 边界

当前 `Phase3SceneBuilder.EnsureOrderPanelControls()` 只检查：

```csharp
panel.Find("Btn_Accept")
```

如果找不到直接子节点 `Btn_Accept`，它会创建一个原型按钮。

所以第一轮必须满足：

```text
Panel_Order/Btn_Accept
```

如果未来美术按钮需要放在更深层级，必须先改 `Phase3SceneBuilder` 的查找逻辑，否则运行 `Phase3/Sync Scene (Ensure Mode)` 后会多出一个原型按钮。

## Phase9 同步要求

Phase9 不是单独复制一份订单 UI，而是通过 `Phase9InteractionBridge` 加载 `Phase3_Prototype`：

```csharp
SceneManager.LoadSceneAsync(phase3SceneName, LoadSceneMode.Additive);
```

并通过反射读取 `GameManager` 的面板引用：

```csharp
typeof(GameManager).GetField("panelOrder", BindingFlags.NonPublic | BindingFlags.Instance)
```

还会在 `CenterGameplayPanels()` 中按名称居中：

```csharp
target.name == "Panel_Order"
```

因此订单 UI 替换后必须同步验证 Phase9：

- `Panel_Order` 名字不能改。
- `GameManager.panelOrder` 必须指向新的 `Panel_Order`。
- `Phase9InteractionBridge` 加载 Phase3 后仍能找到 `GameManager`。
- `GameplayCanvasGroup` 仍能通过 `panelOrder` 控制订单面板。
- `CenterGameplayPanels()` 居中后，新订单面板不会超出屏幕。

这条是硬性要求：只在 Phase3 原型场景验证通过，不代表接入完成。

## 实施步骤

### 1. 创建订单 UI Prefab

建议路径：

```text
Assets/Phase3/Prefabs/UI/OrderPanelArt.prefab
```

步骤：

1. 在 `Phase3_Prototype` 中复制现有 `Panel_Order`。
2. 保留根节点名 `Panel_Order`。
3. 保留 `OrderPanelController`。
4. 添加背景 `Img_OrderPanelBg`，使用 `订单面板 文字.png`。
5. 调整四个动态文本到对应横线右侧。
6. 用 `接单按钮.png` 替换 `Btn_Accept` 视觉。
7. 确认 `Btn_Accept` 仍有 `Button` 组件。
8. 保存为 `OrderPanelArt.prefab`。

### 2. 调整脚本显示文本

修改 `OrderPanelController.ShowOrder()`：

1. 修复中文乱码。
2. 动态文本只显示值。
3. 保留 `acceptButton.onClick -> OnAcceptClicked -> gameManager.GoToShape()`。

### 3. 替换 Phase3 场景引用

在 `Assets/Phase3/Scenes/Phase3_Prototype.unity` 中：

1. 用 `OrderPanelArt.prefab` 替换旧 `Panel_Order`。
2. 检查 `OrderPanelController` 字段全部挂接。
3. 检查 `GameManager.panelOrder`。
4. 检查 `GameManager.orderPanelController`。
5. 保存场景。

### 4. 同步验证 Phase9

在 `Assets/Phase9/Scenes/SampleScene.unity` 中验证：

1. 运行场景。
2. 触发订单区交互。
3. 确认 Phase3 订单面板显示新美术。
4. 确认面板居中后不超屏。
5. 确认接单按钮可点击。
6. 确认进入 Shape 模块后订单面板隐藏。

## 验证清单

### 静态检查

- `Panel_Order` 名字未改变。
- `Btn_Accept` 是 `Panel_Order` 的直接子节点。
- `Btn_Accept` 仍有 `Button` 组件。
- `Img_OrderPanelBg.raycastTarget = false`。
- 四个动态文本引用已挂接。
- `GameManager.panelOrder` 指向新面板。
- `GameManager.orderPanelController` 指向新面板 Controller。
- `Phase3SceneBuilder` 运行后没有新增重复 `Btn_Accept`。

### Phase3 Play Mode 验证

- 进入 `Phase3_Prototype` 后显示订单面板。
- 订单名显示正确。
- 目标器型显示正确。
- 银两奖励显示正确。
- 声望奖励显示正确。
- 点击接单进入 Shape 状态。
- 订单面板隐藏，Shape 面板显示。
- Console 无 Error。

### Phase9 Play Mode 验证

- 进入 `Phase9/Scenes/SampleScene` 后可触发订单区交互。
- Phase3 订单面板通过桥接加载并显示。
- 新订单 UI 没有被 Phase9 居中逻辑拉坏。
- `GameplayCanvasGroup` 能控制订单面板显隐。
- 点击接单后桥接流程继续推进。
- 退出或切换模块后没有残留 UI。
- Console 无 Error。

### 分辨率验证

至少检查：

```text
1920 x 1080
1600 x 900
1366 x 768
```

检查内容：

- 面板不超出屏幕。
- 文本不压到背景标签。
- 按钮不被透明边距挡住。
- 点击区域与视觉按钮基本一致。

## 回退方案

如果接入后出现问题：

1. 恢复旧 `Panel_Order`。
2. 保留美术资源不删除。
3. 恢复 `GameManager.panelOrder` 和 `GameManager.orderPanelController` 指向旧面板。
4. 恢复 `OrderPanelController.ShowOrder()` 原显示逻辑。
5. 再单独排查 Prefab 引用、按钮射线、Phase9 桥接。

## 第一轮完成标准

- `OrderPanelArt.prefab` 创建完成。
- Phase3 原型场景显示新订单 UI。
- Phase9 桥接场景显示同一套新订单 UI。
- 接单按钮行为不变。
- 面板显隐行为不变。
- 无重复按钮。
- 无 Console Error。
- 文本无乱码。

完成以上内容后，才算订单 UI 第一轮接入完成。
