# P10C CH01 Playable Flow Map

## Goal

P10C-01 maps the existing CH01 narrative data into a playable MVP flow.

This document is planning only. It does not implement runtime logic, modify scenes, modify prefabs, or change ProjectSettings.

## Source Constraints

Existing Phase10 source documents:

```text
Assets/Phase10_Narrative/Docs/P10_CH01_NarrativePlan.md
Assets/Phase10_Narrative/Docs/P10C_PlayableStoryMVPScope.md
```

Story source reference remains:

```text
Project/StoryMain.md
```

P10C-01 does not read or import `Project/StoryMain.md` at runtime.

Unknown final prose, exact trigger copy, exact map coordinates, and final production integration details are marked `TBD`. This document does not invent final plot content beyond existing CH01 nodes and contracts.

## MVP Flow Summary

Playable MVP path:

```text
Start CH01
  -> Prologue
  -> Tutorial
  -> ORDER_001 accept
  -> ORDER_001 pass
  -> ORDER_003 accept
  -> ORDER_003 pass
  -> ORDER_004 accept
  -> ORDER_004 climax or normal pass
  -> Chapter ending
  -> Completed
```

Failure nodes exist for ORDER_001, ORDER_003, and ORDER_004, but the first playable MVP should validate the pass path first. Failure path handling remains `TBD` unless a later P10C task explicitly includes it.

## Flow Table

