# P10 Bridge Integration Plan

## Goal

Phase10 Narrative Bridge Integration connects the completed independent Phase10 narrative layer to existing gameplay signals in controlled slices.

The bridge must preserve Phase10 as a narrative layer. It must not make Phase10 own Phase3 gameplay rules, Phase6 workstation behavior, or Phase8 runtime mode policy.

## Current Phase10 Completion State

Phase10 independent narrative layer is complete.

```text
P10-00 Workspace And Contracts: PASS
P10-01 Narrative Foundation: PASS
P10-02 CH01 Content Data: PASS
P10-03 CH01 Overlay Scene: PASS
P10-04 Debug Playback Validation: PASS
P10-05 Bridge Preview Contract: PASS
P10-06 Phase10 Validation: PASS
```

Latest recorded completion state:

```text
Phase10 independent narrative layer: COMPLETE
Ready for later Phase10 Narrative Bridge Integration.
```

## Bridge Architecture

Gameplay to narrative direction:

```text
Phase3 / Phase6 / Phase8 gameplay facts
        -> neutral gameplay fact boundary
        -> P10NarrativeGameplayEventAdapter
        -> P10NarrativeEvent
        -> P10NarrativeEventBus
        -> P10NarrativeStateMachine
        -> P10NarrativeManager / Dialogue UI / Narrative Props
```

Old systems must not directly reference or publish to `P10NarrativeEventBus`.

Gameplay facts must be converted by `P10NarrativeGameplayEventAdapter` or a later formal Bridge Adapter before they enter Phase10 narrative state handling.

Narrative to gameplay direction:

```text
P10NarrativeManager / Phase10 narrative runtime
        -> P10NarrativeCommandBus
        -> P10NarrativeCommandAdapter
        -> old-system execution boundary
        -> Phase8 / Phase3 / Phase6
```

Narrative commands must request old-system behavior through an adapter. Phase10 must not directly pause gameplay, resume gameplay, lock input, unlock input, open old UI, call Phase8 runtime APIs, call Phase3 manager APIs, or call Phase6 input, movement, workstation, or interaction APIs.

Spatial bridge direction:

```text
Phase6 workstation runtime Transform.position
        -> neutral anchor id / Vector3 mapping
        -> P10NarrativeAnchorMapper
        -> Phase10 overlay anchors
```

`P10NarrativeAnchorMapper` may map neutral anchor ids to positions derived from Phase6 workstation runtime positions during bridge tasks. It must not modify Phase6 workstation objects.

## Allowed Modifications

Allowed during P10B bridge tasks only when the specific task states it:

```text
Modify Phase10 bridge documentation.
Modify Phase10 bridge scripts under Assets/Phase10_Narrative/**.
Add formal adapter code in the approved Phase10 bridge task.
Add neutral DTOs or mapping data under Assets/Phase10_Narrative/**.
Perform read-only validation against old scene or old runtime objects.
```

Allowed old-system changes must be explicit in the individual task. If a task does not explicitly allow old-system changes, the default is no old-system modification.

## Forbidden Modifications

Forbidden unless a later human-approved task explicitly changes the boundary:

```text
Make Phase3, Phase6, or Phase8 directly reference P10NarrativeEventBus.
Make old systems call P10NarrativeStateMachine directly.
Move order-internal node flow into P10NarrativeState enum.
Let Phase10 own Phase3 production item data, scoring data, recipes, rewards, or workstation rules.
Modify Assets/Scenes without a task that explicitly allows scene integration.
Modify ProjectSettings.
Submit obj/, Library/, Temp/, P10_04_PlayMode.log, or unrelated local files.
```

## Task Order

```text
P10B-00 Bridge Integration Planning
P10B-01 Bridge Contract Freeze
P10B-02 Gameplay Fact Adapter
P10B-03 Narrative Command Adapter
P10B-04 Anchor Mapping Validation
P10B-05 End-to-End Bridge Slice
P10B-06 Bridge Validation
```

Each P10B task must follow:

```text
DRAFT -> READY -> ACTIVE -> PASS / FAIL / BLOCKED
```

