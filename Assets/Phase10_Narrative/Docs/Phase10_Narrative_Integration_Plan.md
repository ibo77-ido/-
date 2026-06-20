# Phase10 Narrative Layer - Chapter 1 Overlay Scene Plan

## 1. Task Goal

基于当前 Unity 项目新增一个独立的剧情层任务，用于实现第一章《重燃窑火》的故事主线推进。

本任务的核心不是先堆剧情文本，而是先建立可复用的 **Narrative State Machine**、**Narrative Event Bus** 与预留的 **Narrative Command Channel**，让第一章、第二章、第三章都能使用同一套叙事推进结构。

本任务遵守以下边界：

- 不修改现有 Phase3 / Phase6 / Phase8 的脚本、场景、Prefab、ScriptableObject 和核心项目文件。
- 允许新增剧情层文件、剧情 Overlay 场景、剧情 Prefab、剧情 ScriptableObject、剧情 UI、Narrative State Machine、Narrative Event Bus、Narrative Command Bus 和桥接预留接口。
- 当前阶段只实现独立剧情层和剧情场景；后续再通过新增桥接层与现有项目连接。
- 命名统一使用 `Phase10_Narrative` 和 `P10_` 前缀。
- 剧情表现对象只能添加在 Phase10 独立剧情场景中。
- 生产玩法物品不得放入剧情层，仍然归 Phase3 Data / Gameplay Data 所有。

## 2. Current Project Assumption

当前项目已完成：

- Phase3 核心玩法：订单、器型、釉料、烧窑、结果结算。
- Phase6 场景布局：玩家移动、工作台、交互点、基础世界空间。
- Phase8 桥接层：Phase6 世界层与 Phase3 玩法层之间的 Gameplay Bridge、Session、RuntimeMode、UI Host 管理。
- 白盒/灰盒模块已绑定 UI，玩家可以游玩并看到相关表现。

Phase10 不替代 Phase8 Bridge。Phase10 是叙事内容层，不拥有 RuntimeMode，不控制 Session，不控制 Phase3 玩法推进，不拥有玩法物品数据。

## 3. Recommended Directory Structure

新增目录：

```text
Assets/Phase10_Narrative/
  Docs/
    P10_CH01_NarrativePlan.md
    P10_NamingContract.md
    P10_BridgeContract.md
    P10_PropsContract.md
    P10_StateMachineContract.md

  Scenes/
    P10_CH01_NarrativeOverlay.unity

  Scripts/
    Data/
    State/
    Events/
    Commands/
    Props/
    Runtime/
    UI/
    BridgePreview/

  ScriptableObjects/
    Characters/
    Dialogues/
    Chapters/
    Props/

  Prefabs/
    NPC/
    Props/
    Triggers/
    UI/
    Placeholders/
```

禁止本任务修改：

```text
Assets/Phase3/**
Assets/Phase6/**
Assets/Phase8/**
Assets/Scenes/**
ProjectSettings/**
```

## 4. Naming Contract

所有 Phase10 剧情层对象统一使用 `P10_` 或 `P10_CH01_` 前缀。

### Scene

```text
P10_CH01_NarrativeOverlay.unity
```

### Root Objects

```text
P10_CH01_NarrativeRoot
P10_CH01_NarrativeManager
P10_CH01_DialogueUIRoot
P10_CH01_TriggerRoot
P10_CH01_NPCRoot
P10_CH01_PropRoot
P10_CH01_AnchorRoot
P10_CH01_EventBusRoot
```

### NPC Placeholders

```text
P10_CH01_NPC_001_XuLaoBo_Placeholder
P10_CH01_NPC_002_ZhouZhangGui_Placeholder
P10_CH01_NPC_003_ChenShuYuan_Placeholder
P10_CH01_NPC_004_LuKe_Placeholder
```

### Narrative Props

Narrative Props 是纯叙事物件，只承担故事表达，不参与玩法计算，不作为 Phase3 生产资源。

