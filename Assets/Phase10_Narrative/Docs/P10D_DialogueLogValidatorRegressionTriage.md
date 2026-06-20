# P10D Dialogue Log Validator Regression Triage

## Date

2026-06-19

## Context

P10D-23 final validation found two failing validators:

- `P10D-06 Dialogue Log validator`
- `P10D-10 Dialogue Log Snapshot regression`

Later validators still passed, including P10D-20R9B editor-visible dialogue slots and P10D-21 current order panel.

## Failure Cause

The failures were caused by old validator assumptions, not by a confirmed runtime Dialogue Log regression.

The P10D-06 validator still assumed:

- The runtime dialogue surface was always a direct child named `P10_Runtime_DialogueSurface`.
- The bottom dialogue panel anchors matched the old pre-R6 height.
- One `AdvanceDialogue()` call would always move from Prologue to Tutorial.
- Dialogue Log visible meta text still exposed node id labels.

Those assumptions are outdated after:

- R6 text completeness changes expanded the bottom dialogue panel.
- R8/R9B split the bottom dialogue into fixed speaker/body text slots and introduced editor-visible prefab/scene slots.
- R2/R20 line segmentation means one node can contain multiple `DialogueLine` entries, so Continue can advance a line before changing nodes.
- R7 weakened player-facing meta text so node ids do not need to be visible in the Log panel.

## Failure Type

Type: `B/C`

- `Type B`: validator was outdated and hard-coded old UI paths/layout text.
- `Type C`: validator fixture advanced dialogue using old single-line node assumptions.

No runtime repair was required in this triage.

## Modified Files

- `Assets/Phase10_Narrative/Scripts/Editor/P10D06DialogueLogValidator.cs`
- `Assets/Phase10_Narrative/Docs/P10D_DialogueLogValidatorRegressionTriage.md`
- `Assets/Phase10_Narrative/Docs/P10D_FinalValidationSweep.md`
- `AGENTS/RuntimeLayer/MEMORY_Phase10.md`

## Repair

Validator-only repair:

- Added a runtime dialogue surface resolver that supports direct and recursive `P10_Runtime_DialogueSurface` lookup and can fall back through the fixed body slot parent chain.
- Replaced old direct-child surface lookup in P10D-06/P10D-10 assertions.
- Relaxed old exact anchor assertions to require usable non-zero/anchored `RectTransform` surfaces.
- Updated current body text checks to prefer `controller.FixedBodyText`.
- Updated dialogue advancement in P10D-06 to drain segmented DialogueLines until the expected node appears.
- Updated Log meta validation to require sequence visibility rather than old visible node id exposure.

## Validation Results

Build:

- `dotnet build Phase10_Narrative.csproj`: `PASS`, 0 warnings / 0 errors.

Unity batchmode:

| Check | Method | Log | Result | PASS Text |
| --- | --- | --- | --- | --- |
| P10D-06 Dialogue Log | `Phase10_Narrative.P10D06DialogueLogValidator.RunP10D06DialogueLogValidation` | `Logs/P10D23R3_P10D06_DialogueLogValidation.log` | `PASS` | `P10D-06 Dialogue Log validation passed.` |
| P10D-10 Snapshot | `Phase10_Narrative.P10D06DialogueLogValidator.RunP10D10DialogueLogSnapshotValidation` | `Logs/P10D23R3_P10D10_DialogueLogSnapshotValidation.log` | `PASS` | `P10D-10 Dialogue Log Snapshot Extension validation passed.` |
| P10D-20R9B visible slots | `Phase10_Narrative.P10D14Chapter1FlowHarnessValidator.RunP10D20R9BClearlyVisibleDialogueTextSlotObjectsValidation` | `Logs/P10D23R3_P10D20R9B_ClearlyVisibleDialogueTextSlots.log` | `PASS` | `P10D-20R9B Clearly Visible Dialogue Text Slot Objects validation passed.` |
| P10D-21 current order | `Phase10_Narrative.P10D14Chapter1FlowHarnessValidator.RunP10D21CurrentOrderPanelWithCraftingHintsValidation` | `Logs/P10D23R3_P10D21_CurrentOrderPanel.log` | `PASS` | `P10D-21 Current Order Panel with Crafting Hints validation passed.` |

## Final Notes

- Runtime code modified: `NO`.
- Validator modified: `YES`.
- ScriptableObject assets modified: `NO`.
- Prefab/scene modified: `NO`.
- Stage / commit / push: `NO`.

## Recommended Next Step

Rerun `P10D-23 Phase10D Final Validation Sweep` so the final sweep document can be replaced by a fully current all-core-validator matrix.