No P10B task may begin until the previous P10B task has human acceptance `PASS`.

## P10B-00 Bridge Integration Planning

### Objective

Create the formal bridge integration plan.

### Requirements

```text
Document bridge architecture.
Document allowed and forbidden modifications.
Document P10B task order.
Document task-level acceptance rules.
Do not implement bridge code.
Do not modify old systems.
```

### Acceptance Criteria

```text
Only Assets/Phase10_Narrative/Docs/** is modified.
P10_BridgeIntegrationPlan.md exists.
The plan states old systems must not directly reference P10NarrativeEventBus.
The plan states gameplay facts must be converted by adapter.
The plan states narrative commands must request old-system execution through adapter.
The plan states AnchorMapper maps runtime positions and does not modify Phase6 workstations.
The plan states each P10B task requires human PASS before the next task.
```

### Dependencies

```text
P10-00 through P10-06 PASS.
```

### Boundaries

```text
Docs only.
No runtime code changes.
No old-system changes.
No formal bridge implementation.
```

### Deliverables

```text
Assets/Phase10_Narrative/Docs/P10_BridgeIntegrationPlan.md
Verification Report.
```

## P10B-01 Bridge Contract Freeze

### Objective

Freeze the bridge contracts before implementation begins.

### Requirements

```text
Review P10_BridgeContract.md and P10_BridgeIntegrationPlan.md.
Define the final neutral gameplay fact fields.
Define the final narrative command fields.
Define the anchor id list needed for CH01.
Define the allowed old-system read points for later tasks.
No adapter implementation.
```

### Acceptance Criteria

```text
Bridge contract documents identify all allowed event, command, and anchor data crossing the boundary.
Old systems are still forbidden from direct EventBus or StateMachine access.
Narrative commands are still adapter-mediated.
Anchor mapping remains read-only toward Phase6 workstations.
No runtime code is changed unless explicitly approved as a contract-only data type adjustment.
```

### Dependencies

```text
P10B-00 PASS.
```

### Boundaries

```text
Prefer docs only.
If code changes are needed, they must stay under Assets/Phase10_Narrative/** and only adjust neutral bridge data contracts.
No old-system modification.
No scene modification.
```

### Deliverables

```text
Updated Phase10 bridge contract docs.
Final event, command, and anchor contract checklist.
Allowed old-system read point checklist.
Verification Report.
```

### Frozen Contract Checklist

Final neutral gameplay fact types:

```text
GameStarted
OrderCompleted
ScoreThresholdReached
DialogueLineStarted
NarrativeStateEntered
NarrativePropInspected
```

Final neutral gameplay fact fields:

```text
FactType
ChapterId
OrderId
NodeId
TargetId
Payload
Score
```

Final conversion target event types:

```text
GameStarted              -> P10NarrativeEventType.GameStarted
OrderCompleted           -> P10NarrativeEventType.OrderCompleted
ScoreThresholdReached    -> P10NarrativeEventType.ScoreThresholdReached
DialogueLineStarted      -> P10NarrativeEventType.DialogueLineStarted
NarrativeStateEntered    -> P10NarrativeEventType.NarrativeStateEntered
NarrativePropInspected   -> P10NarrativeEventType.NarrativePropInspected
```

Final narrative command types:

```text
NarrativePauseGameplay
NarrativeResumeGameplay
NarrativeRequestInputLock
NarrativeReleaseInputLock
NarrativeRequestOpenDialogue
NarrativeFinishedBlockingSegment
```

Final narrative command fields:

```text
CommandType
Payload
TargetId
NodeId
```

Command execution rule:

```text
Commands remain adapter-mediated.
P10NarrativeCommandBus publishes and records commands only.
P10NarrativeCommandAdapter translates commands into requests.
Old-system execution policy stays outside Phase10.
```

Final CH01 anchor ids:

```text
P10_CH01_Anchor_Order
P10_CH01_Anchor_Wheel
P10_CH01_Anchor_Glaze
P10_CH01_Anchor_Kiln
P10_CH01_Anchor_CourtyardCenter
P10_CH01_Anchor_Gate
```

