# P6-01 Scene Foundation

## 地图规范

Phase6 采用 2.5D 国风俯视/斜俯视地图方案，先保证“可走、可辨认、可验证”。

核心原则：

- 玩家看到的是 `ArtRoot` 下的 2D 地图素材。
- 系统运行的是 `LogicRoot` 下的隐藏逻辑层。
- 当前绿色/灰色白盒块只作为逻辑、调试和验证使用，不作为最终玩家视觉。

## 视角规范

Phase6 地图不是第一人称或第三人称 3D 场景，也不是把一堆可见 3D Cube 当作最终地图，而是 2D 国风地图素材 + 隐藏逻辑层的 2.5D 表现。

实现口径：

- 逻辑平面仍使用 Unity `X/Z` 平面。
- 摄像机使用 Orthographic 正交相机。
- 推荐相机命名：`Camera_2D_Oblique`。
- 推荐相机位置：`(40, 45, -35)`。
- 推荐相机旋转：`(60, 0, 0)`。
- 推荐 `Orthographic Size`：`36~42`，以完整覆盖 `80m x 60m` 地图为准。
- 地图视觉由 2D 图片、Sprite、贴图平面或分层素材表达。
- 白盒块默认隐藏或半透明，仅用于调试、碰撞、寻路和区域验证。
- 不使用 Unity 2D 物理作为 Phase6 地图移动基础；移动、阻挡和区域检测仍使用 3D Collider / Trigger / NavMesh。

优先级：

1. 坐标与可验证性优先。
2. 2D 国风地图素材视觉优先。
3. 逻辑层可验证性优先。
4. 美术细节可逐步替换。

| 项目 | 标准 |
| --- | --- |
| Unity 单位 | 1 Unit = 1 米 |
| 地图建议尺寸 | 80m x 60m |
| 主路宽度 | 4m |
| 次路宽度 | 2.5m |
| 小路宽度 | 1.5m |
| 建筑间距 | >= 3m |
| 建筑与道路间距 | >= 2m |

坐标规范：

- X 轴范围：`0~80`
- Z 轴范围：`0~60`
- 地图中心点：`(40, 0, 30)`
- Y 轴向上

地图白盒方案已合并进本文件和 `tasks/task-p6-01-scene.md`。

当前工程不存在 `Workshop_TestScene`、`SceneAutoBuilder`、`MapRuleValidator`。P6-01 是从零开发地图白盒的任务，后续可在完成 MVP 后再迁移到配置化生成。

## 功能区

- `OrderArea`
- `WheelArea`
- `GlazeArea`
- `KilnArea`
- `StorageArea`
- `MaterialArea`

NPC 区域只做预留，不作为 MVP 必须区域。

## 场景树

```text
Workshop_TestScene
├─ _MapRoot
│  ├─ LogicRoot
│  │  ├─ Ground_Base
│  │  ├─ WalkableRoot
│  │  ├─ StaticBlockerRoot
│  │  ├─ WallRoot
│  │  ├─ AreaTriggerRoot
│  │  ├─ WorkstationRoot
│  │  ├─ RouteDebugRoot
│  │  └─ ExpansionAnchorRoot
│  └─ ArtRoot
│     ├─ Map_Background_2D
│     ├─ Map_Buildings_2D
│     ├─ Map_Foreground_2D
│     └─ Map_Overlay_2D
├─ _PlayerRoot
├─ _CameraRoot
│  └─ Camera_2D_Oblique
├─ _ConfigRoot
└─ _Validators
```

建议 `Systems` 至少包含：

- `Phase6GameManager`
- `AreaManager`
- `ScaleManager`
- `InputManager`

## 目录结构

建议将 Phase6 资源放在独立目录：

```text
Assets/Phase6/
├ Scenes
├ Scripts
├ Prefabs
├ Data
├ Materials
└ Art
  └ Map
```

## 布局原则

