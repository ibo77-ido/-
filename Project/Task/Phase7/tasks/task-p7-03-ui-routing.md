# Task P7-03 UI Routing

## Goal

Route each Phase6 workstation to the correct gameplay panel or temporary panel.

## Dependency

- P7-02 PASS

## Scope

Files:
- `Assets/Phase6/Scripts/Systems/Workstation.cs`
- `Assets/Phase6/Scripts/Systems/TestUIRouter.cs`
- Optional new adapter script under `Assets/Phase6/Scripts/Systems/`

Scene:
- `Assets/Phase6/Scenes/Workshop_TestScene.unity`

Allowed changes:
- UI routing data
- Workstation-to-panel mapping
- Minimal adapter code for Phase3 panel access

Do not rewrite Phase3 UI controllers in this task.

## Target Mapping

Use existing panels if present:

- `OrderStation` -> `Panel_Order`
- `WheelStation` -> `Panel_Shape`
- `GlazeStation` -> `Panel_Glaze`
- `KilnStation` -> `Panel_Firing`
- `StorageStation` -> temporary storage/debug panel
- `MaterialStation` -> temporary material/debug panel

If a target panel is missing:
- First search the active scene.
- Then search Phase3 scene or prefabs.
- If no reusable panel exists, create a minimal temporary whitebox panel for that station.

## UnityMCP Steps

1. Use `find_gameobjects(search_method="by_component", search_term="Workstation")`.
2. Use `find_gameobjects(search_method="by_name")` for each target panel.
3. Inspect `TestUIRouter` mappings.
4. Output a design listing:
   - Existing panels found
   - Missing panels
   - Temporary panels to create
   - Scripts to modify
5. Use `manage_gameobject` and `manage_components` for scene panel creation or binding.
6. Use `script_apply_edits` only if routing logic needs changes.
7. Validate modified scripts.
8. Enter Play Mode and test each workstation.

## Acceptance

- All six workstations open the intended panel.
- Missing panel cases log a clear message or use an approved temporary panel.
- Closing any panel returns to `Playing`.
- Opening one panel while another is open is blocked.
- Movement is restored after close.
- No blocking console errors.

## Rollback

Rollback scene UI mappings and any temporary panels created in this task.
