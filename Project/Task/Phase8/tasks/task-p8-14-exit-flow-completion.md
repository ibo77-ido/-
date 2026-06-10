# Task P8-14 Exit Flow Completion

## Goal

Receive the Phase3 completion signal and close the active session cleanly.

## Scope

- Capture the completion event.
- Verify the current session can exit.
- Close the active module UI.

## Acceptance

- The bridge can end the current module after completion.
- The exit path is event-driven, not guessed.
