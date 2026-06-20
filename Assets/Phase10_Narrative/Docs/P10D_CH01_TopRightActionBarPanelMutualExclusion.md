# P10D-24S TopRight Action Bar and Panel Mutual Exclusion

## Problem Source

Manual PlayMode feedback reported that opening the top-right `记录` button could leave the Dialogue Log panel overlapping the persistent `订单` button. The persistent buttons were independent surface children, so they did not share one layout owner and the overlay panels did not have a single mutual-exclusion target.

## Layout Plan

The Phase10 runtime dialogue surface now owns a single fixed top-right action container:

```text
P10_Runtime_DialogueSurface
└─ P10_TopRightActionBar
   ├─ PersistentLogButton
   └─ PersistentOrderButton
```

`P10_TopRightActionBar` uses a horizontal layout with 16 px spacing. The normal state displays two visible buttons in the top-right order:

```text
[记录] [订单]
```

The button labels remain visible and the buttons are sized by layout elements so they do not overlap.

## Mutual Exclusion Rules

- Normal state: `P10_TopRightActionBar` is active.
- Dialogue Log open: `P10_TopRightActionBar` is inactive; `DialogueLogCloseButton` remains available.
- Current Order panel open: `P10_TopRightActionBar` is inactive; `CurrentOrderCloseButton` remains available.
- Closing either overlay restores `P10_TopRightActionBar`.

## Click-Through Protection

The existing guarded dialogue-box click path remains unchanged. Dialogue-box clicks are ignored while Dialogue Log or Current Order is visible.

The P10D-24S validator confirms:

- clicking the Dialogue Log panel does not advance the underlying dialogue;
- clicking the Current Order panel does not advance the underlying dialogue;
- clicking either close button only closes its panel and does not advance dialogue;
- hidden persistent buttons are inactive while overlay panels are open.

## Validation

| Check | Result |
|---|---|
| `dotnet build Phase10_Narrative.csproj` | PASS, 0 warnings, 0 errors |
| Unity batchmode validator | PASS |
| Method | `Phase10_Narrative.P10D14Chapter1FlowHarnessValidator.RunP10D24STopRightActionBarAndPanelMutualExclusionValidation` |
| Log | `Logs/P10D24S_TopRightActionBarPanelMutualExclusion.log` |
| Required PASS text | `P10D-24S TopRight Action Bar and Panel Mutual Exclusion validation passed.` |

The validator includes P10D-24 dialogue-box click-to-advance regression and P10D-21 current order panel regression inside the P10D-24S validation pass.

## Boundaries

- Runtime code modified: yes, UI surface/layout behavior only.
- Prefab modified: yes, Phase10 dialogue UI prefab only.
- Scene modified: yes, Phase10 narrative overlay scene only.
- New nodes added: no.
- Task progression rules changed: no.
- Dialogue Log snapshot schema changed: no.
- Phase3 / Phase6 / Phase8 / `Assets/Scenes` / `ProjectSettings` modified by this task: no.
- Stage / commit / push: no.

## Manual Recheck

Still requires manual PlayMode review:

- verify top-right buttons visually appear as `[记录] [订单]`;
- open and close Dialogue Log repeatedly and confirm no overlap with the order button;
- open and close Current Order repeatedly and confirm the action bar restores;
- click inside Log / Order panels and confirm no underlying dialogue advance.