| Beat ID | Story Purpose | Phase10 State | Phase10 NodeId | Required NPC / Prop / Trigger | Player Action | Expected Result | Later Placement Need |
|---|---|---|---|---|---|---|---|
| P10C_CH01_BEAT_001_START | Start Chapter 1 playable story. Final opening prose is TBD. | Prologue | P10_CH01_NODE_PROLOGUE_01 | Trigger: P10_CH01_TRIGGER_StartPrologue; Anchor: P10_CH01_Anchor_CourtyardCenter; NPC: P10_CH01_NPC_001_XuLaoBo_Placeholder | Enter start trigger or press debug/start interact. | State enters Prologue; current node becomes prologue node. | Place start trigger near courtyard center in later approved placement task. |
| P10C_CH01_BEAT_002_TUTORIAL | Introduce basic narrative/tutorial beat. Exact instruction text is TBD. | Tutorial | P10_CH01_NODE_TUTORIAL_01 | NPC: P10_CH01_NPC_001_XuLaoBo_Placeholder; Anchor: P10_CH01_Anchor_Wheel; Trigger: P10_CH01_TRIGGER_Tutorial | Click NPC or interact at tutorial anchor. | State advances to Tutorial; tutorial node is shown. | Place mentor NPC near wheel/tutorial anchor. |
| P10C_CH01_BEAT_003_ORDER_001_ACCEPT | Present first trust/order beat. Final dialogue is TBD. | Order001 | P10_CH01_NODE_ORDER_001_ACCEPT | NPC: P10_CH01_NPC_002_ZhouZhangGui_Placeholder; Anchor: P10_CH01_Anchor_Order; Trigger: P10_CH01_TRIGGER_Order001Accept | Click merchant NPC or dialogue accept button. | ORDER_001 narrative accept node is active. | Place merchant NPC and order trigger near order anchor. |
| P10C_CH01_BEAT_004_ORDER_001_PASS | Reflect successful ORDER_001 completion. | Order001 | P10_CH01_NODE_ORDER_001_PASS | NPC: P10_CH01_NPC_002_ZhouZhangGui_Placeholder; Trigger: P10_CH01_TRIGGER_Order001Pass | Complete ORDER_001 gameplay fact or use MVP validation action. | ORDER_001 pass node plays; flow can proceed toward ORDER_003. | Later task must bind neutral OrderCompleted(ORDER_001) to this beat. |
| P10C_CH01_BEAT_005_ORDER_001_FAIL_OPTIONAL | Optional failure/retry beat. Exact MVP inclusion is TBD. | Order001 | P10_CH01_NODE_ORDER_001_FAIL | NPC: P10_CH01_NPC_002_ZhouZhangGui_Placeholder; Trigger: TBD | ORDER_001 failure fact or debug validation action. | ORDER_001 fail node may play if failure path is included. | Placement and retry rules TBD; not required for first pass path. |
| P10C_CH01_BEAT_006_ORDER_003_ACCEPT | Present higher expectation/order beat. Final dialogue is TBD. | Order003 | P10_CH01_NODE_ORDER_003_ACCEPT | NPC: P10_CH01_NPC_003_ChenShuYuan_Placeholder; Anchor: P10_CH01_Anchor_Glaze; Trigger: P10_CH01_TRIGGER_Order003Accept | Click scholar NPC or dialogue continue button. | State advances to Order003; accept node is shown. | Place scholar NPC near glaze or inspection anchor. |
| P10C_CH01_BEAT_007_ORDER_003_PASS | Reflect successful ORDER_003 completion. | Order003 | P10_CH01_NODE_ORDER_003_PASS | NPC: P10_CH01_NPC_003_ChenShuYuan_Placeholder; Trigger: P10_CH01_TRIGGER_Order003Pass | Complete ORDER_003 gameplay fact or use MVP validation action. | ORDER_003 pass node plays; flow can proceed toward ORDER_004. | Later task must bind neutral OrderCompleted(ORDER_003) to this beat. |
| P10C_CH01_BEAT_008_ORDER_003_FAIL_OPTIONAL | Optional failure/retry beat. Exact MVP inclusion is TBD. | Order003 | P10_CH01_NODE_ORDER_003_FAIL | NPC: P10_CH01_NPC_003_ChenShuYuan_Placeholder; Trigger: TBD | ORDER_003 failure fact or debug validation action. | ORDER_003 fail node may play if failure path is included. | Placement and retry rules TBD; not required for first pass path. |
| P10C_CH01_BEAT_009_ORDER_004_ACCEPT | Present late-stage stakes/order beat. Final dialogue is TBD. | Order004 | P10_CH01_NODE_ORDER_004_ACCEPT | NPC: P10_CH01_NPC_004_LuKe_Placeholder; Prop: P10_PROP_004_AncientOrder; Anchor: P10_CH01_Anchor_Gate; Trigger: P10_CH01_TRIGGER_Order004Accept | Click visitor NPC, inspect ancient order prop, or dialogue accept button. | State advances to Order004; accept node is shown. | Place visitor NPC and AncientOrder prop near gate/order approach. |
| P10C_CH01_BEAT_010_ORDER_004_PASS_NORMAL | Complete ORDER_004 with normal ending path. | Order004 | P10_CH01_NODE_ORDER_004_PASS_NORMAL | NPC: P10_CH01_NPC_004_LuKe_Placeholder; Trigger: P10_CH01_TRIGGER_Order004PassNormal | Complete ORDER_004 normal gameplay fact or MVP validation action. | ORDER_004 normal pass node plays. | Later task must choose normal or climax path conditions. |
| P10C_CH01_BEAT_011_ORDER_004_CLIMAX | Complete ORDER_004 with high quality/climax path. | Order004 | P10_CH01_NODE_ORDER_004_CLIMAX | NPC: P10_CH01_NPC_004_LuKe_Placeholder; Anchor: P10_CH01_Anchor_Kiln; Trigger: P10_CH01_TRIGGER_Order004Climax | Reach ScoreThresholdReached(95) or MVP validation action. | ORDER_004 climax node plays. | Later task must bind score threshold fact to this beat. |
| P10C_CH01_BEAT_012_ORDER_004_FAIL_OPTIONAL | Optional failure/retry beat. Exact MVP inclusion is TBD. | Order004 | P10_CH01_NODE_ORDER_004_FAIL | NPC: P10_CH01_NPC_004_LuKe_Placeholder; Trigger: TBD | ORDER_004 failure fact or debug validation action. | ORDER_004 fail node may play if failure path is included. | Placement and retry rules TBD; not required for first pass path. |
| P10C_CH01_BEAT_013_ENDING | Conclude Chapter 1. Final ending prose is TBD. | Ending | P10_CH01_NODE_CHAPTER_ENDING | NPCs: TBD; Props: P10_PROP_001_FatherLedger and P10_PROP_005_FamilyLetter optional; Anchor: P10_CH01_Anchor_CourtyardCenter | Dialogue continue button after ORDER_004 pass/climax. | Ending node plays and chapter can complete. | Place optional closing props near courtyard center if later approved. |
| P10C_CH01_BEAT_014_COMPLETED | Mark CH01 playable MVP complete. | Completed | P10_CH01_NODE_CHAPTER_ENDING | Trigger: none required; UI/debug completion marker optional | Finish ending dialogue. | State reaches Completed; current node remains chapter ending. | P10C-06 must verify end-to-end completion. |

