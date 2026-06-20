# P10E Narrative Gameplay Integration Plan

## Phase10E Goal

Phase10E defines the next planning stage after Phase10D final-candidate readiness. Its goal is to connect the independent Phase10 narrative layer to real gameplay flow in controlled, bounded slices without turning Phase10 into an owner of Phase3 gameplay data, Phase6 world logic, or Phase8 runtime policy.

The immediate planning target is to define how Chapter 1 narrative should move from Phase10-only validation toward gameplay-aware interaction:

- NPC dialogue should require proximity instead of allowing remote click entry.
- NPC interaction availability should be gated by main quest state.
- Order acceptance, current objective display, and order completion response should feel closer to the real game loop.
- Scene anchor usage should become clearer and more structured.
- Later Yu branch, Phase3, and Phase6 bridges should stay explicitly bounded.

## Scope

Phase10E planning covers:

- docs-only planning for gameplay-aware narrative integration
- NPC interaction distance-gate design
- NPC interaction layout and anchor layout planning
- main quest state gate planning for Chapter 1 NPC progression
- current objective tracker planning
- order acceptance / completion bridge planning
- gameplay event adapter planning
- save/load planning for the new integration points
- validation planning for later implementation tasks
- bridge-boundary planning for Yu / Phase3 / Phase6

## Out of Scope

Phase10E-00 does not implement runtime integration.

Out of scope for this task:

- runtime code changes
- ScriptableObject changes
- prefab changes
- scene changes
- any modification under `Assets/Phase3/**`
- any modification under `Assets/Phase6/**`
- any modification under `Assets/Phase8/**`
- any modification under `Assets/Scenes/**`
- any modification under `ProjectSettings/**`
- any handling of `Workshop_TestScene` / `NavMesh` dirty or deleted state
- any cleanup of `Logs/**`, `obj/**`, `Temp/**`, or other local outputs
- any staging, commit, or push

## Relationship to Original Phase10 Plan

Phase10E does not exceed the original Phase10 plan in direction, but it is beyond the already accepted Phase10D scope.

The original Phase10 planning already reserved:

- `Future Bridge Direction`
- `Phase10 Narrative Bridge Integration`
- adapter-mediated gameplay-to-narrative facts
- adapter-mediated narrative-to-gameplay commands
- runtime anchor mapping

Phase10E should therefore be treated as:

- a later follow-up stage under the original bridge direction
- not part of Phase10D
- not permission to perform broad old-system integration in one pass

Phase10E may plan bridge behavior, but implementation must be split into separate tasks with explicit boundaries and acceptance.

## Relationship to Phase10D

Phase10D established the independent Phase10 narrative slice and brought it to final-candidate readiness:

- Chinese CH01 dialogue assets
- finalized dialogue UI behavior
- Dialogue Log and snapshot
- Current Order panel with crafting hints
- click-to-advance
- scrollbar drag interaction
- validator matrix PASS
- selective stage list draft

Phase10E starts after that point and must not be merged back into Phase10D scope.

Phase10D answer:

- Can Phase10 tell the Chapter 1 story in a self-contained way

Phase10E answer:

- How should that self-contained Phase10 narrative layer connect to actual gameplay interaction flow without breaking ownership boundaries

## Integration Principles

- Phase10 remains a narrative layer, not a gameplay-system owner.
- Phase10 must not own Phase3 order data, recipe data, scoring rules, reward data, or workstation behavior.
- Phase10 must not directly depend on Phase6 movement, interaction, or workstation implementations.
- Gameplay facts should reach Phase10 only through a formal adapter boundary.
- Narrative commands should leave Phase10 only through a formal adapter boundary.
- Distance-gated NPC interaction should be validated first in a Phase10-only harness before any formal old-system hookup.
- Scene anchors should remain Phase10-owned placeholders until a later runtime bridge maps them to real positions.
- Each bridge concern should be implemented as a separate task with its own acceptance and validation.

## NPC Interaction Distance Gate

Phase10E should introduce a distance-gated NPC interaction rule:

- clicking an NPC from outside the interaction radius must not open dialogue
- dialogue entry should require the player to be within range of the NPC interaction point
- `interactionDistance` should start with a recommended value of `2.0` or `2.5`

Recommended staging:

- `P10E-01` defines the distance-gate contract and interaction states
- `P10E-02` validates the rule using a Phase10-only test player transform
- later bridge work replaces the test player transform with a bridge-provided player position source

