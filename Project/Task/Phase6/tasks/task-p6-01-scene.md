# task-p6-01 Scene

## 目标

搭建 `Workshop_TestScene` 和可扩展 2.5D 国风俯视/斜俯视地图骨架。

本任务已合并地图白盒方案。当前工程不存在 `Workshop_TestScene`，也未检出 `SceneAutoBuilder` / `MapRuleValidator`，因此本任务是从零创建 Phase6 地图白盒，同时保留后续配置化生成入口。

核心目标：

- 地图表现采用 2D 国风地图素材 + 俯视/斜俯视正交相机
- 地图逻辑采用隐藏 `LogicRoot`
- 地图视觉采用 `ArtRoot`
- 地图尺寸采用 `80m x 60m`
- 坐标沿用 `X: 0~80`、`Z: 0~60`
- 地图中心点为 `(40, 0, 30)`
- 所有可移动区域统一绿色 Debug 逻辑层
- 所有静止障碍统一灰色 Debug 逻辑层
- 区域触发与可移动地面分离
- 工作站按 `LogicRoot / ArtRoot` 分层

## 视角与组件口径

Phase6 不是 3D 第三人称地图，也不是把 3D Cube 作为最终地图视觉，而是 2D 国风地图素材视觉。

- 逻辑平面：Unity `X/Z`
- 相机：Orthographic 正交相机
- 推荐相机：`Camera_2D_Oblique`
- 推荐位置：`(40, 45, -35)`
- 推荐旋转：`(60, 0, 0)`
- 推荐 `Orthographic Size`：`36~42`
- 视觉地图：2D 图片、Sprite、贴图平面或分层素材
- 逻辑地图：隐藏或半透明 Debug 白盒块
- 移动基础：仍使用 3D Collider / Trigger / NavMesh，不改成 Rigidbody2D / Collider2D
- `Ground_Base` 只做逻辑底板或 Debug 底板，不参与 `NavMesh`
- `WalkableRoot` 才是绿色可走逻辑区和 `NavMesh` 来源
- `ArtRoot` 才是玩家最终看到的地图视觉

## 步骤

1. 新建 `Workshop_TestScene`。
2. 创建 `_MapRoot/LogicRoot` 与 `_MapRoot/ArtRoot`。
3. 在 `LogicRoot` 下创建基础地面、可移动区域、静止障碍、围墙。
4. 在 `ArtRoot` 下创建 2D 地图素材占位层。
5. 按坐标放置六大功能区逻辑区。
6. 放置工作站和 `InteractionPoint`。
7. 放置扩展锚点。
8. 创建 `Camera_2D_Oblique` 并确认整张地图可见。
9. 烘焙或准备 `NavMesh`。
10. 放置玩家出生点。
11. 放置测试 UI 占位。
12. 执行地图基础规则测试。

## 场景根结构

建议结构：

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

## 材质与分层规则

可移动区域：

- 根节点：`LogicRoot/WalkableRoot`
- 材质：`Mat_Walkable_Green`
- 建议颜色：`#DFF2BF`
- 用途：玩家移动、任务移动、搬运路径、`NavMesh` 地面
- 表现：隐藏或半透明 Debug 层，不作为最终玩家视觉

静止障碍：

- 根节点：`LogicRoot/StaticBlockerRoot`
- 材质：`Mat_Static_Gray`
- 建议颜色：`#B8B8B8`
- 必须有 `Collider`
- 必须阻挡 `NavMesh`
- 表现：隐藏或半透明 Debug 层，不作为最终玩家视觉

区域触发：

- 根节点：`LogicRoot/AreaTriggerRoot`
- 只表达进入了哪个功能区
- 不承担阻挡职责
- 可与可移动区域重叠
- 使用 3D Trigger，不使用 2D Collider

地图视觉：

- 根节点：`ArtRoot`
- `Map_Background_2D`：整张或分块国风地图底图
- `Map_Buildings_2D`：建筑、窑炉、树、摊位等视觉素材
- `Map_Foreground_2D`：屋檐、树冠等前景遮挡
- `Map_Overlay_2D`：区域高亮、交互提示、Debug 覆盖层
- Phase6 可先使用占位图或纯色 Sprite，后续替换国风美术素材

工作站：

```text
WheelStation
├─ LogicRoot
│  ├─ Collider
│  ├─ InteractionPoint
│  └─ Workstation
└─ ArtRoot
```

## 可移动区域坐标

以下对象全部放入 `LogicRoot/WalkableRoot`，并使用 `Mat_Walkable_Green`。

| ID | 名称 | 中心点 X,Z | 尺寸 W,D | 说明 |
| --- | --- | ---: | ---: | --- |
| `OrderArea_Walkable` | 订单区可移动区 | 11, 14.5 | 20 x 26 | 接单、查看目标、流程入口 |
| `WheelArea_Walkable` | 拉坯区可移动区 | 11, 45.5 | 20 x 26 | 拉坯台周围移动与交互 |
| `GlazeArea_Walkable` | 配釉区可移动区 | 39, 48 | 24 x 22 | 配釉工作台和釉料交互 |
| `StorageArea_Walkable` | 仓储区可移动区 | 39, 10 | 24 x 18 | 物料、库存扩展 |
| `KilnArea_Walkable` | 烧窑区可移动区 | 67, 45.5 | 24 x 26 | 窑炉前操作、开窑交互 |
| `MaterialArea_Walkable` | 材料区可移动区 | 67, 14.5 | 24 x 26 | 原料堆、配方材料来源 |
| `MainRoad_Walkable` | 主通道 | 24, 30 | 5.5 x 60 | 南北向主通道 |
| `SecondaryRoad_Walkable` | 次通道 | 40, 30 | 80 x 4.5 | 东西向连接通路 |
| `KilnBranch_Walkable` | 窑区支路 | 53.5, 28 | 4 x 18 | 连接窑区、仓储、材料 |

