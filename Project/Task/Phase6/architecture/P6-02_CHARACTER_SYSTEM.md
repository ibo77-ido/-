# P6-02 Character System

## 核心对象

### `CharacterConfigSO`

保存角色移动、交互和默认状态参数。

### `ICharacter`

用于避免交互逻辑写死 Player。

最小接口：

```csharp
public interface ICharacter
{
    Transform Transform { get; }
    CharacterState CurrentState { get; }
}
```

### `PlayerCharacter`

玩家角色根对象，负责聚合：

- `MovementController`
- `InteractionController`
- `CharacterStateMachine`
- `NavMeshAgent`
- `LogicRoot`
- `ArtRoot`

Phase6 角色以 2.5D 国风俯视/斜俯视方式呈现。逻辑移动仍发生在 `X/Z` 平面上，表现层使用 `ArtRoot` 下的 2D/2.5D 角色素材，逻辑层与视觉层分离。

## 状态定义

MVP 状态：

- `Idle`
- `Moving`
- `Interacting`
- `UIOpen`
- `Working`

冻结状态：

- `Waiting`
- `Resting`
- `Disabled`

## 控制器职责

### `MovementController`

- 接收移动目标
- 将屏幕点击转换为 `Y=0` 地面上的 `X/Z` 目标点
- 调用 `NavMeshAgent`
- 处理到达与停止

移动输入口径：

- 使用 `Camera_2D_Oblique` 从屏幕点发射射线。
- 射线命中地面或 `Y=0` 平面后得到目标点。
- 目标点必须落在 `NavMesh` 上。
- UI 打开时不处理地图点击移动。
- 玩家看到的地图来自 `_MapRoot/ArtRoot`，移动判断来自 `_MapRoot/LogicRoot`。

### `CameraFollow2D`

P6-02 可新增轻量相机跟随组件，挂在 `_CameraRoot` 或 `Camera_2D_Oblique` 上。

- 保持 Orthographic。
- 保持斜俯视角度不变。
- 只跟随玩家 `X/Z` 位移。
- 不旋转相机，不切换透视相机。

### `CharacterStateMachine`

- 统一管理状态切换
- 约束移动、交互和 UI 的互斥关系

### `InputManager`

- 统一接收输入
- 将输入分发给移动、交互或流程控制层

## 规则

1. UI 打开时禁止移动和重复交互。
2. `PlayerCharacter` 不直接打开 UI。
3. `Working` 仅作为 Phase7 预留状态。
4. Phase6 不使用第一人称或第三人称跟随相机。
5. Phase6 不使用 Rigidbody2D / Collider2D 作为移动基础。
6. 角色视觉素材不参与导航判断，导航只看隐藏逻辑层。
