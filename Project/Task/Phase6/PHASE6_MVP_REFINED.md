# Phase6 架构审查与优化建议（MVP版）

## 审查结论

当前 Phase6 设计已经达到进入开发阶段的标准。

设计质量：`A-`

允许进入开发阶段。

但当前方案存在一定过度设计问题，Phase6 应优先服务 20 天游赛 MVP 目标。

Phase6 的真正目标是：

- 能走
- 能交互
- 能验证
- 能扩展

而不是提前建设未来多个阶段才会用到的架构。

---

## 一、必须保留的设计

### 1. SO 驱动架构

必须保留：
- `CharacterConfigSO`
- `WorkstationConfigSO`
- `AreaConfigSO`
- `AssetScaleConfigSO`

原因：
- 新增内容应通过配置扩展，而不是修改代码

### 2. LogicRoot / ArtRoot 分层

必须保留：

```text
Object
├ LogicRoot
└ ArtRoot
```

原因：
- 后续模型替换、美术升级、动画接入、特效接入都不会污染逻辑层

### 3. Workstation 架构

必须保留：

```text
Workstation
├ WorkstationConfigSO
├ InteractionPoint
├ WorkstationVisualController
└ ProductionLogic（预留）
```

原因：
- 后续拉坯台、配釉台、烧窑台都可复用同一套结构

### 4. AreaTrigger

必须保留：

```text
AreaTrigger
```

原因：
- 地图空间化玩法必须依赖区域检测

### 5. GameManager

必须保留：

```text
GameManager
```

建议职责：
- 全局游戏状态
- 流程切换
- 系统初始化
- 阶段入口统一管理

原因：
- Phase7 及后续阶段仍然需要统一入口
- 提前建立流程控制层，后续扩展成本更低

### 6. RefreshVisual()

必须保留：

```text
RefreshVisual()
```

原因：
- 后续升级工作台时只需刷新外观，无需重建逻辑

---

## 二、建议精简的设计

### 1. CharacterStateMachine 精简

当前保留状态：
- `Idle`
- `Moving`
- `Interacting`
- `Working`

冻结不进入 Phase6：
- `Waiting`
- `Resting`
- `Disabled`

原因：
- 当前项目只有玩家角色，暂时没有 NPC、工人、员工、顾客等实体需求

### 2. AreaVisualController 暂缓

当前白盒阶段可先不做复杂区域视觉控制。

建议 Phase6 只保留：

```text
Area
├ AreaConfigSO
├ AreaTrigger
└ Workstations
```

待美术和风格替换阶段再加入 `AreaVisualController`。

### 3. AssetScaleConfigSO 精简

Phase6 建议先保留：

- `CharacterScale`
- `WorkstationScale`
- `BuildingScale`

后续资源增多时再扩展更多字段。

---

## 三、建议延期的系统

### 1. Phase6SceneBootstrapper

建议冻结，不进入 Phase6。

理由：
- 当前只是一张测试地图，手工搭建场景更直接

### 2. Phase6SceneValidator

建议冻结，不进入 Phase6。

理由：
- 当前项目规模较小，人工检查效率更高

---

## 四、推荐任务结构

最终保留七个主任务即可。

### P6-01 地图白盒
- `Workshop_TestScene`
- 六大区域
- 地面
- 道路
- `NavMesh`

### P6-02 Character 系统
- `CharacterConfigSO`
- `MovementController`
- `CharacterStateMachine`

### P6-03 Interaction 系统
- `InteractionPoint`
- `InteractionController`

### P6-04 Workstation 系统
- `Workstation`
- `WorkstationConfigSO`
- `RefreshVisual()`

### P6-05 Area 系统
- `AreaConfigSO`
- `AreaTrigger`

### P6-06 Scale 系统
- `LogicRoot`
- `ArtRoot`
- `AssetScaleConfigSO`

### P6-07 WhiteBoxTest
- 移动测试
- 区域测试
- 交互测试
- `RefreshVisual` 测试
- 状态机测试
- `NavMesh` 测试

---

## 五、最终脚本规模控制

Phase6 建议控制在一组精干的核心脚本内，不再扩展新的大模块。

推荐核心脚本：

- `CharacterConfigSO`
- `WorkstationConfigSO`
- `AreaConfigSO`
- `AssetScaleConfigSO`
- `PlayerCharacter`
- `MovementController`
- `InteractionController`
- `CharacterStateMachine`
- `IInteractable`
- `InteractionPoint`
- `Workstation`
- `WorkstationVisualController`
- `AreaTrigger`
- `AreaManager`
- `GameManager`
- `ScaleManager`
- `InputManager`

原则：
- 先完成闭环
- 再扩展功能
- 不为未来阶段提前堆空架构

---

## 六、Phase6 完成定义（DoD）

仅当以下全部满足时允许进入 Phase7：

```text
✓ 无 Console Error
✓ 玩家可移动
✓ Camera Follow 正常
✓ 区域检测正常
✓ E 键交互正常
✓ Workstation 结构完成
✓ RefreshVisual 正常
✓ AssetScaleConfigSO 正常
✓ SO 驱动正常
✓ 完整空间化流程可运行
```

满足以上条件后：

```text
Phase6 = PASS
允许进入 Phase7
```
