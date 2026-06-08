# Phase6 脚本与场景细化说明（MVP版）

> 说明
> - 这份细化版严格按 [PHASE6_MVP_REFINED.md](/D:/unity/UnityProject/Director/Project/Task/Phase6/PHASE6_MVP_REFINED.md) 收敛
> - 只保留当前 Phase6 真正需要的脚本、组件和场景对象

## 1. 复用原则

Phase6 优先复用 Phase3 的数据驱动思路，但不照搬 Phase3 的 UI 状态切换框架。

复用点：

- `OrderData` 的配置化习惯
- `GameConfigSO` 的全局参数思路
- `SO` 驱动而非硬编码
- Editor 工具一键补全的工作流

不保留的过度设计：

- `Phase6SceneBootstrapper`
- `Phase6SceneValidator`
- `Waiting / Resting / Disabled` 等暂时无用状态
- 复杂区域视觉控制

最终冻结边界：
- 不再扩展新的大模块
- 只做 MVP 必需脚本
- 只做可验证闭环

---

## 2. 必要脚本清单

### 2.1 数据脚本

#### `CharacterConfigSO`

职责：
- 保存角色移动、交互和默认状态参数

字段建议：
- `moveSpeed`
- `rotationSpeed`
- `interactionDistance`
- `interactionCooldown`

#### `WorkstationConfigSO`

职责：
- 保存工作台的交互范围、提示文本、默认外观和区域绑定信息

字段建议：
- `workstationType`
- `interactionRange`
- `promptText`
- `defaultPrefab`
- `areaType`

#### `AreaConfigSO`

职责：
- 保存区域类型、区域名和基础范围

字段建议：
- `areaType`
- `areaName`
- `boundsSize`
- `boundsCenter`

#### `AssetScaleConfigSO`

职责：
- 保存 Phase6 最小可用缩放配置

字段建议：
- `CharacterScale`
- `WorkstationScale`
- `BuildingScale`

---

### 2.2 角色脚本

#### `PlayerCharacter`

职责：
- 作为角色根控制器，聚合移动、交互和状态机

要求：
- 不写业务规则
- 不直接操作 UI

#### `MovementController`

职责：
- 接收移动目标
- 调用 `NavMeshAgent`
- 控制到达后的状态切换

#### `InteractionController`

职责：
- 处理交互输入
- 查找目标
- 判断目标是否可交互
- 调用 `IInteractable.Interact()`

#### `CharacterStateMachine`

职责：
- 统一管理角色状态切换

建议状态：
- `Idle`
- `Moving`
- `Interacting`
- `Working`

---

### 2.3 交互与工作站脚本

#### `IInteractable`

职责：
- 定义统一交互接口

接口建议：
```csharp
public interface IInteractable
{
    void Interact();
}
```

说明：
- `InteractionController` 只负责找目标、判定、调用
- `Workstation`、`OrderBoard`、`StorageBox`、`NPC` 等对象可实现该接口

#### `InteractionPoint`

职责：
- 定义交互入口
- 处理进入、离开和交互触发

#### `Workstation`

职责：
- 作为标准工作站实体
- 承载配置、视觉和交互点

标准结构：

```text
Workstation
├ WorkstationConfigSO
├ InteractionPoint
├ WorkstationVisualController
└ ProductionLogic（预留）
```

#### `WorkstationVisualController`

职责：
- 刷新外观
- 应用统一缩放

建议接口：
- `RefreshVisual()`
- `ApplyScale()`

#### `AreaTrigger`

职责：
- 监听角色进入和离开区域

#### `AreaConfigSO`

职责：
- 提供区域基础参数

---

### 2.4 系统脚本

#### `GameManager`

职责：
- 全局游戏状态
- 流程切换
- 系统初始化
- 阶段入口统一管理

#### `AreaManager`

职责：
- 管理场景中的所有区域
- 提供区域查询和统一切换入口

#### `ScaleManager`

职责：
- 统一应用 `AssetScaleConfigSO`
- 对角色、工作站和建筑进行缩放

#### `InputManager`

职责：
- 统一接收玩家输入
- 将输入分发给移动、交互或流程控制层

---

## 3. 场景层级结构

### 3.1 场景根结构

建议 `Workshop_TestScene` 结构如下：

```text
SceneRoot
├ Environment
│  ├ Ground
│  ├ Boundary
│  ├ Lighting
│  └ NavMesh
├ Areas
│  ├ OrderArea
│  ├ WheelArea
│  ├ GlazeArea
│  ├ KilnArea
│  ├ StorageArea
│  └ MaterialArea
├ Characters
│  └ PlayerCharacter
├ Workstations
│  ├ WheelStation
│  ├ GlazeStation
│  └ KilnStation
├ UI
│  └ PromptCanvas
└ Systems
   ├ GameManager
   ├ AreaManager
   ├ ScaleManager
   └ InputManager
```

### 3.2 对象挂载建议

#### PlayerCharacter

建议组件：
- `PlayerCharacter`
- `MovementController`
- `InteractionController`
- `CharacterStateMachine`
- `NavMeshAgent`

建议子节点：
- `LogicRoot`
- `ArtRoot`

#### Area 对象

建议组件：
- `AreaTrigger`
- `AreaConfigSO` 引用

#### Workstation 对象

建议组件：
- `Workstation`
- `WorkstationVisualController`
- `InteractionPoint`

建议子节点：
- `LogicRoot`
- `ArtRoot`

---

## 4. 目录建议

建议在 `Assets` 下新增：

```text
Assets
└ Phase6
   ├ Scenes
   ├ Scripts
   │  ├ Core
   │  ├ Data
   │  ├ Characters
   │  ├ Interactions
   │  ├ Areas
   │  ├ Workstations
   │  └ Editor
   ├ Prefabs
   │  ├ Characters
   │  ├ Areas
   │  ├ Workstations
   │  └ UI
   └ Data
```

原则：

- 脚本、场景、Prefab、数据分目录
- 测试资源和正式资源分开
- 目录结构和职责一一对应

---

## 5. 场景初始化方式

建议：

- 手工搭建 `Workshop_TestScene`
- 场景中保持六大区域可见
- 提前放好玩家、工作站和提示 UI 占位

不建议：

- 为 Phase6 单独开发复杂的场景自动生成器
- 为 Phase6 单独开发自动结构校验器

原因：

- 当前规模较小，人工搭建和人工检查更直接

---

## 6. 具体开发顺序

1. 地图白盒
2. `CharacterConfigSO`
3. `PlayerCharacter`
4. `MovementController`
5. `CharacterStateMachine`
6. `IInteractable`
7. `InteractionPoint`
8. `InteractionController`
9. `Workstation`
10. `WorkstationVisualController`
11. `AreaTrigger`
12. `GameManager`
13. `AreaManager`
14. `ScaleManager`
15. `InputManager`
16. `AssetScaleConfigSO`
17. 白盒测试

---

## 7. 这个版本的目标

这个细化版只服务 MVP，不追求未来阶段的一次性铺平。

它的目标是：

- 让你可以直接开始建场景
- 让程序可以直接开始写脚本
- 让测试可以直接执行 PASS / FAIL
- 让后续 Phase7 能无缝接上
