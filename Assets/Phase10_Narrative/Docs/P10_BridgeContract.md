# P10 Bridge Contract

## Goal

Phase10 is a narrative content layer. It does not replace Phase8 and does not directly control Phase3 gameplay or Phase6 world behavior.

The current Phase10 task sequence builds an independent narrative layer first. Formal integration with the existing game must happen later through a bridge task.

## Current Phase Boundary

Allowed now:

```text
Create Phase10 narrative scripts.
Create Phase10 data types.
Create Phase10 EventBus and CommandBus.
Create a standalone Phase10 overlay scene.
Create bridge preview interfaces and adapters under Phase10.
```

Forbidden now:

```text
Modify Phase3 gameplay scripts, data, prefabs, or scenes.
Modify Phase6 movement, workstation, interaction, or scene files.
Modify Phase8 bridge/runtime/session files.
Modify Assets/Scenes.
Modify ProjectSettings.
Make old systems reference Phase10 directly.
```

## Gameplay To Narrative Direction

Future bridge direction:

```text
Phase3 / Phase6 / Phase8
        -> P10NarrativeGameplayEventAdapter
        -> P10NarrativeEventBus
        -> P10NarrativeStateMachine
        -> P10NarrativeManager / Dialogue UI / Narrative Props
```

Do not create this dependency:

```text
Phase3 -> P10NarrativeEventBus
Phase6 -> P10NarrativeEventBus
Phase8 -> P10NarrativeStateMachine
```

Gameplay facts must be translated into neutral `P10NarrativeEvent` values by an adapter.

Old systems must not directly reference or publish to `P10NarrativeEventBus`.
Gameplay facts must pass through `P10NarrativeGameplayEventAdapter` or a later formal Bridge Adapter before they enter Phase10 narrative state handling.

## Frozen Gameplay Fact Contract

P10B-01 freezes the neutral gameplay fact boundary for the first bridge implementation pass.

Allowed fact types:

```text
GameStarted
OrderCompleted
ScoreThresholdReached
DialogueLineStarted
NarrativeStateEntered
NarrativePropInspected
```

Allowed fact fields:

```text
FactType
ChapterId
OrderId
NodeId
TargetId
Payload
Score
```

Allowed conversion targets:

```text
GameStarted              -> P10NarrativeEventType.GameStarted
OrderCompleted           -> P10NarrativeEventType.OrderCompleted
ScoreThresholdReached    -> P10NarrativeEventType.ScoreThresholdReached
DialogueLineStarted      -> P10NarrativeEventType.DialogueLineStarted
NarrativeStateEntered    -> P10NarrativeEventType.NarrativeStateEntered
NarrativePropInspected   -> P10NarrativeEventType.NarrativePropInspected
```

The bridge may copy only these neutral values into `P10NarrativeEvent`:

```text
ChapterId
OrderId
NodeId
Payload
Score
```

Gameplay facts must not carry Phase3 item data, Phase6 workstation instances, Phase8 runtime mode objects, scoring implementation objects, recipes, rewards, or production resource records.

## Narrative To Gameplay Direction

Future command direction:

```text
P10NarrativeManager / P10NarrativeStateMachine
        -> P10NarrativeCommandBus
        -> P10NarrativeCommandAdapter
        -> Phase8 / Phase3 / Phase6
```

Phase10 may request narrative commands, but it must not decide how old systems execute them.

Reserved command examples:

```text
NarrativePauseGameplay
NarrativeResumeGameplay
NarrativeRequestInputLock
NarrativeReleaseInputLock
NarrativeRequestOpenDialogue
NarrativeFinishedBlockingSegment
```

## Frozen Narrative Command Contract

P10B-01 freezes the narrative command boundary for the first bridge implementation pass.

Allowed command types:

```text
NarrativePauseGameplay
NarrativeResumeGameplay
NarrativeRequestInputLock
NarrativeReleaseInputLock
NarrativeRequestOpenDialogue
NarrativeFinishedBlockingSegment
```

Allowed command fields:

```text
CommandType
Payload
TargetId
NodeId
```

Execution rule:

```text
P10NarrativeCommandBus publishes and records narrative commands.
P10NarrativeCommandAdapter receives commands and translates them into adapter-mediated requests.
Old-system execution policy stays outside Phase10.
Phase10 must not directly execute gameplay pause, resume, input lock, UI opening, or runtime mode changes.
```

## Event Bus Boundary

`P10NarrativeEventBus` is only responsible for:

```text
Init
Subscribe
Unsubscribe
Publish
Dispose
```

It must not:

```text
Advance state directly.
Open UI.
Play audio.
Spawn prefabs.
Pause gameplay.
Reference Phase3, Phase6, or Phase8 types.
```

## Command Bus Boundary

`P10NarrativeCommandBus` is only responsible for publishing narrative commands and recording debug command history when needed.

It must not:

```text
Call Phase8 runtime mode APIs.
Call Phase3 game manager APIs.
Call Phase6 input or movement APIs.
Decide gameplay pause, resume, or input lock behavior.
```

## Spatial Bridge Preview

Future bridge work should use:

```text
P10NarrativeAnchorMapper
```

The mapper may read or discover Phase6 workstation positions during integration and then move or bind Phase10 anchors. It must not modify Phase6 workstation objects.

Current independent overlay validation only requires approximate placeholder anchors.
Current P10-05 preview work does not treat exact spatial alignment as formal acceptance. During the later bridge integration stage, `P10NarrativeAnchorMapper` is the reserved runtime mapping point for neutral anchor ids to positions derived from Phase6 Workstation `Transform.position` values, without creating a compile-time dependency on Phase6 types.

## Frozen CH01 Anchor Contract

P10B-01 freezes the CH01 anchor ids for the first bridge implementation pass.

```text
P10_CH01_Anchor_Order
P10_CH01_Anchor_Wheel
P10_CH01_Anchor_Glaze
P10_CH01_Anchor_Kiln
P10_CH01_Anchor_CourtyardCenter
P10_CH01_Anchor_Gate
```

Allowed anchor fields:

```text
AnchorId
Position
```

Mapping rule:

```text
P10NarrativeAnchorMapper maps neutral anchor ids to Vector3 positions.
Later bridge tasks may read Phase6 workstation runtime Transform.position values.
The mapper must not modify Phase6 workstation objects.
The mapper must not require Phase10 to reference Phase6 workstation types at compile time.
```

## Frozen Old-System Read Points

P10B-01 documents read-only points that later bridge tasks may request after human approval.

Allowed read points for later tasks:

```text
Phase3 order completion facts by neutral OrderId.
Phase3 score or quality threshold facts by neutral integer Score.
Phase6 workstation runtime Transform.position values for anchor mapping.
Phase8 runtime/session state only as neutral mode or availability facts.
Existing scene object names or Transform positions only for validation.
```

Read point rules:

```text
Read points are documentation permissions, not implementation permission.
Each later P10B task must explicitly state which read point it uses.
Read points must not mutate old objects, old scenes, old ScriptableObjects, or ProjectSettings.
Read data must cross into Phase10 as neutral facts, neutral commands, or neutral anchor id / Vector3 mappings.
```

## Acceptance

- Old systems do not directly reference Phase10 buses or state machine.
- Gameplay facts are converted by adapter code before reaching Phase10.
- Narrative commands are published through `P10NarrativeCommandBus`.
- Narrative commands are executed only through an adapter-mediated old-system boundary.
- Bridge preview code stays under `Assets/Phase10_Narrative/Scripts/BridgePreview/`.
- Formal old-system integration is deferred to a separate `Phase10 Narrative Bridge Integration` task.
