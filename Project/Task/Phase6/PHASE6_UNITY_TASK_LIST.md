# Phase6 MVP 最终任务清单

> 依据
> - [PHASE6_MVP_REFINED.md](/D:/unity/UnityProject/Director/Project/Task/Phase6/PHASE6_MVP_REFINED.md)
> - [PHASE6_SCRIPT_AND_SCENE_DETAIL.md](/D:/unity/UnityProject/Director/Project/Task/Phase6/PHASE6_SCRIPT_AND_SCENE_DETAIL.md)
> - [景德镇御窑厂MVP开发实施规范.docx](/D:/unity/UnityProject/Director/Project/Task/Phase6/景德镇御窑厂MVP开发实施规范.docx)
> - [继承瓷厂_Phase6_V2.1_开发与白盒测试规范_完整版.docx](/D:/unity/UnityProject/Director/Project/Task/Phase6/继承瓷厂_Phase6_V2.1_开发与白盒测试规范_完整版.docx)

## 0. 任务原则

Phase6 的目标是验证空间化玩法闭环，不追求过度架构。

必须达成的能力只有四类：

- 能走
- 能交互
- 能验证
- 能扩展

---

# P6-01 地图白盒

内容：
- `Workshop_TestScene`
- 六大区域
- 地面
- 道路
- `NavMesh`

验收：
- 玩家可自由移动
- 地图边界清楚
- 六大区域可辨认

---

# P6-02 Character 系统

内容：
- `CharacterConfigSO`
- `PlayerCharacter`
- `MovementController`
- `CharacterStateMachine`
- `GameManager`
- `InputManager`

建议状态：
- `Idle`
- `Moving`
- `Interacting`
- `Working`

验收：
- 玩家移动正常
- 状态切换正常
- 不需要 NPC 也能闭环

---

# P6-03 Interaction 系统

内容：
- `IInteractable`
- `InteractionPoint`
- `InteractionController`

验收：
- 玩家可交互
- 进入/离开范围反馈正常
- E 键触发稳定

---

# P6-04 Workstation 系统

内容：
- `Workstation`
- `WorkstationConfigSO`
- `WorkstationVisualController`
- `RefreshVisual()`

验收：
- 工作台可配置
- 外观可刷新
- 逻辑和表现分离

---

# P6-05 Area 系统

内容：
- `AreaConfigSO`
- `AreaTrigger`
- `AreaManager`

验收：
- 区域检测正常
- 进出区域不抖动
- 区域参数可配置

---

# P6-06 Scale 系统

内容：
- `LogicRoot`
- `ArtRoot`
- `AssetScaleConfigSO`
- `ScaleManager`

建议字段：
- `CharacterScale`
- `WorkstationScale`
- `BuildingScale`

验收：
- 缩放统一管理
- 原始 Prefab 尺寸不污染逻辑

---

# P6-07 WhiteBoxTest

内容：
- 移动测试
- 区域测试
- 交互测试
- `RefreshVisual` 测试
- 状态机测试
- `NavMesh` 测试

验收：
- 全部 PASS
- 无 Console Error
- 可完整走通：
  `订单区 → 拉坯区 → 配釉区 → 烧窑区 → 返回订单区`

---

# P6 完成定义

仅当以下全部满足时，允许进入 Phase7：

- 无 Console Error
- 玩家可移动
- `Camera Follow` 正常
- 区域检测正常
- `E` 键交互正常
- `Workstation` 结构完成
- `RefreshVisual()` 正常
- `AssetScaleConfigSO` 正常
- `SO` 驱动正常
- 完整空间化流程可运行

---

# 推荐顺序

1. P6-01
2. P6-02
3. P6-03
4. P6-04
5. P6-05
6. P6-06
7. P6-07
