# P10E Phase10-Only Interaction Test Harness

## Purpose

P10E-02 adds a bounded Phase10-only interaction gate harness for the P10E-01 distance design.

The harness verifies the decision result for NPC clicks from a fake player transform to fake NPC interact points. It does not enter real dialogue, does not connect to Phase3 or Phase6 gameplay, and does not use Yu bridge data.

## Test Objects

The validator creates temporary editor objects only:

- `TestPlayerTransform`
- `TestNPC_XuLaoBo_InteractPoint`
- `TestNPC_ZhouZhangGui_InteractPoint`
- `TestNPC_ChenShuYuan_InteractPoint`
- `TestNPC_LuKe_InteractPoint`

These objects are destroyed before the validator exits and are not saved to a scene or prefab.

## Gate Result Enum

`P10NpcInteractionGateResult` defines the current Phase10 gate decisions:

- `AllowDialogue`
- `TooFar`
- `QuestNotAvailableYet`
- `InvalidNpc`

`P10NpcInteractionGateEvaluator.Evaluate(playerPosition, interactPointPosition, currentState, npcId, interactionDistance)` applies the frozen P10E-01 priority:

1. Reject unknown NPC ids as `InvalidNpc`.
2. Check distance first.
3. If distance is greater than `interactionDistance`, return `TooFar`.
4. If in range, verify the current mainline NPC for the Phase10 narrative state.
5. Return `AllowDialogue` for the current NPC, otherwise `QuestNotAvailableYet`.

## Coverage Rules

The frozen distance value is:

- `interactionDistance = 2.5`

The validator covers:

- distance `2.5` returns in-range behavior
- distance `2.51` returns `TooFar`
- current mainline NPC plus in range returns `AllowDialogue`
- current mainline NPC plus out of range returns `TooFar`
- non-current NPC plus in range returns `QuestNotAvailableYet`
- non-current NPC plus out of range returns `TooFar`
- invalid NPC returns `InvalidNpc`

The current mainline NPC map is:

- `Prologue` -> `XuLaoBo`
- `Order001` -> `ZhouZhangGui`
- `Order003` -> `ChenShuYuan`
- `Order004` -> `LuKe`
- `Ending` -> `XuLaoBo`

The validator also checks that the harness does not mutate a fresh `P10NarrativeStateMachine` snapshot and does not create or open a `P10DialogueController`.

## Boundaries

P10E-02 remains Phase10-only:

- no Phase3 order data is read
- no Phase6 player, movement, NPC, scene, or NavMesh state is read or modified
- no Phase8 state is referenced
- no Yu bridge implementation is referenced
- no Phase10 overlay scene or prefab is modified
- no runtime debug UI is added
- no dialogue node is expanded
- no narrative state is advanced
- no Dialogue Log entry is written

## Validator

Entry point:

- `Phase10_Narrative.P10E02InteractionTestHarnessValidator.RunP10E02InteractionTestHarnessValidation`

Required pass text:

- `P10E-02 Phase10-only Interaction Test Harness validation passed.`

## Next Steps

- Keep this harness as the baseline for P10E quest-state and distance-gate work.
- In a later bridge task, provide player position through a neutral Phase10/Yu-facing adapter instead of directly reading Phase6 player objects.
- In a later runtime task, route real NPC clicks through the same gate decision before opening dialogue.
