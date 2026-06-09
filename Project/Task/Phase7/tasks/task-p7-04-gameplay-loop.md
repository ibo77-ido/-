# Task P7-04 Gameplay Loop

## Goal

Complete the first playable Phase7 production loop inside the Phase6 workshop scene.

## Dependency

- P7-03A PASS

## Loop

```text
Order -> Shape -> Glaze -> Firing -> Result -> Next Order
```

## Scope

Use existing Phase3 gameplay systems and calculators:
- `OrderManager`
- `ShapeSystem`
- `GlazeSystem`
- `FiringSystem`
- `ResultSystem`
- Existing Phase3 UI controllers where possible

Allowed changes:
- Add a Phase7 bridge or router script if needed.
- Add scene references that connect Phase6 workstations to Phase3 systems.
- Add minimal temporary UI flow glue.

Do not:
- Rewrite Phase3 calculators.
- Replace Phase3 scoring rules.
- Add formal art.
- Add new economy systems.

## Recommended Data Flow

`Workstation.Interact`
-> `TestUIRouter` or Phase7 router
-> target gameplay panel
-> existing Phase3 system/controller
-> result state
-> return to Phase6 `Playing`

## UnityMCP Steps

1. Confirm P7-03A bridge evidence is recorded.
2. Use existing Phase3 logic as the source of truth.
3. Bind UI buttons so each completed step can advance to the next station or result.
4. Run Play Mode.
5. Complete one order loop manually or through deterministic scripted calls.
6. Complete three consecutive loops.

## Acceptance

- One full order can reach the result screen.
- Three consecutive orders can complete without data corruption.
- Result data changes according to existing Phase3 logic.
- Returning to the workshop restores `Playing`.
- Player can move to the next station after closing each panel.
- No blocked state remains after each panel close.
- No blocking console errors.

## Rollback

Rollback bridge scripts, scene references, and temporary UI flow glue. Do not modify Phase3 calculator behavior during rollback.
