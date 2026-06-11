# P8-01 Bridge Runtime Contract

**Status**: FROZEN
**Date**: 2026-06-10
**Supersedes**: None
**Superseded by**: None

本文件是 Phase8 所有实现任务的唯一架构约束源。
P8-02 ~ P8-19 必须遵守本 Contract。任何违反 Contract 的实现视为无效。

---

## 1. Runtime Ownership Contract

World Runtime 与 Gameplay Runtime 必须严格分离。

### World Runtime

| 组件 | 实际拥有权 |
|------|-----------|
| Phase6GameManager | 状态管理与权限门控（`CanMove()`/`CanInteract()`/`CanTransitionTo()`） |
| PlayerCharacter + MovementController | 玩家移动 |
| InputManager | 世界输入路由（依赖 Phase6GameManager 权限门控） |
| InteractionController | Workstation 检测（帧轮询 FindNearestWorkstation） |
| AreaManager | 区域切换 |
| CameraFollow2D | Camera 跟随 |
| ScaleManager | ArtRoot 缩放 |

Phase6GameManager 不直接拥有移动、输入、检测、区域、相机。
它是**权限门控者**，不是**执行者**。

### Gameplay Runtime

| 组件 | 拥有权 |
|------|--------|
| GameManager | Gameplay State + Panel Lifecycle + Module Progression |
| OrderManager | 订单数据 |
| ShapeSystem | 器型计算 |
| GlazeSystem | 釉料计算 |
| FiringSystem | 烧窑模拟 |
| ResultSystem | 结果聚合计算 |
| 各 PanelController | Gameplay UI 交互 + 按钮回调 |

### Bridge Ownership

| 组件 | GameplayBridgeCoordinator |
|------|--------------------------|
| Runtime Mode Switching | Bridge 拥有 |
| Session Lifecycle | Bridge 拥有 |
| Input Lock（调用 Phase6GameManager 权限） | Bridge 拥有 |
| Gameplay Launch（调用 GameManager.StartGameplayLoop()） | Bridge 拥有 |
| Gameplay Exit（监听 onExitGameplay UnityEvent） | Bridge 拥有 |
| Runtime Transfer | Bridge 拥有 |

Bridge 不拥有：
- Gameplay Calculation
- Gameplay Rules
- Gameplay Progression Logic

---

## 2. Dependency Direction Contract

Phase3 永远不知道 Phase6 存在。

### 允许的方向

```
Phase6 → Bridge → Phase3
```

- Bridge 调用 GameManager 公开方法
- Bridge 监听 Phase3 UnityEvent
- Workstation 调用 Bridge（替代直接调用 TestUIRouter）
- Bridge 控制 UI Host

### 禁止的方向

```
Phase3 → Phase6  [FORBIDDEN]
Phase3 → Bridge  [FORBIDDEN]
```

- GameManager 不得引用 Phase6 类型
- Phase3 不得引用 Workstation / InputManager / Camera / Phase6GameManager
- Phase3 不得控制 World State

### 当前已验证合规

Phase3 代码中零 Phase6 类型引用。唯一桥接点：
- `GameManager.StartGameplayLoop()` / `StopGameplayLoop()` — public 方法，设计为 UnityEvent 场景绑定
- `ResultPanelController.onExitGameplay` — UnityEvent，设计为场景 Inspector 绑定

两者均通过 UnityEvent 解耦，不产生类型依赖。

---

## 3. Gameplay Progression Contract

Gameplay Progression is UI-driven.

当前推进结构：

```
Button.onClick → GameManager.GoToXxx() → SetState() → UpdatePanels()
```

- Bridge 不拥有 progression
- Bridge 不重写 progression
- Bridge 不替代 GoToXxx()

Bridge 仅允许 **Gate progression validity**：
- Session 是否有效
- Runtime Mode 是否允许
- Gameplay 是否已退出

禁止实现：
- Auto Scheduler
- Runtime Phase Machine
- Auto Advance Manager
- 任何自动推进机制

---

## 4. Session Contract

一次 Workstation 交互 = 一个 Gameplay Session。

### Session 数据

| 字段 | 类型 | 说明 |
|------|------|------|
| SessionId | int 或 Guid | 唯一标识 |
| SourceWorkstation | Workstation | 触发来源 |
| SourceArea | AreaType | 区域类型 |
| RuntimeMode | enum | World / Gameplay |
| IsActive | bool | 是否活跃 |

### Session 生命周期

