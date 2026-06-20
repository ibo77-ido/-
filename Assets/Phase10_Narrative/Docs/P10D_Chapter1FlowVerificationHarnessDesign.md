# P10D-13 Chapter 1 Flow Verification Harness Design

## Problem Statement

Chapter 1 needs a Phase10-only verification entry that can walk the full narrative flow before the complete game loop is integrated. The harness must cover the first chapter start, NPC dialogue, order gates, next-order entry, Dialogue Log behavior, snapshot save/load, and text rendering without changing the formal runtime rules that currently prevent accept nodes from jumping directly to pass nodes.

The goal is not to add a player-facing skip feature. The goal is to provide an Editor-only validation harness that simulates missing gameplay facts from the future full game while keeping the Phase10 runtime progression rules intact.

Current Phase10 inventory checked for this design:

| Capability | Current Phase10 status | Notes |
| --- | --- | --- |
| Validator | Present | Existing static validators include `P10C05SaveStateSafetyValidator`, `P10D05InGameAcceptanceValidator`, `P10D06DialogueLogValidator`, and bridge validators under `Scripts/Editor`. |
| Test runner | Partial | No NUnit-style test runner is present, but Unity batchmode can invoke static validator entry points. `P10NarrativePlayModeValidator` can open the Phase10 overlay scene and enter Play Mode. |
| Editor-only tooling | Present | `Scripts/Editor` contains validation tooling; `P10DialogueController` also has editor-only asset population behind `UNITY_EDITOR`. |
| Dev harness | Present but not preferred | `P10NarrativeDebugPanel` exists as runtime dev UI. This design avoids expanding it because the task forbids new formal runtime debug UI. |
| Narrative bootstrap | Present | `P10NarrativeManager`, `P10NarrativeSceneBindingHub`, `P10NarrativeSceneTrigger`, `P10NarrativeNpcInteraction`, and `P10DialogueController` can bootstrap a temporary narrative flow. |
| Task / quest progression API | Partial | `P10NarrativeEventType.OrderCompleted` is handled for `ORDER_001` and `ORDER_003`; `ScoreThresholdReached` drives the climax path; first-pass scene triggers can enter approved pass nodes in order. |
| Dialogue node jump API | Restricted | `TryAdvanceDialogueNode` and `HandleSceneTrigger` enforce allowed ordering. There is no unrestricted public "jump to any node" API, which is correct for formal runtime safety. |

## Constraints

This P10D-13 task is docs-only. It does not implement the harness.

Allowed design and future implementation scope:

```text
Assets/Phase10_Narrative/**
```

Allowed local memory update:

```text
AGENTS/RuntimeLayer/MEMORY_Phase10.md
```

Forbidden scope:

```text
Assets/Phase3/**
Assets/Phase6/**
Assets/Phase8/**
Assets/Scenes/**
ProjectSettings/**
```

The design must also preserve these rules:

```text
No stage, commit, or push.
No handling of Phase6 Workshop_TestScene / NavMesh dirty or deleted state.
No new formal runtime debug panel.
No change to formal task progression rules.
No main-scene skip logic.
No bridge contract change for Yu branch integration.
```

## Proposed Harness

Preferred approach: add a future Editor-only validator/harness under `Assets/Phase10_Narrative/Scripts/Editor`, for example:

```text
P10D13Chapter1FlowHarnessValidator.RunP10D13Chapter1FlowHarnessValidation()
```

The harness should not require a persistent Unity scene. It should instantiate temporary objects in editor validation scope, mirror the object construction pattern already used by `P10D05InGameAcceptanceValidator` and `P10D06DialogueLogValidator`, then destroy all temporary objects at the end of the run.

Recommended structure:

| Harness part | Responsibility |
| --- | --- |
| `P10D13Chapter1FlowHarnessValidator` | Public batchmode entry point and failure reporting. |
| Temporary root GameObject | Owns all temporary manager, hub, NPC, and dialogue controller objects. |
| `P10NarrativeManager` | Runtime object under test; formal progression logic remains unchanged. |
| `P10NarrativeSceneBindingHub` | Routes simulated NPC/trigger interactions to the manager. |
| `P10DialogueController` | Records Dialogue Log entries and exposes snapshot import/export behavior through the manager. |
| Temporary NPC interactions | Simulate user interaction with Xu Lao Bo, Zhou Zhang Gui, Chen Shu Yuan, and Lu Ke without touching scene assets. |
| Harness step table | Declares expected node, state, simulated player/NPC action, and simulated gameplay fact for each chapter step. |

Triggering the Chapter 1 start:

```text
Create a temporary Xu Lao Bo NPC object.
Attach P10NarrativeNpcInteraction.
Configure it through P10NarrativeSceneBindingHub.
Call interaction.Interact().
Assert state = Prologue and node = P10_CH01_NODE_PROLOGUE_01.
```