Final anchor fields:

```text
AnchorId
Position
```

Allowed old-system read points for later P10B tasks:

```text
Phase3 order completion facts by neutral OrderId.
Phase3 score or quality threshold facts by neutral integer Score.
Phase6 workstation runtime Transform.position values for anchor mapping.
Phase8 runtime/session state only as neutral mode or availability facts.
Existing scene object names or Transform positions only for validation.
```

Read point constraints:

```text
Read points are not implementation permission by themselves.
Each later task must explicitly name the read point it uses.
Read points must not mutate old systems.
Read values must cross into Phase10 only as neutral facts, commands, or anchor mappings.
```

## P10B-02 Gameplay Fact Adapter

### Objective

Implement the first formal path for neutral gameplay facts to become Phase10 narrative events.

### Requirements

```text
Use P10NarrativeGameplayEventAdapter or a formal successor under Phase10.
Convert neutral facts into P10NarrativeEvent values.
Publish through P10NarrativeEventBus only inside Phase10 bridge code.
Do not make Phase3, Phase6, or Phase8 reference P10NarrativeEventBus.
Do not read production item data into Phase10.
```

### Acceptance Criteria

```text
Adapter converts required CH01 facts: game started, order completed, score threshold, dialogue started, state entered.
Adapter does not call P10NarrativeStateMachine directly.
Old systems do not directly reference P10NarrativeEventBus.
No Phase3 / Phase6 / Phase8 types are referenced by Phase10 unless the task explicitly approves a separate boundary assembly.
Unity compile has no C# Compile Error.
```

### Dependencies

```text
P10B-01 PASS.
```

### Boundaries

```text
Modify only approved Phase10 bridge code unless a human-approved old-system hook task is created.
No formal command execution.
No gameplay pause, resume, or input lock.
```

### Deliverables

```text
Gameplay fact adapter implementation or update.
Adapter validation notes.
Verification Report.
```

## P10B-03 Narrative Command Adapter

### Objective

Implement the first formal path for Phase10 narrative commands to request old-system behavior through an adapter boundary.

### Requirements

```text
Subscribe to or receive P10NarrativeCommand values through Phase10 command flow.
Translate commands into neutral requests for old-system execution.
Keep execution policy outside Phase10.
Do not directly call Phase8 runtime mode APIs from Phase10 unless explicitly approved by a later bridge execution task.
Do not directly call Phase3 GameManager APIs or Phase6 input, movement, workstation, or interaction APIs from Phase10.
```

### Acceptance Criteria

```text
NarrativePauseGameplay and NarrativeResumeGameplay are represented as adapter-mediated requests.
Input lock and dialogue commands are represented as adapter-mediated requests.
P10NarrativeCommandBus remains a publisher/history boundary, not an executor.
No real gameplay pause, resume, or lock is implemented unless explicitly included in a later approved slice.
Unity compile has no C# Compile Error.
```

### Dependencies

```text
P10B-02 PASS.
```

### Boundaries

```text
Phase10 adapter code only by default.
No old-system mutation unless a later task explicitly allows it.
No scene integration.
```

### Deliverables

```text
Narrative command adapter implementation or update.
Command request validation notes.
Verification Report.
```

## P10B-04 Anchor Mapping Validation

### Objective

Validate neutral anchor mapping between Phase10 overlay anchors and Phase6 workstation runtime positions.

### Requirements

```text
Use P10NarrativeAnchorMapper as the mapping point.
Represent mappings as neutral anchor id plus Vector3.
Read or sample Phase6 workstation Transform.position only in the approved bridge validation context.
Do not modify Phase6 workstation objects.
Do not require exact spatial alignment beyond the task's stated tolerance.
```

### Acceptance Criteria

```text
Anchor ids exist for Order, Wheel, Glaze, Kiln, CourtyardCenter, and Gate.
Mapper can store and return runtime positions by neutral anchor id.
Validation records whether placeholder overlay anchors can be aligned in the bridge layer.
No Phase6 workstation object is modified.
Unity compile has no C# Compile Error.
```

### Dependencies

```text
P10B-03 PASS.
```

