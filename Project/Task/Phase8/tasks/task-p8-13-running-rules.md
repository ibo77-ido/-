# Task P8-13 Running Rules

## Goal

Lock the rules that must hold while a module is active.

## Scope

- Only the active module may receive UI input.
- Phase6 movement remains locked.
- The bridge must not open another station mid-session.
- Phase3 must not auto-advance to the next module.

## Acceptance

- The active module stays isolated.
- The bridge run does not leak into other stations.
