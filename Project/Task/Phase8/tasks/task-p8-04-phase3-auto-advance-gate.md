# Task P8-04 Phase3 Auto-Advance Gate

## Goal

Block Phase3 automatic chaining when the bridge is active.

## Scope

- Add the minimal gate to `GameManager`.
- Guard the auto-advance path.
- Keep standalone Phase3 behavior unchanged.
- Keep gameplay rule code intact.

## Acceptance

- Standalone Phase3 still works normally.
- Bridge mode blocks unwanted module chaining.
