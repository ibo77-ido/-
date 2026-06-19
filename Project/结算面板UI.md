# 结算面板 UI 接入方案

## 目标

将 Phase3 新增的订单完成/结算美术资源接入现有结果面板 `Panel_Result`，实现“逐秒揭示结算信息”的展示节奏：

1. 先显示各项匹配度。
2. 再显示最终品级。
3. 最后显示银两、声望奖励和继续按钮。

本方案只替换和增强结果面板 UI 表现，不修改订单、器型、釉料、烧制、结果计算的核心流程。

## 当前资源

资源目录：

```text
Assets/Phase3/UI/订单完成/
```

当前可用图片：

```text
订单完成 空.png
0 成品展示.png
1.png
2.png
3.png
接单按钮.png
```

资源用途建议：

- `订单完成 空.png`：结算面板初始空底图，可作为默认背景。
- `1.png`：第一阶段底图，用于显示器型/釉料/火候等匹配度。
- `2.png`：第二阶段底图，用于显示品级和订单结果。
- `3.png`：第三阶段底图，用于显示银两、声望等奖励。
- `0 成品展示.png`：成品展示图，可作为结果页中的作品预览图。
- `接单按钮.png`：用于替换 `Btn_NextOrder` 的按钮视觉。

注意：该目录下图片需要先由 Unity 导入并生成 `.meta`，否则场景和 prefab 中无法稳定引用这些 Sprite。

## 现有逻辑入口

结果面板由 `GameManager` 管理显隐和流程推进。

关键字段：

```csharp
[SerializeField] private GameObject panelResult;
[SerializeField] private ResultPanelController resultPanelController;
```

关键流程：

```csharp
public void GoToResult()
{
    if (resultPanelController != null) resultPanelController.ShowResult();
    advanceOrderOnNextStart = true;
    SetState(GameState.Result);
}

public void GoToNextOrder()
{
    BeginOrder(true);
}
```

因此接入时必须保持：

```text
GameManager.panelResult           -> Panel_Result
GameManager.resultPanelController -> Panel_Result 上的 ResultPanelController
```

`ResultPanelController.ShowResult()` 是最适合放置逐秒播放逻辑的位置。

## 必须保留的对象和引用

根节点不要改名：

```text
Panel_Result
```

继续按钮建议保留原名：

```text
Btn_NextOrder
```

原因：`Phase3SceneBuilder.EnsureResultPanelControls()` 会按名字查找 `Panel_Result` 下的结果面板控件。如果这些节点被改名或移到不可查找的位置，运行 `Phase3/Sync Scene (Ensure Mode)` 后可能自动补出一套灰盒控件，导致重复文本或重复按钮。

`ResultPanelController` 当前必须继续挂接这些引用：

```text
resultSystem
orderManager
gameManager
gradeText
shapeMatchText
glazeMatchText
fireScoreText
silverRewardText
reputationRewardText
orderResultText
nextOrderButton
exitGameplayButton
```

`exitGameplayButton` 对 Phase3 单独流程不是必须，但对 Phase9 Bridge 很重要。Bridge 会通过 `ResultPanelController.OnExitGameplayEvent` 订阅退出玩法逻辑，所以如果结果面板美术替换后还需要桥接退出链路，必须保留一个退出按钮并挂到该字段。

## 推荐层级

建议在现有 `Panel_Result` 内改造，不删除旧根节点，避免丢失 `GameManager` 和 `ResultPanelController` 的序列化引用。

```text
Panel_Result
├── Img_ResultStage
├── Img_ProductPreview
├── Group_Match
│   ├── Text_ShapeMatchResult
│   ├── Text_GlazeMatchResult
│   └── Text_FireScoreResult
├── Group_Grade
│   ├── Text_Grade
│   └── Text_OrderResult
├── Group_Reward
│   ├── Text_SilverReward
│   ├── Text_ReputationReward
│   ├── Btn_NextOrder
│   └── Btn_ExitGameplay
```