### Boundaries

```text
No production scene modification unless explicitly approved.
No workstation behavior modification.
No gameplay item data ownership.
```

### Deliverables

```text
Anchor mapping validation notes.
Any approved Phase10 mapping data or script updates.
Verification Report.
```

## P10B-05 End-to-End Bridge Slice

### Objective

Run one narrow gameplay-to-narrative-to-command slice through the bridge boundary.

### Requirements

```text
Choose one CH01 flow slice, such as GameStarted -> Prologue or ORDER_001 completion -> next node.
Route gameplay fact through adapter.
Publish narrative event through Phase10 event bus.
Let state machine update narrative state and node.
Publish any narrative command through command bus and command adapter as a request.
Keep old-system execution minimal and explicitly bounded.
```

### Acceptance Criteria

```text
The selected slice reaches the expected P10NarrativeState and CurrentNodeId.
Old systems still do not directly call P10NarrativeEventBus or P10NarrativeStateMachine.
Narrative commands are adapter-mediated.
No unrelated old systems are modified.
Play Mode has no blocking Console Error.
```

### Dependencies

```text
P10B-04 PASS.
```

### Boundaries

```text
Only the selected slice is in scope.
No broad bridge rollout.
No full chapter production integration.
No unrelated scene or prefab changes.
```

### Deliverables

```text
End-to-end bridge slice implementation or validation.
Play Mode evidence.
Verification Report.
```

## P10B-06 Bridge Validation

### Objective

Validate that the bridge integration is bounded, testable, and ready for later production expansion.

### Requirements

```text
Verify P10B-00 through P10B-05 acceptance.
Verify no forbidden direct references exist.
Verify compile and Play Mode results.
Verify bridge docs match implementation.
Verify old-system modifications, if any, are limited to approved files and tasks.
```

### Acceptance Criteria

```text
P10B-00 through P10B-05 are PASS.
No old system directly references P10NarrativeEventBus.
Gameplay facts enter Phase10 through adapter.
Narrative commands exit Phase10 through adapter.
AnchorMapper does not modify Phase6 workstation objects.
Unity compile has no C# Compile Error.
Play Mode has no blocking Console Error.
Forbidden path check is clean except for explicitly approved bridge-slice files.
```

### Dependencies

```text
P10B-05 PASS.
```

### Boundaries

```text
Validation only unless a blocking issue requires returning to the failed P10B task.
No new bridge feature during validation.
No expansion beyond the approved bridge slice.
```

### Deliverables

```text
Final P10B Verification Report.
Forbidden path check.
Unity compile and Play Mode summary.
Bridge readiness decision.
```

## Merge And Branch Safety Rules

```text
Develop only on Gong unless a task explicitly says otherwise.
Do not switch to main for development.
Do not modify Yu branch.
One small task equals one commit after human PASS.
Before commit, return git diff --name-status --cached.
Do not stage obj/, Library/, Temp/, P10_04_PlayMode.log, or unrelated files.
Do not stage e10 narrative workspace contracts... or other accidental local files.
Do not proceed to the next P10B task without human PASS.
FAIL means fix only the failed task point.
BLOCKED means stop and report blocker, evidence, and suggested next action.
```

## Validation Strategy

Every P10B task must include:

```text
git status --short
git diff --name-status
git diff --name-status --cached
Forbidden path check for Assets/Phase3/, Assets/Phase6/, Assets/Phase8/, Assets/Scenes/, ProjectSettings/
Phase10 scope check when the task is Phase10-only
Unity compile check when scripts change
Play Mode check when runtime or scene behavior changes
Console error summary
```

Reference checks:

```text
No direct old-system reference to P10NarrativeEventBus.
No direct old-system reference to P10NarrativeStateMachine.
No Phase10 compile-time reference to Phase3 / Phase6 / Phase8 unless a later approved bridge architecture explicitly introduces a boundary assembly.
No production gameplay item data duplicated into Phase10.
No state enum expansion for order-internal nodes.
```

The final bridge validation must prove the bridge direction remains adapter-mediated in both directions before any broader production narrative integration begins.