## Placeholder Requirements

### NPC Placeholders

```text
P10_CH01_NPC_001_XuLaoBo_Placeholder
P10_CH01_NPC_002_ZhouZhangGui_Placeholder
P10_CH01_NPC_003_ChenShuYuan_Placeholder
P10_CH01_NPC_004_LuKe_Placeholder
```

NPC final dialogue text, animation, facing, and final staging remain `TBD`.

### Narrative Prop Placeholders

```text
P10_PROP_001_FatherLedger
P10_PROP_002_OldKilnTool
P10_PROP_003_BrokenBowl
P10_PROP_004_AncientOrder
P10_PROP_005_FamilyLetter
```

Props remain narrative-only. They must not become Phase3 production item data.

### Trigger Placeholders For P10C-02

```text
P10_CH01_TRIGGER_StartPrologue
P10_CH01_TRIGGER_Tutorial
P10_CH01_TRIGGER_Order001Accept
P10_CH01_TRIGGER_Order001Pass
P10_CH01_TRIGGER_Order003Accept
P10_CH01_TRIGGER_Order003Pass
P10_CH01_TRIGGER_Order004Accept
P10_CH01_TRIGGER_Order004PassNormal
P10_CH01_TRIGGER_Order004Climax
P10_CH01_TRIGGER_ChapterEnding
```

Optional failure triggers remain `TBD`:

```text
P10_CH01_TRIGGER_Order001Fail
P10_CH01_TRIGGER_Order003Fail
P10_CH01_TRIGGER_Order004Fail
```

### Anchor Needs

```text
P10_CH01_Anchor_Order
P10_CH01_Anchor_Wheel
P10_CH01_Anchor_Glaze
P10_CH01_Anchor_Kiln
P10_CH01_Anchor_CourtyardCenter
P10_CH01_Anchor_Gate
```

Exact Phase6 map coordinates are `TBD`.

## Player Trigger Methods

Allowed MVP trigger methods:

```text
proximity trigger
click or interact on placeholder NPC
click or inspect placeholder prop
dialogue continue or accept button
neutral OrderCompleted gameplay fact
neutral ScoreThresholdReached gameplay fact
debug validation action for acceptance testing
```

P10C-01 does not implement these trigger methods.

## Phase6 Map Position Needs

P10C-01 records placement needs only. It does not modify Phase6 scenes.

Later approved tasks may request placeholder placement near:

```text
Courtyard center: start and ending beats
Wheel area: tutorial beat
Order counter or merchant area: ORDER_001 beats
Glaze or inspection area: ORDER_003 beats
Kiln area: ORDER_004 climax beat
Gate area: ORDER_004 accept / visitor beat
```

Any Phase6 scene edit must be explicitly approved by task and must name exact file paths.

## P10C-02 Placement Input

P10C-02 should place or validate placeholders for:

```text
4 NPC placeholders
5 narrative prop placeholders where needed
required trigger placeholders for pass-path MVP
6 CH01 anchor placeholders
optional fail-path trigger placeholders only if explicitly approved
```

P10C-02 must not connect final art, audio, production item data, or broad old-system behavior.

## Acceptance For CH01 Playable MVP Flow

The MVP flow is acceptable when a player can proceed:

```text
Prologue
  -> Tutorial
  -> ORDER_001 accept
  -> ORDER_001 pass
  -> ORDER_003 accept
  -> ORDER_003 pass
  -> ORDER_004 accept
  -> ORDER_004 pass normal or climax
  -> Chapter ending
  -> Completed
```

The first acceptance path may use placeholders and debug validation actions. Final prose, exact coordinates, final NPC blocking, failure/retry behavior, and production-quality presentation remain `TBD`.

## P10C-01 Declarations

```text
Serialized References Changed: NONE
Scene Mutation: NONE
Code Changes: NONE
Prefab Changes: NONE
ProjectSettings Changes: NONE
```

P10C-01 does not enter P10C-02.
