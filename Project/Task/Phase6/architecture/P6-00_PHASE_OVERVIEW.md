# P6-00 Phase Overview

## Phase6 目标

Phase6 是空间化玩法验证阶段，核心不是生产系统，而是打通地图、移动、交互、区域、缩放和测试闭环。

只做四件事：

1. 能走。
2. 能交互。
3. 能验证。
4. 能扩展。

## 开发边界

必须保留：

- `SO` 驱动
- `LogicRoot / ArtRoot`
- `Workstation + InteractionPoint`
- `AreaTrigger`
- `Phase6GameManager`
- `RefreshVisual()`

## 命名规则

Phase6 代码沿用当前工程命名习惯：默认不引入 namespace，类名使用清晰职责名。

已有全局命名必须避让：

- Phase3 已存在 `GameManager`
- Phase3 已存在 `GameState`

因此 Phase6 使用：

- `Phase6GameManager`
- `Phase6GameState`

其他未冲突类名继续使用既有风格，例如 `AreaManager`、`ScaleManager`、`InputManager`、`MovementController`。

必须冻结：

- 正式生产系统
- 工匠 AI
- NPC 行为
- 复杂区域视觉控制
- 自动场景生成器
- 自动场景校验器
- Phase5 遗留的 `PenaltySource` / 缺陷判定 stub

Phase6 只验证空间化入口和测试 UI 接入，不解决正式烧窑缺陷逻辑。

## 完成定义

允许进入 Phase7 的条件：

- 无 Console Error
- 玩家可移动
- 摄像机跟随正常
- 区域检测正常
- E 键交互正常
- 工作台结构完成
- `RefreshVisual()` 正常
- `AssetScaleConfigSO` 正常
- `SO` 驱动正常
- 完整空间化流程可运行

## 开发顺序

1. `P6-01_SCENE_FOUNDATION`
2. `P6-02_CHARACTER_SYSTEM`
3. `P6-03_INTERACTION_SYSTEM`
4. `P6-04_WORKSTATION_SYSTEM`
5. `P6-05_AREA_SYSTEM`
6. `P6-06_SCALE_SYSTEM`
7. `P6-07_WHITEBOX_TEST`
