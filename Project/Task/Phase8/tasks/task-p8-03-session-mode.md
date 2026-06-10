# Task P8-03 Session Mode

## Goal

Define `GameplayModuleSession` and make `RuntimeMode` session-owned.

## Scope

- Add a session data container.
- Store module, area, workstation, and runtime mode in the session.
- Keep runtime mode out of global state.
- Make the session represent one module run from enter to exit.

## Acceptance

- Runtime mode belongs to the session.
- The session can fully describe the active bridge run.