`Group_Match`、`Group_Grade`、`Group_Reward` 是本方案新增的空父节点，当前场景和 `ResultPanelController` 里都还不存在。接入时需要在 Unity Editor 中创建这三个空节点，把现有 Text/Button 拖到对应分组下，然后在 `ResultPanelController` 新增序列化字段并挂接它们。

推荐绑定关系：

```text
Img_ResultStage.sprite 初始使用 订单完成 空.png
Img_ResultStage.raycastTarget = false

Img_ProductPreview.sprite = 0 成品展示.png
Img_ProductPreview.raycastTarget = false

Btn_NextOrder.Image.sprite = 接单按钮.png
Btn_NextOrder 继续保留 Button 组件

Btn_ExitGameplay -> ResultPanelController.exitGameplayButton
Btn_ExitGameplay 可选显示，仅桥接退出玩法需要
```

如果 `1.png`、`2.png`、`3.png` 是完整阶段底图，推荐只用一个 `Img_ResultStage` 逐秒替换 Sprite。这样引用更少，状态更干净。

如果三张图是局部装饰，也可以使用三个 Image 叠放：

```text
Img_Stage1
Img_Stage2
Img_Stage3
```

然后逐秒 `SetActive`。但第一轮接入更推荐单 Image 换 Sprite。

## 播放节奏

进入结果页后执行：

```text
0 秒：显示 1.png，显示匹配度组
1 秒：切到 2.png，显示品级组
2 秒：切到 3.png，显示奖励组和 Btn_NextOrder
```

展示内容：

```text
Group_Match
- Text_ShapeMatchResult
- Text_GlazeMatchResult
- Text_FireScoreResult

Group_Grade
- Text_Grade
- Text_OrderResult

Group_Reward
- Text_SilverReward
- Text_ReputationReward
- Btn_NextOrder
```

按钮建议最后一阶段再显示，避免玩家在动画揭示中提前点击下一单。

## 脚本调整建议

在 `ResultPanelController` 中新增字段：

```csharp
[Header("Reveal Sequence")]
[SerializeField] private Image stageImage;
[SerializeField] private Sprite stage1Sprite;
[SerializeField] private Sprite stage2Sprite;
[SerializeField] private Sprite stage3Sprite;
[SerializeField] private GameObject matchGroup;
[SerializeField] private GameObject gradeGroup;
[SerializeField] private GameObject rewardGroup;
[SerializeField] private float revealInterval = 1f;

private Coroutine revealRoutine;
```

`ShowResult()` 中先计算并写入所有文本，再启动揭示流程：

```csharp
public void ShowResult()
{
    if (resultSystem == null) return;

    ResultData data = resultSystem.CalculateResult();
    ApplyResultText(data);

    if (revealRoutine != null)
    {
        StopCoroutine(revealRoutine);
    }

    revealRoutine = StartCoroutine(RevealResultSequence());
}
```

`ApplyResultText(data)` 是把现有 `ShowResult()` 中的文本写入逻辑单独抽出来，不改变结果计算，只改变显示节奏。第一版建议保持现有文案格式，避免在不确认底图标签的情况下误删信息：

```csharp
private void ApplyResultText(ResultData data)
{
    if (gradeText != null)
    {
        string displayName = GradeDisplayNames.ContainsKey(data.grade)
            ? GradeDisplayNames[data.grade] : data.grade;
        gradeText.text = displayName;
        gradeText.color = GradeDisplayColors.ContainsKey(data.grade)
            ? GradeDisplayColors[data.grade] : Color.white;
    }

    SetText(shapeMatchText, $"器型匹配：{data.shapeScore:F1}%");
    SetText(glazeMatchText, $"釉料匹配：{data.glazeScore:F1}%");
    SetText(fireScoreText, $"火候评分：{data.fireScore:F1}%");

    if (data.orderResult == "Fail")
    {
        SetText(orderResultText, $"订单失败({data.failReason})");
    }
    else
    {
        SetText(orderResultText, $"订单结果：{data.orderResult}");
    }

    SetText(silverRewardText, $"银两：{data.goldReward}");
    SetText(reputationRewardText, $"声望：{data.reputationReward}");
}
```

