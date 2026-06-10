# P8-03A Bridge Freeze

## Core Principles

1. `GameplayBridgeManager` is the only runtime coordinator.
2. `GameplayModuleSession` carries one module run from enter to exit.
3. `RuntimeMode` belongs to the session, not to the whole game.
4. Phase3 must emit a completion event for the bridge.
5. Bridge only bridges; it does not own gameplay rules.
6. `Bridge -> Phase3` is allowed; `Phase3 -> Phase6` is forbidden.

## Minimal Phase3 Patch Rule

Phase3 should only receive a small gate patch:

- block auto-advance when running in bridge mode
- expose a completion event for Result / exit
- avoid touching calculators, scoring, or gameplay rules

## What Not to Do

- Do not make the adapter decide the next stage.
- Do not use a global runtime mode.
- Do not make UI panels directly control progression.
- Do not let the bridge guess completion by polling.
