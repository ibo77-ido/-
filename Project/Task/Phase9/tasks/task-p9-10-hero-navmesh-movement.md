# Task P9-10: 女主基于既有 NavMesh 的寻路移动

## 目标

为当前 Phase9 世界场景中的 `女主` 增加点击寻路移动能力。移动方案参考 Phase6 的角色移动系统，但只消费当前已经烘焙完成的 NavMesh，不重新制作、不扩展、不重烘焙可行走区域。

当前 NavMesh 约定：

- `NavMesh-walkable` 根节点持有现有 `NavMeshSurface`
- `NavMesh-walkable (1)` 是唯一可行走区域来源
- `NavMesh-walkable (1)` 图片覆盖范围内可行走
- 覆盖范围外不可行走
- 已通过抽样验收：内部点可命中 NavMesh，外部点不可命中 NavMesh

## 修正结论

本任务不能简单新增一个独立 `InputManager.Update()`。Phase9 已经有 `Phase9InteractionBridge.Update()` 处理世界模式输入、E 键交互、Escape 退出玩法模块等逻辑。如果再并行新增一个点击移动输入脚本，会产生输入路由冲突。

修正后的原则是：

- `Phase9InteractionBridge` 作为 Phase9 世界输入总入口
- 点击移动要么直接整合进 `Phase9InteractionBridge`
- 要么由 `Phase9InteractionBridge` 调用一个无独立 Update 的移动输入/目标解析 helper
- 不允许 `Phase9InteractionBridge` 和新 `InputManager` 各自独立消费输入

## 关键约束

1. 不改已有 NavMesh  
   本任务只给 `女主` 增加使用现有 NavMesh 的能力，不新增可行走面，不改 `NavMeshSurface`、`NavMeshModifier`、`NavMesh-walkable (1)` Collider 或 NavMeshData。

2. 不复制 Phase6 脚本  
   直接复用 `Assets/Phase6/Scripts/` 下已有组件，例如 `MovementController`、`CharacterStateMachine`、`PlayerCharacter`。不要在 `Assets/Phase9/Scripts/` 下创建同名副本。  
   如确实需要 Phase9 特有逻辑，只新增小型适配脚本或修改 `Phase9InteractionBridge`。

3. 统一移动入口  
   所有移动请求统一进入 `PlayerCharacter.SetDestination()` 或 `PlayerCharacter.StopMoving()`。其他系统不要直接改 `Transform`，也不要直接调用 `NavMeshAgent.SetDestination()`。

4. 不允许圈外点击吸附到圈内  
   点击点如果在 `NavMesh-walkable (1)` 覆盖范围外，应直接忽略。不能用大半径 `NavMesh.SamplePosition` 把外部点击吸到最近 NavMesh 点。

5. 只接受完整路径  
   目标点通过范围验证后，还必须 `NavMeshAgent.CalculatePath` 成功，并且路径状态为 `NavMeshPathStatus.PathComplete`，才允许移动。

## Phase9 输入路由设计

### 当前问题

`Phase9InteractionBridge.Update()` 已经处理：

- `RuntimeMode.GameplayMode` 下 Escape 退出模块
- `RuntimeMode.WorldMode` 下 E 键交互
- 交互点距离检测

如果再新增独立 `InputManager.Update()` 处理鼠标点击移动，会出现：

- 点击移动和 E 键交互互相不知道对方状态
- UI 打开时是否允许移动没有统一判断
- 交互进入玩法模块时，移动可能还在继续
- 后续 Phase11 快捷入口也不知道应该调用谁

### 修正方案

`Phase9InteractionBridge` 应升级为世界模式输入总协调器：

```text
Phase9InteractionBridge.Update()
  -> GameplayMode: 处理 Escape / 玩法模块退出
  -> 非 WorldMode: return
  -> WorldMode:
       1. 确保 Phase3 已加载
       2. 确保 playerCharacter / movementController / interactionController 引用有效
       3. E 键交互
       4. 鼠标点击移动
```

点击移动可以写成 `Phase9InteractionBridge` 内部方法：

```csharp
private void HandleWorldClickMove()
```

也可以抽成 helper：

```csharp
public sealed class Phase9ClickMoveResolver : MonoBehaviour
{
    public bool TryResolveMoveTarget(Vector3 screenPosition, out Vector3 navTarget);
}
```

