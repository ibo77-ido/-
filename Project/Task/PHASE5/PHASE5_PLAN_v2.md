# Phase5 Plan v2

> **版本**: 2.0  
> **日期**: 2026-06-06  
> **架构原则**: Runtime Layer ⚡ 与 Gameplay Rule Layer 🎯 分离

---

## 架构设计

### 双层分离

```
┌──────────────────────────────────────────────────┐
│              FiringRuntimeSnapshot               │  ← 运行时快照（数据容器）
│  temperatureCurve, stageDurations,               │
│  flameStates, checkpointPass, elapsedTime        │
└────────────────────┬─────────────────────────────┘
                     │ 注入
         ┌───────────┴───────────┐
         ▼                       ▼
┌──────────────────┐  ┌──────────────────────────┐
│ Runtime Layer ⚡  │  │ Gameplay Rule Layer 🎯   │
│                  │  │                          │
│ F1 温度曲线引擎   │  │ G1 温度检查点 CP1~CP6    │
│ F2 阶段跟踪器    │  │ G2 缺陷判定器 D1~D11     │
│ F3 火焰状态追踪   │  │ G3 PenaltySource V1.1   │
│                  │  │ G4 FireCalculator 集成    │
└──────────────────┘  └──────────────────────────┘
         │                       │
         └─────── FiringSystem ──┘
                     │
                     ▼
            ┌──────────────────┐
            │    Debug UI 🖥️   │
            │ H1~H4 实时观测   │
            └──────────────────┘
```

**Runtime Layer ⚡** — 负责「发生了什么」
- 温度如何随时间变化（曲线引擎）
- 当前处于哪个阶段（阶段跟踪）
- 火焰颜色何时切换（火焰追踪）
- 输出：`FiringRuntimeSnapshot`

**Gameplay Rule Layer 🎯** — 负责「该扣多少分」
- 6 个 CP 温度检查点判定
- 11 种缺陷触发判定
- PenaltySource 归并去重
- 调用 FireCalculator 计算最终 score

### FiringRuntimeSnapshot 定义

```csharp
public struct FiringRuntimeSnapshot
{
    public float[] temperatureReadings;   // 温度序列（时间序列）
    public float[] timeStamps;            // 对应时间戳（秒）
    public float[] stageElapsed;          // [8] 各阶段已用时长（小时）
    public string currentStageId;         // S1~S8 当前阶段 ID
    public bool[] flameStates;            // [4] FS1~FS4 是否已切换
    public bool[] checkpointPassed;       // [6] CP1~CP6 是否已达标
    public float rampRate;                // 当前升温速率（°C/h）
    public float maxTemp;                 // 最高温度
    public float stopFireTemp;            // 停火温度
    public float kilnOpenTemp;            // 开窑温度
    public bool isFiring;                 // 是否正在烧制
}
```

---

## Task Breakdown

### Task-F: Runtime Simulation ⚡

**依赖**: 无（新建 FiringRuntime 目录）  
**边界**: 不修改现有 System/Calculator/UI 代码

| 子任务 | 内容 | 产出 |
|--------|------|------|
| **F1** 温度曲线引擎 | 实现分段非线性升温速率（不同阶段不同速率），支持风门/投柴影响 | `FiringTemperatureCurve.cs` |
| **F2** 阶段跟踪器 | 根据温度自动识别 S1~S8 阶段切换，记录各阶段实际耗时 | `FiringStageTracker.cs` |
| **F3** 火焰状态追踪 | 实现 FS1~FS4 四阶段火焰颜色自动切换逻辑 | `FiringFlameTracker.cs` |

### Task-G: Fire Gameplay Rules 🎯

**依赖**: Task-F（需要 FiringRuntimeSnapshot）  
**边界**: 不修改 Shape/Glaze/Result 模块

| 子任务 | 内容 | 产出 |
|--------|------|------|
| **G1** 温度检查点 | 实现 6 个 CP 超限判定（升温速率/石英转化/气氛切换/成熟窗口/冷却/开窑时机） | `FireCheckpointSystem.cs` |
| **G2** 缺陷判定器 | 根据运行时数据触发 D1~D11 缺陷判定，支持致命缺陷强制归零 | `FireDefectDetector.cs` |
| **G3** PenaltySource | 实现同一 Source CP 扣分与缺陷扣分 max 不叠加逻辑 | 修改 `FireCalculator.cs` |
| **G4** FireCalculator 集成 | 将 FiringRuntimeSnapshot + 缺陷列表 + CP 结果注入 FireCalculator，替换当前 FiringSystem.CalculateScore() 简化实现 | 修改 `FiringSystem.cs` |

### Task-H: Debug UI 🖥️

**依赖**: Task-F, Task-G（需要运行时数据来源）  
**边界**: 仅修改 `FiringPanelController.cs`，不修改 Calculator/System

| 子任务 | 内容 | 产出 |
|--------|------|------|
| **H1** 阶段显示 | 实时显示当前阶段 (S1~S8)、阶段名称中文、已用时长 | 修改 `FiringPanelController.cs` |
| **H2** 火焰显示 | 显示当前火焰颜色状态、FS1~FS4 切换状态 | 修改 `FiringPanelController.cs` |
| **H3** 缺陷显示 | 显示已触发的缺陷列表（含扣分和是否致命） | 修改 `FiringPanelController.cs` |
| **H4** T/D/F/P 明细 | 显示温度扣分/时长扣分/火焰扣分/缺陷扣分四项明细 | 修改 `FiringPanelController.cs` |

### Task-I: Save System MVP 💾

**依赖**: 无（独立模块）  
**边界**: 仅持久化核心进度数据，不实现云存档/多存档槽

| 子任务 | 内容 | 产出 |
|--------|------|------|
| **I1** Save | 将玩家数据序列化为 JSON（银两/声望/当前订单索引） | `SaveManager.cs` |
| **I2** Load | 从存档恢复游戏状态 | `SaveManager.cs` |
| **I3** Reset | 重置所有数据为新游戏 | `SaveManager.cs` |

---

## 执行顺序

```
F1 ──→ F2 ──→ F3 ──→ G1 ──→ G2 ──→ G3 ──→ G4 ──→ H1
                                                      ├──→ H2
                                                      ├──→ H3
                                                      └──→ H4

I1 ──→ I2 ──→ I3 (独立，可在任意点并行执行)
```

## 依赖图

```
Task-F (Runtime)
    ↓
Task-G (Gameplay Rules)
    ↓
Task-H (Debug UI)

Task-I (Save) ── 独立并行
```

## 验收标准

| Task | 验收项 |
|------|--------|
| F | 温度曲线可观测（不同风门/投柴产生不同升温曲线），阶段自动切换，火焰状态自动变化 |
| G | CP 超限触发扣分，缺陷触发归零，PenaltySource 不叠加可验证，全链路 fireScore 正确 |
| H | 阶段/火焰/缺陷/TDFP 明细在 Panel_Firing 实时显示 |
| I | 存档/读档/重置功能完整，数据正确持久化 |

---

*PHASE5_PLAN_v2 — 2026-06-06*