```text
P10_PROP_001_FatherLedger
P10_PROP_002_OldKilnTool
P10_PROP_003_BrokenBowl
P10_PROP_004_AncientOrder
P10_PROP_005_FamilyLetter
```

### Narrative Nodes

```text
P10_CH01_NODE_PROLOGUE_01
P10_CH01_NODE_TUTORIAL_01
P10_CH01_NODE_ORDER_001_ACCEPT
P10_CH01_NODE_ORDER_001_PASS
P10_CH01_NODE_ORDER_001_FAIL
P10_CH01_NODE_ORDER_003_ACCEPT
P10_CH01_NODE_ORDER_003_PASS
P10_CH01_NODE_ORDER_003_FAIL
P10_CH01_NODE_ORDER_004_ACCEPT
P10_CH01_NODE_ORDER_004_PASS_NORMAL
P10_CH01_NODE_ORDER_004_FAIL
P10_CH01_NODE_ORDER_004_CLIMAX
P10_CH01_NODE_CHAPTER_ENDING
```

## 5. Narrative State Machine

Phase10 优先建立 Narrative State Machine，而不是直接堆叙事物件或对白文本。

第一章状态：

```csharp
public enum P10NarrativeState
{
    None,
    Prologue,
    Tutorial,
    Order001,
    Order003,
    Order004,
    Ending,
    Completed
}
```

第一章状态流：

```text
None
  -> Prologue
  -> Tutorial
  -> Order001
  -> Order003
  -> Order004
  -> Ending
  -> Completed
```

状态职责：

```text
Prologue   负责废窑清晨、徐老伯出场、周掌柜来访。
Tutorial   负责拉坯、施釉、烧窑、开窑教学的叙事引导。
Order001   负责 ORDER_001 接取、成功、失败、重试、奖励叙事。
Order003   负责 ORDER_003 接取、成功、失败、重试、陈书院出场。
Order004   负责 ORDER_004 接取、失败、普通完成、精品高潮。
Ending     负责第一章结尾、徐老伯认可、第二阶段伏笔。
Completed  代表第一章叙事完成。
```

建议新增脚本：

```text
Assets/Phase10_Narrative/Scripts/State/P10NarrativeState.cs
Assets/Phase10_Narrative/Scripts/State/P10NarrativeStateMachine.cs
```

状态机只管理叙事阶段，不直接操作 Phase3 玩法状态，不修改 Phase6 世界状态，不接管 Phase8 RuntimeMode。

## 6. Narrative Event Bus And Command Channel

Phase10 不应直接依赖 Phase3。正式桥接方向应分成两条通道：

```text
Gameplay Event Channel:
Phase3 / Phase6 / Phase8
        ↓
P10NarrativeEventBus
        ↓
P10NarrativeStateMachine
        ↓
P10NarrativeManager / Dialogue UI / Narrative Props

Narrative Command Channel:
P10NarrativeStateMachine / P10NarrativeManager
        ↓
P10NarrativeCommandBus
        ↓
Future Bridge Adapter
        ↓
Phase8 / Phase3 / Phase6
```

不是：

```text
Phase3 -> Phase10
Phase10 -> Phase3
```

Phase10 当前阶段只实现独立验证所需的 EventBus 和测试 Command 定义；正式让剧情暂停玩法、恢复玩法、锁定输入，必须等桥接阶段通过 Bridge Adapter 完成。

建议新增：

```text
Assets/Phase10_Narrative/Scripts/Events/P10NarrativeEventBus.cs
Assets/Phase10_Narrative/Scripts/Events/P10NarrativeEvent.cs
Assets/Phase10_Narrative/Scripts/Events/P10NarrativeEventType.cs
Assets/Phase10_Narrative/Scripts/Commands/P10NarrativeCommandBus.cs
Assets/Phase10_Narrative/Scripts/Commands/P10NarrativeCommand.cs
Assets/Phase10_Narrative/Scripts/Commands/P10NarrativeCommandType.cs
```

