# P10 State Machine Contract

## Goal

The Phase10 narrative layer starts with a reusable narrative state machine.

The state machine owns chapter-level narrative progression, current node tracking, played node history, narrative flags, transition checks, and snapshot save/load data. It does not play UI, spawn scene objects, pause gameplay, inspect workstations, or directly call Phase3, Phase6, or Phase8 systems.

## Chapter State

`P10NarrativeState` represents only large chapter phases.

Initial Chapter 1 enum:

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

Do not add order-internal nodes such as `Order001Accept`, `Order001Pass`, `Order004Climax`, or `RewardPlayed` to this enum.

## Chapter 1 State Flow

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

## State, Node, Played Set, And Flags

The state machine must keep these concepts separate:

```text
ChapterState     Current large chapter phase.
CurrentNodeId    Current narrative node string ID.
PlayedNodeSet    Set of node IDs already played.
NarrativeFlags   Small set of cross-node or cross-state facts.
```

Example:

```text
ChapterState = Order001
CurrentNodeId = P10_CH01_NODE_ORDER_001_ACCEPT
PlayedNodeSet = {
  P10_CH01_NODE_PROLOGUE_01,
  P10_CH01_NODE_TUTORIAL_01
}
NarrativeFlags = {
  ORDER_001_RETRY_AVAILABLE = true
}
```

`NarrativeFlags` must not replace node flow. Order internals should be represented by `CurrentNodeId`, event conditions, and `PlayedNodeSet` whenever possible.

For Chapter 1, `NarrativeFlags` should normally stay at or below seven keys, matching the number of chapter states. If more flags are needed, the task document must explain why the fact cannot be represented by node flow, played nodes, or chapter state.

## Required Snapshot Data

`P10NarrativeSnapshot` must contain at least:

```csharp
public int SnapshotVersion;
public P10NarrativeState ChapterState;
public string CurrentNodeId;
public List<string> PlayedNodeIds;
public Dictionary<string, bool> NarrativeFlags;
```

## Required API

`P10NarrativeStateMachine` must reserve these methods:

```csharp
void StartChapter(string chapterId);
void HandleEvent(P10NarrativeEvent evt);
bool CanTransition(P10NarrativeState targetState);
bool AdvanceToState(P10NarrativeState targetState);
bool AdvanceToNode(string nodeId);
bool ReplayNode(string nodeId);
void ResetChapter();
P10NarrativeSnapshot SaveSnapshot();
void LoadSnapshot(P10NarrativeSnapshot snapshot);
string GetCurrentNode();
P10NarrativeState GetCurrentState();
```

## Responsibility Boundary

Allowed:

```text
Track ChapterState.
Track CurrentNodeId.
Track PlayedNodeSet.
Track NarrativeFlags.
Validate transitions.
Save and load snapshots.
React to P10NarrativeEvent.
```

Forbidden:

```text
Open or render Dialogue UI.
Spawn or destroy scene objects.
Pause, resume, or lock gameplay directly.
Reference Phase3, Phase6, or Phase8 types.
Own production gameplay item data.
Define audio systems or playback logic.
```

## Acceptance

- `P10NarrativeState` contains only chapter-level states.
- Order-internal flow is expressed by node IDs, not enum members.
- `P10NarrativeSnapshot` includes `SnapshotVersion`.
- `P10NarrativeStateMachine` owns transition rules.
- UI, triggers, prop views, and debug panels do not bypass the state machine to mutate chapter state.
