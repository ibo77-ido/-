# P10E NPC Empty Object / Anchor Layout Design

## Goal

Design the Phase10E Chapter 1 NPC empty-object and anchor layout before any scene or prefab implementation.

This layout gives later tasks a stable structure for:

- NPC interaction distance checks
- dialogue UI, portrait, or bubble positioning
- quest marker positioning
- semantic workstation and scene anchors
- later Bridge Adapter, AnchorMapper, Yu, and Phase6 integration

This pass is docs-only. It does not modify scenes, prefabs, ScriptableObject assets, runtime code, or existing gameplay systems.

## Problem Statement

P10E-01 freezes the distance rule and P10E-02 validates the gate decision with fake transforms. The next risk is layout ambiguity:

- NPC distance origins could drift if they are tied to visual mesh centers.
- Dialogue UI placement could accidentally depend on the same point used for range checks.
- Quest markers need a stable point that can move independently from interaction distance.
- Later bridge work needs semantic anchors without directly moving or reading Phase6 scene objects too early.

The layout must keep Phase10 authoring clear while preserving the boundary around Phase3, Phase6, Phase8, and Yu.

## Layout Principles

- Use named empty GameObjects as semantic anchors.
- Keep each anchor single-purpose.
- Make manual editor placement explicit and predictable.
- Do not make NPC body transforms responsible for every UI and interaction concern.
- Do not bind directly to Phase6 workstation or player objects in this phase.
- Preserve P10E-01 distance-first behavior by keeping `P10_InteractPoint` authoritative for range checks.
- Keep all later runtime bridge input neutral: positions are provided to Phase10 rather than Phase10 reaching into old systems.

## Root Object Hierarchy

Recommended Chapter 1 hierarchy:

```text
P10_CH01_NarrativeRoot
  P10_CH01_NPCRoot
    P10_NPC_XuLaoBo
      P10_InteractPoint
      P10_DialogueAnchor
      P10_QuestMarkerAnchor
    P10_NPC_ZhouZhangGui
      P10_InteractPoint
      P10_DialogueAnchor
      P10_QuestMarkerAnchor
    P10_NPC_ChenShuYuan
      P10_InteractPoint
      P10_DialogueAnchor
      P10_QuestMarkerAnchor
    P10_NPC_LuKe
      P10_InteractPoint
      P10_DialogueAnchor
      P10_QuestMarkerAnchor

  P10_CH01_TriggerRoot
    P10_Trigger_PrologueStart
    P10_Trigger_TutorialStart
    P10_Trigger_Order001Accept
    P10_Trigger_Order003Accept
    P10_Trigger_Order004Accept
    P10_Trigger_ChapterEnding

  P10_CH01_AnchorRoot
    P10_Anchor_OrderStation
    P10_Anchor_WheelStation
    P10_Anchor_GlazeStation
    P10_Anchor_KilnStation
    P10_Anchor_CourtyardCenter
    P10_Anchor_Gate
```

## NPC Object Structure

Each narrative NPC root should have exactly three Phase10 semantic child anchors:

- `P10_InteractPoint`
- `P10_DialogueAnchor`
- `P10_QuestMarkerAnchor`

The NPC root represents the narrative NPC identity. The child anchors represent separate interaction, UI, and marker concerns.

Recommended NPC ids for gate evaluation remain:

- `XuLaoBo`
- `ZhouZhangGui`
- `ChenShuYuan`
- `LuKe`

The object names can remain human-readable and semantic, while the runtime gate should continue using stable NPC ids rather than localized display names.

## Interact Point Rules

`P10_InteractPoint` is the distance detection center point.

Rules:

- Place it near the NPC feet or practical interaction center.
- Use it for P10E distance checks.
- Do not use dialogue bubble offsets or quest marker offsets as the distance origin.
- Moving `P10_DialogueAnchor` or `P10_QuestMarkerAnchor` must not change interaction distance.
- The initial valid range remains `interactionDistance = 2.5`.

This preserves P10E-01 and P10E-02 behavior:

- distance `<= 2.5` can pass the range gate
- distance `> 2.5` returns `TooFar`

## Dialogue Anchor Rules

`P10_DialogueAnchor` is the reference point for dialogue-facing presentation.

Potential later uses:

- dialogue box target position
- portrait target point
- world-space bubble position
- camera or look-at hint if a later task needs it

Rules:

- Designers may manually move `P10_DialogueAnchor`.
- Moving it must not move the NPC body.
- Moving it must not affect interaction range.
- It should usually sit near head or upper-body height, but exact placement is an editor-authored decision.
- Runtime dialogue entry must still be gated by `P10_InteractPoint` and quest state before using this anchor.

## Quest Marker Anchor Rules

`P10_QuestMarkerAnchor` is the reference point for task markers.

Potential later uses:

- exclamation mark
- current objective marker
- selected NPC highlight
- minimap or world marker projection

Rules:

- Designers may manually move `P10_QuestMarkerAnchor`.
- Moving it must not move the NPC body.
- Moving it must not affect interaction distance.
- It can be placed above the NPC, at shoulder height, or at a clearer visible point depending on marker style.
- It should not be used as the distance-check origin.

## Trigger Object Plan

`P10_Trigger_*` objects are narrative trigger placeholders for later implementation.

Responsibilities:

- `P10_Trigger_PrologueStart`: future prologue start trigger.
- `P10_Trigger_TutorialStart`: future tutorial start trigger.
- `P10_Trigger_Order001Accept`: future Order001 dialogue entry trigger.
- `P10_Trigger_Order003Accept`: future Order003 dialogue entry trigger.
- `P10_Trigger_Order004Accept`: future Order004 dialogue entry trigger.
- `P10_Trigger_ChapterEnding`: future chapter ending trigger.

