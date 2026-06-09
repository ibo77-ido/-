# task-p6-02 Character

## 目标

实现玩家角色、移动、状态机和输入入口。

角色服务于 2.5D 国风俯视/斜俯视地图。玩家看到 `_MapRoot/ArtRoot` 的 2D 地图素材，移动逻辑仍基于 `_MapRoot/LogicRoot`、Unity `X/Z` 平面和 `NavMeshAgent`，输入需要通过 `Camera_2D_Oblique` 将屏幕点击转换为地面目标点。

## 步骤

1. 创建 `CharacterConfigSO`。
2. 创建 `ICharacter`。
3. 创建 `PlayerCharacter`。
4. 创建 `MovementController`。
5. 创建 `CharacterStateMachine`。
6. 创建 `Phase6GameManager`。
7. 创建 `Phase6GameState`。
8. 创建 `InputManager`。
9. 创建或挂载 `CameraFollow2D`。
10. 让输入通过斜俯视相机驱动移动。
11. 让状态机约束 UI 打开与移动互斥。

## 验收

- 玩家移动正常
- 斜俯视相机下点击地图可移动
- 相机保持 Orthographic，不切换第三人称透视
- 点击视觉地图时，实际移动目标落到隐藏逻辑层/NavMesh
- 状态切换正常
- UI 打开时禁止移动
- 不需要 NPC 也能闭环

## 依赖

- `task-p6-01-scene.md`

## 交付物

- `PlayerCharacter`
- `MovementController`
- `CharacterStateMachine`
- `Phase6GameManager`
- `Phase6GameState`
- `InputManager`
- `CameraFollow2D`
- `STATE.md` / `DECISIONS.md` 已更新
