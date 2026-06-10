# Task P8-17 Implementation Order

## Goal

Lock the implementation sequence so the runtime bridge is built in a safe order.

## Scope

1. Session and runtime mode
2. Bridge manager and input lock
3. Bridge canvas host and adapter
4. Phase3 gate
5. Result exit relay
6. Phase6 entry wiring
7. Play Mode validation

## Acceptance

- The build order is explicit.
- Implementation can proceed step by step without re-planning every time.
