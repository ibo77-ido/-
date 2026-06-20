# P10D ORDER_001 Quest Slice

## Goal

P10D-04 narrows the first playable quest slice to ORDER_001 accept and pass.

The slice verifies that Phase10 can show the ORDER_001 accept dialogue from an approved story entry, then wait for a neutral `OrderCompleted(ORDER_001)` fact before showing the pass dialogue.

## Scope

Included:

```text
P10_CH01_NODE_ORDER_001_ACCEPT
P10_CH01_NODE_ORDER_001_PASS
OrderCompleted(ORDER_001)
```

Excluded:

```text
OrderFailed event type
ORDER_001 fail runtime branch
ORDER_003 implementation expansion
ORDER_004 implementation expansion
Phase3 order data ownership
Scene placement changes
```

Failure and retry behavior remains documented as `TBD` and is not part of P10D-04.

## Runtime Rule

ORDER_001 pass is valid only after the narrative manager is already at:

```text
State: Order001
Node: P10_CH01_NODE_ORDER_001_ACCEPT
```

Then this neutral gameplay fact can advance the story:

```text
OrderCompleted(ORDER_001)
        -> P10_CH01_NODE_ORDER_001_PASS
```

The ORDER_001 accept dialogue button must not skip directly to the pass node.

## Implementation

Modified files:

```text
Assets/Phase10_Narrative/Scripts/Runtime/P10NarrativeManager.cs
Assets/Phase10_Narrative/Scripts/UI/P10DialogueController.cs
```

`P10NarrativeManager` now exposes a checked dialogue advance path:

```text
TryAdvanceDialogueNode(currentNodeId, targetNodeId)
```

This lets dialogue UI request progression without bypassing Phase10 sequence rules.

`HandleOrderCompleted("ORDER_001")` now requires the current node to be `P10_CH01_NODE_ORDER_001_ACCEPT` before entering `P10_CH01_NODE_ORDER_001_PASS`.

`P10DialogueController.AdvanceDialogue()` now uses the checked manager path. If a next node is not currently allowed, the dialogue surface closes and waits for the correct gameplay fact or later approved trigger.

## Acceptance Path

Expected P10D-04 validation path:

```text
StartPrologue
  -> P10_CH01_NODE_PROLOGUE_01
Dialogue Next
  -> P10_CH01_NODE_TUTORIAL_01
Dialogue Next
  -> P10_CH01_NODE_ORDER_001_ACCEPT
Close accept dialogue
OrderCompleted(ORDER_001)
  -> P10_CH01_NODE_ORDER_001_PASS
```

## Boundary

P10D-04 does not modify:

```text
Assets/Phase3/**
Assets/Phase6/**
Assets/Phase8/**
Assets/Scenes/**
ProjectSettings/**
```

It does not add an `OrderFailed` event type.

## Serialized References Changed

```text
NONE
```

## Scene Mutation

```text
NONE
```

## Acceptance

P10D-04 passes when:

- ORDER_001 accept dialogue cannot skip directly to pass through the dialogue button
- `OrderCompleted(ORDER_001)` advances to ORDER_001 pass only from the accept node
- out-of-order `OrderCompleted(ORDER_001)` does not mutate narrative state
- `dotnet build Phase10_Narrative.csproj` passes with zero errors
