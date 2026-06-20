# P10D Runtime Mount Contract

## Goal

Define how the Phase10 narrative layer is mounted in the game runtime without touching Phase3, Phase6, Phase8, or ProjectSettings.

This contract keeps the narrative layer self-owned inside `Assets/Phase10_Narrative/**` and treats in-game deployment as a Phase10 concern.

## Mount Target

Primary narrative scene:

```text
Assets/Phase10_Narrative/Scenes/P10_CH01_NarrativeOverlay.unity
```

This scene is the default runtime mount target for Phase10 story playback.

## Existing Mount Roots

The overlay scene already provides these runtime roots:

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

The mount contract must preserve these roles:

- `NarrativeRoot` is the top-level narrative container.
- `NarrativeManager` owns Phase10 chapter flow.
- `DialogueUIRoot` owns visible dialogue surface.
- `TriggerRoot` owns story entry triggers.
- `NPCRoot` owns story-facing NPC placeholders.
- `PropRoot` owns narrative props only.
- `AnchorRoot` owns narrative placement anchors.
- `EventBusRoot` owns narrative event plumbing.

## Mount Order

Recommended runtime mount order:

```text
1. Load the overlay scene.
2. Resolve NarrativeManager.
3. Resolve EventBus and CommandBus.
4. Bind Dialogue UI.
5. Bind NPC / Trigger / Prop runtime objects.
6. Activate chapter start flow.
```

## Runtime Data Flow

```text
Game entry
  -> load P10_CH01_NarrativeOverlay
  -> resolve P10NarrativeManager
  -> initialize event and command buses
  -> bind dialogue controller
  -> bind narrative triggers and placeholder NPCs
  -> begin narrative playback
```

## Mount Policy

P10D runtime mount must:

- stay inside Phase10-owned files unless a later task explicitly approves a wider scope
- support standalone narrative playback
- support later additive deployment only by separate approval
- keep old gameplay systems outside direct compile-time dependence on `P10NarrativeStateMachine`
- keep narrative commands adapter-mediated

## Scene Policy

This contract does not approve editing `Workshop_TestScene.unity` or any Phase3 / Phase6 scene.

If a later task wants an additive in-game mount into old scenes, it must explicitly name:

```text
allowed scene file
allowed objects
allowed serialized references
validation evidence
```

## UI Policy

The runtime mount must expose dialogue UI as a visible narrative surface.

The first approved implementation may use placeholder UI assets or existing Phase10 UI prefabs, but the mount contract does not require final art.

## Prefab Policy

The following Phase10 prefab families are valid mount inputs:

```text
Phase10_Narrative/Prefabs/NPC
Phase10_Narrative/Prefabs/Props
Phase10_Narrative/Prefabs/Triggers
Phase10_Narrative/Prefabs/UI
```

The mount contract does not authorize production gameplay items from Phase3.

## Startup and Shutdown

Startup must be able to:

- enter the narrative scene
- initialize narrative state
- show dialogue UI
- accept trigger or NPC entry

Shutdown must be able to:

- stop the current narrative playback
- preserve narrative state if the task requires it
- return without mutating old gameplay systems

## Acceptance

P10D-01 passes when:

- this document exists under `Assets/Phase10_Narrative/Docs/`
- the runtime mount target is explicit
- the mount root roles are explicit
- the mount order is explicit
- the scene policy is explicit
- the UI policy is explicit
- no runtime code is modified
- no scene file is modified
- no prefab, ScriptableObject, or ProjectSettings file is modified
- no git add, commit, or push is performed
