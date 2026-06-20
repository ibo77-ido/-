# P10 CH01 Narrative Plan

## Goal

Chapter 1, `重燃窑火`, is implemented first as an independent Phase10 narrative layer.

The first implementation pass prioritizes reusable narrative infrastructure and a standalone overlay validation scene. Full gameplay bridge integration is deferred.

## Source

Story source:

```text
Project/StoryMain.md
```

The current Phase10 runtime must not depend on reading `Project/StoryMain.md` at runtime. This document records the Chapter 1 structure and serves as the local Phase10 planning source for later dialogue node data.

## Chapter Theme

Chapter 1 focuses on reopening the kiln, restoring trust, and proving that the player can carry the family craft forward.

The first pass may use placeholder dialogue. Final prose can be refined later after the state machine, node flow, and debug playback path are verified.

## Characters

```text
P10_CH01_NPC_001_XuLaoBo_Placeholder
Role: Elder craftsman and early mentor.

P10_CH01_NPC_002_ZhouZhangGui_Placeholder
Role: Merchant contact and early order giver.

P10_CH01_NPC_003_ChenShuYuan_Placeholder
Role: Scholar/customer who raises quality expectations.

P10_CH01_NPC_004_LuKe_Placeholder
Role: Visitor or later-stage observer for world flavor and future setup.
```

## Chapter State Flow

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

## Narrative Nodes

```text
P10_CH01_NODE_PROLOGUE_01
P10_CH01_NODE_TUTORIAL_01
P10_CH01_NODE_ORDER_001_ACCEPT
P10_CH01_NODE_ORDER_001_PASS
P10_CH01_NODE_ORDER_001_FAIL
P10_CH01_NODE_ORDER_003_ACCEPT
P10_CH01_NODE_ORDER_003_PASS
P10_CH01_NODE_ORDER_003_FAIL
P10_CH01_NODE_ORDER_004_ACCEPT
P10_CH01_NODE_ORDER_004_PASS_NORMAL
P10_CH01_NODE_ORDER_004_FAIL
P10_CH01_NODE_ORDER_004_CLIMAX
P10_CH01_NODE_CHAPTER_ENDING
```

Order-internal progress stays in node IDs and is not promoted into `P10NarrativeState`.

## Order Mapping

```text
ORDER_001
  Accept: P10_CH01_NODE_ORDER_001_ACCEPT
  Pass:   P10_CH01_NODE_ORDER_001_PASS
  Fail:   P10_CH01_NODE_ORDER_001_FAIL

ORDER_003
  Accept: P10_CH01_NODE_ORDER_003_ACCEPT
  Pass:   P10_CH01_NODE_ORDER_003_PASS
  Fail:   P10_CH01_NODE_ORDER_003_FAIL

ORDER_004
  Accept:     P10_CH01_NODE_ORDER_004_ACCEPT
  NormalEnd:  P10_CH01_NODE_ORDER_004_PASS_NORMAL
  Fail:       P10_CH01_NODE_ORDER_004_FAIL
  Climax:     P10_CH01_NODE_ORDER_004_CLIMAX
```

Phase10 may react to `OrderId` values in narrative events, but it does not own Phase3 order gameplay data.

## Narrative Props

```text
P10_PROP_001_FatherLedger
P10_PROP_002_OldKilnTool
P10_PROP_003_BrokenBowl
P10_PROP_004_AncientOrder
P10_PROP_005_FamilyLetter
```

These props are narrative objects only. They cannot be used as production gameplay items.

## Placeholder Anchors

Chapter 1 overlay scene should later include approximate anchors:

```text
P10_CH01_Anchor_Order
P10_CH01_Anchor_Wheel
P10_CH01_Anchor_Glaze
P10_CH01_Anchor_Kiln
P10_CH01_Anchor_CourtyardCenter
P10_CH01_Anchor_Gate
```

Current anchors are placeholders. Exact alignment with Phase6 workstations is deferred to bridge integration through `P10NarrativeAnchorMapper`.

## Task Order

```text
P10-00 Workspace And Contracts
P10-01 Narrative Foundation
P10-02 CH01 Content Data
P10-03 CH01 Overlay Scene
P10-04 Debug Playback Validation
P10-05 Bridge Preview Contract
P10-06 Phase10 Validation
```

## Current Task Acceptance

- `Assets/Phase10_Narrative/**` workspace exists.
- Contract documents exist under `Assets/Phase10_Narrative/Docs/`.
- Task order from P10-00 to P10-06 is recorded.
- Chapter 1 state flow and node IDs are recorded.
- Narrative props are listed as story-only props.
- No old project files are modified.

## P10-02 Content Data Pass

P10-02 converts the Chapter 1 plan into data assets only. It does not create scene objects, prefabs, UI, triggers, bridge adapters, audio systems, or gameplay item data.

StoryMain source remains:

```text
Project/StoryMain.md
```

The source document is recorded for planning and authoring traceability. Phase10 runtime content data does not read `Project/StoryMain.md` directly.

### Data Assets

Chapter flow:

```text
Assets/Phase10_Narrative/ScriptableObjects/Chapters/P10_CH01_ChapterFlow.asset
```

Character placeholders:

```text
P10_CH01_NPC_001_XuLaoBo_Placeholder
P10_CH01_NPC_002_ZhouZhangGui_Placeholder
P10_CH01_NPC_003_ChenShuYuan_Placeholder
P10_CH01_NPC_004_LuKe_Placeholder
```

Dialogue nodes:

```text
P10_CH01_NODE_PROLOGUE_01
P10_CH01_NODE_TUTORIAL_01
P10_CH01_NODE_ORDER_001_ACCEPT
P10_CH01_NODE_ORDER_001_PASS
P10_CH01_NODE_ORDER_001_FAIL
P10_CH01_NODE_ORDER_003_ACCEPT
P10_CH01_NODE_ORDER_003_PASS
P10_CH01_NODE_ORDER_003_FAIL
P10_CH01_NODE_ORDER_004_ACCEPT
P10_CH01_NODE_ORDER_004_PASS_NORMAL
P10_CH01_NODE_ORDER_004_FAIL
P10_CH01_NODE_ORDER_004_CLIMAX
P10_CH01_NODE_CHAPTER_ENDING
```

Narrative props:

```text
P10_PROP_001_FatherLedger
P10_PROP_002_OldKilnTool
P10_PROP_003_BrokenBowl
P10_PROP_004_AncientOrder
P10_PROP_005_FamilyLetter
```

### Role Relationships

```text
Xu Lao Bo -> mentor, memory keeper, and recovery guide.
Zhou Zhang Gui -> merchant contact who tests restored trust through ORDER_001.
Chen Shu Yuan -> scholar customer who raises expectations through ORDER_003.
Lu Ke -> visitor and observer who frames later stakes through ORDER_004.
```

### State And Node Flow

Chapter-level state remains:

```text
None -> Prologue -> Tutorial -> Order001 -> Order003 -> Order004 -> Ending -> Completed
```

Order-internal flow remains node-based:

```text
ORDER_001: ACCEPT -> PASS / FAIL
ORDER_003: ACCEPT -> PASS / FAIL
ORDER_004: ACCEPT -> PASS_NORMAL / FAIL / CLIMAX
```

`P10NarrativeState` is not expanded for order accept, pass, fail, normal completion, or climax beats.

### Production Data Boundary

Phase10 content data may mention craft practice in placeholder prose, but it does not own or duplicate production gameplay data such as glaze IDs, clay IDs, fuel IDs, recipe materials, kiln resources, shape configuration, scoring data, reward data, or Phase3 order data.
