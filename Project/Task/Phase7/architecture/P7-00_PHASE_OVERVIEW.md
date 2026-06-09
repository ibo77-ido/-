# P7-00 Phase Overview

## Goal

Phase7 connects the completed Phase3 gameplay loop to the Phase6 2.5D whitebox space.

The target vertical slice is:

```text
Order -> Shape -> Glaze -> Firing -> Result -> Next Order
```

Phase7 is successful when this loop can be completed in the Phase6 workshop scene through movement, workstation interaction, UI routing, and stable state transitions.

## Priority

1. Gameplay loop integration
2. UI and movement state gates
3. Workstation routing
4. Area and interaction stability
5. Repeatable validation

## Required Boundaries

Keep:
- `LogicRoot / ArtRoot` separation
- `Phase6GameManager`
- `AreaTrigger`
- `Workstation + InteractionPoint`
- `ScaleManager`
- Existing Phase3 calculator and system logic
- Whitebox or test UI as a transition layer

Do not do in Phase7:
- Formal art replacement
- Large-scale Phase3 scoring rewrite
- New economy systems
- Full firing defect system
- NPC behavior
- Procedural scene generation

## Scene Target

Main scene:

```text
Assets/Phase6/Scenes/Workshop_TestScene.unity
```

Phase7 may connect Phase3 systems or UI into this scene, but it must not move Phase6 logic objects away from their current `LogicRoot / ArtRoot` structure.

## Exit Criteria

Phase7 can close when:
- A single order can be completed from Order to Result.
- Three consecutive orders can be completed without state corruption.
- Movement, interaction, UI open, and UI close remain stable.
- Area enter and exit events remain stable.
- Play Mode has no blocking console errors.
- `STATE.md` and `DECISIONS.md` are updated.

## Task Order

1. `P7-01` Scene Binding Audit
2. `P7-02` State Gate
3. `P7-03` UI Routing
4. `P7-03A` Gameplay Bridge
5. `P7-04` Gameplay Loop
6. `P7-05` Area Stability
7. `P7-06` Validation