Current boundary:

- This design does not add trigger components.
- This design does not modify task progression rules.
- This design does not wire trigger objects to Phase3, Phase6, Phase8, or Yu.

## Workstation / Anchor Mapping Plan

`P10_Anchor_*` objects represent semantic scene positions used by narrative and later bridge mapping.

Responsibilities:

- `P10_Anchor_OrderStation`: order handoff or order board semantic location.
- `P10_Anchor_WheelStation`: wheel or shaping station semantic location.
- `P10_Anchor_GlazeStation`: glazing station semantic location.
- `P10_Anchor_KilnStation`: kiln or firing station semantic location.
- `P10_Anchor_CourtyardCenter`: courtyard center semantic location.
- `P10_Anchor_Gate`: gate or scene transition semantic location.

These anchors are not Phase6 workstation objects. They are Phase10 semantic placeholders that a later AnchorMapper can align, bind, or map to Phase6-provided positions.

## Editor Manual Placement Rules

- The user can move `P10_DialogueAnchor` without affecting the NPC body.
- The user can move `P10_QuestMarkerAnchor` without affecting interaction distance.
- `P10_InteractPoint` should remain near the NPC feet or practical interaction center.
- Future bridge code should not move the NPC body as a first choice; it should move, bind, or update Phase10 anchors.
- Do not directly align to Phase6 workstations until the AnchorMapper stage.
- Do not modify Phase6 scene layout to satisfy Phase10 authoring in this phase.
- Keep object names exact so later validators can check the hierarchy deterministically.

## Runtime Bridge Boundary

Current phase boundaries:

- Do not read Phase6 scene objects.
- Do not read Phase6 player or movement objects.
- Do not read Phase3 order data.
- Do not depend on Yu branch implementation.
- Do not modify Phase3, Phase6, Phase8, `Assets/Scenes`, or `ProjectSettings`.

Later allowed direction:

- Bridge Adapter can inject player position.
- AnchorMapper can inject workstation positions.
- AnchorMapper can inject NPC or anchor positions.
- Phase10 consumes neutral position data and stable ids.
- Phase10 does not own the old-system player, workstation, or NPC implementation.

## Yu / Phase6 Bridge Notes

Yu and Phase6 should be treated as later providers of neutral bridge data, not as direct dependencies for this design pass.

Recommended future flow:

1. Phase6 or Yu-facing bridge gathers old-system player and workstation positions.
2. Bridge Adapter converts them into neutral Phase10 data.
3. AnchorMapper applies or registers positions against `P10_Anchor_*` and NPC anchor ids.
4. P10E interaction gates consume the player position and `P10_InteractPoint`.

Forbidden for this pass:

- direct compile-time dependency on Phase6 player types
- direct lookup of Phase6 workstation scene objects
- direct Phase3 order asset reads
- runtime Yu branch dependency

## Validation Strategy

Later P10E-04 or implementation-stage validators should verify:

- each Chapter 1 NPC has exactly three child anchors
- each NPC has `P10_InteractPoint`
- each NPC has `P10_DialogueAnchor`
- each NPC has `P10_QuestMarkerAnchor`
- anchor names match exactly
- `P10_InteractPoint` can be used by the distance gate
- `P10_DialogueAnchor` can be resolved for UI positioning
- `P10_QuestMarkerAnchor` can be resolved for task marker positioning
- trigger object names match the planned list
- `P10_Anchor_*` objects exist under `P10_CH01_AnchorRoot`
- validation does not modify Phase3 or Phase6 scenes

The validation should run against Phase10-owned objects only.

## Implementation Recommendation

Recommended next steps:

- If continuing docs-first: `P10E-04 Quest State Gated NPC Dialogue Design`.
- If starting layout implementation: `P10E-03A NPC Anchor Layout Implementation in Phase10 Overlay Scene`.
- If adding any new runtime helper first: `P10E-R1 Runtime Helper File Organization`.

For implementation, create anchors in the Phase10 overlay scene only after explicit implementation approval. Do not implement this layout as part of P10E-03.

## Technical Debt Note

P10E-02 added `P10NpcInteractionGateResult` and `P10NpcInteractionGateEvaluator` as a temporary Phase10-only helper inside `P10NarrativeManager.cs`.

Current decision:

- Do not split it during P10E-03.
- Do not add more interaction helper code to `P10NarrativeManager.cs`.
- Before adding any further runtime helper, run `P10E-R1 Runtime Helper File Organization`.
- `P10E-R1` should move helpers into independent files and maintain the generated/explicit compile configuration so `dotnet build Phase10_Narrative.csproj` remains valid.

This avoids allowing `P10NarrativeManager.cs` to continue growing as a catch-all runtime file.

## Required User Decisions

No additional user decision is required for this docs-only pass.

Decisions deferred to later tasks:

- whether to implement the hierarchy in the Phase10 overlay scene
- exact manual positions of each NPC anchor
- exact visual style of quest markers
- whether dialogue UI uses screen-space projection, world-space bubbles, or another presentation mode
- AnchorMapper timing and bridge payload shape

## Recommended Next Step

Default recommended next step:

- `P10E-04 Quest State Gated NPC Dialogue Design`

Alternative if the team wants scene layout work next:

- `P10E-03A NPC Anchor Layout Implementation in Phase10 Overlay Scene`

Required before any further runtime helper work:

- `P10E-R1 Runtime Helper File Organization`