```
EnterGameplay()
  ↓
Session Created
  ↓
Gameplay Running
  ↓
ExitGameplay()
  ↓
Session Destroyed
```

### 禁止状态

- 多 Session 并存
- Gameplay 无 Session
- Session Active 但 Gameplay 已关闭

---

## 5. Runtime Mode Contract

World 与 Gameplay 不能同时拥有输入。

### WorldMode

| 允许 | 禁止 |
|------|------|
| 移动 | Gameplay Input |
| 交互 | |
| Camera 跟随 | |

### GameplayMode

| 允许 | 禁止 |
|------|------|
| Gameplay UI | 世界移动 |
| Gameplay Progression | 世界交互 |
| | 新 Workstation Enter |

### Phase6GameState 当前枚举

```csharp
public enum Phase6GameState
{
    Playing,    // WorldMode 对应
    UIOpen,     // GameplayMode 对应
    Interacting, // 已定义，无转换路径（保留）
    Working      // 已定义，无转换路径（保留）
}
```

Phase8 不修改此枚举。Bridge 通过 Phase6GameManager 已有的 `SetState()` 切换模式。

---

## 6. UI Ownership Contract

Gameplay UI 必须由 Bridge Host 管理。

Bridge 必须：
- 接管 UI 生命周期
- 控制 Gameplay Panel 显隐
- 控制 Runtime UI Host

禁止：
- Workstation 直接 SetActive
- TestUIRouter 继续扩展
- Gameplay 自行管理 World UI

### 当前 TestUIRouter.OpenUI() 执行顺序

```
1. gameManager.SetState(Phase6GameState.UIOpen)
2. playerCharacter.StopMoving()
3. uiMap[areaType] lookup → panel
4. panel.SetActive(true)
```

注意：SetState 在 SetActive 之前。如果 lookup 失败，会回滚 SetState(Playing)。
Bridge 替换时必须保持此顺序语义。

---

## 7. Legacy Infrastructure Contract

TestUIRouter 属于 Transitional Legacy Infrastructure。

允许：
- 临时兼容
- 渐进迁移

禁止：
- 新功能继续添加到 TestUIRouter
- 新 Runtime Logic 放入 TestUIRouter
- 新 Session Logic 放入 TestUIRouter

### TestUIRouter 初始化链

```
TestUIRouter.Awake()
  → List<UIMapping> → Dictionary<AreaType, GameObject>
TestUIRouter.Start()
  → FindObjectsOfType<Workstation>()
  → ws.Initialize(this)  [每个 Workstation 获得 router 引用]
```

Bridge 替换 TestUIRouter 时必须接管此初始化逻辑。

---

## 8. Calculator Isolation Contract

以下系统属于 Pure Calculation Layer，禁止修改：

- ShapeCalculator
- GlazeCalculator
- FireCalculator
- ResultCalculator

禁止：
- Bridge 引用 Calculator
- Runtime Layer 引用 Calculator
- 任何 Phase8 Task 修改 Calculator 逻辑

Phase4 的 31/31 E2E 测试必须持续通过。

---

## 9. Serialized Reference Safety Contract

Phase8 实现中，以下 Inspector 绑定是高风险区域：

| 对象 | 字段 | 风险等级 |
|------|------|----------|
| TestUIRouter.uiMappings | List<UIMapping> (AreaType → GameObject) | HIGH — Bridge 替换时需迁移 |
| GameManager.panel* | 5 个 GameObject | HIGH — 添加 BridgeCanvas 时需重新挂载 |
| GameManager.*System | 5 个系统引用 | HIGH — 修改 GameManager 时需确保不丢失 |
| ResultPanelController.onExitGameplay | UnityEvent | HIGH — 唯一出口，必须正确绑定到 Bridge |
| FiringPanelController.openKilnButton | Button.onClick | MEDIUM — GoToResult 触发点 |
| Workstation → TestUIRouter | Initialize(router) | HIGH — 替换路由器时需确保初始化仍被调用 |

任何修改这些引用的 Task 必须在完成输出中声明 Serialized References Changed。

---

## Violation Protocol

任何后续 Task 如果需要违反本 Contract：

1. 必须在 Design 阶段显式声明违反项
2. 必须说明违反原因
3. 必须等待用户审批
4. 未经审批的违反视为无效实现

---

## Change Log

| 日期 | 变更 | 说明 |
|------|------|------|
| 2026-06-10 | Contract Created | 基于 P8-00 Baseline 审计结果，经代码验证修正 |
