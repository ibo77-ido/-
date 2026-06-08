# 数据层审查与修正建议报告（CodeBuddy执行前审查）

Version: 1.0
Status: Review Required
Date: 2026-06-06

## 1. 审查背景

本报告用于审查《数据层完整理解报告》是否准确反映当前冻结数据层设计。

审查对象来源：

- ShapeScoreSpecification.md
- GlazeScoreSpecification.md
- GlazeCalculator.md
- FireCalculator.md
- ResultCalculator.md
- ScoringSystem_Phase4_Freeze.md

---

## 2. 总体评价

| 项目 | 评分 |
|--------|--------|
| 数据理解准确度 | 92 |
| 公式理解 | 95 |
| 模块边界理解 | 88 |
| Unity落地性 | 75 |
| Task拆分合理性 | 80 |
| 可直接作为开发蓝图 | 78 |

综合评价：86 / 100

结论：

- 可作为参考文档
- 不可直接作为最终开发蓝图
- 需补充缺失设计后再进入实现阶段

---

## 3. 已确认正确的部分

### 3.1 ShapeScore理解正确

- 5部位比例参数：Mouth、Neck、Shoulder、Belly、Foot
- 权重：0.15 / 0.15 / 0.30 / 0.30 / 0.10

公式：

```text
RawShapeError = Σ(w × error)

ShapeScore =
100 × max(0,1-RawShapeError/0.35)
```

结论：Shape评分公式理解正确。

### 3.2 GlazeScore理解正确

三元素：

- Cu
- Fe
- Co

公式：

```text
d_min = min(EuclideanDistance)

GlazeScore =
100 × max(0,1-d_min/0.02)
```

结论：Glaze评分公式理解正确。

### 3.3 FireScore理解正确

```text
FireScore =
100 - T - D - F - P
```

其中：

- T = TemperaturePenalty
- D = DurationPenalty
- F = FlamePenalty
- P = DefectPenalty

结论：Fire评分核心公式理解正确。

### 3.4 ResultScore理解正确

```text
FinalScore =
0.35 × ShapeScore
+ 0.25 × GlazeScore
+ 0.40 × FireScore
```

结论：最终评分公式正确。

---

## 4. 发现的问题与遗漏

### Issue-01：ShapeScore 与 OrderMatch 被混淆

实际设计：

ShapeCalculator负责：

- 匹配最接近标准器型
- 输出 MatchedShape 与 ShapeScore

订单系统负责：

```text
Order.RequiredShape
vs
MatchedShape
```

示例：

- 玩家制作：玉壶春（95分）
- 订单要求：梅瓶

结果：

- ShapeScore = 95
- Order Fail

修正要求：

```csharp
MatchedShapeID
OrderRequiredShapeID
ShapeOrderMatched
```

禁止直接使用 ShapeScore 判定订单成功。

---

### Issue-02：GlazeScore 与 OrderGlaze 被混淆

GlazeCalculator负责：

```csharp
MatchedGlazeID
GlazeScore
```

Order负责：

```csharp
MatchedGlazeID
RequiredGlazeID
GlazeOrderMatched
```

高评分不等于满足订单。

---

### Issue-03：FireCalculator不仅是评分器

当前理解缺少 PenaltySource 系统。

应输出：

```csharp
FireResult
{
    float FireScore;
    List<PenaltySource> Penalties;
}
```

PenaltySource示例：

- OverFired
- ReductionFailure
- Crack
- GlazeRun
- Pinhole

用途：

- UI反馈
- 教学提示
- 新手引导
- 烧制报告

---

### Issue-04：ResultCalculator职责被低估

不仅是聚合器。

应负责：

1. FinalScore
2. Grade
3. OrderResult
4. GoldReward
5. ReputationReward
6. RuntimeSafetyValidation

OrderResult：

```csharp
Perfect
Excellent
Normal
Fail
```

必须包含：

```csharp
OrderResult
RewardResult
RuntimeSafetyResult
```

---

## 5. Unity数据层修正建议

当前方案：

- A1 ShapeSO
- A2 GlazeSO
- A3 FireSO
- A4 GameConfigSO

问题：

对于3人团队拆分过细。

推荐：

### Task-A：DataModel Layer

统一完成：

- ShapeConfigSO
- GlazeConfigSO
- FireConfigSO
- GameConfigSO
- OrderSO

---

## 6. 推荐开发顺序

### Phase-A 数据模型

- ShapeConfigSO
- GlazeConfigSO
- FireConfigSO
- GameConfigSO
- OrderSO

### Phase-B 计算层

- ShapeCalculator
- GlazeCalculator
- FireCalculator

### Phase-C 结果层

ResultCalculator负责：

- Grade
- OrderResult
- Reward
- RuntimeSafety

### Phase-D UI层

- Shape UI
- Glaze UI
- Fire UI
- Result UI

### Phase-E E2E测试

流程：

```text
Order
↓
Shape
↓
Glaze
↓
Fire
↓
Result
↓
Reward
```

---

## 7. CodeBuddy执行前新增要求

必须新增：

### DataLayer → ScriptableObject Mapping Design

明确：

- 每个SO字段
- Calculator输入结构
- Calculator输出结构
- OrderResult流程
- Reward流程
- Runtime Safety流程

---

## 8. 最终结论

当前《数据层完整理解报告》：

适合作为：

- 数据理解文档

暂不适合作为：

- CodeBuddy直接执行蓝图

进入开发前应补充：

- ScriptableObject映射设计
- Calculator输入输出设计
- OrderResult设计
- RuntimeSafety设计

完成后再进入执行阶段，可显著降低返工风险。