1. 核心路线形成闭环。
2. 工作台朝向主路或中央空间。
3. `InteractionPoint` 不放在模型内部。
4. 道路不要过窄。
5. 区域边界能被玩家识别。
6. 所有核心对象在 2D 地图素材上必须可读，不依赖近距离 3D 观察。
7. 白盒逻辑层可通过 Debug 开关显示，但默认不作为最终视觉。

## 组件规则

- `LogicRoot` 只负责碰撞、触发、导航、交互点、调试验证。
- `ArtRoot` 只负责 2D 国风地图素材、建筑图层、前景遮挡和视觉高亮。
- `Ground_Base` 只做逻辑底板或调试底板，不作为最终视觉，也不作为 `NavMesh` 来源。
- `WalkableRoot` 下的绿色区域是隐藏可移动和 `NavMesh` 来源。
- `StaticBlockerRoot` 与 `WallRoot` 使用 3D `BoxCollider` 表达隐藏阻挡。
- `AreaTriggerRoot` 后续使用 3D Trigger，覆盖在可移动区域上方。
- 交互点先用空 GameObject 标记位置，建议加 Debug 半径占位，逻辑组件留给 P6-03。
- `_CameraRoot` 必须包含 2D 斜俯视正交相机。
- `Map_Background_2D` 是整张或分块 2D 国风地图底图。
- `Map_Buildings_2D` 放建筑、窑炉、树、摊位等可替换视觉素材。
- `Map_Foreground_2D` 放屋檐、树冠等可遮挡玩家的前景素材。
- `Map_Overlay_2D` 放区域高亮、交互提示和调试覆盖层。

## 白盒坐标

可移动区域统一放入 `LogicRoot/WalkableRoot`，使用 `Mat_Walkable_Green`。该层默认作为隐藏逻辑层或 Debug 半透明层。

| ID | 中心点 X,Z | 尺寸 W,D |
| --- | ---: | ---: |
| `OrderArea_Walkable` | 11, 14.5 | 20 x 26 |
| `WheelArea_Walkable` | 11, 45.5 | 20 x 26 |
| `GlazeArea_Walkable` | 39, 48 | 24 x 22 |
| `StorageArea_Walkable` | 39, 10 | 24 x 18 |
| `KilnArea_Walkable` | 67, 45.5 | 24 x 26 |
| `MaterialArea_Walkable` | 67, 14.5 | 24 x 26 |
| `MainRoad_Walkable` | 24, 30 | 5.5 x 60 |
| `SecondaryRoad_Walkable` | 40, 30 | 80 x 4.5 |
| `KilnBranch_Walkable` | 53.5, 28 | 4 x 18 |

静止障碍统一放入 `LogicRoot/StaticBlockerRoot`，使用 `Mat_Static_Gray`。该层默认作为隐藏逻辑层或 Debug 半透明层。

| ID | 中心点 X,Z | 尺寸 W,D | 高度 |
| --- | ---: | ---: | ---: |
| `Warehouse_Blocker` | 39, 29 | 13 x 12 | 3 |
| `Workshop_Blocker` | 53, 29 | 13 x 12 | 3.5 |
| `Kiln_Blocker` | 69, 50 | 14 x 12 | 4 |
| `WheelObstacle_01` | 10, 38 | 9 x 4 | 1 |
| `StorageObstacle_01` | 47, 9 | 5 x 9 | 1.5 |
| `MaterialPile_01` | 59, 11 | 6 x 7 | 1.2 |
| `MaterialObstacle_02` | 74, 8 | 6 x 5 | 1.2 |

核心交互点：

| ID | 建议位置 X,Z | 半径 |
| --- | ---: | ---: |
| `Interaction_OrderBoard` | 11, 12 | 2 |
| `Interaction_WheelStation` | 11, 43 | 2 |
| `Interaction_GlazeStation` | 39, 46 | 2 |
| `Interaction_Kiln` | 69, 43 | 2.5 |
| `Interaction_Storage` | 39, 12 | 2 |
| `Interaction_Material` | 67, 12 | 2 |
