# Task P7-02 State Gate

## Goal

Unify movement, interaction, UI open, UI close, and working state rules.

## Dependency

- P7-01 PASS

## Scope

Files:
- `Assets/Phase6/Scripts/Core/Phase6GameState.cs`
- `Assets/Phase6/Scripts/Core/Phase6GameManager.cs`
- `Assets/Phase6/Scripts/Systems/CharacterStateMachine.cs`
- `Assets/Phase6/Scripts/Systems/InputManager.cs`
- `Assets/Phase6/Scripts/Systems/PlayerCharacter.cs`
- `Assets/Phase6/Scripts/Systems/InteractionController.cs`
- `Assets/Phase6/Scripts/Systems/TestUIRouter.cs`

Allowed changes:
- State transition logic
- Movement and interaction permission checks
- UI open and close state restoration

Do not change NavMesh baking, movement path calculation, or Phase3 scoring logic.

## State Rules

`Playing`:
- Movement allowed
- Interaction allowed
- UI closed

`Interacting`:
- Short transition state
- Movement blocked
- Interaction input ignored until transition completes

`UIOpen`:
- Movement blocked
- Interaction blocked
- Current movement stopped
- Closing UI returns to `Playing`

`Working`:
- Reserved for gameplay actions that must block movement and interaction
- Must be able to return to `Playing`
- Must not become a permanent lock state

## UnityMCP Steps

1. Read current scripts or use `find_in_file` for state methods.
2. Output a design before editing.
3. Use `script_apply_edits` for method-level changes.
4. Use `validate_script` on every modified script.
5. Request Unity compile or wait for compile completion.
6. Use `read_console(types=["error"])`.
7. Enter Play Mode.
8. Verify:
   - Click ground moves player in `Playing`.
   - Press `E` near a workstation opens UI.
   - While UI is open, click ground does not move player.
   - Press close button returns to `Playing`.
   - Movement works after closing UI.

## Acceptance

- UI open reliably blocks movement.
- UI open reliably blocks repeated interaction.
- UI close restores movement.
- `Working` is not a dead-end state.
- No state transition log spam during normal interaction.
- Play Mode has no blocking console errors.

## Rollback

Rollback modified scripts only. Do not modify scene layout for rollback.