事件类型示例：

```csharp
public enum P10NarrativeEventType
{
    GameStarted,
    WorkstationInteracted,
    OrderStarted,
    OrderCompleted,
    OrderFailed,
    ResultShown,
    ScoreThresholdReached,
    ChapterCompleted
}
```

事件数据示例：

```csharp
public struct P10NarrativeEvent
{
    public P10NarrativeEventType Type;
    public string OrderId;
    public string AreaType;
    public float TotalScore;
    public string ResultGrade;
    public string SourceId;
}
```

命令类型预留：

```csharp
public enum P10NarrativeCommandType
{
    NarrativePauseGameplay,
    NarrativeResumeGameplay,
    NarrativeRequestInputLock,
    NarrativeReleaseInputLock,
    NarrativeRequestOpenDialogue,
    NarrativeFinishedBlockingSegment
}
```

命令数据示例：

```csharp
public struct P10NarrativeCommand
{
    public P10NarrativeCommandType Type;
    public string Reason;
    public string NodeId;
    public string StateId;
}
```

桥接示例：

```text
Phase3 发出：OrderCompleted(ORDER_001)
P10NarrativeEventBus 广播事件
P10NarrativeStateMachine 检查当前状态是否为 Order001
P10NarrativeManager 播放周掌柜完成剧情
状态推进到 Order003

Phase10 进入 Tutorial 阻塞对白
P10NarrativeCommandBus 发出 NarrativePauseGameplay
Future Bridge Adapter 接收命令
Bridge Adapter 决定是否请求 Phase8/Phase3 暂停或锁输入
对白结束后发出 NarrativeResumeGameplay
```

这样后续第二章、第三章、第四章可以复用同一套事件总线和命令通道，不需要每章直接引用玩法系统。

## 7. Narrative Props Contract

需要拆分“剧情物品”和“生产玩法物品”。

### 允许放在 Phase10 的 Narrative Props

这些物品只承担叙事表达，不参与生产、评分、订单、奖励、配方、烧窑计算。

```text
父亲的账本
旧窑工工具
残缺瓷片
旧订单
家书
旧窑炉标记
父亲留下的图纸碎片
```

推荐命名：

```text
P10_PROP_001_FatherLedger
P10_PROP_002_OldKilnTool
P10_PROP_003_BrokenBowl
P10_PROP_004_AncientOrder
P10_PROP_005_FamilyLetter
```

建议新增：

```text
Assets/Phase10_Narrative/Scripts/Props/P10NarrativePropDataSO.cs
Assets/Phase10_Narrative/Scripts/Props/P10NarrativePropView.cs
Assets/Phase10_Narrative/ScriptableObjects/Props/
Assets/Phase10_Narrative/Prefabs/Props/
```

### 禁止放在 Phase10 的 Gameplay Items

这些仍然属于 Phase3 Data / Gameplay Data。

```text
甜白釉
影青釉
祭红釉
瓷土
木柴
燃料
生产订单材料
实际配方材料
烧窑资源
器型配置
釉料配置
奖励数据
```

禁止出现：

```text
Phase10 剧情层定义一个甜白釉 ID
Phase3 玩法层也定义一个甜白釉 ID
剧情引用 P10 甜白釉，玩法引用 Phase3 甜白釉
```

这种双 ID 会导致剧情和玩法数据分裂，后期维护风险极高。

规则：

```text
P10_PROP_* 不得被 Phase3 Calculator / System / OrderData 引用。
Phase3 Gameplay Item ID 不得被 Phase10 当成 Narrative Prop ID 使用。
Phase10 可以通过文本提到玩法物品，但不能拥有玩法物品数据。
```

## 8. Independent Narrative Scene Design

