# P10E NPC Interaction Distance Gate Design

## Goal

Define the Phase10E NPC interaction distance-gate rule for Chapter 1 narrative flow.

The design goal is simple:

- the player must approach an NPC interaction point before dialogue can begin
- remote clicking must not directly open dialogue
- the gate must remain Phase10-bounded during early validation
- formal player-position hookup must be deferred to a later bridge adapter stage

This task is docs-only and does not implement runtime behavior.

## Problem Statement

Phase10D validated a self-contained narrative slice, but it still allows a planning gap:

- NPC dialogue entry is not yet formally bound to physical proximity
- the player should not be able to click a far-away NPC and immediately enter dialogue
- NPC interaction availability also needs to cooperate with current main quest progression

Without a clear distance-gate design, later implementation risks:

- unrealistic remote interaction
- unclear click feedback
- inconsistent NPC gate behavior
- early accidental coupling to Phase6 player or world systems

## User-Facing Rule

Phase10E freezes the following user-facing interaction rule:

- a player may start dialogue only when the clicked NPC is the current mainline NPC and the player is within interaction range of that NPC's `P10_InteractPoint`
- a player outside the allowed range must not enter dialogue
- a player clicking a non-current mainline NPC while in range must see `任务尚未到达`

Recommended priority order:

- distance check first
- quest-state gate second

That means:

- outside range: show `距离太远`
- inside range but not the current mainline NPC: show `任务尚未到达`

## Interaction Distance

Frozen initial interaction distance:

- `interactionDistance = 2.5`

Rule:

- if `distance(playerPosition, npcInteractPointPosition) <= 2.5`, the interaction is in range
- if `distance(playerPosition, npcInteractPointPosition) > 2.5`, the interaction is out of range

This value is an initial design constant for later validation, not proof that final production tuning is complete.

## Required Empty Objects

Each Chapter 1 narrative NPC should expose the same semantic empty-object structure:

```text
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
```

Responsibilities:

- `P10_InteractPoint`: distance-check origin
- `P10_DialogueAnchor`: dialogue-facing anchor
- `P10_QuestMarkerAnchor`: objective-marker anchor

## Player Position Source

The player position source is intentionally split by phase.

For early Phase10E validation:

- use a `Phase10-only test player transform`

For later formal integration:

- player position should come from a `Bridge Adapter` or Yu bridge-facing contract

Frozen boundary rules:

- do not directly read Phase3 gameplay state
- do not directly read Phase6 player movement state
- do not modify Phase6 player objects
- do not require a compile-time dependency on Phase6 player types

## NPC Interaction Point

`P10_InteractPoint` is the authoritative position used for range checks.

Design rule:

- distance checks should use the player position and the NPC's `P10_InteractPoint`
- the check should not depend on raw visual mesh center, portrait state, or dialogue UI position

Reason:

- this keeps the range gate stable even if later NPC visuals or marker offsets change

## Current Quest State Gate

The distance gate is necessary but not sufficient.

Dialogue should open only when:

- the NPC is the current mainline NPC
- the player is within `2.5`

Current Chapter 1 intended gate mapping:

- `Prologue` -> `XuLaoBo`
- `Order001` -> `ZhouZhangGui`
- `Order003` -> `ChenShuYuan`
- `Order004` -> `LuKe`
- `Ending` -> `XuLaoBo`

Boundary rule:

- the gate must not directly read Phase3 or Phase6 gameplay state
- the gate should consume Phase10-owned narrative state or later neutral bridge facts only

## Click Behavior

Frozen recommended behavior:

- current mainline NPC + in range: enter the corresponding dialogue
- current mainline NPC + out of range: do not enter dialogue, show `距离太远`
- non-current NPC + in range: do not enter mainline dialogue, show `任务尚未到达`
- non-current NPC + out of range: do not enter dialogue, show `距离太远`

This adopts the recommended priority:

- distance-first feedback

## Out-of-Range Behavior

If the clicked NPC interaction point is farther than `2.5` from the player:

- do not enter dialogue
- do not advance narrative state
- do not open order acceptance flow
- show `距离太远`

Recommended consistency rule:

- all out-of-range NPC clicks should use the same blocked message

This keeps the first player-facing rule easy to learn.

## Non-Current NPC Behavior

Default frozen decision:

- if the player is in range but the clicked NPC is not the current mainline NPC, show `任务尚未到达`

Behavior rule:

- do not open the mainline dialogue for that NPC
- do not advance the state machine
- do not silently ignore the click

This keeps Chapter 1 progression understandable without requiring ambient side dialogue in the first implementation slice.

## UI Feedback

Initial UI feedback recommendations:

- out of range message: `距离太远`
- wrong NPC for current mainline state: `任务尚未到达`

Feedback goals:

- short
- deterministic
- easy to localize
- easy to validate in a Phase10-only harness

The initial tracker plan remains text-only. Marker or world-space indicator behavior is not required by this design task.

## Bridge Boundary

This design must preserve the later bridge contract.

Allowed design direction:

- Phase10 checks distance against a Phase10-provided or bridge-provided player position
- later bridge code may provide player position through a neutral adapter

Forbidden direction:

- direct reads of Phase3 gameplay state
- direct reads of Phase6 player or NPC runtime objects in this design stage
- direct modification of Phase6 player, NPC, or scene layout

Formal integration target:

- `Bridge Adapter / Yu` provides player position
- Phase10 consumes a neutral position input
- Phase10 does not own the old-system player implementation

## Validation Strategy

Validation should remain Phase10-only for the first implementation pass.

Required validation coverage:

- Phase10-only test player transform
- mock NPC interact points
- in-range click tests
- out-of-range click tests
- current mainline NPC gate tests
- non-current NPC gate tests
- verification that the setup does not require Phase3 or Phase6 hookup

Recommended harness approach:

- create a Phase10-only test player transform
- place mock NPC roots with mock `P10_InteractPoint`
- drive the current Phase10 narrative state manually
- verify feedback and dialogue-entry eligibility by state and distance

This validation explicitly avoids:

- Phase3 order hookup
- Phase6 movement hookup
- old-system player controller hookup

## Proposed Next Implementation Task

Recommended next task:

- `P10E-02 Phase10-only Interaction Test Harness`

Reason:

- the distance-gate design should be validated in a bounded Phase10-only harness before any formal runtime bridge or scene integration task begins

## Risks

- `2.5` may need later tuning once real scene scale and movement feel are tested
- if distance is not checked first, user feedback becomes inconsistent
- if the gate reads old-system state directly, the bridge boundary will be broken too early
- if `P10_InteractPoint` placement is inconsistent across NPCs, interaction feel will drift
- if blocked clicks provide no feedback, users may think interaction is broken

## Required User Decisions

No additional user decision is required for this design pass because the task already adopted the default decisions:

- non-current NPC behavior: `任务尚未到达`
- `interactionDistance = 2.5`
- objective tracking: text-only first
- order acceptance: explicit confirmation UI later
- Yu bridge: contract first, implementation later