但 helper 不应有自己的 `Update()`。

## Player 引用设计

### 当前问题

`Phase9InteractionBridge` 目前通过：

```csharp
GameObject.Find(playerName)
```

拿到裸 `Transform player`，并用它做距离计算和 `SendMessage("StopMoving")`。

这会导致桥接层不知道：

- `PlayerCharacter`
- `MovementController`
- `InteractionController`
- `CharacterStateMachine`

后续点击移动、快捷入口、交互进入玩法模块都无法走统一接口。

### 修正方案

`Phase9InteractionBridge` 应持有运行时角色引用：

```csharp
[SerializeField] private PlayerCharacter playerCharacter;
[SerializeField] private MovementController movementController;
[SerializeField] private InteractionController interactionController;
```

必要时仍可保留 `Transform player` 作为兼容字段，但应从 `playerCharacter.Transform` 派生：

```csharp
private Transform PlayerTransform =>
    playerCharacter != null ? playerCharacter.Transform : player;
```

交互距离检测使用 `PlayerTransform.position`。  
停止移动使用 `playerCharacter.StopMoving()`。  
点击移动使用 `playerCharacter.SetDestination(navTarget)`。

## 移动权限门控

### 当前问题

Phase6 的 `PlayerCharacter.SetDestination()` 有可选的 `Phase6GameManager` 检查：

```csharp
if (gameManager != null && !gameManager.CanMove()) return;
```

Phase9 场景通常没有 `Phase6GameManager`。如果直接复用但不补门控，`gameManager == null` 时会跳过检查，导致 UI 或玩法模块打开时仍可能点击移动。

### 修正方案

Phase9 的移动权限由 `Phase9InteractionBridge` 负责，不依赖 `Phase6GameManager`。

推荐先采用最小改动：

- 不在 Phase9 场景中新增 `Phase6GameManager`
- 不复制 `PlayerCharacter`
- 点击移动只从 `Phase9InteractionBridge` 进入
- `Phase9InteractionBridge` 在调用 `SetDestination` 前检查：
  - `currentRuntimeMode == RuntimeMode.WorldMode`
  - Phase3 UI 未处于可交互阻塞状态
  - 指针不在 UI 上
  - 未处于交互进入/退出流程中

如果后续需要让 Phase11 或更多系统发起移动，应再补一个正式接口，例如：

```csharp
public bool CanMoveInWorldMode()
```

或引入：

```csharp
public interface IMoveAuthority
{
    bool CanMove();
}
```

但第一阶段不要为了接口化过度改动 Phase6 脚本。

## 组件复用策略

直接复用：

- `Assets/Phase6/Scripts/Systems/MovementController.cs`
- `Assets/Phase6/Scripts/Systems/PlayerCharacter.cs`
- `Assets/Phase6/Scripts/Systems/CharacterStateMachine.cs`
- `Assets/Phase6/Scripts/Core/ICharacter.cs`

谨慎复用：

- `Assets/Phase6/Scripts/Systems/InputManager.cs`

Phase9 不建议挂载 Phase6 的 `InputManager`，因为输入入口应由 `Phase9InteractionBridge` 统一管理。

`InteractionController` 不作为 Phase9 世界交互的主入口。当前 Phase9 已通过 `Phase9InteractionBridge` 自己维护 entry point 距离检测和 `EnterGameplay()`，这与 Phase6 的 `Workstation` / `InteractionController.TryInteract()` 是两套交互模型。后续 Phase11 HUDQuickBar 如果要触发 Phase9 世界交互，应调用 `Phase9InteractionBridge` 暴露的等价入口，而不是调用 Phase6 的 `InteractionController.TryInteract()`。

## 执行方案

### 1. 绑定女主基础移动组件

给 `女主` 添加或确认以下组件：

- `NavMeshAgent`
- `MovementController`
- `PlayerCharacter`
- `CharacterStateMachine`

不新增同名脚本副本。

`NavMeshAgent` 建议：

- `updatePosition = false`
- `updateRotation = false`
- `speed` 使用保守默认值或已有配置
- `stoppingDistance` 设置为较小值
- `radius`、`height`、`baseOffset` 需要结合当前 NavMesh 平面验证