`P10_CH01_NarrativeOverlay.unity` 是剧情专用 Overlay 场景，只放剧情表现对象：

```text
临时 NPC 方块
对白触发器
剧情锚点
剧情 UI
章节推进管理器
剧情调试按钮
Narrative Props
```

它不复制 Phase3 玩法系统，不修改 Phase6 主场景，也不直接改 Phase8 Bridge。

未来运行时结构应为：

```text
Workshop_TestScene
  + Phase3_Prototype additive
  + P10_CH01_NarrativeOverlay additive
```

当前任务只创建并验证 `P10_CH01_NarrativeOverlay` 自身。

## 9. Layout Based On Phase6

剧情场景应依据 Phase6 的世界布局摆放剧情对象：

```text
订单相关剧情 -> Order 工作台附近
拉坯教学 -> Wheel 工作台附近
施釉教学 -> Glaze 工作台附近
烧窑高潮 -> Kiln 工作台附近
章节结尾 -> 窑院中心或窑炉前
```

建议在剧情 Overlay 场景中新增剧情锚点：

```text
P10_CH01_Anchor_OrderStation
P10_CH01_Anchor_WheelStation
P10_CH01_Anchor_GlazeStation
P10_CH01_Anchor_KilnStation
P10_CH01_Anchor_CourtyardCenter
P10_CH01_Anchor_Gate
```

当前独立验证阶段，这些锚点只能作为近似占位，不能要求与 `Workshop_TestScene.unity` 中的 Workstation 精确对齐。原因是 `P10_CH01_NarrativeOverlay.unity` 是独立场景，它没有共享 Phase6 主场景的运行时坐标参照；在 Additive 加载之前，手动摆放的锚点并不天然等于 Phase6 Workstation 的世界坐标。

因此，当前阶段验收只验证：锚点命名存在、状态机能运行、剧情对象可按近似布局组织。不验证锚点与 Phase6 工作台的精确坐标重合。

后续桥接阶段必须新增：

```text
P10NarrativeAnchorMapper
```

`P10NarrativeAnchorMapper` 的职责：

```text
1. 运行时查找 Phase6 Workstation。
2. 读取每个 Workstation 的 AreaType 与 Transform.position。
3. 将 P10_CH01_Anchor_OrderStation / WheelStation / GlazeStation / KilnStation 映射或移动到对应 Workstation 位置。
4. 为无法匹配的锚点输出 Warning，不静默失败。
5. 不修改 Phase6 Workstation，只移动或绑定 Phase10 Anchor。
```

桥接阶段前，任何依赖精确空间位置的剧情触发都不能作为正式验收标准。

## 10. Story Mapping Based On Phase3 Gameplay

第一章《重燃窑火》主线对应 Phase3 核心玩法节点：

```text
PROLOGUE
  废窑清晨
  徐老伯出场
  周掌柜来访
  解锁 ORDER_001

TUTORIAL
  拉坯教学 -> ShapeScore
  施釉教学 -> GlazeScore
  烧窑教学 -> FireScore
  开窑 -> ResultCalculator

ORDER_001
  甜白釉茶碗
  通过 / 失败 / 重试
  完成后进入 ORDER_003

ORDER_003
  影青釉茶碗
  陈书院出场
  通过 / 失败 / 重试
  完成后进入 ORDER_004

ORDER_004
  祭红釉香筒
  卢客出场
  普通完成 / 精品高潮 / 失败重试

ENDING
  徐老伯认可
  第一章完成
```

注意：Phase10 可以根据 `OrderId` 触发剧情，但不拥有订单玩法数据。`ORDER_001`、`ORDER_003`、`ORDER_004` 的生产数据仍归 Phase3 所有。

## 11. Characters

根据 `StoryMain.md`，第一章需要以下角色。

`StoryMain.md` 当前来源路径：

```text
C:/Users/lenovo/Documents/游戏开发/项目整理_2026-06-08/正式文件/02_故事线/第一章/StoryMain.md
```

