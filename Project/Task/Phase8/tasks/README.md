# Phase8 Tasks

## Index

- [task-p8-00-workspace.md](task-p8-00-workspace.md)
- [task-p8-01-bridge-contract.md](task-p8-01-bridge-contract.md)
- [task-p8-02-single-coordinator.md](task-p8-02-single-coordinator.md)
- [task-p8-03-session-mode.md](task-p8-03-session-mode.md)
- [task-p8-04-phase3-auto-advance-gate.md](task-p8-04-phase3-auto-advance-gate.md)
- [task-p8-05-phase3-completion-event.md](task-p8-05-phase3-completion-event.md)
- [task-p8-06-scene-host-structure.md](task-p8-06-scene-host-structure.md)
- [task-p8-07-data-ownership.md](task-p8-07-data-ownership.md)
- [task-p8-08-file-responsibility-matrix.md](task-p8-08-file-responsibility-matrix.md)
- [task-p8-09-enter-flow-area-detection.md](task-p8-09-enter-flow-area-detection.md)
- [task-p8-10-enter-flow-input-lock.md](task-p8-10-enter-flow-input-lock.md)
- [task-p8-11-enter-flow-module-open.md](task-p8-11-enter-flow-module-open.md)
- [task-p8-12-enter-flow-runtime-mode-switch.md](task-p8-12-enter-flow-runtime-mode-switch.md)
- [task-p8-13-running-rules.md](task-p8-13-running-rules.md)
- [task-p8-14-exit-flow-completion.md](task-p8-14-exit-flow-completion.md)
- [task-p8-15-exit-flow-unbind-and-cleanup.md](task-p8-15-exit-flow-unbind-and-cleanup.md)
- [task-p8-16-abort-flow.md](task-p8-16-abort-flow.md)
- [task-p8-17-implementation-order.md](task-p8-17-implementation-order.md)
- [task-p8-18-first-validation-slice.md](task-p8-18-first-validation-slice.md)
- [task-p8-19-go-no-go.md](task-p8-19-go-no-go.md)

## Task Design Rule

Each task should be small enough to implement, inspect, and validate in one CodeBuddy pass.

## Execution Rule

For every task:

1. Read the active scene and console state.
2. Inspect the relevant objects through UnityMCP.
3. Output a short design and wait for approval.
4. Implement only the approved scope.
5. Validate the result in Play Mode.

## Scope Rule

- Keep each task tied to one module or one lifecycle step.
- Do not mix bridge principles, runtime gating, scene wiring, and validation into one task.
- Prefer one file group per task.
