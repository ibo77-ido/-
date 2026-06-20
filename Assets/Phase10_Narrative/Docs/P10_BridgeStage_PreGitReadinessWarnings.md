# Phase10 桥接前检测与 Git 提醒

## Draft Date

2026-06-20

## Basis

本文件用于 Phase10 初步完成后、进入 Git 提交和后续各阶段桥接前的提醒清单。

本次检测基于：

- 当前分支：`Gong`
- `git status --short`
- `git diff --name-status -- Assets/Phase10_Narrative`
- `dotnet build Phase10_Narrative.csproj`
- Phase10 脚本静态搜索：Phase3 / Phase6 / Phase8 直接依赖、距离 gate、移动锁、桥接相关脚本
- 现有桥接文档：`P10_BridgeContract.md`、`P10_BridgeIntegrationPlan.md`

## Detection Summary

| 检测项 | 结果 | 说明 |
|---|---|---|
| 当前分支 | `Gong` | Phase10 相关工作位于 Gong 分支 |
| C# 编译 | PASS | `dotnet build Phase10_Narrative.csproj` 通过，0 warning / 0 error |
| 暂存区 | EMPTY | 当前未发现 staged 文件 |
| Phase10 runtime 直接依赖 Phase3 / Phase6 / Phase8 | 未发现明确 runtime 直接引用 | 搜索脚本中没有发现 runtime 直接 `using Phase3/Phase6/Phase8` 或直接调用旧系统 API |
| Phase10 Editor 工具触碰 Phase6 风险 | 存在 | `P10E06TriggerGroupAggregationValidator.cs` 硬编码 `Assets/Phase6/Scenes/Workshop_TestScene.unity`，其中 migration 方法会保存 Phase6 scene |
| Phase10 UI / 叙事资源 | 存在大量候选改动 | 包括 scripts、dialogue assets、speaker assets、prefab、overlay scene、Resources/P10Art |
| 非 Phase10 脏状态 | 存在 | Phase3 多个数据/场景修改，Phase6 Workshop_TestScene 和 NavMesh 删除/修改状态仍存在 |
| 距离 / 移动锁旧方案 | 未作为正式 runtime 接入 | 未发现 `P10MvpTutorialMovementLock` / `P10MvpTutorialNpcInteraction` runtime 类型；`P10NpcInteractionGateEvaluator` 仍存在但应视为 Phase10-only helper |

## Git Before Bridge

提交前必须使用 selective stage，不要使用：

```text
git add .
```

推荐先把提交拆成至少两类：

1. Phase10 自身稳定成果提交。
2. 后续桥接提交。

Phase10 自身提交建议只包含：

- `Assets/Phase10_Narrative/Scripts/**`
- `Assets/Phase10_Narrative/ScriptableObjects/**`
- `Assets/Phase10_Narrative/Prefabs/**`
- `Assets/Phase10_Narrative/Scenes/P10_CH01_NarrativeOverlay.unity`
- `Assets/Phase10_Narrative/Resources/P10Art/**`
- `Assets/Phase10_Narrative/Docs/**`
- 对应 `.meta`

提交前必须人工确认是否纳入：

- `Assets/Phase10_Narrative/Prefabs/UI/P10_CH01_DialogueUI_Placeholder.prefab`
- `Assets/Phase10_Narrative/Scenes/P10_CH01_NarrativeOverlay.unity`
- `Assets/Phase10_Narrative/Resources/P10Art/**`
- `Project/StoryMain.md`
- Phase10E docs / validators
- bridge preview docs / validators

默认不要 stage：

- `AGENTS/RuntimeLayer/MEMORY_Phase10.md`
- `AGENTS/WorkFlowLayerV1.3/**`
- `Assets/Phase3/**`
- `Assets/Phase6/**`
- `Assets/Phase8/**`
- `Assets/Scenes/**`
- `ProjectSettings/**`
- `Logs/**`
- `obj/**`
- `.vs/**`
- `P10_04_PlayMode.log`
- `Assets/Workshop_TestScene*`
- 根目录素材源文件夹，除非明确决定把源素材纳入仓库

## Bridge Boundary Reminders

桥接阶段必须继续遵守：

