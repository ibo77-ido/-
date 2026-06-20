# P10D In-Game Acceptance

## Goal

P10D-05 validates the narrow in-game acceptance loop for Phase10D without expanding gameplay scope.

The target is a demonstrable Phase10-only path:

```text
NPC or approved entry
  -> visible dialogue surface
  -> Prologue
  -> Tutorial
  -> ORDER_001_ACCEPT
  -> OrderCompleted(ORDER_001)
  -> ORDER_001_PASS
```

## Scope

Included:

```text
P10NarrativeManager
P10NarrativeSceneBindingHub
P10DialogueController
P10NarrativeNpcInteraction
OrderCompleted(ORDER_001)
```

Excluded:

```text
Phase3 real order system binding
Workshop_TestScene edits
Scene asset mutation
ProjectSettings changes
OrderFailed event type
ORDER_003 or ORDER_004 expansion
```

## Validator

P10D-05 adds an Editor validation entry:

```text
Phase10_Narrative.P10D05InGameAcceptanceValidator.RunP10D05InGameAcceptanceValidation
```

The validator creates temporary Editor objects only. It does not save or mutate a Unity scene.

The dialogue runtime surface uses Unity built-in `LegacyRuntime.ttf` for Unity 2022 batchmode compatibility.

## Coverage

The validator checks:

- minimal runtime combination can be created:
  - `P10NarrativeManager`
  - `P10NarrativeSceneBindingHub`
  - `P10DialogueController`
- dialogue runtime surface creation path is callable
- `StartPrologue` enters `P10_CH01_NODE_PROLOGUE_01`
- dialogue Next advances Prologue to Tutorial
- dialogue Next advances Tutorial to `P10_CH01_NODE_ORDER_001_ACCEPT`
- dialogue Next from `ORDER_001_ACCEPT` does not enter `ORDER_001_PASS`
- `OrderCompleted(ORDER_001)` enters `P10_CH01_NODE_ORDER_001_PASS`
- `P10NarrativeNpcInteraction.Interact()` can enter Prologue through the binding hub
- `P10NarrativeNpcInteraction.Interact()` can enter `ORDER_001_ACCEPT` from Tutorial through the binding hub

## Acceptance

P10D-05 passes when:

- `dotnet build Phase10_Narrative.csproj` passes with zero errors
- Unity Editor validation logs:

```text
P10D-05 In-Game Acceptance validation passed.
```

## File Mutation Summary

Created:

```text
Assets/Phase10_Narrative/Docs/P10D_InGameAcceptance.md
Assets/Phase10_Narrative/Docs/P10D_InGameAcceptance.md.meta
```

Modified:

```text
Assets/Phase10_Narrative/Scripts/Editor/P10C05SaveStateSafetyValidator.cs
Assets/Phase10_Narrative/Scripts/UI/P10DialogueController.cs
```

Serialized References Changed:

```text
NONE
```

Scene Mutation:

```text
NONE
```
