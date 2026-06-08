# Task-D: UI Layer

> **参考冻结规则**：见 [PHASE4_MAPPING_DESIGN.md](../PHASE4_MAPPING_DESIGN.md#一冻结数据层核心规则不可修改)
>
> **依赖 System 层**：见 [TASK_C_SYSTEM.md](TASK_C_SYSTEM.md)

---

## UI 参数调整 ⚠️ 破坏性变更

### Glaze 面板 Slider 调整

**现有**：5 个 Slider（Kaolin / Ash / Copper / Iron / Cobalt）

**调整后**：3 个 Slider（Cu / Fe / Co）

```
Panel_Glaze
├── Group_GlazeSliders
│   ├── Row_Copper     [Slider_Copper]  [Text_CopperValue]    // 新增
│   ├── Row_Iron       [Slider_Iron]    [Text_IronValue]      // 新增
│   └── Row_Cobalt     [Slider_Cobalt]  [Text_CobaltValue]    // 新增
-   ├── Row_Kaolin     [Slider_Kaolin]  [Text_KaolinValue]    // 删除
-   └── Row_Ash        [Slider_Ash]     [Text_AshValue]       // 删除
```

**Slider 范围**：
- Cu: 0 ~ 0.02
- Fe: 0 ~ 0.02
- Co: 0 ~ 0.02

---

## 各面板控制器职责

### ShapePanelController
- 绑定 5 个 Slider（mouth/neck/shoulder/belly/foot）
- Slider 变化时调用 `shapeSystem.Calculate(ShapeInput)`
- 显示 ShapeScoreResult.overallScore 到 UI

### GlazePanelController
- 绑定 3 个 Slider（copper/iron/cobalt）
- Slider 变化时调用 `glazeSystem.Calculate(GlazeInput)`
- 显示 GlazeScoreResult.overallScore 到 UI

### FiringPanelController
- **范围声明**：仅显示火候评分（方案A），暂不显示 Stage/Flame/Defect/PenaltySource 明细
- 保留现有烧窑控制 UI（风门/投柴/开窗/停止/开窑）
- 温度/区域/火候评分显示逻辑不变

### ResultPanelController
- 调用 `resultSystem.CalculateResult()` 获取 ResultData
- **ResultData → UI 映射**：

| ResultData 字段 | UI 组件 | 显示格式 |
|----------------|---------|---------|
| shapeScore | Text_ShapeMatchResult | `器型匹配：{value:F1}%` |
| glazeScore | Text_GlazeMatchResult | `釉料匹配：{value:F1}%` |
| fireScore | Text_FireScoreResult | `火候评分：{value:F1}%` |
| finalScore | —（仅在 result 内部使用） | — |
| grade | Text_Grade | S→贡品/金, A→精品/紫, B→佳品/蓝, C→良品/绿, D→次品/黄, E→废品/灰 |
| orderResult | Text_OrderResult | `订单结果：Perfect/Excellent/Normal` 或 `订单失败({failReason})` |
| goldReward | Text_SilverReward | `银两：{value}` |
| reputationReward | Text_ReputationReward | `声望：{value}` |
| matchedShapeID | —（调试用途） | — |
| matchedGlazeID | —（调试用途） | — |

---

## Inspector 绑定验收

以下 SerializeField 需在场景 Inspector 中手动赋值：

| 组件 | 需绑定的引用 |
|------|-------------|
| ShapeSystem | shapeTemplates（ShapeConfigSO[5]）、gameConfig、orderManager |
| GlazeSystem | glazeTemplates（GlazeConfigSO[5]）、gameConfig、orderManager |
| FiringSystem | fireConfig（FireConfigSO） |
| ResultSystem | shapeSystem、glazeSystem、firingSystem、orderManager、gameConfig |
| ShapePanelController | shapeSystem、5个Slider、5个Text、btnToGlaze |
| GlazePanelController | glazeSystem、3个Slider、3个Text、btnToFiring |
| FiringPanelController | firingSystem、各控件引用 |
| ResultPanelController | resultSystem、orderManager、各Text、btnNextOrder |

---

## Task-D 分解

| 子任务 | 内容 | 破坏性 |
|--------|------|--------|
| D1 | 调整 Panel_Shape（适配 ShapeInput） | 无 |
| D2 | 重构 Panel_Glaze（5→3 Slider，范围 0~0.02） | ⚠️ |
| D3 | 重构 Panel_Firing（仅火候评分，暂不显示 Stage/Flame/Defect） | ⚠️ |
| D4 | 重构 Panel_Result（适配 ResultData：grade/orderResult/三项分数/奖励） | ⚠️ |
| D5 | Inspector 绑定验收（shapeTemplates/glazeTemplates/gameConfig/fireConfig） | 无 |