揭示流程建议：

```csharp
private IEnumerator RevealResultSequence()
{
    SetGroup(matchGroup, false);
    SetGroup(gradeGroup, false);
    SetGroup(rewardGroup, false);

    if (nextOrderButton != null)
    {
        nextOrderButton.gameObject.SetActive(false);
    }

    SetStage(stage1Sprite);
    SetGroup(matchGroup, true);

    yield return new WaitForSecondsRealtime(revealInterval);

    SetStage(stage2Sprite);
    SetGroup(gradeGroup, true);

    yield return new WaitForSecondsRealtime(revealInterval);

    SetStage(stage3Sprite);
    SetGroup(rewardGroup, true);

    if (nextOrderButton != null)
    {
        nextOrderButton.gameObject.SetActive(true);
    }
}
```

辅助方法：

```csharp
private void SetStage(Sprite sprite)
{
    if (stageImage != null && sprite != null)
    {
        stageImage.sprite = sprite;
    }
}

private void SetGroup(GameObject group, bool active)
{
    if (group != null)
    {
        group.SetActive(active);
    }
}
```

为了防止面板隐藏后 coroutine 残留，建议补充：

```csharp
private void OnDisable()
{
    if (revealRoutine != null)
    {
        StopCoroutine(revealRoutine);
        revealRoutine = null;
    }
}
```

## 文本显示建议

如果美术底图已经自带固定标签，动态文本建议只显示数值，避免与背景标签重复。

推荐显示：

```text
shapeMatchText       -> 85.0%
glazeMatchText       -> 92.0%
fireScoreText        -> 78.0%
gradeText            -> 精品 / 佳品 / 良品
orderResultText      -> 订单完成 / 订单失败
silverRewardText     -> 120
reputationRewardText -> 8
```

如果底图没有标签，则继续显示完整文案：

```text
器型匹配：85.0%
釉料匹配：92.0%
火候评分：78.0%
银两：120
声望：8
```

这取决于 `1.png`、`2.png`、`3.png` 是否已经包含文字标签。

`orderResultText` 第一版建议保持现有格式：

```text
订单结果：Perfect / Excellent / Normal
订单失败(reason)
```

如果 `2.png` 已经自带“订单完成”或“订单结果”标签，再考虑把它改成只显示结果值、品级名，或者直接隐藏该文本。这个点需要看最终美术底图后再定。

## Phase3SceneBuilder 同步策略

当前 `Phase3SceneBuilder.EnsureResultPanelControls()` 会检查并自动创建这些旧灰盒控件：

```text
Text_Grade
Text_ShapeMatchResult
Text_GlazeMatchResult
Text_FireScoreResult
Text_SilverReward
Text_ReputationReward
Btn_NextOrder
```

如果新美术面板已经接入，但 SceneBuilder 没有识别美术节点，后续运行 `Phase3/Sync Scene (Ensure Mode)` 时可能补出旧占位控件，导致重复 UI。

建议参照烧制 UI 的处理方式，在 `EnsureResultPanelControls()` 开头增加美术节点检测。检测到 `Img_ResultStage` 后，只确保 `ResultPanelController` 存在，然后直接返回：

```csharp
if (panel.Find("Img_ResultStage") != null)
{
    if (panel.GetComponent<ResultPanelController>() == null)
    {
        panel.gameObject.AddComponent<ResultPanelController>();
        created.Add("Component: ResultPanelController on Panel_Result");
    }

    return created;
}
```

