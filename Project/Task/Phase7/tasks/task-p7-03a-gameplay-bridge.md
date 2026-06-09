# Task P7-03A Gameplay Bridge

## Goal

Verify that Phase3 gameplay systems can be accessed from `Workshop_TestScene` before the full gameplay loop is tested.

This task catches bridge failures between Phase6 workstation UI and Phase3 gameplay systems.

## Dependency

- P7-03 PASS

## Scope

Phase3 gameplay systems:
- Order
- Shape
- Glaze
- Firing
- Result

Phase6 scene:
- `Assets/Phase6/Scenes/Workshop_TestScene.unity`

Allowed changes:
- Scene references
- Minimal adapter or bridge script under `Assets/Phase6/Scripts/Systems/`
- Minimal manager object creation if a required Phase3 system is missing from the Phase6 scene

Do not:
- Rewrite Phase3 calculators.
- Rewrite Phase3 scoring rules.
- Replace UI visuals.
- Start the full order loop in this task.

## Required Bridge Checks

Each panel must reach its target gameplay system:

- `Panel_Order` -> `OrderManager`
- `Panel_Shape` -> `ShapeSystem`
- `Panel_Glaze` -> `GlazeSystem`
- `Panel_Firing` -> `FiringSystem`
- `Panel_Result` -> `ResultSystem`

Each gameplay system must be able to return valid data:

- `OrderManager.GetCurrentOrder()` returns a non-null order.
- `ShapeSystem` can calculate or expose current shape score.
- `GlazeSystem` can calculate or expose current glaze score.
- `FiringSystem` can calculate or expose current fire score.
- `ResultSystem` can produce or expose result data.

## UnityMCP Steps

1. Use `manage_scene(action="get_active")` and confirm `Workshop_TestScene` is active.
2. Use `find_gameobjects(search_method="by_name")` for:
   - `Panel_Order`
   - `Panel_Shape`
   - `Panel_Glaze`
   - `Panel_Firing`
   - `Panel_Result`
3. Use `find_gameobjects(search_method="by_component")` for:
   - `OrderManager`
   - `ShapeSystem`
   - `GlazeSystem`
   - `FiringSystem`
   - `ResultSystem`
4. Inspect the panel controller components and their serialized references.
5. Inspect each gameplay system component and its serialized dependencies.
6. Output a bridge design before editing:
   - Panels found
   - Systems found
   - Missing scene objects
   - Missing serialized references
   - Adapter script needed or not needed
7. Repair scene bindings with `manage_components(action="set_property")`.
8. If a bridge script is needed, create or edit it with `create_script` or `script_apply_edits`.
9. Validate modified scripts with `validate_script`.
10. Enter Play Mode and call each panel/system bridge path.
11. Check console with `read_console(types=["error"])`.

## Acceptance

- Every required panel exists or has an approved temporary substitute.
- Every required Phase3 gameplay system exists in `Workshop_TestScene` or is reachable through an approved bridge.
- Every panel can call its target gameplay system.
- Every gameplay system can return data.
- No `NullReferenceException`.
- No missing singleton dependency.
- No missing required scene object.
- No blocking console errors.

## Rollback

Rollback bridge scripts, scene references, or temporary manager objects created in this task.

Do not rollback Phase3 calculator code because this task must not modify calculator behavior.
