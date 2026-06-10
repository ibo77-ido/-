# Task P8-16 Abort Flow

## Goal

Recover cleanly from errors, forced exits, and invalid session states.

## Scope

- Close the active module UI.
- Unbind events.
- Release input locks.
- Dispose the session.
- Restore Phase6 control.

## Acceptance

- The player is not left stuck.
- The area can be re-entered after abort.
