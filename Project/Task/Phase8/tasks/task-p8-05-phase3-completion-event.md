# Task P8-05 Phase3 Completion Event

## Goal

Expose a bridge-safe completion event from Phase3.

## Scope

- Add a completion relay for the bridge.
- Ensure Phase3 emits completion, not the bridge guessing it.
- Reserve the completion event for Result or module exit.

## Acceptance

- The bridge can reliably know when Phase3 is done.
- The event does not change gameplay calculations.