- Phase10 是叙事层，不拥有 Phase3 订单、配方、评分、奖励、烧窑、器型、釉料玩法数据。
- Phase10 不直接控制 Phase6 玩家移动、NPC、工作台、NavMesh 或场景布局。
- Phase10 不直接控制 Phase8 RuntimeMode、Session 或旧 UI Host。
- 旧系统不要直接引用 `P10NarrativeEventBus`、`P10NarrativeStateMachine`、`P10DialogueController`。
- 旧系统事实必须先转成 neutral gameplay fact，再进入 Phase10。
- Phase10 发出的玩法请求必须通过 command adapter，不直接调用旧系统 API。

推荐桥接方向：

```text
Phase3 / Phase6 / Phase8
        -> Bridge Adapter / neutral facts
        -> P10NarrativeEventBus
        -> P10NarrativeStateMachine
        -> P10NarrativeManager / Dialogue UI
```

```text
Phase10 narrative command
        -> P10NarrativeCommandBus
        -> Command Adapter
        -> Phase8 / Phase3 / Phase6 execution boundary
```

## High Risk Items

### 1. Phase6 Workshop / NavMesh Dirty

当前工作区仍显示：

- `Assets/Phase6/Scenes/Workshop_TestScene.unity` modified
- `Assets/Phase6/Scenes/Workshop_TestScene.meta` deleted
- `Assets/Phase6/Scenes/Workshop_TestScene/NavMesh.asset` deleted
- `Assets/Phase6/Scenes/Workshop_TestScene/NavMesh.asset.meta` deleted

这些文件不要混进 Phase10 提交。桥接前要决定：

- 是否先单独处理 Phase6 场景/NavMesh 状态；
- 是否恢复 NavMesh；
- 是否把 Phase6 变更作为独立提交；
- 是否完全绕开这些脏状态，只提交 Phase10。

### 2. P10E06 Validator Can Mutate Phase6 Scene

`Assets/Phase10_Narrative/Scripts/Editor/P10E06TriggerGroupAggregationValidator.cs` 中：

- `RunP10E06TriggerGroupAggregationMigrationAndValidation()` 会打开 `Assets/Phase6/Scenes/Workshop_TestScene.unity`
- 会执行 migration
- 会 `MarkSceneDirty`
- 会 `SaveScene`

桥接前不要随手运行 migration 菜单。若要运行，必须先明确：

- 本任务允许修改 Phase6 scene；
- Phase6 dirty/NavMesh 状态已经处理；
- 验证后要检查 `git diff --name-status Assets/Phase6`。

### 3. Distance Gate Is Not Formal Phase6 Integration

当前仍有：

- `P10NpcInteractionGateEvaluator`
- `DefaultInteractionDistance = 2.5f`

但它应被视为 Phase10-only helper，不代表已经接入 Phase6 玩家、真实 NPC、真实点击逻辑。

如果后续重启距离检测，必须重新确认：

- 使用 `1.5` 还是 `2.5`
- 玩家位置来源来自 Phase6 / Yu / Bridge Adapter 中哪一个
- 距离外是否完全无反馈
- 是否需要保存 gate 状态
- 是否会影响玩家移动

不要恢复旧的 `P10MvpTutorialMovementLock` 或 `P10MvpTutorialNpcInteraction`，除非新任务明确批准。

### 4. Story Gate Must Be First-Class

用户当前需要的是：

- 每个 NPC 可以交互；
- 只有完成前一个剧情/任务后，才能进入下一个 NPC 的任务对话；
- NPC 交互前提来自 `StoryMain` / Phase10 narrative state，而不是只靠距离。

桥接时应优先做：

```text
current narrative state
        -> allowed NPC / allowed node
        -> click interaction
        -> open dialogue
```

不要让每个 NPC 永远裸点进入任意订单节点。

### 5. Current Order Panel Is Narrative-Derived

当前订单面板仍是 Phase10 当前 node/state 推导：

- 不读取正式 Phase3 order data
- 不接 Phase3 order system
- 只显示当前叙事订单和制作提示

正式桥接订单完成时，应只传 neutral `OrderCompleted(OrderId)` 或等价事实。不要把 Phase3 order asset、评分对象、奖励结构复制进 Phase10 snapshot。

### 6. UI Assets Must Travel With Prefab

Phase10 UI 依赖：

