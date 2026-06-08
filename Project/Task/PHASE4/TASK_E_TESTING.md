# Task-E: E2E Testing

> **参考冻结规则**：见 [PHASE4_MAPPING_DESIGN.md](../PHASE4_MAPPING_DESIGN.md#一冻结数据层核心规则不可修改)
>
> **数据依赖**：Task-A DataModel / Task-B Calculator / Task-C System / Task-D UI

---

## 测试范围

覆盖全链路：接订单 → 器型调整 → 釉料调配 → 烧窑控制 → 开窑 → 结果展示

### 验证场景（26 个）

| 编号 | 场景 | 覆盖模块 |
|------|------|---------|
| E01 | 完美匹配（所有参数对齐目标） | 全链路 |
| E02 | 器型部分偏差（单个维度偏差 0.1） | Shape |
| E03 | 器型完全偏离（全维度偏差 0.5） | Shape |
| E04 | 釉色完美匹配 | Glaze |
| E05 | 釉色单元素偏差 | Glaze |
| E06 | 影青/冬青温度判定 | Glaze(Level2) |
| E07 | 欠烧（温度不足） | Fire |
| E08 | 过烧（温度过高） | Fire |
| E09 | 火焰误判 | Fire |
| E10 | 致命缺陷 | Fire |
| E11 | 器型不匹配（matched != required） | Result |
| E12 | 釉色不匹配 | Result |
| E13 | 订单完美完成（Perfect） | Result |
| E14 | 订单优秀完成（Excellent） | Result |
| E15 | 订单普通完成（Normal） | Result |
| E16 | 订单失败（Fail） | Result |
| E17 | 奖励计算正确性 | Result |
| E18 | Runtime Safety（gold/rep 上限） | Result |
| E19 | 连续 3 轮测试 | 全链路 |
| E20 | 面板状态重置正确 | Data/System |
| E21 | 编译检查（0 编译错误） | 项目 |
| E22 | Inspector 引用完整性 | 场景 |
| E23 | 缺失脚本检查 | 场景 |
| E24 | Play Mode 无报错 | 项目 |
| E25 | 订单切换循环 | Order/System |
| E26 | UI 品阶显示正确 | UI |
| E27 | T 扣分边界（欠烧 T=35上限、过烧 T=35上限、正常区 T=0） | Fire |
| E28 | D 扣分边界（严重不足 D=20、偏长 D=15、正常区 D=0） | Fire |
| E29 | F 扣分边界（全误判 F=20、无误判 F=0、单 FS 误判） | Fire |
| E30 | PenaltySource 不叠加（同一 Source 的 CP 扣分与缺陷扣分取 max） | Fire/V1.1 |
| E31 | Fatal 判定（D1/D2 致命缺陷 → fireScore=0） | Fire |

---

## Task-E 分解

| 子任务 | 内容 |
|--------|------|
| E1 | 31 个测试场景验证通过 |