This uses the same Phase10 NPC-entry path as runtime without editing Phase6 or main scenes. Direct `manager.HandleSceneTrigger("StartPrologue", P10_CH01_NODE_PROLOGUE_01)` may be kept as a deterministic fallback assertion, but the primary harness path should exercise NPC interaction.

Simulating NPC dialogue:

```text
After each accepted node, call controller.SetCurrentNode(manager.GetCurrentNode()).
Assert one visible dialogue line is resolved.
Assert the Dialogue Log receives one non-empty entry with sequence, nodeId, speakerName, and dialogueText.
Use controller.AdvanceDialogue() only for legal dialogue-next transitions.
Assert accept nodes that require gameplay completion do not skip directly to pass nodes.
```

Simulating task completion conditions:

```text
ORDER_001: publish P10NarrativeEvent(OrderCompleted) with OrderId = ORDER_001.
ORDER_003: publish P10NarrativeEvent(OrderCompleted) with OrderId = ORDER_003.
ORDER_004 normal path: use the existing approved Phase10 first-pass trigger pair Order004PassNormal -> P10_CH01_NODE_ORDER_004_PASS_NORMAL, or add a future Editor-only wrapper that calls the same existing Phase10 manager API without changing runtime progression.
ORDER_004 climax path: optional separate branch using ScoreThresholdReached = 95 if climax coverage is required.
```

Entering the next task:

```text
After ORDER_001_PASS is displayed, call controller.AdvanceDialogue() and assert ORDER_003_ACCEPT.
After ORDER_003_PASS is displayed, call controller.AdvanceDialogue() and assert ORDER_004_ACCEPT.
After ORDER_004_PASS_NORMAL is displayed, use the existing approved ChapterEnding trigger path if the harness must assert Completed.
```

The bypass exists only in the Editor-only harness. It supplies the missing gameplay completion facts or approved first-pass trigger calls that the complete game would normally provide. It must not add a runtime "next task" button or relax `TryAdvanceDialogueNode`.

Recording Dialogue Log:

```text
Assert log count increments only when a new node is displayed.
Assert repeated controller.SetCurrentNode(currentNode) does not duplicate the latest nodeId + dialogueText.
Assert speakerName is populated.
Assert nodeId and dialogueText are retained in entry views and snapshot entries.
Assert the log can be opened while dialogue UI is closed.
```

Validating snapshot save/load:

```text
Save after at least one mid-chapter checkpoint, preferably ORDER_001_PASS or ORDER_003_ACCEPT.
Assert SnapshotVersion = 2.
Assert CurrentNodeId, ChapterState, PlayedNodeIds, NarrativeFlags, and DialogueLogEntries are present.
Create a new temporary manager/controller pair.
Load the snapshot.
Assert restored state and node match the saved source.
Assert restored Dialogue Log entries match the source.
Call controller.SetCurrentNode(restored current node) and assert no duplicate log entry is appended.
Continue one legal transition after restore to prove load did not block progression.
```

Validating Chinese text display:

```text
If official Chinese P10DialogueNodeSO assets exist, the harness should scan Phase10 dialogue assets for CJK text and drive those nodes through P10DialogueController.
If current assets still contain only English text, the harness should create a temporary editor-only probe line such as "中文显示测试：窑火重燃" and assign it to a temporary dialogue node/controller path.
Assert the UI Text receives the exact Chinese string.
Assert the resolved runtime font is non-null.
Assert the Dialogue Log entry stores and displays the exact Chinese string.
Assert snapshot save/load preserves the exact Chinese string.
```

This check must remain Editor-only and must not add a localization system, new runtime font asset, or serialized scene reference in this task chain.

## Isolation Guarantees

The future harness should be isolated by construction:

```text
It lives under Assets/Phase10_Narrative/Scripts/Editor.
It is compiled as editor-only code.
It creates only temporary GameObjects.
It destroys temporary objects and generated EventSystem objects after validation.
It does not open or save Phase3, Phase6, Phase8, Assets/Scenes, or ProjectSettings assets.
It does not modify serialized scene or prefab references.
It does not add a runtime debug UI.
It does not call git add, commit, or push.
```

The formal runtime code remains the system under test:

```text
P10NarrativeManager.HandleSceneTrigger
P10NarrativeManager.TryAdvanceDialogueNode
P10NarrativeManager.PublishEvent(OrderCompleted)
P10NarrativeManager.SaveSnapshot
P10NarrativeManager.LoadSnapshot
P10DialogueController.SetCurrentNode
P10DialogueController.AdvanceDialogue
```

The harness should fail if these APIs no longer enforce the accepted ordering rules. It should not weaken those rules to make the validation pass.

## Yu Branch Bridge Safety

The harness must not change the Phase10 bridge contract that a future Yu branch integration depends on.

Bridge-safe guarantees:

```text
Do not modify P10NarrativeBridgePort.
Do not modify P10NarrativeGameplayFactType.
Do not modify bridge preview adapters.
Do not introduce Phase3, Phase6, or Yu branch type references.
Do not add serialized references from bridge assets to temporary harness objects.
Do not change anchor mapping behavior.
Do not change narrative command behavior.
```

The harness may simulate facts that the bridge would later submit, but only through existing Phase10-owned event surfaces:

```text
OrderCompleted(ORDER_001)
OrderCompleted(ORDER_003)
ScoreThresholdReached(95) for optional climax coverage
```

This keeps the bridge boundary stable. Yu branch work can later replace the harness fact injection with real gameplay facts without needing a changed Phase10 runtime contract.

## Verification Flow

Recommended normal-path validation sequence:

| Step | Simulated action | Expected state | Expected node | Notes |
| --- | --- | --- | --- | --- |
| 1 | Xu Lao Bo NPC `Interact()` | Prologue | `P10_CH01_NODE_PROLOGUE_01` | Chapter start through NPC entry. |
| 2 | Display current node | Prologue | `P10_CH01_NODE_PROLOGUE_01` | Dialogue Log entry 1. |
| 3 | Dialogue Next | Tutorial | `P10_CH01_NODE_TUTORIAL_01` | Legal dialogue transition. |
| 4 | Display current node | Tutorial | `P10_CH01_NODE_TUTORIAL_01` | Dialogue Log entry 2. |
| 5 | Dialogue Next | Order001 | `P10_CH01_NODE_ORDER_001_ACCEPT` | Enters first order accept. |
| 6 | Display current node | Order001 | `P10_CH01_NODE_ORDER_001_ACCEPT` | Dialogue Log entry 3. |
| 7 | Dialogue Next | Order001 | `P10_CH01_NODE_ORDER_001_ACCEPT` | Must not skip directly to pass. |
| 8 | Publish `OrderCompleted(ORDER_001)` | Order001 | `P10_CH01_NODE_ORDER_001_PASS` | Simulated gameplay completion. |
| 9 | Save snapshot | Order001 | `P10_CH01_NODE_ORDER_001_PASS` | Assert version 2 and log payload. |
| 10 | Load snapshot into new temp manager/controller | Order001 | `P10_CH01_NODE_ORDER_001_PASS` | Assert restored state and log. |
| 11 | Dialogue Next | Order003 | `P10_CH01_NODE_ORDER_003_ACCEPT` | Enters next task through legal pass-node transition. |
| 12 | Display current node | Order003 | `P10_CH01_NODE_ORDER_003_ACCEPT` | Dialogue Log records next task. |
| 13 | Dialogue Next | Order003 | `P10_CH01_NODE_ORDER_003_ACCEPT` | Must not skip directly to pass. |
| 14 | Publish `OrderCompleted(ORDER_003)` | Order003 | `P10_CH01_NODE_ORDER_003_PASS` | Simulated gameplay completion. |
| 15 | Dialogue Next | Order004 | `P10_CH01_NODE_ORDER_004_ACCEPT` | Enters final order. |
| 16 | Display current node | Order004 | `P10_CH01_NODE_ORDER_004_ACCEPT` | Dialogue Log records final accept. |
| 17 | Trigger `Order004PassNormal` | Order004 | `P10_CH01_NODE_ORDER_004_PASS_NORMAL` | Existing approved first-pass pass-node trigger. |
| 18 | Trigger `ChapterEnding` | Completed | `P10_CH01_NODE_CHAPTER_ENDING` | Existing manager trigger path advances through Ending to Completed. |
| 19 | Display final node | Completed | `P10_CH01_NODE_CHAPTER_ENDING` | Dialogue Log records the closing line. |

Additional assertions:

```text
Out-of-order trigger fails without changing state.
Duplicate trigger fails without changing state.
Repeated current-node refresh does not duplicate Dialogue Log.
Chinese probe text displays, logs, saves, and restores exactly.
No Phase3 / Phase6 / Phase8 / Assets/Scenes / ProjectSettings file is opened for save.
No serialized references are changed.
```

## Out-of-Scope

This task does not implement:

```text
Runtime debug panel changes.
Runtime skip / chapter select UI.
Formal task progression rule changes.
Phase3 order-system integration.
Phase6 Workshop_TestScene or NavMesh repair.
Phase8 integration.
Main scene integration.
Yu branch bridge contract changes.
New localization system.
New font asset pipeline.
NUnit test framework migration.
Git stage, commit, or push.
```

## Recommended Next Step

Implement the preferred Editor-only validator/harness as the next approved task:

```text
P10D-14 Chapter 1 Flow Verification Harness Implementation
```

The implementation should stay under `Assets/Phase10_Narrative/Scripts/Editor`, reuse the temporary object pattern from existing validators, and produce a Unity batchmode entry point that validates the normal Chapter 1 flow without changing runtime progression rules, serialized scene references, or Yu branch bridge contracts.
