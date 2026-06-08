# Phase5 TODO

## PenaltySource 去重机制

**来源**: BUG-4（Phase4 审查发现）
**文件**: `Assets/Phase3/Scripts/Calculators/FireCalculator.cs:ApplyPenaltySourceNonStacking`
**规格**: PHASE4_MAPPING_DESIGN.md 1.3 — 同一 PenaltySource 的 CP 扣分与缺陷扣分不叠加，取 max(CP扣分, 缺陷扣分)

### 当前状态
- `ApplyPenaltySourceNonStacking` 方法体为预留接口
- `CalcTempPenalty` 返回整体 T 值，未拆分为 CP1~CP6 独立 PenaltySource
- 缺陷扣分已实现（按 D1~D11 逐项输出 PenaltySource），但无法与 CP 同源去重

### 待实现
1. 重构 `CalcTempPenalty`，输出按 CP1~CP6 拆分的独立 PenaltySource
2. 实现 `ApplyPenaltySourceNonStacking`：按 sourceId 分组（如 CP4 与 D4 同属温度过烧），同组取 max
3. 修正 totalPenalty 计算逻辑，避免重复计入去重后的扣分