这样 SceneBuilder 不会在美术面板下继续补旧灰盒控件。需要注意：跳过创建后，所有 Text、Button、Stage Image、Group 引用都必须由场景或 prefab 预先挂好。

## Prefab 建议

可以将改造后的结果面板保存为：

```text
Assets/Phase3/Prefabs/UI/ResultPanelArt.prefab
```

但替换到场景后必须重新确认：

```text
GameManager.panelResult
GameManager.resultPanelController
ResultPanelController.resultSystem
ResultPanelController.orderManager
ResultPanelController.gameManager
ResultPanelController.nextOrderButton
ResultPanelController.exitGameplayButton
ResultPanelController 各 Text 引用
ResultPanelController stageImage / stage sprites / groups
```

如果只是第一轮接入，优先建议直接改造 `Phase3_Prototype.unity` 中现有的 `Panel_Result`，完成验证后再保存 prefab。

## Phase9 桥接注意事项

Phase9 会加载 Phase3 玩法，并依赖 Phase3 中的 `GameManager` 和面板引用。结果面板接入后要同步验证：

```text
Panel_Result 名字不变
GameManager.panelResult 指向 Panel_Result
GameManager.resultPanelController 指向 Panel_Result 上的 ResultPanelController
GameplayCanvasGroup 仍能控制结果面板显隐
Btn_NextOrder 点击后仍能推进下一单或完成桥接流程
```

不要把 `Panel_Result` 替换成一个没有被 `GameManager` 引用的新对象，否则 Phase9 桥接场景可能加载到旧引用或空引用。

## 验收清单

静态检查：

- `Panel_Result` 名字未改变。
- `Btn_NextOrder` 名字未改变，并保留 `Button` 组件。
- `Img_ResultStage.raycastTarget = false`。
- `Img_ProductPreview.raycastTarget = false`。
- `ResultPanelController` 所有文本、按钮、阶段图片、分组引用均已挂接。
- `ResultPanelController.exitGameplayButton` 在需要 Phase9 退出玩法时已挂接。
- `GameManager.panelResult` 指向当前 `Panel_Result`。
- `GameManager.resultPanelController` 指向当前 `ResultPanelController`。
- `订单完成` 目录下图片均已生成 `.meta`。
- 运行 `Phase3/Sync Scene (Ensure Mode)` 后不会补出重复的旧灰盒控件。

Phase3 Play Mode 检查：

- 进入结果页后，先显示匹配度。
- 约 1 秒后显示品级。
- 再约 1 秒后显示银两、声望和继续按钮。
- 播放过程中按钮不会提前可点。
- 点击 `Btn_NextOrder` 后正常进入下一单。
- Console 无 Error。

Phase9 桥接检查：

- 从 Phase9 触发 Phase3 后，结果页仍显示新结算 UI。
- 逐秒揭示流程正常播放。
- 桥接场景中面板显隐正常。
- 点击继续按钮后桥接流程继续推进。
- Console 无 Error。

分辨率检查：

```text
1920 x 1080
1600 x 900
1366 x 768
```

重点检查：

- 结算面板不超出屏幕。
- 文本没有压到美术固定标签。
- 成品展示图不遮挡关键结算信息。
- 按钮点击区域与视觉按钮基本一致。

## 回退方案

如果接入后出现问题：

1. 保留美术资源，不删除。
2. 将 `GameManager.panelResult` 指回旧 `Panel_Result`。
3. 将 `GameManager.resultPanelController` 指回旧 `ResultPanelController`。
4. 暂时移除 `stageImage`、阶段 Sprite 和分组揭示逻辑。
5. 恢复 `ShowResult()` 一次性显示全部结果文本。

第一轮完成标准：

- Phase3 原型场景结果页显示新结算 UI。
- `1.png`、`2.png`、`3.png` 按阶段逐秒播放。
- 匹配度、品级、银两声望按顺序出现。
- 下一单按钮行为不变。
- Phase9 桥接验证通过。
- Console 无 Error。
