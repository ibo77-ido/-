# Task P8-12 Enter Flow Runtime Mode Switch

## Goal

Switch Phase3 into bridge mode when the module opens.

## Scope

- Set session runtime mode to `Phase6Bridge`.
- Notify Phase3 that auto-advance must be blocked.
- Keep standalone mode available for normal Phase3 use.

## Acceptance

- Bridge mode and standalone mode are clearly separated.
- Runtime mode changes do not alter gameplay rules.