注意：该文件当前不在 Unity 项目目录内，因此 Phase10 首轮实现不应依赖运行时读取它。首轮可以使用占位对白，但必须在 Phase10 文档目录中新增 `P10_CH01_NarrativePlan.md`，记录故事结构、角色关系、状态流、订单剧情映射和 StoryMain 来源路径，作为后续补正式 DialogueNode 的依据。

```text
NPC_001 徐老伯：老窑工，新手教学、剧情推进、第一章认可者
NPC_002 周掌柜：茶馆老板，ORDER_001 / ORDER_003 客户原型
NPC_003 陈书院：书院先生，ORDER_003 客户
NPC_004 卢客：外地商人，ORDER_004 客户，第一章高潮触发角色
Player 年轻继承人：玩家视角，不需要固定台词，可用选项表达态度
```

当前阶段所有 NPC 可用方块或简单 Placeholder 表示，后续美术线再替换为正式角色 Prefab。

## 12. Suggested Data Types

只新增 Phase10 剧情数据类型，不接入旧代码。

```text
P10CharacterDataSO
  characterId
  displayName
  role
  placeholderColor
  portrait

P10DialogueNodeSO
  nodeId
  speakerId
  lines
  choices
  nextNodeId

P10ChapterFlowSO
  chapterId
  startState
  stateList
  nodeList

P10NarrativeCondition
  conditionType
  orderId
  minScore
  requiredState
  requiredNode

P10NarrativePropDataSO
  propId
  displayName
  description
  linkedNodeId
  inspectDialogueNodeId
```

建议数据 ID 对齐 StoryMain：

```text
NPC_001
NPC_002
NPC_003
NPC_004
ORDER_001
ORDER_003
ORDER_004
PROLOGUE
TUTORIAL
CHAPTER_ENDING
```

Narrative Props 使用独立 ID：

```text
P10_PROP_001_FatherLedger
P10_PROP_002_OldKilnTool
P10_PROP_003_BrokenBowl
P10_PROP_004_AncientOrder
P10_PROP_005_FamilyLetter
```

## 13. Suggested Runtime Components

```text
P10NarrativeStateMachine
  管理 Prologue / Tutorial / Order001 / Order003 / Order004 / Ending / Completed。

P10NarrativeEventBus
  统一接收并广播叙事事件，避免 Phase3 直接依赖 Phase10。

P10NarrativeCommandBus
  统一发出剧情层命令，桥接阶段由 Adapter 决定是否请求 Gameplay 暂停、恢复或锁输入。

P10NarrativeManager
  管理当前章节、当前节点、已播放节点、剧情推进。

P10DialogueController
  控制对白 UI，显示角色名、对白文本、选项按钮。

P10NarrativeTrigger
  场景触发器，用于触发指定剧情节点。

P10NarrativePropView
  控制 Narrative Props 的显示、检查、剧情节点触发。

P10NarrativeState
  记录当前剧情状态、订单剧情进度、是否已播放关键节点。

P10NarrativeDebugPanel
  独立调试面板，用于跳转剧情节点，不依赖 Phase3/Phase6/Phase8。
```

当前阶段这些组件只在 `P10_CH01_NarrativeOverlay.unity` 内运行。

## 14. Bridge Preview Contract

当前任务只预留桥接接口，不真正修改旧项目。

新增目录：

```text
Assets/Phase10_Narrative/Scripts/BridgePreview/
```

建议预留：

```text
P10NarrativeBridgePort.cs
P10NarrativeGameplayEventAdapter.cs
P10NarrativeCommandAdapter.cs
P10NarrativeAnchorMapper.cs
```

事件统一通过 `P10NarrativeEventBus`：

