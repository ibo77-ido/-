# Task P7-06 Validation

## Goal

Verify that Phase7 is ready to close and move to the next stage.

## Dependency

- P7-05 PASS

## Required Checks

- Compile check
- Missing Script check
- Play Mode smoke test
- Movement test
- Interaction test
- UI open and close test
- Gameplay bridge test
- Area enter and exit test
- One full order loop
- Three consecutive order loops
- Console error check

## UnityMCP Steps

1. Use `read_console(types=["error","warning"])` before validation.
2. Use `manage_editor(action="play")`.
3. Move from spawn to each key station.
4. Press `E` near each workstation.
5. Open and close the routed panel for each workstation.
6. Verify each panel can reach its target Phase3 gameplay system.
7. Complete one full loop:
   - Order
   - Shape
   - Glaze
   - Firing
   - Result
   - Next Order
8. Complete three consecutive loops.
9. Use `read_console(types=["error"])`.
10. Use `manage_editor(action="stop")`.
11. Capture a screenshot with `manage_camera(action="screenshot")` if visual evidence is needed.

## Evidence To Record

Record in `STATE.md`:
- Scene name
- Console error count
- Movement result
- Workstation routing result
- Gameplay bridge result
- One-loop result
- Three-loop result
- Any known residual risks

Record in `DECISIONS.md`:
- Any Phase7 integration strategy decisions
- Any adapter or bridge approach used
- Any intentionally deferred scope

## Acceptance

- Compile succeeds.
- Play Mode starts.
- Movement works after UI closes.
- All required workstations route correctly.
- One loop succeeds.
- Three loops succeed.
- No blocking console errors remain.
- `STATE.md` and `DECISIONS.md` are updated.

## Rollback

Validation should not create gameplay changes. If validation discovers a blocker, mark Phase7 as `BLOCKED` or return to the failed task instead of patching unrelated systems.
