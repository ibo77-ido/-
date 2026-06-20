# P10 Props Contract

## Goal

Phase10 may define pure narrative props, but it must not own production gameplay item data.

This keeps story expression separate from Phase3 calculators, orders, recipes, glaze data, kiln data, rewards, and scoring systems.

## Narrative Props

Narrative props are story-only objects. They can be inspected, displayed, mentioned in dialogue, and used as narrative context.

Allowed Chapter 1 narrative props:

```text
P10_PROP_001_FatherLedger
P10_PROP_002_OldKilnTool
P10_PROP_003_BrokenBowl
P10_PROP_004_AncientOrder
P10_PROP_005_FamilyLetter
```

Allowed data:

```text
PropId
DisplayName
Description
StoryHint
RelatedNodeId
OptionalInspectText
```

Allowed behavior:

```text
Show placeholder visual.
Allow inspect interaction.
Publish NarrativePropInspected event.
Display story text.
Unlock or reference a narrative node through manager-controlled flow.
```

## Gameplay Items

These remain owned by Phase3 or other gameplay data systems:

```text
Glaze IDs
Clay IDs
Fuel IDs
Order material IDs
Recipe material IDs
Kiln resources
Shape configuration
Scoring data
Reward data
```

Examples of data Phase10 must not own:

```text
甜白釉
祭红釉
瓷土
木柴
燃料
生产订单材料
实际配方材料
烧窑资源
器型配置
釉料配置
奖励数据
```

Phase10 dialogue may mention these items in text, but Phase10 must not create duplicate gameplay IDs or gameplay ScriptableObjects for them.

## Forbidden Coupling

Forbidden examples:

```text
Phase10 defines a gameplay glaze ID.
Phase10 defines a gameplay clay ID.
Phase10 order data duplicates Phase3 order data.
Phase3 calculators reference P10_PROP_*.
Phase10 prop data is used as gameplay material input.
```

## Prop View Boundary

`P10NarrativePropView` may:

```text
Display prop visuals.
Handle prop inspection.
Publish P10NarrativeEventType.NarrativePropInspected.
Forward prop context to P10NarrativeManager.
```

It must not:

```text
Call P10NarrativeStateMachine.AdvanceToState directly.
Call P10NarrativeStateMachine.AdvanceToNode directly.
Modify Phase3, Phase6, or Phase8 systems.
Own gameplay item data.
```

## Acceptance

- All narrative props use `P10_PROP_*`.
- Narrative prop data lives under `Assets/Phase10_Narrative/**`.
- Phase10 does not define production gameplay item data.
- Phase10 can mention gameplay items in text without owning their IDs.
- Prop inspection is represented by neutral narrative events.