- `Assets/Phase10_Narrative/Prefabs/UI/P10_CH01_DialogueUI_Placeholder.prefab`
- `Assets/Phase10_Narrative/Scenes/P10_CH01_NarrativeOverlay.unity`
- `Assets/Phase10_Narrative/Resources/P10Art/**`
- `P10DialogueController`

如果只提交脚本、不提交 prefab/scene/resources，PlayMode 可能退回 fallback UI 或出现按钮/图片缺失。

提交前必须确认：

- 右上角 `订单` / `记录` 按钮可点击；
- 底部对话框没有残留关闭按钮；
- 文本在框内且清晰；
- 记录和订单面板关闭按钮只显示图片；
- Resources 中按钮、背景、头像素材都被纳入。

### 7. Encoding Warning

PowerShell 中看到的 `鍏`、`璁`、`鐢` 等乱码不一定代表文件内容真的坏了。不要仅凭终端输出修中文。

如果要查中文，优先在 Unity Inspector、支持 UTF-8 的编辑器、或通过 validator 检查实际 string。

### 8. ORDER_002 Gap Is Intentional Unless Story Changes

当前第一章主线使用：

- `ORDER_001`
- `ORDER_003`
- `ORDER_004`

不要为了编号连续而自动新增或重命名 `ORDER_002`。若要补 `ORDER_002`，必须先改 story mapping、dialogue nodes、order panel、validators 和 Phase3 order bridge contract。

## Suggested Bridge Task Order

推荐不要直接把 Phase10 接进旧系统。建议顺序：

1. `P10B-00`：重新确认桥接范围和提交范围。
2. `P10B-01`：冻结 neutral facts、commands、anchor ids。
3. `P10B-02`：Gameplay Fact Adapter，只把 Phase3/Phase6/Phase8 事实翻译成 Phase10 neutral event。
4. `P10B-03`：Narrative Command Adapter，只处理 Phase10 请求旧系统行为的出口。
5. `P10B-04`：Anchor Mapping，只读 Phase6 runtime Transform.position，不修改 Phase6 objects。
6. `P10B-05`：端到端小切片，只接一个订单完成事实和一个后续 NPC 解锁。
7. `P10B-06`：完整验证和人工 PlayMode 验收。

每一步都应独立 PASS，再进入下一步。

## Validation Checklist Before Git

提交前建议至少执行：

```text
dotnet build Phase10_Narrative.csproj
git diff --cached --name-status
git status --short
```

提交前建议人工 PlayMode 检查：

- Phase10 对话框显示正常。
- 点击对话框任意处能推进对话。
- 底部无继续按钮、无关闭按钮。
- 右上角 `记录` / `订单` 可打开面板。
- 记录面板滚动条可拖动。
- 订单面板能显示当前订单和提示。
- 完成一个任务后，后续 NPC / 后续订单不会提前乱触发。

如果 Unity batchmode 可用，建议重跑 Phase10D 最终矩阵或当前最新 Phase10 UI / dialogue validators。

## Suggested Git Add Commands

以下只是建议，不要自动执行：

```text
git add -- Assets/Phase10_Narrative/Scripts
git add -- Assets/Phase10_Narrative/ScriptableObjects
git add -- Assets/Phase10_Narrative/Prefabs
git add -- Assets/Phase10_Narrative/Scenes/P10_CH01_NarrativeOverlay.unity Assets/Phase10_Narrative/Scenes/P10_CH01_NarrativeOverlay.unity.meta
git add -- Assets/Phase10_Narrative/Resources
git add -- Assets/Phase10_Narrative/Docs
```

执行前必须先人工检查：

```text
git diff --cached --name-status
```

确认 cached diff 不包含：

```text
Assets/Phase3/**
Assets/Phase6/**
Assets/Phase8/**
Assets/Scenes/**
ProjectSettings/**
AGENTS/**
Logs/**
obj/**
.vs/**
P10_04_PlayMode.log
```

## Current Conclusion

Phase10 C# 编译当前可通过。Phase10 runtime 未发现直接依赖 Phase3 / Phase6 / Phase8 的明显编译耦合。

但当前工作区不适合直接 `git add .` 或直接提交，因为仍有大量 Phase3 / Phase6 / workflow / temp 脏状态。桥接前应先进行 selective stage，并把 Phase10 自身提交和 Phase10-to-old-system bridge 提交分开。

