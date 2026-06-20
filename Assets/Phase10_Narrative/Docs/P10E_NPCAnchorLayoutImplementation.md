# P10E NPC Anchor Layout Implementation

## Purpose

P10E-03A implements the MVP empty-object and anchor layout planned in P10E-03 inside the Phase10 Chapter 1 overlay scene.

This pass creates spatial authoring structure only. It does not add runtime interaction logic, distance gate behavior, quest state gates, order system changes, or formal Phase3 / Phase6 / Yu integration.

## Implemented Hierarchy

Implemented in:

- `Assets/Phase10_Narrative/Scenes/P10_CH01_NarrativeOverlay.unity`

Required Phase10 hierarchy:

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

Existing legacy Phase10 overlay objects are preserved. The P10E-03A objects are added as exact-name semantic anchors for later validation and integration work.

## MVP Interaction Decision

For the tutorial MVP:

- this task creates spatial anchors only
- out-of-range NPC click behavior is not implemented
- out-of-range NPC clicks should currently have no new feedback
- no `distance too far` polish is added in this pass
- no quest state gate is added in this pass

Future polish can add feedback after the MVP spatial foundation is stable.

## Anchor Placement

Initial positions are approximate and intentionally easy to adjust.

Per NPC:

- `P10_InteractPoint`: at the NPC root, intended as the future distance-check center
- `P10_DialogueAnchor`: above the NPC root for future dialogue box, portrait, or bubble placement
- `P10_QuestMarkerAnchor`: higher than the dialogue anchor for future quest marker placement

The three child anchors are not placed at the exact same local position.

## Manual Editing Rules

Designers may manually move:

- `P10_DialogueAnchor` to tune dialogue UI or bubble placement
- `P10_QuestMarkerAnchor` to tune marker readability

Designers should keep:

- `P10_InteractPoint` near the NPC feet or practical interaction center
- object names unchanged
- Phase10 anchors independent from direct Phase6 workstation alignment until AnchorMapper work begins

Moving dialogue or quest marker anchors must not be treated as changing interaction distance.

## Boundaries

P10E-03A does not:

- modify runtime code
- modify ScriptableObject assets
- modify prefabs
- add runtime interaction scripts
- add distance gate runtime behavior
- add quest state gate runtime behavior
- change the dialogue system
- change the order system
- read Phase3 order data
- connect formal Phase3 gameplay
- connect formal Phase6 gameplay
- depend on Yu implementation
- handle Phase6 Workshop_TestScene / NavMesh dirty or deleted state

## Validator

Validator:

- `Assets/Phase10_Narrative/Scripts/Editor/P10E03AAnchorLayoutValidator.cs`

Setup method:

- `Phase10_Narrative.P10E03AAnchorLayoutValidator.SetupP10E03AAnchorLayout`

Validation method:

- `Phase10_Narrative.P10E03AAnchorLayoutValidator.RunP10E03AAnchorLayoutValidation`

Required pass text:

- `P10E-03A NPC Anchor Layout validation passed.`

Validation covers:

- root hierarchy exists
- four semantic NPC roots exist
- each semantic NPC has the three required anchors
- six trigger placeholders exist
- six scene anchors exist
- planned objects have exact names
- NPC child anchors do not overlap at identical local positions
- planned hierarchy does not use Phase3 or Phase6 components

## Next Step

Recommended next task:

- `P10E-04 MVP Tutorial Interaction Gate`

That task can decide how the tutorial MVP consumes these anchors without implementing the full later mainline quest-state gate.
