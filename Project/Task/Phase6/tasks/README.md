# Phase6 Tasks

## 索引

- [task-p6-01-scene.md](task-p6-01-scene.md)
- [task-p6-02-character.md](task-p6-02-character.md)
- [task-p6-03-interaction.md](task-p6-03-interaction.md)
- [task-p6-04-workstation.md](task-p6-04-workstation.md)
- [task-p6-05-area.md](task-p6-05-area.md)
- [task-p6-06-scale.md](task-p6-06-scale.md)
- [task-p6-07-test.md](task-p6-07-test.md)

## 使用方式

任务文件只回答三件事：

1. 怎么做。
2. 做到什么程度算完成。
3. 先做谁、后做谁。

架构约束请先看 `architecture/`。

## 命名规则

Phase6 沿用当前工程命名习惯，默认不引入 namespace。

已有全局类名必须避让：

- `GameManager` 已被 Phase3 使用，Phase6 使用 `Phase6GameManager`
- `GameState` 已被 Phase3 使用，Phase6 使用 `Phase6GameState`

未冲突的类名继续使用既有风格，例如 `AreaManager`、`ScaleManager`、`InputManager`。

## 完成记录要求

每个 P6 task 完成后必须更新：

- `AGENTS/WorkFlowLayerV1.3/STATE.md`
- `AGENTS/WorkFlowLayerV1.3/DECISIONS.md`

未更新记录时，不视为 task 完成。

## Phase5 Stub 边界

Phase5 遗留的 `PenaltySource` / 缺陷判定 stub 不在 Phase6 中解决。

Phase6 只验证空间化入口、工作台交互和测试 UI 接入。正式烧窑缺陷逻辑留到 Phase7+ 或单独任务处理。

## 地图方案合并状态

`task-p6-01-scene.md` 已吸收地图白盒方案的当前实施内容，包括：

- 2D 俯视/斜俯视正交相机口径
- 2.5D 国风地图：`ArtRoot` 显示 2D 素材，`LogicRoot` 承担隐藏逻辑
- `80m x 60m` 地图坐标
- `_MapRoot` 分层
- 可移动区域坐标
- 静止障碍坐标
- 围墙与交互点坐标
- `NavMesh` 与地图规则测试

当前工程不存在 Phase6 地图场景，因此 `task-p6-01-scene.md` 是创建任务，不是改造任务。
