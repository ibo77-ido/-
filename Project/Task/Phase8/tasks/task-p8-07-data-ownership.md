# Task P8-07 Data Ownership

## Goal

Define which runtime data is persistent and which data belongs to one session.

## Scope

- Keep `GameplayBridgeManager` persistent for runtime coordination.
- Keep `Phase3RuntimeContext` persistent for shared state.
- Keep `GameplayModuleSession` disposable after one run.
- Keep temporary UI and input states inside the session.

## Acceptance

- Persistent and disposable data are clearly separated.
- The bridge can reopen modules without stale session data leaking.