### 2. 升级 Phase9InteractionBridge 的角色引用

在 `Phase9InteractionBridge` 中补充：

- `PlayerCharacter`
- `MovementController`
- 可选 `InteractionController`

绑定策略：

- Inspector 优先
- 运行时 fallback：通过 `playerName` 找 `GameObject`，再 `GetComponent<PlayerCharacter>()`
- 如果仍找不到，记录 warning，但不要崩溃

### 3. 将 StopPlayerMotion 改为强类型调用

当前：

```csharp
player.SendMessage("StopMoving", SendMessageOptions.DontRequireReceiver);
```

建议改为：

```csharp
if (playerCharacter != null)
{
    playerCharacter.StopMoving();
}
```

如需兼容旧对象，可保留 `SendMessage` 作为最后 fallback。

### 4. 在 Phase9InteractionBridge 内处理点击移动

只在以下条件满足时处理点击移动：

- `currentRuntimeMode == RuntimeMode.WorldMode`
- `isPhase3Loaded == true`
- 鼠标左键按下
- 指针不在 UI 上
- `playerCharacter != null`
- 当前没有进入玩法模块流程

不要新增独立 `InputManager.Update()`。

### 5. 目标点解析与合法性检查

移动前必须依次通过：

1. 屏幕点转换为世界候选点
2. 候选点在 `NavMesh-walkable (1)` 覆盖 bounds 内
3. 使用小半径 `NavMesh.SamplePosition`
4. 命中点仍在 `NavMesh-walkable (1)` 覆盖 bounds 内
5. `MovementController` 或 `PlayerCharacter` 计算完整路径
6. 路径状态为 `NavMeshPathStatus.PathComplete`

推荐封装：

```csharp
private bool TryResolveMoveTarget(Vector3 screenPosition, out Vector3 navTarget)
```

普通点击不要使用大半径吸附。容差建议 `0.1f` 到 `0.3f`。

### 6. 手动路径移动

沿用 Phase6 `MovementController` 的方式：

- `NavMeshAgent.CalculatePath`
- 缓存 `NavMeshPath.corners`
- 每帧 `Vector3.MoveTowards`
- 同步 `navMeshAgent.nextPosition`
- 到达终点后停止

现有 `Assets/Phase6/Scripts/Systems/MovementController.cs` 的 `SetDestination()` 目前使用的是：

```csharp
if (pathFound && manualPath.status != NavMeshPathStatus.PathInvalid && manualPath.corners.Length > 1)
```

这会放行 `NavMeshPathStatus.PathPartial`。本任务应把复用组件本体修正为只接受完整路径：

```csharp
if (pathFound && manualPath.status == NavMeshPathStatus.PathComplete && manualPath.corners.Length > 1)
```

这个修改发生在 `Assets/Phase6/Scripts/Systems/MovementController.cs`，不是创建 Phase9 新副本。它符合“不复制 Phase6 脚本”的原则，属于对共享复用组件的安全修复。

### 7. 状态与互斥

最小状态要求：

- `WorldMode` 时允许移动和 E 键交互
- `GameplayMode` 时禁止移动
- 进入玩法模块时停止当前移动
- UI 指针命中时禁止点击移动

后续如果接入 `CharacterStateMachine`：

- 新移动开始：`Moving`
- 到达目标：`Idle`
- 进入玩法模块：`Interacting` 或 `UIOpen`
- 退出玩法模块：`Idle`

## Phase11 衔接

P9-10 是 Phase11 HUDQuickBar 的前置任务。

Phase11 可能依赖：

- `MovementController.HasReachedDestination()`
- `PlayerCharacter.SetDestination()`
- `PlayerCharacter.StopMoving()`
- `Phase9InteractionBridge` 暴露的等价交互入口

因此 P9-10 验收时需要保证：

- `女主` 上可以稳定获取 `MovementController`
- `Phase9InteractionBridge` 可以稳定获取 `PlayerCharacter`
- 后续快捷入口能通过统一接口发起移动或交互
- 不需要 Phase11 直接操作 `Transform`

Phase11 快捷入口触发交互时，应调用 `Phase9InteractionBridge` 的 Phase9 等价入口，而非 Phase6 的 `InteractionController.TryInteract()`。P9-10 或后续衔接任务需要在桥接层提供明确接口，例如：