## 静止障碍坐标

以下对象全部放入 `LogicRoot/StaticBlockerRoot`，并使用 `Mat_Static_Gray`。

| ID | 名称 | 中心点 X,Z | 尺寸 W,D | 高度 | 说明 |
| --- | --- | ---: | ---: | ---: | --- |
| `Warehouse_Blocker` | 仓库建筑 | 39, 29 | 13 x 12 | 3 | 后续可接内景 |
| `Workshop_Blocker` | 工坊建筑 | 53, 29 | 13 x 12 | 3.5 | 后续可接内景 |
| `Kiln_Blocker` | 窑炉 | 69, 50 | 14 x 12 | 4 | 烧窑核心设备 |
| `WheelObstacle_01` | 拉坯区设备障碍 | 10, 38 | 9 x 4 | 1 | 设备或石堆占位 |
| `StorageObstacle_01` | 仓储障碍 | 47, 9 | 5 x 9 | 1.5 | 货架或货箱 |
| `MaterialPile_01` | 材料堆 | 59, 11 | 6 x 7 | 1.2 | 原料占位 |
| `MaterialObstacle_02` | 材料区障碍 | 74, 8 | 6 x 5 | 1.2 | 石堆或陶土堆 |

## 围墙坐标

围墙放入 `LogicRoot/WallRoot`，用于明确边界和约束 `NavMesh`。

| ID | 中心点 X,Z | 尺寸 W,D | 高度 |
| --- | ---: | ---: | ---: |
| `Wall_North` | 40, 60 | 80 x 0.3 | 2 |
| `Wall_South` | 40, 0 | 80 x 0.3 | 2 |
| `Wall_East` | 80, 30 | 0.3 x 60 | 2 |
| `Wall_West` | 0, 30 | 0.3 x 60 | 2 |

## 交互点坐标

| 交互点 | 建议位置 X,Z | 半径 | 说明 |
| --- | ---: | ---: | --- |
| `Interaction_OrderBoard` | 11, 12 | 2 | 接单 |
| `Interaction_WheelStation` | 11, 43 | 2 | 拉坯 |
| `Interaction_GlazeStation` | 39, 46 | 2 | 配釉 |
| `Interaction_Kiln` | 69, 43 | 2.5 | 烧窑 |
| `Interaction_Storage` | 39, 12 | 2 | 仓储 |
| `Interaction_Material` | 67, 12 | 2 | 材料 |

## 扩展锚点

Phase6 只放空 GameObject，不连接实际内容。

| ID | 位置 | 用途 |
| --- | ---: | --- |
| `Expansion_NorthGate` | 40, 60 | 后续连接展陈区或御窑遗址区 |
| `Expansion_EastGate` | 80, 30 | 后续连接市场、外部街区或运输区 |
| `Expansion_SouthGate` | 40, 0 | 后续连接玩家住所、教程入口或剧情区 |
| `Expansion_WestGate` | 0, 30 | 后续连接 NPC 工作区或备用生产区 |

## NavMesh 规则

- 只在绿色可移动区域上烘焙 `NavMesh`
- `Ground_Base` 不参与 `NavMesh`
- 灰色障碍必须阻挡 `NavMesh`
- 围墙必须阻挡 `NavMesh`
- `AreaTrigger` 不影响 `NavMesh`
- `RouteDebugRoot` 不影响 `NavMesh`

## 必测路线

| 路径 | 期望 |
| --- | --- |
| 订单区 -> 拉坯区 | 可到达 |
| 拉坯区 -> 配釉区 | 可到达 |
| 配釉区 -> 烧窑区 | 可到达 |
| 烧窑区 -> 仓储区 | 可到达 |
| 仓储区 -> 材料区 | 可到达 |
| 材料区 -> 订单区 | 可到达 |
| 点击灰色障碍 | 不可进入 |
| 点击围墙外 | 不可进入 |

## 验收

- 场景可运行
- 正交斜俯视相机能完整看到 `80m x 60m` 地图
- `_MapRoot` 下存在 `LogicRoot` 与 `ArtRoot`
- 玩家最终视觉来自 `ArtRoot` 的 2D 地图素材层
- 玩家能移动
- 六大区域可辨认
- 玩家不会离开地图
- 所有可移动逻辑区统一使用绿色 Debug 材质
- 所有静止障碍逻辑区统一使用灰色 Debug 材质
- 所有障碍有 Collider
- 所有核心交互点位于可移动区域内
- 所有核心区域之间 `NavMesh` 连通

## 依赖

- 无

## 交付物

- `Workshop_TestScene`
- 2.5D 国风地图结构
- 基础场景树
- 2D 斜俯视正交相机
- `LogicRoot` 隐藏逻辑层
- `ArtRoot` 2D 地图素材层
- 六大区域可移动逻辑块
- 静止障碍与围墙逻辑块
- 六个核心交互点
- 四个扩展锚点
- `Phase6MapStructureMigrator` 场景结构迁移工具
- `STATE.md` / `DECISIONS.md` 已更新
