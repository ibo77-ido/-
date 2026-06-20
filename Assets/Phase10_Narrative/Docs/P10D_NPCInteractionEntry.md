# P10D NPC Interaction Entry

## Goal

Make Phase10 NPC placeholders act as story entry points.

P10D-03 adds a Phase10-owned NPC interaction component that can open a narrative node through `P10NarrativeSceneBindingHub`, then let `P10DialogueController` show the line.

## Implementation

Modified file:

```text
Assets/Phase10_Narrative/Scripts/Runtime/P10NarrativeManager.cs
```

The new `P10NarrativeNpcInteraction`:

- requires a Collider
- supports trigger-based interaction
- supports explicit `Interact()`
- resolves `bindingHub` at runtime if not assigned
- marks one-shot interactions when configured
- maps NPC placeholder ids to first-pass CH01 node ids

The `P10NarrativeNpcInteractionBinder`:

- looks for the four NPC placeholder names in the active scene
- adds a trigger collider if missing
- adds `P10NarrativeNpcInteraction` if missing
- wires each NPC to the Phase10 binding hub at runtime

## NPC Mapping

Default mapping:

```text
XuLaoBo -> P10_CH01_NODE_PROLOGUE_01
ZhouZhangGui -> P10_CH01_NODE_ORDER_001_ACCEPT
ChenShuYuan -> P10_CH01_NODE_ORDER_003_ACCEPT
LuKe -> P10_CH01_NODE_ORDER_004_ACCEPT
```

## Boundary

P10D-03 does not modify:

```text
Assets/Phase3/**
Assets/Phase6/**
Assets/Phase8/**
Assets/Scenes/**
ProjectSettings/**
```

It does not require direct compile-time reference from old systems to `P10NarrativeStateMachine`.

## Runtime Flow

```text
Player entry or explicit Interact()
        -> P10NarrativeNpcInteraction
        -> P10NarrativeSceneBindingHub
        -> P10NarrativeManager
        -> P10DialogueController
        -> visible dialogue text
```

## Serialized References Changed

Expected:

```text
[NEW SerializeField] npcId on P10NarrativeNpcInteraction
[NEW SerializeField] triggerId on P10NarrativeNpcInteraction
[NEW SerializeField] targetNodeId on P10NarrativeNpcInteraction
[NEW SerializeField] bindingHub on P10NarrativeNpcInteraction
[NEW SerializeField] interactOnce on P10NarrativeNpcInteraction
```

## Scene Mutation

```text
NONE
```

No scene file is modified by P10D-03.

## Acceptance

P10D-03 passes when:

- NPC placeholder prefabs or runtime scene objects can trigger narrative entry
- the interaction component can run without old-system changes
- visible dialogue is shown by the Phase10 dialogue surface
- `dotnet build Phase10_Narrative.csproj` passes with zero errors