```text
GameStarted
WorkstationInteracted(Order)
WorkstationInteracted(Wheel)
WorkstationInteracted(Glaze)
WorkstationInteracted(Kiln)
OrderStarted(ORDER_001)
OrderCompleted(ORDER_001)
OrderFailed(ORDER_001)
OrderCompleted(ORDER_003)
OrderFailed(ORDER_003)
OrderCompleted(ORDER_004)
OrderFailed(ORDER_004)
ScoreThresholdReached(95)
ChapterCompleted(CH01)
```

命令统一通过 `P10NarrativeCommandBus` 预留：

```text
NarrativePauseGameplay
NarrativeResumeGameplay
NarrativeRequestInputLock
NarrativeReleaseInputLock
NarrativeRequestOpenDialogue
NarrativeFinishedBlockingSegment
```

后续正式桥接时，由新增 `MVPBridge` 或 `P10NarrativeBridgeAdapter` 把 Phase3 / Phase6 / Phase8 的事件转发给 `P10NarrativeEventBus`；由 `P10NarrativeCommandAdapter` 接收 Phase10 命令，并决定如何请求 Phase8 / Phase3 / Phase6 执行暂停、恢复、输入锁或 UI 阻塞。

## 15. Future Bridge Direction

后续桥接任务名称：

```text
Phase10 Narrative Bridge Integration
```

推荐桥接结构：

```text
Gameplay -> Narrative:
Phase6 Workstation / Phase8 GameplayBridge / Phase3 GameManager
        ↓
P10NarrativeEventBus
        ↓
P10NarrativeStateMachine
        ↓
P10NarrativeManager
        ↓
P10_CH01_NarrativeOverlay

Narrative -> Gameplay:
P10NarrativeManager / P10NarrativeStateMachine
        ↓
P10NarrativeCommandBus
        ↓
P10NarrativeCommandAdapter
        ↓
Phase8 GameplayBridge / Phase3 GameManager / Phase6 Input Gate

Spatial Binding:
Phase6 Workstation Transform.position
        ↓
P10NarrativeAnchorMapper
        ↓
P10_CH01_Anchor_* runtime positions
```

如果仍坚持不改旧文件，桥接可以先使用运行时发现方式：

```text
FindObjectOfType<GameplayBridgeManager>()
FindObjectsOfType<Workstation>()
读取 AreaType
轮询 CurrentRuntimeMode / CurrentSession / GameManager.CurrentState
转换为 P10NarrativeEvent
发送到 P10NarrativeEventBus
```

此方式可用于原型验证，但正式版本更建议给旧桥接层增加极窄事件出口，只广播事件，不写剧情逻辑。

## 16. Acceptance Criteria For Current Task

本任务完成时，必须满足：

