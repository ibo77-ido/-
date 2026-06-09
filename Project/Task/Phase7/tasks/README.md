# Phase7 Tasks

## Index

- [task-p7-00-workspace.md](task-p7-00-workspace.md)
- [task-p7-01-scene-binding.md](task-p7-01-scene-binding.md)
- [task-p7-02-state-gate.md](task-p7-02-state-gate.md)
- [task-p7-03-ui-routing.md](task-p7-03-ui-routing.md)
- [task-p7-03a-gameplay-bridge.md](task-p7-03a-gameplay-bridge.md)
- [task-p7-04-gameplay-loop.md](task-p7-04-gameplay-loop.md)
- [task-p7-05-area-stability.md](task-p7-05-area-stability.md)
- [task-p7-06-validation.md](task-p7-06-validation.md)

## Execution Rule

Each task must follow this sequence:

1. Read the active scene and console state.
2. Inspect relevant GameObjects and components through UnityMCP.
3. Output a design and wait for approval.
4. Implement only the approved scope.
5. Validate with UnityMCP.
6. Update `STATE.md` and `DECISIONS.md` after a completed task.

## UnityMCP Baseline

Use these tools as the default workflow:
- `manage_scene`
- `find_gameobjects`
- `manage_components`
- `script_apply_edits`
- `validate_script`
- `read_console`
- `manage_editor`
- `manage_camera` when visual confirmation is needed
