# P8-03C Bridge Flow

## Enter Flow

```text
1. Player reaches a Phase6 workstation
2. Player presses `E`
3. InputManager forwards the action to the bridge
4. Bridge checks the current area type
5. Bridge creates a new session
6. Bridge locks Phase6 input
7. Bridge opens the matching Phase3 module
8. Bridge switches Phase3 runtime mode to bridge mode
9. Module starts in-place
```

## Running Rules

- Only the current module can receive UI input.
- Phase6 movement and interaction stay locked.
- Phase3 cannot auto-advance to the next module.
- The bridge must not open another station during the active session.

## Exit Flow

```text
1. Module reaches completion
2. Phase3 emits the completion event
3. Bridge receives the event
4. Bridge closes the current module
5. Bridge unbinds module events
6. Bridge disposes the session
7. Bridge releases Phase6 input
8. Player regains movement in Phase6
```

## Abort Flow

- Close the active module UI.
- Unbind all module events.
- Release input lock.
- Dispose the session.
- Restore Phase6 control.
