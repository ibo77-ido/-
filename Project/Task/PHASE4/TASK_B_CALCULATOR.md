# Task-B: Calculator Layer

> **参考冻结规则**：见 [PHASE4_MAPPING_DESIGN.md](../PHASE4_MAPPING_DESIGN.md#一冻结数据层核心规则不可修改)
> - ShapeCalculator → §1.1 ShapeScore
> - GlazeCalculator → §1.2 GlazeScore
> - FireCalculator → §1.3 FireScore
> - ResultCalculator → §1.4 ResultScore
>
> 🔒 **冻结声明**：Task-B 禁止修改 Task-A 创建的数据模型文件。
> - 冻结字段：D_max=0.35（Shape）、D_max=0.02（Glaze）、Cu/Fe/Co 范围=[0,0.02]、FireScore 公式 T+D+F+P、权重 35/25/40、Grade 阈值 S≥95/A≥85/B≥70/C≥50/D≥30/E<30、奖励乘数表
> - Task-B 仅读取 ShapeConfigSO / GlazeConfigSO / FireConfigSO / GameConfigSO / OrderData 中的数据，不得修改其结构

---

## 设计约束

1. Calculator 保持纯静态函数，无副作用
2. Shape/Glaze/Fire 内部不含 grade 字段（grade 仅由 ResultCalculator 产出）
3. matchedShapeID = ShapeCalculator 的 argmin 结果，ResultCalculator 检测 `matchedID == requiredID`

---

## ShapeCalculator

**位置**：`Scripts/Calculators/ShapeCalculator.cs`

**输入结构（ShapeInput）**：
- mouth（浮点数）：口部比例
- neck（浮点数）：颈部比例
- shoulder（浮点数）：肩部比例
- belly（浮点数）：腹部比例
- foot（浮点数）：足部比例

**输出结构（ShapeScoreResult）**：
- overallScore（浮点数）：总体得分，范围 0~100 🔒
- matchedShapeID（字符串）：最佳匹配器型 ID 🔒
- matchedShapeNameCN（字符串）：匹配器型中文名
- matchedShapeNameEN（字符串）：匹配器型英文名
- partScores（浮点数数组，长度 5）：各部位得分，用于雷达图 🔒
- rawError（浮点数）：原始误差值 🔒

**计算流程**：
1. 遍历 5 个模板，计算每个模板的 RawShapeError(i)
2. 取最小误差对应的模板作为 matchedShapeID
3. ShapeScore = 100 × max(0, 1 - bestError / 0.35)
4. PartScore_k = 100 × (1 - |P_k - T_k|)

---

## GlazeCalculator

**位置**：`Scripts/Calculators/GlazeCalculator.cs`

**输入结构（GlazeInput）**：
- copper（浮点数）：铜元素含量，范围 [0, 0.02] 🔒
- iron（浮点数）：铁元素含量，范围 [0, 0.02] 🔒
- cobalt（浮点数）：钴元素含量，范围 [0, 0.02] 🔒
- firingTempMax（可空浮点数）：最高烧成温度，用于影青/冬青二级判定 🔒

**输出结构（GlazeScoreResult）**：
- overallScore（浮点数）：总体得分，范围 0~100 🔒
- matchedGlazeID（字符串）：最佳匹配釉色 ID 🔒
- matchedGlazeNameCN（字符串）：匹配釉色中文名
- matchedGlazeNameEN（字符串）：匹配釉色英文名
- dMin（浮点数）：最短欧几里得距离 🔒
- requiresTempConfirm（布尔值）：影青/冬青平局标记 🔒

**计算流程**：
1. 计算欧几里得距离：d(i) = sqrt((Cu-Cu_i)² + (Fe-Fe_i)² + (Co-Co_i)²)
2. 取最小距离：d_min = min(d)
3. GlazeScore = 100 × max(0, 1 - d_min / 0.02)
4. Level 2 判定（影青/冬青）：根据温度判定

---

## FireCalculator（独立验证）

**位置**：`Scripts/Calculators/FireCalculator.cs`

**输入结构（FireInput）**：
- temperatureReadings（浮点数数组）：温度读数序列
- timeStamps（浮点数数组）：时间戳序列
- stageDurations（浮点数数组，长度 8）：各阶段时长 S1~S8
- reductionSwitchTemp（浮点数）：还原气氛切换温度
- reductionWasSwitched（布尔值）：是否已切换还原气氛
- flameJudgments（布尔值数组，长度 4）：火焰判断 FS1~FS4
- stopFireTemp（浮点数）：停火温度
- stopFireTime（浮点数）：停火时间
- kilnOpenTemp（浮点数）：开窑温度

**输出结构（FireScoreResult）**：
- fireScore（浮点数）：火候得分，范围 0~100
- temperatureZone（字符串）：温度区域，值为 欠烧/正常/过烧
- tempPenalty（浮点数）：温度扣分 T
- durationPenalty（浮点数）：时长扣分 D
- flamePenalty（浮点数）：火焰扣分 F
- defectPenalty（浮点数）：缺陷扣分 P
- penalties（PenaltySource 列表）：扣分明细列表
- isFatalDefect（布尔值）：是否存在致命缺陷

**PenaltySource 结构**：
- sourceId（字符串）：扣分源 ID
- sourceNameCN（字符串）：扣分源中文名
- descriptionCN（字符串）：中文描述
- penalty（浮点数）：扣分值
- suggestionCN（字符串）：中文建议
- feedbackLevel（FeedbackLevel 枚举）：反馈等级

**计算流程**：
1. T = 温度误差扣分（0~35），基于停火温度与目标窗口（1280~1320°C）偏差
2. D = 时长扣分（0~20），基于 S1~S8 实际时长与推荐中位数对比
3. F = 火焰扣分（0~20），基于 FS1~FS4 误判累计
4. P = 缺陷扣分（0~25），致命缺陷强制 fireScore=0
5. FireScore = max(0, 100 - T - D - F - P)
6. PenaltySource 机制：同一来源的 CP 扣分与缺陷扣分不叠加，取 max

---

## ResultCalculator

**位置**：`Scripts/Calculators/ResultCalculator.cs`

**输入结构（ResultInput）**：
- shapeResult（ShapeScoreResult）：器型评分结果
- glazeResult（GlazeScoreResult）：釉色评分结果
- fireResult（FireScoreResult）：火候评分结果
- orderData（OrderData）：订单数据
- defectList（DefectConfig 列表）：缺陷列表

**输出结构（ResultData）**：
- shapeScore（浮点数）：器型得分
- glazeScore（浮点数）：釉色得分
- fireScore（浮点数）：火候得分
- finalScore（浮点数）：最终加权得分
- grade（字符串）：等级，值为 S/A/B/C/D/E（仅在 Result 层）
- matchedShapeID（字符串）：匹配的器型 ID
- matchedGlazeID（字符串）：匹配的釉色 ID
- orderResult（字符串）：订单结果，值为 Perfect/Excellent/Normal/Fail
- failReason（字符串）：失败原因
- goldReward（整数）：金币奖励
- reputationReward（整数）：声望奖励
- defectList（PenaltySource 列表）：缺陷列表
- errorFlag（布尔值）：错误标记
- errorCode（字符串）：错误代码
- version（整数）：版本号

**计算流程（8 步）**：
- Step 0: ValidateInput() — 输入验证（Runtime Safety）
- Step 1: CalcFinalScore() — 加权求和：0.35×Shape + 0.25×Glaze + 0.40×Fire
- Step 2: CalcGrade() — 等级判定（S/A/B/C/D/E）
- Step 3: CheckOrder() — Shape/Glaze 匹配检查（matchedID vs requiredID）
- Step 4: CalcOrderResult() — 订单结果判定
- Step 5: CalcRewards() — 奖励计算
- Step 6: MapGradeDisplay() — 品阶映射（UI 层处理）
- Step 7: BuildResultData() — 构建结果数据
- Step 8: ValidateResultData() — 结果验证（goldMax/repMax 安全上限）

---

## Task-B 分解

| 子任务 | 内容 |
|--------|------|
| B1 | ShapeCalculator（含 18 测试案例） |
| B2 | GlazeCalculator（含 19 测试案例 + 三级判定） |
| B3 | FireCalculator（T+D+F+P + PenaltySource，单独验证） |
| B4 | ResultCalculator（含 25 测试案例 + Runtime Safety） |
