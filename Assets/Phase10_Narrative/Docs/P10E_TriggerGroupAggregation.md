# P10E Trigger Group Aggregation

Draft Date: 2026-06-20

## Goal

Aggregate the repeated child `P10NarrativeSceneTrigger` scripts under `P10_CH01_Triggers` into one parent-level `P10NarrativeSceneTriggerGroup`, while preserving each existing scripted child trigger object's Transform, Collider, trigger id, target node id, binding hub, and trigger-once behavior.

## Implementation

- Parent object: `P10_CH01_Triggers`
- New parent component: `P10NarrativeSceneTriggerGroup`
- Parent physics helper: kinematic `Rigidbody`, gravity disabled
- Child objects: keep their Collider and scene position
- Child objects: no longer carry `P10NarrativeSceneTrigger`
- Routing remains through `P10NarrativeSceneBindingHub.HandleSceneTrigger(triggerId, targetNodeId)`
- The migration reads the existing child scripts before removing them, so it does not create new trigger behavior for child placeholders that previously had no trigger script.

## Validation

- The parent group contains the same routed mappings that previously existed on child scripts.
- All routed child trigger Colliders remain trigger Colliders.
- No child `P10NarrativeSceneTrigger` components remain under `P10_CH01_Triggers`.
- Runtime routing still reaches the same narrative manager entry point.
- `triggerOnce` still blocks duplicate trigger activation.

## Boundary

- No Phase3 gameplay integration.
- No Phase6 movement logic changes.
- No ProjectSettings changes.
- No stage / commit / push.
