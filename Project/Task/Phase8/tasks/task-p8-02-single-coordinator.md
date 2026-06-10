# Task P8-02 Single Coordinator

## Goal

Confirm that `GameplayBridgeManager` is the only runtime coordinator.

## Scope

- Define bridge ownership of session lifecycle.
- Define bridge ownership of module entry and exit.
- Define bridge ownership of runtime state switching.
- Define bridge ownership of phase6 control restoration.

## Acceptance

- One object owns the runtime coordination path.
- No other object is allowed to schedule the main bridge flow.
