# P8-08 File Responsibility Matrix

**Status**: FROZEN
**Date**: 2026-06-10

每个文件只允许做一件事。任何跨职责的行为视为违反 Contract。

---

## Phase8 — Bridge Layer

| 文件 | 职责 | 允许 | 禁止 |
|------|------|------|------|
| `GameplayBridgeManager.cs` | 唯一 Runtime 协调器 | 拥有 RuntimeMode 切换；拥有 Session 生命周期；控制 Enter/Exit 流程；注入 ProgressionAuthority；订阅 Exit 事件 | 不直接 SetActive Panel；不引用 Calculator；不拥有 Gameplay State；不替代 GoToXxx() |
| `GameplayModuleSession.cs` | Session 状态容器 | 携带 SessionId/SourceArea/RuntimeMode/State；提供状态查询；验证 Session↔RuntimeMode 一致性 | 不引用 MonoBehaviour；不调用任何外部方法；不持有 GameObject 引用 |
| `GameplayRuntimeHost.cs` | Gameplay UI 宿主 | 控制 GameplayCanvasRoot 整体显隐；验证 Setup 完整性 | 不控制单个 Panel 切换；不引用 Gameplay State；不引用 Session |
| `GameplayCanvasGroup.cs` | Panel 引用容器 | 聚合 5 个 Panel 引用；验证引用完整性 | 不控制 Panel 显隐；不持有运行时状态 |
| `IInteractionEntryHandler.cs` | 交互入口接口 | 定义 OnWorkstationInteracted(AreaType) | 不定义 Gameplay/Runtime 语义方法 |
| `RuntimeMode.cs` | 枚举定义 | 定义 WorldMode/GameplayMode | 不包含逻辑 |

---

## Phase3 — Gameplay Layer (Bridge 相关变更)

| 文件 | 职责 | 允许的 Bridge 相关变更 | 禁止 |
|------|------|----------------------|------|
| `GameManager.cs` | Gameplay 状态机 + 模块推进 | `SetProgressionAuthority()` 注入；`CanProgress()` 查询；`GoToXxx()` 开头加 CanProgress gate | 不持有 RuntimeMode；不引用 Phase6/Phase8 类型；不自动推进 |
| `ResultPanelController.cs` | 结果面板 + 退出信号 | `OnExitGameplayEvent` 公开属性（只读） | 不控制 Runtime 状态；不调用 Bridge 方法 |
| `IGameplayProgressionAuthority.cs` | 推进权限接口 | 定义 IsGameplayProgressionAllowed() | 不定义其他方法 |

---

## Phase6 — World Layer (Bridge 相关变更)

| 文件 | 职责 | 允许的 Bridge 相关变更 | 禁止 |
|------|------|----------------------|------|
| `Workstation.cs` | 交互目标 | `Initialize(IInteractionEntryHandler)` 替代 `Initialize(TestUIRouter)` | 不引用 GameplayBridgeManager 类型；不持有 Session |
| `InteractionPoint.cs` | 交互点 | `Initialize(IInteractionEntryHandler)` 替代 `Initialize(TestUIRouter)` | 不引用 RuntimeMode |
| `TestUIRouter.cs` | Legacy UI 路由 | World/Test UI Open/Close（纯 UI）；OnUIClosed 事件；Workstation 初始化委托；独立 fallback | 不控制 Runtime 状态；不新增 Session/Runtime 逻辑；不扩展新功能 |

---

## 不受 P8 影响的文件（禁止修改）

| 层 | 文件 | 原因 |
|----|------|------|
| Phase3 | `ShapeCalculator.cs` | Calculator Isolation Contract |
| Phase3 | `GlazeCalculator.cs` | Calculator Isolation Contract |
| Phase3 | `FireCalculator.cs` | Calculator Isolation Contract |
| Phase3 | `ResultCalculator.cs` | Calculator Isolation Contract |
| Phase3 | `OrderManager.cs` | 仅数据持有者，无运行时逻辑 |
| Phase3 | `ShapeSystem.cs` | 纯计算系统，无 Bridge 交互 |
| Phase3 | `GlazeSystem.cs` | 纯计算系统，无 Bridge 交互 |
| Phase3 | `FiringSystem.cs` | 纯计算系统，无 Bridge 交互 |
| Phase3 | `ResultSystem.cs` | 纯计算系统，无 Bridge 交互 |
| Phase3 | `OrderPanelController.cs` | UI 交互，无 Bridge 交互 |
| Phase3 | `ShapePanelController.cs` | UI 交互，无 Bridge 交互 |
| Phase3 | `GlazePanelController.cs` | UI 交互，无 Bridge 交互 |
| Phase3 | `FiringPanelController.cs` | UI 交互，无 Bridge 交互 |
| Phase6 | `Phase6GameManager.cs` | Bridge 通过 SetState() 间接控制，不修改其代码 |
| Phase6 | `InputManager.cs` | 通过 CanMove()/CanInteract() 门控，不修改其代码 |
| Phase6 | `InteractionController.cs` | 通过 TryInteract() 间接控制，不修改其代码 |
| Phase6 | `PlayerCharacter.cs` | Bridge 调用 StopMoving()，不修改其代码 |
| Phase6 | `MovementController.cs` | 复杂 NavMesh 逻辑，禁止触碰 |

---

## Dependency Direction Summary

```
Phase6 ──→ IInteractionEntryHandler ←── Phase8 (GameplayBridgeManager)
Phase8 ──→ Phase3GameManager.StartGameplayLoop() / StopGameplayLoop()
Phase8 ──→ IGameplayProgressionAuthority ←── Phase3 (GameManager queries)
Phase8 ←── ResultPanelController.OnExitGameplayEvent
Phase8 ──→ GameplayRuntimeHost.ShowGameplayUI() / HideGameplayUI()

Phase3 ──→ (never references Phase6 or Phase8)
Phase6 ──→ (never references Phase3 or Phase8)
```

---

## Violation Detection

如果未来出现以下情况，视为违反本 Matrix：

1. `GameplayBridgeManager` 直接 `panel.SetActive()` → 应通过 RuntimeHost
2. `GameplayModuleSession` 引用 MonoBehaviour → Session 是纯 C# 类
3. `TestUIRouter` 新增 Session/Runtime 逻辑 → Legacy 不扩展
4. `GameManager` 引用 Phase6/Phase8 类型 → 依赖方向违规
5. `Workstation` 引用 `GameplayBridgeManager` 类型 → 仅依赖接口
6. 任何 Phase8 文件引用 Calculator → Calculator Isolation
