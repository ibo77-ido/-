# Phase6 核心规范设计稿

> 设计来源
> - [景德镇御窑厂MVP开发实施规范.docx](/D:/unity/UnityProject/Director/Project/Task/Phase6/景德镇御窑厂MVP开发实施规范.docx)
> - [继承瓷厂_Phase6_V2.1_开发与白盒测试规范_完整版.docx](/D:/unity/UnityProject/Director/Project/Task/Phase6/继承瓷厂_Phase6_V2.1_开发与白盒测试规范_完整版.docx)

## 1. 设计目标

Phase6 的目标不是做“完整成品”，而是验证一套可持续扩展的地图空间化基础架构，确保后续 10 个阶段不需要推倒重来。

核心关注点如下：

- 先把地图、移动、交互、状态机和比例体系打通
- 所有升级逻辑、业务内容和外观变化都走 `SO` 或配置驱动
- 逻辑尺寸与视觉尺寸分离，避免 Prefab 原始大小反向污染系统设计
- 通过白盒测试验证路径、区域、交互入口、状态切换和视觉刷新

## 2. 核心规范

### 2.1 架构原则

- 先预留，后扩展
- 禁止开发 AI 生产逻辑
- 所有升级系统和内容变化必须由 `SO` 或配置表驱动
- 逻辑层只负责规则、状态、触发和数据读写
- 表现层只负责模型、外观和刷新

### 2.2 统一比例原则

- Unity 基准：`1 Unit = 1 米`
- 逻辑地图按真实尺度组织
- 视觉模型和交互碰撞体不直接依赖原始 Prefab 尺寸
- 以 `AssetScaleConfigSO` 统一管理缩放

### 2.3 扩展边界原则

- Phase6 只做地图空间化与行走/交互底座
- 不提前开发复杂生产逻辑
- 不在本阶段把系统写死在场景里
- 所有可替换内容都要能在 Inspector 或配置层修改

## 3. 架构设计

### 3.1 Character 架构

角色采用“配置 + 控制器 + 状态机”结构。

```text
Character
├─ CharacterConfigSO
├─ MovementController
├─ InteractionController
├─ CharacterStateMachine
└─ NavMeshAgent
```

状态集合：

- `Idle`
- `Moving`
- `Interacting`
- `Working`
- `Waiting`
- `Resting`
- `Disabled`

设计要点：

- 移动、交互、工作状态互斥控制
- 状态机统一收口，避免多个系统直接改角色行为
- 后续 NPC、工人或特殊角色复用同一套结构

### 3.2 Workstation 架构

工作站采用“配置 + 视觉刷新 + 交互点 + 预留生产逻辑”的结构。

```text
Workstation
├─ WorkstationConfigSO
├─ WorkstationVisualController
├─ InteractionPoint
└─ ProductionLogic (预留)
```

设计要点：

- `RefreshVisual()` 负责刷新外观，不承担规则计算
- `InteractionPoint` 只负责交互入口与范围判断
- `ProductionLogic` 先预留，Phase6 不展开复杂生产

### 3.3 Area 架构

区域采用“配置 + 视觉控制 + 区域触发器 + 工作站集合”的结构。

```text
Area
├─ AreaConfigSO
├─ AreaVisualController
├─ AreaTrigger
└─ Workstations
```

设计要点：

- 区域支持整体替换墙体、地面、设备和装饰
- 区域切换不能依赖硬编码
- 后续扩区、换皮、功能拆分都应只改配置

## 4. 地图与空间设计

### 4.1 地图范围

- 地图建议尺寸：`80m × 60m`
- 主路：`4m`
- 次路：`2.5m`
- 小路：`1.5m`
- 建筑间距：`≥ 3m`
- 建筑与道路间距：`≥ 2m`

### 4.2 功能分区

Phase6 的地图建议按功能分区组织：

| 区域 | 职责 |
|---|---|
| 订单区 | 接单、查看目标、流程入口 |
| 拉坯区 | 形体制作与移动交互 |
| 配釉区 | 配方操作与材料交互 |
| 烧窑区 | 温度、火候、开窑相关交互 |
| 仓库区 | 材料、物料和补给 |
| NPC 区域 | 预留互动与后续扩展 |

### 4.3 设计表达方式

- Figma 用于整体空间规划
- Unity 先用 `Plane`、`Cube` 做白盒布局
- 地图第一目标是“可走、可交互、可验证”，不是美术完成度

## 5. 比例与缩放系统

### 5.1 Character-Centric Scale System

以玩家高度作为比例基准：

- 玩家：`1.0`
- NPC：`0.95 ~ 1.05`
- 门：`1.3`
- 拉坯台：`0.8`
- 配釉台：`0.9`
- 窑炉：`2.0`
- 仓库：`3.0`
- 工坊：`3.5`
- 树木：`3 ~ 5`

### 5.2 逻辑与视觉分离

所有对象统一分为两个根节点：

