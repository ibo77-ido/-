# P10E-R1 Runtime Helper File Organization

## Purpose

P10E-R1 moves the NPC interaction gate helper introduced in P10E-02 out of `P10NarrativeManager.cs` before P10E-04 adds MVP tutorial interaction behavior.

This keeps `P10NarrativeManager.cs` focused on narrative manager responsibilities and prevents additional interaction helpers from accumulating in the manager file.

## Moved Runtime Helpers

Moved out of:

- `Assets/Phase10_Narrative/Scripts/Runtime/P10NarrativeManager.cs`

New files:

- `Assets/Phase10_Narrative/Scripts/Runtime/P10NpcInteractionGateResult.cs`
- `Assets/Phase10_Narrative/Scripts/Runtime/P10NpcInteractionGateEvaluator.cs`

## API Compatibility

The public API remains unchanged:

- namespace remains `Phase10_Narrative`
- enum remains `P10NpcInteractionGateResult`
- evaluator remains `P10NpcInteractionGateEvaluator`
- `DefaultInteractionDistance` remains `2.5f`
- NPC id constants remain unchanged
- `Evaluate(...)`, `IsCurrentMainlineNpc(...)`, and `IsKnownNpc(...)` remain public static members

## Gate Logic

The gate behavior is unchanged from P10E-02:

- invalid NPC id returns `InvalidNpc`
- distance check is evaluated before quest-state availability
- distance greater than `interactionDistance` returns `TooFar`
- current mainline NPC inside distance returns `AllowDialogue`
- non-current mainline NPC inside distance returns `QuestNotAvailableYet`
- negative interaction distance is clamped through `Mathf.Max(0f, interactionDistance)`

## Compile Configuration

`Phase10_Narrative.csproj` uses an explicit `Compile Include` list, so P10E-R1 adds the two new runtime files to that list.

No ProjectSettings or non-Phase10 project configuration is modified.

## Regression Validation

Completed validation:

- `dotnet build Phase10_Narrative.csproj`: PASS, 0 warnings / 0 errors
- `Phase10_Narrative.P10E02InteractionTestHarnessValidator.RunP10E02InteractionTestHarnessValidation`: PASS
  - log: `Logs/P10ER1_P10E02InteractionHarnessRegression_Rerun.log`
  - pass text: `P10E-02 Phase10-only Interaction Test Harness validation passed.`
- `Phase10_Narrative.P10E03AAnchorLayoutValidator.RunP10E03AAnchorLayoutValidation`: PASS
  - log: `Logs/P10ER1_P10E03AAnchorLayoutRegression.log`
  - pass text: `P10E-03A NPC Anchor Layout validation passed.`

The first P10E-02 regression run produced a transient Unity import-time `CS0246` while the new helper files were being imported, followed by PASS. It was rerun after import completed, and the rerun log contains the required PASS text without `error CS`.

## Boundary

P10E-R1 does not modify scene, prefab, ScriptableObject assets, Phase3, Phase6, Phase8, `Assets/Scenes`, or `ProjectSettings`.

It does not change narrative progression rules, order logic, dialogue nodes, dialogue UI, distance behavior, or quest-state gate behavior.

## Follow-up Constraint

Do not add more interaction helper types to `P10NarrativeManager.cs`.

Future interaction helpers should live in focused Phase10 runtime files and must be added to the explicit compile configuration when required.
