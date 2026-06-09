# Task P7-01 Scene Binding Audit

## Goal

Verify and repair critical Phase6 scene references before gameplay integration starts.

## Dependency

- Phase6 P6-07 completed.
- P7-00 completed.

## Scope

Scene:
- `Assets/Phase6/Scenes/Workshop_TestScene.unity`

Read or inspect:
- `Phase6GameManager`
- `PlayerCharacter`
- `InputManager`
- `TestUIRouter`
- `AreaManager`
- `ScaleManager`
- All `Workstation` objects
- `Camera_2D_Oblique`

Allowed changes:
- Inspector references
- Missing component attachment when the target object already exists
- Minimal scene-only repair for broken bindings

Do not change gameplay logic in this task.

## UnityMCP Steps

1. Use `manage_scene(action="get_active")` and confirm `Workshop_TestScene` is active.
2. Use `read_console(types=["error","warning"])` to record current issues.
3. Use `find_gameobjects` for:
   - `Phase6GameManager`
   - `PlayerCharacter`
   - `InputManager`
   - `TestUIRouter`
   - `AreaManager`
   - `ScaleManager`
   - `Camera_2D_Oblique`
4. Use `find_gameobjects(search_method="by_component", search_term="Workstation")`.
5. Read component data for each critical object.
6. Repair missing serialized references with `manage_components(action="set_property")`.
7. Enter Play Mode with `manage_editor(action="play")`.
8. Check console with `read_console(types=["error"])`.
9. Exit Play Mode with `manage_editor(action="stop")`.

## Required Bindings

`Phase6GameManager`:
- `playerCharacter`
- `inputManager`
- `testUIRouter`

`InputManager`:
- `gameManager`
- `playerCharacter`
- `targetCamera`
- `interactionController`

`PlayerCharacter`:
- `movementController`
- `stateMachine`
- `navMeshAgent`
- `logicRoot`
- `artRoot`
- `interactionController`

`TestUIRouter`:
- `gameManager`
- `playerCharacter`
- UI mappings for `Order`, `Wheel`, `Glaze`, `Kiln`, `Storage`, and `Material`

Each `Workstation`:
- `config`
- `logicRoot`
- `artRoot`
- `interactionPoint`
- `visualController`

## Acceptance

- Active scene is `Workshop_TestScene`.
- No Missing Script components remain on Phase6 critical objects.
- No unassigned required serialized references remain.
- Play Mode starts and stops without blocking console errors.
- No scene layout changes are made.

## Rollback

Rollback is scene binding only. Revert Inspector reference changes if a binding points to the wrong object.