```text
1. 新建 `Assets/Phase10_Narrative/**` 目录结构。
2. 新建 `P10_CH01_NarrativeOverlay.unity`。
3. 新增 P10NarrativeStateMachine，包含 Prologue / Tutorial / Order001 / Order003 / Order004 / Ending / Completed。
4. 新增 P10NarrativeEventBus，并能在独立场景中发送测试事件。
5. 新增 P10NarrativeCommandBus 或命令类型预留，并能在独立场景中记录测试命令。
6. 场景可独立打开运行。
7. 场景内有 4 个 NPC Placeholder。
8. 场景内有 Order / Wheel / Glaze / Kiln / CourtyardCenter / Gate 剧情锚点。
9. 当前阶段锚点只要求命名和近似布局，不要求与 Phase6 Workstation 精确对齐。
10. 文档明确桥接阶段必须由 P10NarrativeAnchorMapper 运行时映射 Phase6 Workstation Transform.position。
11. 场景内允许出现 Narrative Props，例如 FatherLedger / OldKilnTool / BrokenBowl / AncientOrder。
12. 场景内不得出现 Phase3 生产玩法物品，例如甜白釉、祭红釉、瓷土、木柴等作为剧情层拥有的数据。
13. 能从 Prologue 播放到 Completed。
14. ORDER_001 / ORDER_003 / ORDER_004 有接取、成功、失败剧情节点。
15. ORDER_004 有普通完成与精品高潮两条路径。
16. 新增 `Assets/Phase10_Narrative/Docs/P10_CH01_NarrativePlan.md`，记录 StoryMain 来源、故事结构、角色关系、状态流、订单剧情映射。
17. 所有新增文件都位于 `Assets/Phase10_Narrative/**`。
18. 不修改 `Assets/Phase3/**`、`Assets/Phase6/**`、`Assets/Phase8/**`、`Assets/Scenes/**`、`ProjectSettings/**`。
```

## 17. Recommended Execution Steps

```text
1. 创建 `Assets/Phase10_Narrative` 目录结构。
2. 创建 `P10_CH01_NarrativeOverlay.unity`。
3. 创建 P10NarrativeState / P10NarrativeStateMachine。
4. 创建 P10NarrativeEvent / P10NarrativeEventType / P10NarrativeEventBus。
5. 创建 P10NarrativeCommand / P10NarrativeCommandType / P10NarrativeCommandBus 预留。
6. 在剧情场景内创建 Root 对象：NarrativeRoot、EventBusRoot、NPCRoot、PropRoot、TriggerRoot、AnchorRoot、DialogueUIRoot。
7. 创建 4 个 NPC Placeholder Prefab。
8. 创建 Narrative Props Prefab：FatherLedger、OldKilnTool、BrokenBowl、AncientOrder。
9. 根据 Phase6 工作台布局摆放近似剧情锚点；不得把精确对齐作为当前阶段验收。
10. 创建 Dialogue UI Prefab。
11. 创建 `P10_CH01_NarrativePlan.md`，记录 StoryMain 来源、故事结构、角色关系、状态流、订单剧情映射。
12. 将 StoryMain 第一章拆成占位 DialogueNode / ChapterFlow / State Flow 数据；首轮允许占位对白。
13. 实现独立剧情播放流程。
14. 添加 DebugPanel：发送 GameStarted、OrderCompleted(ORDER_001)、OrderCompleted(ORDER_003)、ScoreThresholdReached(95) 等测试事件。
15. 添加 DebugPanel：发送 NarrativePauseGameplay、NarrativeResumeGameplay 等测试命令并记录日志。
16. 验证状态机能从 Prologue 推进到 Completed。
17. 编写 `P10_BridgeContract.md`、`P10_PropsContract.md`、`P10_StateMachineContract.md`，并在 BridgeContract 中声明 P10NarrativeAnchorMapper 为桥接阶段必需项。
18. 验证没有任何旧项目文件变动。
```

## 18. Collaboration Rule

剧情开发者允许修改：

```text
Assets/Phase10_Narrative/**
```

剧情开发者不得修改：

```text
Assets/Phase3/**
Assets/Phase6/**
Assets/Phase8/**
Assets/Scenes/**
ProjectSettings/**
```

如后续必须接入旧项目，应另开桥接任务，并通过 Gitee 分支进行代码审查和融合。

## 19. Summary

Phase10 的核心原则是：当前阶段先建立可复用的 Narrative State Machine、P10NarrativeEventBus 和 P10NarrativeCommandBus 预留，再实现第一章剧情内容和独立剧情 Overlay 场景。剧情层可以拥有纯叙事的 Narrative Props，但不得拥有生产玩法物品。所有临时人物、触发器、剧情 UI、Narrative Props 和叙事数据都放在 `Assets/Phase10_Narrative` 中。当前阶段锚点只做近似占位，不要求与 Phase6 精确对齐；后续必须通过 P10NarrativeAnchorMapper 映射 Phase6 Workstation 的运行时 Transform.position。后续桥接应通过 EventBus 和 CommandBus 双通道连接 Phase6 空间布局、Phase3 玩法结果与 Phase10 叙事状态，而不是让 Phase10 直接依赖 Phase3。
