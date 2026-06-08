# Task-C: System Layer

> **参考冻结规则**：见 [PHASE4_MAPPING_DESIGN.md](../PHASE4_MAPPING_DESIGN.md#一冻结数据层核心规则不可修改)
>
> **依赖计算逻辑**：见 [TASK_B_CALCULATOR.md](TASK_B_CALCULATOR.md)

---

## System 宿主设计 ⚙️

**决策**：保留现有 MonoBehaviour 结构，内部调用 Calculator

### OrderManager（Scripts/Systems/Order/OrderManager.cs）
- **适配变更**：OrderData 旧字段 `shapeRecipe` / `glazeRecipe` 已替换为 `requiredShapeID` / `requiredGlazeID`
- OrderManager 仍通过 `OrderData[] orders` 数组 + `currentIndex` 读取当前订单
- `GetCurrentOrder()` 返回的 OrderData 中包含 `requiredShapeID`、`requiredGlazeID`、`baseGold`、`baseReputation`、`difficulty`
- ShapeSystem/GlazeSystem 通过 `requiredShapeID` / `requiredGlazeID` 从模板数组中查找匹配项
- 不修改 OrderManager 的读取逻辑，仅确保现有 Scene Inspector 中的绑定指向新 OrderData 资产

### ShapeSystem（Scripts/Systems/Shape/ShapeSystem.cs）
- 继承自 MonoBehaviour
- 字段：
  - shapeTemplates（ShapeConfigSO 数组）：器型模板数组
  - gameConfig（GameConfigSO）：游戏配置
  - orderManager（OrderManager）：订单管理器
- 属性：
  - TargetMouth/TargetNeck/TargetShoulder/TargetBelly/TargetFoot（浮点数）：目标器型参数
  - LastResult（ShapeScoreResult）：最近一次计算结果（只读）
- 方法：
  - LoadTargetFromCurrentOrder()：从当前订单查找 requiredShapeID 对应的模板，缓存目标参数
  - Calculate(ShapeInput input)：调用 ShapeCalculator.Calculate，保存并返回结果

### GlazeSystem（Scripts/Systems/Glaze/GlazeSystem.cs）
- 继承自 MonoBehaviour
- 字段：
  - glazeTemplates（GlazeConfigSO 数组）：釉色模板数组
  - gameConfig（GameConfigSO）：游戏配置
  - orderManager（OrderManager）：订单管理器
- 属性：
  - TargetCopper/TargetIron/TargetCobalt（浮点数）：目标釉色参数
  - LastResult（GlazeScoreResult）：最近一次计算结果（只读）
- 方法：
  - LoadTargetFromCurrentOrder()：从当前订单查找 requiredGlazeID 对应的模板，缓存目标参数
  - Calculate(GlazeInput input)：调用 GlazeCalculator.Calculate，保存并返回结果

### FiringSystem（Scripts/Systems/Firing/FiringSystem.cs）
- 保留现有烧窑模拟逻辑（温度增长、风门、投柴、开窗、停止）
- 新增：
  - fireConfig（FireConfigSO）：火候配置引用
  - CalculateScore()：构建 FireInput 调用 FireCalculator，返回 FireScoreResult
- 保留原有 GetFireScore() 向后兼容

### ResultSystem（Scripts/Systems/Result/ResultSystem.cs）
- 字段：
  - shapeSystem/glazeSystem/firingSystem（System 组件引用）
  - orderManager（OrderManager）：用于获取订单数据
  - gameConfig（GameConfigSO）：游戏配置
- 方法：
  - CalculateResult()：从三个 System 收集 Shape/Glaze/Fire 结果，构建 ResultInput 调用 ResultCalculator，返回 ResultData

---

## Task-C 分解

| 子任务 | 内容 |
|--------|------|
| C1 | 重构 ShapeSystem（内部调用 ShapeCalculator） |
| C2 | 重构 GlazeSystem（内部调用 GlazeCalculator） |
| C3 | 重构 FiringSystem（新增 CalculateScore 委托 FireCalculator） |
| C4 | 重构 ResultSystem（委托 ResultCalculator 执行 8 步流程） |
| C5 | 适配 OrderManager（requiredShapeID/requiredGlazeID 替代旧引用） |