- `LogicRoot`：导航、碰撞、交互
- `ArtRoot`：视觉表现

原则：

- 逻辑尺寸先稳定，视觉尺寸后适配
- 不允许直接拿 Prefab 原始尺寸去推导系统逻辑
- 缩放统一由 `AssetScaleConfigSO` 控制

### 5.3 AssetScaleConfigSO

建议字段：

- `CharacterBaseScale`
- `DoorScale`
- `WorkstationScale`
- `KilnScale`
- `BuildingScale`
- `WarehouseScale`
- `TreeScale`

用途：

- 统一管理导入资源缩放
- 保持不同资产之间的视觉一致性
- 支持后续整体换皮而不改逻辑

## 6. Prefab 规范

### 6.1 标准结构

```text
Workshop
├─ LogicRoot
├─ Collider
├─ Trigger
└─ ArtRoot

Kiln
├─ LogicRoot
├─ InteractionPoint
└─ ArtRoot
```

### 6.2 强制约束

- 所有视觉资源挂载在 `ArtRoot`
- 所有碰撞、导航和交互挂载在 `LogicRoot` 或其子节点
- 不允许把逻辑脚本直接绑在视觉模型根节点上
- 不允许用 Prefab 尺寸替代比例规范

## 7. 数据与配置设计

Phase6 的所有内容必须数据化，至少覆盖以下配置对象：

| 配置对象 | 作用 |
|---|---|
| `ShapeRecipeSO` | 器型目标参数集合 |
| `GlazeRecipeSO` | 釉料目标参数集合 |
| `OrderSO` | 订单目标、奖励和引用关系 |
| `CharacterConfigSO` | 角色移动、交互与状态参数 |
| `WorkstationConfigSO` | 工作站表现和交互参数 |
| `AreaConfigSO` | 区域范围、视觉和工作站集合 |
| `AssetScaleConfigSO` | 统一缩放标准 |

设计原则：

- 新增内容尽量不改代码
- 所有新内容优先新增配置项
- 数值计算与内容定义解耦

## 8. Phase6 实施顺序

建议按以下顺序推进：

1. 搭建测试场景与基础地图白盒
2. 落地 `SO` 和配置结构
3. 实现角色移动与摄像机跟随
4. 实现交互入口与 UI 提示
5. 完成区域触发和工作站基础结构
6. 接入统一缩放系统
7. 做白盒测试与回归验证

这个顺序的目的，是先把“可走、可交互、可验证”建立起来，再逐步补齐表现层。

## 9. 白盒测试设计

### 9.1 测试目标

验证以下内容：

- 地图比例是否合理
- 移动是否稳定
- 区域触发是否准确
- 交互入口是否可用
- 状态机切换是否正确
- 视觉刷新是否正常
- 缩放系统是否统一生效

### 9.2 测试场景

- `Workshop_TestScene`

### 9.3 测试区域

- `OrderArea`
- `WheelArea`
- `GlazeArea`
- `KilnArea`
- `StorageArea`
- `MaterialArea`

### 9.4 核心用例

| 编号 | 用例 | 期望结果 |
|---|---|---|
| 1 | 连续点击移动 100 次 | 无卡死、无路径异常 |
| 2 | 点击障碍物 | 角色不能穿模 |
| 3 | 点击地图边缘 | 行走边界正确 |
| 4 | `AreaTrigger` 进入/离开 100 次 | 触发稳定无抖动 |
| 5 | `InteractionPoint` 触发 | 可正常进入交互 |
| 6 | `RefreshVisual` 切换 Prefab | 外观刷新正确 |
| 7 | `AssetScaleConfigSO` 动态缩放 | 缩放结果统一生效 |
| 8 | 状态机切换 | 状态切换无冲突 |
| 9 | NavMesh 烘焙 | 可正常寻路 |
| 10 | 地图比例验证 | 地图满足空间规范 |

### 9.5 报告模板

```text
PHASE 6 TEST REPORT
Scale Test
Movement Test
Area Trigger Test
Interaction Test
Visual Test
StateMachine Test
Map Rule Test
Final Result
```

## 10. Phase7 准入条件

Phase6 只有满足以下条件，才能进入 Phase7：

- 无控制台错误
- 所有白盒测试通过
- `RefreshVisual()` 正常
- `SO` 驱动正常
- 能完整走通 `订单区 → 拉坯区 → 配釉区 → 烧窑区 → 订单区`

任意一项核心测试失败，都视为 Phase6 未完成。

## 11. 本阶段交付物

- 地图白盒场景
- 角色移动系统
- 交互系统
- 区域触发系统
- 统一缩放配置
- Prefab 结构规范
- 白盒测试报告
- Phase7 准入结论

## 12. 结论

Phase6 的设计重点不是“做得多”，而是“做得稳”。

只要本阶段把空间结构、角色运动、交互入口、比例系统和测试门槛全部标准化，后续 UI、资源、美术、生产和结果系统都可以在不推翻底层的前提下继续扩展。
