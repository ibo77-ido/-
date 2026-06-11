# P8-07 Data Ownership

**Status**: FROZEN
**Date**: 2026-06-10

## Classification

### Persistent (跨 Session 存活)

| 数据 | 拥有者 | 说明 |
|------|--------|------|
| `GameplayModuleSession.nextId` (static) | Session | 单调递增 ID 生成器，设计如此 |
| `OrderManager.currentIndex` | Phase3 | 订单进度追踪，玩家下次进入继续 |
| `GameplayBridgeManager.*` SerializeField 引用 | Bridge | 场景绑定，跨 Session 不变 |
| `Phase6GameManager.*` SerializeField 引用 | Phase6 | 场景绑定，跨 Session 不变 |
| `GameManager.*` SerializeField 引用 | Phase3 | 场景绑定，跨 Session 不变 |

### Disposable (Session 内临时，必须清理)

| 数据 | 拥有者 | 清理时机 | 清理方式 |
|------|--------|----------|----------|
| `GameplayBridgeManager.currentRuntimeMode` | Bridge | Exit/Abort | → WorldMode |
| `GameplayBridgeManager.currentSession` | Bridge | Exit/Abort | → Close() + null |
| `Phase6GameManager.CurrentState` | Phase6 | Exit/Abort | Bridge 调用 SetState(Playing) |
| `GameManager.currentState` | Phase3 | StopGameplayLoop() | → None |
| `GameManager.advanceOrderOnNextStart` | Phase3 | StopGameplayLoop() | → false |
| `FiringSystem.*` 运行时状态 | Phase3 | BeginOrder() → ResetFiring() | 自动重置 |
| `ShapeSystem.*` target 字段 | Phase3 | BeginOrder() → LoadTargetFromCurrentOrder() | 自动覆盖 |
| `GlazeSystem.*` target 字段 | Phase3 | BeginOrder() → LoadTargetFromCurrentOrder() | 自动覆盖 |
| `ResultSystem.LastResult` | Phase3 | 下次 CalculateResult() | 自动覆盖 |
| `GameplayRuntimeHost.IsVisible` | Bridge | Exit/Abort | → HideGameplayUI() |

### Non-Data (Config / Reference，不属于运行时数据)

| 类型 | 说明 |
|------|------|
| SerializeField 引用 | Inspector 绑定，跨 Session 不变 |
| ScriptableObject 配置 | 只读配置数据 |
| Calculator 静态类 | 无状态纯计算 |

---

## Data Flow Across Session

```
Session 1 (Order #0):
  Enter → Session Created → Gameplay Active → Result → Exit
  ↓
  Session Closed + Null
  GameManager.advanceOrderOnNextStart = false (cleared by StopGameplayLoop)
  FiringSystem.ResetFiring() will run on next BeginOrder
  ↓
Session 2 (Order #0 again, or #1 if advanceOrder was true):
  Enter → Session Created → ...
```

---

## Stale Data Risk Assessment

| 风险 | 状态 | 说明 |
|------|------|------|
| `advanceOrderOnNextStart` 泄漏 | **FIXED** | `StopGameplayLoop()` 现在重置为 false |
| `currentSession` 引用未清空 | **FIXED** | Exit/Abort 后设为 null |
| FiringSystem 温度残留 | **LOW** | `BeginOrder()` → `ResetAllSystems()` → `ResetFiring()` 自动清理 |
| ShapeSystem/GlazeSystem target 残留 | **LOW** | `BeginOrder()` → `LoadTargetFromCurrentOrder()` 自动覆盖 |
| ResultSystem.LastResult 残留 | **LOW** | 下次 `CalculateResult()` 自动覆盖 |

---

## Ownership Matrix

| 数据类别 | World (Phase6) | Bridge (Phase8) | Gameplay (Phase3) |
|----------|---------------|-----------------|-------------------|
| World State | **Owns** | | |
| RuntimeMode | | **Owns** | |
| Session | | **Owns** | |
| Input Permission | **Owns** (via Phase6GameManager) | **Controls** (via SetState) | |
| Gameplay State | | | **Owns** |
| Gameplay Progression | | **Gates** (via CanProgress) | **Owns** (GoToXxx) |
| Gameplay Calculation | | | **Owns** |
| Gameplay UI Host | | **Owns** (via RuntimeHost) | |
| Panel Switching | | | **Owns** (via UpdatePanels) |
| Order Progression | | | **Owns** (via OrderManager) |
| Gameplay Result Data | | | **Owns** (via ResultSystem) |

---

## Rules

1. Bridge 不拥有 Gameplay 数据，只拥有 Runtime Mode + Session
2. Phase3 不拥有 Runtime 数据，只拥有 Gameplay State + Progression
3. Phase6 不拥有 Gameplay 数据，只拥有 World State
4. Disposable 数据必须在 Session 关闭时清理
5. Persistent 数据不得在 Session 之间泄漏 Session-scoped 状态