```csharp
public bool TryEnterNearestGameplayModule()
```

## 需要避免的漏洞

### 输入路由冲突

错误方向：

```text
Phase9InteractionBridge.Update() 处理 E 键
InputManager.Update() 处理点击移动
```

正确方向：

```text
Phase9InteractionBridge.Update() 统一处理 WorldMode 输入
```

### 圈外点击被吸进 NavMesh

错误做法：

```csharp
NavMesh.SamplePosition(clickPoint, out hit, 3f, NavMesh.AllAreas)
```

正确做法：

- 先检查点击点是否在 `NavMesh-walkable (1)` 覆盖范围内
- 再用小半径 `SamplePosition`
- 命中点还要再次检查范围

### 只检查有路径，不检查完整路径

错误做法：

```csharp
if (agent.CalculatePath(target, path)) Move(path);
```

正确做法：

```csharp
if (agent.CalculatePath(target, path) &&
    path.status == NavMeshPathStatus.PathComplete)
{
    Move(path);
}
```

### 继续使用裸 Transform 作为主角接口

错误方向：

```csharp
Transform player;
player.SendMessage("StopMoving");
```

正确方向：

```csharp
PlayerCharacter playerCharacter;
playerCharacter.StopMoving();
```

### Phase9 依赖 Phase6GameManager

不要为了移动权限在 Phase9 场景中硬塞 `Phase6GameManager`。Phase9 的运行模式已经由 `Phase9InteractionBridge` 管理，移动权限应从该桥接层判断。

## 验收标准

### 功能验收

- WorldMode 下点击 `NavMesh-walkable (1)` 覆盖区域内，女主可以移动
- 点击覆盖区域外，女主不移动
- 点击 UI 时，女主不移动
- GameplayMode 下，女主不响应点击移动
- 按 E 进入玩法模块时，会停止当前移动
- 路径不完整或不可达时，女主不移动
- 移动结束后不继续残留速度

### 数据验收

- 不修改现有 NavMesh 烘焙来源
- `静态层` 不重新持有 `NavMeshSurface`
- 唯一导航来源仍为现有 `NavMesh-walkable`
- `NavMesh-walkable (1)` 仍是唯一可走覆盖区域
- 不新增额外可走 Collider 或可走 Mesh

### 代码验收

- 不创建 Phase6 移动脚本的 Phase9 同名副本
- 不挂载独立运行的 Phase6 `InputManager`
- `Phase9InteractionBridge` 持有或能解析 `PlayerCharacter`
- `StopPlayerMotion` 使用强类型调用优先
- `Assets/Phase6/Scripts/Systems/MovementController.cs` 中的 `SetDestination()` 明确检查 `NavMeshPathStatus.PathComplete`，不放行 `PathPartial`

### Phase11 前置验收

- `女主` 上可获取 `MovementController`
- `女主` 上可获取 `PlayerCharacter`
- 后续 HUDQuickBar 不需要直接移动 Transform
- 交互入口有明确调用方式：调用 `Phase9InteractionBridge` 的等价桥接方法，而不是 Phase6 `InteractionController.TryInteract()`

### 运行验收

- Console 无编译错误
- Play Mode 下点击移动无异常
- 女主不会漂出 NavMesh 覆盖范围
- CameraFollow2D 跟随正常

## 推荐实施顺序

1. 给 `女主` 绑定复用的 Phase6 移动组件
2. 升级 `Phase9InteractionBridge` 的 player 引用为 `PlayerCharacter` 优先
3. 将 `StopPlayerMotion` 改为强类型调用
4. 在 `Phase9InteractionBridge` 中增加点击移动入口
5. 实现 `NavMesh-walkable (1)` bounds 检查
6. 实现小半径 `SamplePosition` 与 `PathComplete` 检查
7. 验证 WorldMode / GameplayMode 输入互斥
8. 验证 Phase11 所需运行时引用是否可获取

## 暂不处理

以下内容不在本任务第一阶段内：

- 重做或扩展 NavMesh
- 自动避障
- 多角色寻路
- 复杂四方向或八方向动画
- 地图动态阻挡
- Phase11 HUDQuickBar 的完整 UI 实现
