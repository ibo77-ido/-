# Task P7-05 Area Stability

## Goal

Stabilize area transitions, interaction availability, and visual refresh isolation after the gameplay loop is connected.

## Dependency

- P7-04 PASS

## Scope

Files:
- `Assets/Phase6/Scripts/Systems/AreaManager.cs`
- `Assets/Phase6/Scripts/Systems/AreaTrigger.cs`
- `Assets/Phase6/Scripts/Systems/InteractionController.cs`
- `Assets/Phase6/Scripts/Systems/WorkstationVisualController.cs`
- `Assets/Phase6/Scripts/Systems/ScaleManager.cs`

Allowed changes:
- Area enter and exit debounce
- Current area reporting
- Interaction availability checks
- `ArtRoot` refresh safety
- Minimal `ScaleManager` cleanup if current logic is still a no-op

Do not move map layout or resize visible areas.

## UnityMCP Steps

1. Use Play Mode to move through all six areas.
2. Record area enter and exit logs.
3. Verify interaction works after each area transition.
4. Trigger `RefreshVisual()` on each workstation.
5. Verify only `ArtRoot` children changed.
6. Verify `LogicRoot`, `InteractionPoint`, colliders, and triggers remain stable.
7. Validate modified scripts and check console.

## Acceptance

- Area enter and exit do not flicker during normal movement.
- `AreaManager.CurrentArea` returns `None` when outside all areas.
- Workstation interaction remains available after area transitions.
- `RefreshVisual()` never destroys or moves `LogicRoot`.
- `ScaleManager` does not scale `LogicRoot`.
- No layout changes are made.
- No blocking console errors.

## Rollback

Rollback script changes only. Do not revert Phase6 layout or NavMesh assets unless this task explicitly changed them.