Boundary rules:

- do not hard-wire the player transform to Phase6 in Phase10E-00
- Phase10E-01 / E-02 may use a temporary Phase10-only transform provider
- formal player position should come from a bridge adapter later

Expected behavior:

- within range: NPC click may open allowed dialogue
- outside range: NPC click does nothing or shows blocked feedback, but does not enter dialogue

## NPC / Empty Object Layout Plan

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

Layout purpose:

- `P10_InteractPoint` is the distance-check source
- `P10_DialogueAnchor` is the camera / bubble / focus reference
- `P10_QuestMarkerAnchor` is the tracker / marker reference
- trigger root separates story-state-driven triggers from NPC entry points
- anchor root keeps world-interest points neutral and bridge-ready

## Dialogue Anchor / Quest Marker Anchor Plan

Each Chapter 1 NPC should expose at least two anchor types:

- `P10_DialogueAnchor`
- `P10_QuestMarkerAnchor`

Recommended responsibilities:

- dialogue anchor positions dialogue-facing visual focus near the NPC
- quest marker anchor positions a current-objective marker above or near the NPC
- station anchors under `P10_CH01_AnchorRoot` identify non-NPC destinations such as order, wheel, glaze, kiln, gate, and courtyard center

These anchors should remain Phase10-owned semantic points. Later bridge tasks may map them against real world references, but Phase10E-00 should not assume exact Phase6 coordinates.

## Main Quest State Gate

NPC interactivity should be controlled by main quest state instead of leaving every story NPC equally clickable at all times.

Required Chapter 1 gate mapping:

- `Prologue`: XuLaoBo
- `Order001`: ZhouZhangGui
- `Order003`: ChenShuYuan
- `Order004`: LuKe
- `Ending`: XuLaoBo

Meaning:

- the active main quest state determines which NPC is the expected progression contact
- non-current NPC behavior must be defined intentionally instead of falling through to unrestricted full dialogue

Open behavior options for non-current NPCs:

- normal ambient talk
- not interactable
- interactable but responds with `quest not available yet`

The final choice is still a user decision and should be documented before implementation.

## Current Objective Tracker

Phase10E should add a clearer objective surface above the current order panel layer.

Recommended tracker responsibilities:

- show the current high-level Chapter 1 objective
- identify the relevant NPC or station
- update when the player accepts an order
- update when the player should craft, deliver, or return to an NPC
- clear or switch when the narrative state advances

Recommended content model:

- short objective title
- short instruction line
- optional target label
- optional marker target id

The tracker should remain narrative-facing. It should describe goals, not own gameplay rule evaluation.

## Order Acceptance / Completion Flow

Phase10E should define a more gameplay-aligned order loop:

1. NPC proposes the order.
2. Player confirms order acceptance.
3. Current order panel updates.
4. Player completes crafting through gameplay.
5. Phase10 receives an `OrderCompleted` event.
6. Current order panel removes the completed order.
7. Narrative advances to the next story state.

Important ownership rule:

- Phase10 must not own Phase3 order data

Recommended bridge meaning:

- Phase10 owns the narrative framing and state response
- gameplay systems own the actual order definition, crafting evaluation, and completion truth
- the bridge passes only neutral facts such as `OrderAccepted`, `OrderCompleted`, or `OrderFailed`

## Gameplay Event Adapter Plan

Phase10E should reuse the original adapter direction rather than inventing direct calls.

Recommended event flow:

```text
Gameplay system
  -> neutral gameplay fact
  -> Phase10 bridge adapter
  -> Phase10 event bus / narrative manager
  -> state advance / UI response
```

Phase10E event topics should at least plan for:

- player proximity available
- NPC interaction requested
- order accepted
- order completed
- order failed
- objective target available

Adapter rules:

- no direct old-system reference to Phase10 buses from gameplay systems
- no direct Phase10 ownership of gameplay item or order payloads
- only neutral facts cross the boundary

## Phase3 / Phase6 / Yu Bridge Boundaries

Phase10E must keep the bridge boundaries explicit.

Phase3 boundary:

- Phase3 remains owner of order data, recipes, scoring, results, and completion truth
- Phase10 may react to neutral order facts, but may not copy or own Phase3 production logic

Phase6 boundary:

- Phase6 remains owner of player movement, world interaction, and workstation placement
- Phase10E may plan distance-gate behavior, but formal player position and workstation hookup must come through bridge adapters

Yu bridge boundary:

- Yu branch bridge work must stay contract-based and task-split
- Phase10E may define the future contract edge, but should not broaden into direct Yu implementation in this planning task

Practical rule:

- Phase10E planning may describe the future bridge
- later execution must stay separated into bounded tasks like adapter contract, distance validation, anchor layout, quest gate, and Yu branch contract

## Save/Load Considerations

Phase10E should preserve save/load clarity before implementation begins.

Recommended save/load considerations:

- save the current main quest narrative state
- save current objective tracker state
- save accepted-order narrative context without duplicating Phase3 order ownership
- save NPC interaction gate state if it is derived from narrative progress
- restore marker target and current NPC gate after load

Rules:

- Phase10 save data should remain narrative-facing
- gameplay completion truth should still come from gameplay-owned systems or bridge facts
- avoid duplicating full gameplay order payloads into Phase10 snapshot data

## Validation Strategy

Validation should be split by task stage.

Recommended validation ladder:

- docs review for Phase10E-00 and contract tasks
- Phase10-only harness validation for distance gate and state gating
- bounded vertical slice validation for accepted-order to completed-order narrative advance
- manual acceptance for feel, clarity, and blocked-interaction behavior

Key validation questions:

- can out-of-range NPC clicks be blocked reliably
- can current-state NPC gating be verified deterministically
- can objective tracker updates be verified without formal Phase3 ownership
- can order completion advance the correct next story state through a neutral event path
- can save/load restore the right gate and objective surface
- do all implementations preserve Phase3 / Phase6 / Yu ownership boundaries

## Proposed Task Breakdown

- `P10E-00 Narrative Gameplay Integration Plan`
- `P10E-01 NPC Interaction Distance Gate Design`
- `P10E-02 Phase10-only Interaction Test Harness`
- `P10E-03 NPC Empty Object / Anchor Layout`
- `P10E-04 Quest State Gated NPC Dialogue`
- `P10E-05 Order Acceptance Flow Cleanup`
- `P10E-06 Current Objective Tracker`
- `P10E-07 Gameplay Event Adapter Contract`
- `P10E-08 Yu Branch Bridge Contract`
- `P10E-09 Integrated Vertical Slice Validation`
- `P10E-10 Manual Acceptance`

Recommended sequence notes:

- `P10E-01` and `P10E-02` should happen before any real bridge hookup
- `P10E-03` should stabilize semantic layout before quest markers and anchor-dependent UI
- `P10E-04` should define who is interactable when
- `P10E-05` and `P10E-06` should align the player-facing flow
- `P10E-07` should freeze the adapter boundary before formal bridge implementation
- `P10E-08` should keep Yu branch work isolated
- `P10E-09` and `P10E-10` should validate the integrated slice only after prior tasks pass

## Risks

- Phase10E can easily sprawl into real integration work if task boundaries are not enforced.
- Distance-gate behavior may become coupled to Phase6 too early if player position sourcing is not adapter-mediated.
- Quest gating can become unclear if non-current NPC fallback behavior is not decided early.
- Objective tracker wording can drift from actual gameplay truth if neutral event ownership is not respected.
- Order acceptance / completion flow can duplicate gameplay data unless the bridge only carries neutral facts.
- Anchor plans may be mistaken for exact production layout; they should remain semantic until a later mapping step.
- Yu branch planning can become too broad if its contract edge is not separated from implementation.

## Required User Decisions

- For non-current mainline NPCs, should the default behavior be ambient talk, blocked interaction, or `quest not available yet`.
- Should Phase10E objective tracking remain text-only first, or should marker logic be planned as part of the initial implementation slice.
- Should `interactionDistance` start at `2.0` or `2.5`.
- Should Phase10E-05 include explicit player confirmation UI for order acceptance, or treat existing dialogue confirmation as sufficient.
- Should Yu bridge planning stay as a contract-only task in `P10E-08`, or should it later expand into a separate implementation track outside Phase10E.

## Recommended Next Step

Start with bounded planning-to-design tasks, not gameplay hookup.

Recommended next execution order:

1. `P10E-01 NPC Interaction Distance Gate Design`
2. `P10E-02 Phase10-only Interaction Test Harness`
3. `P10E-03 NPC Empty Object / Anchor Layout`
4. `P10E-04 Quest State Gated NPC Dialogue`

This keeps the next slice Phase10-only while preparing the system for later bridge tasks without mixing the work back into Phase10D or directly modifying Phase3 / Phase6.
