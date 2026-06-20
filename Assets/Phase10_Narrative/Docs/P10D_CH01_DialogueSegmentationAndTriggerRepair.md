# P10D-20R2 Dialogue Segmentation and Trigger Repair

## Problem Cause

P10D-20 manual PlayMode acceptance exposed two risks in the Chapter 1 runtime slice.

First, Chinese narrative content needs each displayed dialogue line to carry exactly one speaker identity. Mixed narrator and NPC text in one displayed line makes the Dialogue Log ambiguous because speaker ownership is stored by asset fields, not by inline text prefixes.

Second, task progression had to be verified against the state machine rather than UI display order. The risky path was result-node entry after gameplay events: the manager can advance the current node before the dialogue UI `Update()` observes the new node.

## Modified Dialogue Assets

The current 13 Chapter 1 dialogue assets were audited. The assets already use `DialogueLines` and keep the existing node IDs.

Segmentation-sensitive assets covered by validation:

- `P10_CH01_NODE_PROLOGUE_01`: narrator, Xu Laobo, system prompt lines.
- `P10_CH01_NODE_ORDER_001_PASS`: Zhou Zhanggui, narrator, UI result lines.
- `P10_CH01_NODE_ORDER_003_PASS`: Chen Shuyuan, narrator, UI result lines.
- `P10_CH01_NODE_ORDER_004_ACCEPT`: Lu Ke, Xu Laobo, system prompt lines.
- `P10_CH01_NODE_ORDER_004_CLIMAX`: narrator, Lu Ke, Xu Laobo lines.
- `P10_CH01_NODE_CHAPTER_ENDING`: narrator, Xu Laobo, narrator, UI result lines.

No node IDs were changed and no new node was added.

## Segmentation Rules

- Speaker identity must come from `SpeakerCharacterId`.
- Dialogue body text is content-only for the current P10D-20R2 asset repair.
- Dialogue body text must not contain authored display speaker prefixes in the current Phase10 assets.
- Each `DialogueLines` entry must resolve to a `P10CharacterDataSO` asset.
- Runtime Dialogue Log must record the resolved speaker display name and the line text separately.

## Abnormal Text Audit

Checked scope:

- 13 Chapter 1 dialogue assets under `Assets/Phase10_Narrative/ScriptableObjects/Dialogues`.
- Chapter 1 character assets under `Assets/Phase10_Narrative/ScriptableObjects/Characters`.
- P10D-20R2 repair doc, validator, and runtime dialogue controller.

Checked patterns:

- Replacement character `U+FFFD`.
- Private-use glyphs.
- Common mojibake fragments.
- Invalid display speaker prefixes inside `DialogueText`.
- `???` placeholder text.
- Empty `DialogueText` or empty `SpeakerCharacterId` fields.

Result:

- No abnormal text was found in the checked dialogue assets or character display fields.
- Current assets remain body-only, with no authored display prefix in dialogue body text.
- Latin technical tokens in system/result text, such as `ShapeScore`, `GlazeScore`, `FireScore`, `ORDER_001`, `GLAZE_002`, and `CODEX_*`, are intentional gameplay identifiers.
- `P10DialogueCatalog` still contains fallback-only English technical strings; P10D-20R2 does not synchronize fallback catalog text.

## Trigger Stability Repair and Validation

Runtime repair:

- `P10DialogueController.SetCurrentNode()` now resets the dialogue-line cursor whenever the requested node differs from the cursor node, even if `currentNodeId` has not yet been synchronized by `Update()`.
- This prevents result nodes entered by `OrderCompleted` / `ScoreThresholdReached` from displaying or logging with a stale line index from the previous node.

Validation:

- `P10D14Chapter1FlowHarnessValidator.RunP10D20R2DialogueSegmentationAndTaskTriggerStabilityValidation`
- Verifies all 13 dialogue assets have resolvable line speakers and no authored display speaker prefixes.
- Verifies Dialogue Log records split speaker/text entries.
- Repeats ORDER_001 -> ORDER_003 -> ORDER_004 -> CLIMAX flows to catch drift.
- Verifies blocked Dialogue Next does not skip accept nodes or append duplicate log entries.
- Verifies no-UI state-machine flows for normal ORDER_004, climax ORDER_004, and queued climax.
- Reuses the P10D-19 Chinese vertical slice coverage inside the P10D-20R2 validator.

## Task Trigger Stability Report

### Scope

This report covers Phase10-owned Chapter 1 narrative trigger stability only. It does not connect, repair, or validate formal Phase3 / Phase6 gameplay order systems.

Covered trigger surfaces:

- Approved scene triggers through `P10NarrativeManager.HandleSceneTrigger()`.
- Dialogue Next transitions through `P10DialogueController.AdvanceDialogue()` and `P10NarrativeManager.TryAdvanceDialogueNode()`.
- Gameplay fact events submitted as Phase10 narrative events:
  - `OrderCompleted(ORDER_001)`
  - `OrderCompleted(ORDER_003)`
  - `OrderCompleted(ORDER_004)`
  - `ScoreThresholdReached(...)`
- Dialogue Log open / close behavior.
- Snapshot save / load restore behavior.

Excluded trigger surfaces:

- Phase3 production order UI.
- Phase6 workstation interaction repair.
- `Workshop_TestScene` / NavMesh dirty or deleted state.
- ProjectSettings and scene build configuration.

### Stability Matrix

| Trigger path | Expected behavior | Stability check | Result |
|---|---|---|---|
| StartPrologue scene trigger | Enters `P10_CH01_NODE_PROLOGUE_01` only from empty / None state. | Approved trigger probe validates first node entry and duplicate blocking. | Covered |
| Dialogue Next inside a multi-line node | Advances only `DialogueLines[currentLineIndex]`; does not advance state before all lines are consumed. | R2 harness drains multi-line nodes and checks log count / node identity per line. | Covered |
| Dialogue Next from node to next node | Advances only to approved next node and blocks out-of-order jumps. | Chapter flow harness checks normal and climax paths. | Covered |
| ORDER_001 completion | From `ORDER_001_ACCEPT`, enters `ORDER_001_PASS`; does not enter pass through Dialogue Next alone. | `OrderCompleted(ORDER_001)` flow and blocked Dialogue Next assertion. | Covered |
| ORDER_003 completion | From `ORDER_003_ACCEPT`, enters `ORDER_003_PASS`; does not enter pass through Dialogue Next alone. | `OrderCompleted(ORDER_003)` flow and blocked Dialogue Next assertion. | Covered |
| ORDER_004 normal completion | From `ORDER_004_ACCEPT`, enters `ORDER_004_PASS_NORMAL` when no climax flag is active. | No-UI normal completion cycle and full UI flow. | Covered |
| ORDER_004 climax completion | From `ORDER_004_ACCEPT`, enters `ORDER_004_CLIMAX` when score threshold permits climax. | No-UI climax completion cycle and full UI flow. | Covered |
| Queued climax threshold | Score threshold before `ORDER_004_ACCEPT` must not advance state early; it may be consumed once ORDER_004 completion occurs. | Queued climax completion cycle. | Covered |
| Duplicate completion event | Repeated completion events must not drift state after result node entry. | Repeated no-UI normal / climax cycles. | Covered |
| Dialogue Log open / close | Opens and closes without advancing node, changing state, or appending duplicate log entries. | `AssertDialogueLogPanelDoesNotMutate`. | Covered |
| Snapshot restore | Restored log entries must not replay node events or append duplicates on current-node refresh. | Snapshot restore validation. | Covered |

### Known Risk After Manual Review

The R2 repair verified trigger stability and line segmentation, but later manual review found the bottom dialogue speaker label still needs stronger validation. The current trigger-stability result should therefore be read narrowly:

- Task trigger order: stable by automation.
- Dialogue Log speaker/text: covered by automation.
- Bottom dialogue speaker label visual sync: requires the later speaker-display repair validator.

### Report Conclusion

Within Phase10-only automation, task trigger progression is stable for the approved Chapter 1 path:

```text
PROLOGUE -> TUTORIAL -> ORDER_001_ACCEPT
ORDER_001_ACCEPT --OrderCompleted(ORDER_001)--> ORDER_001_PASS
ORDER_001_PASS -> ORDER_003_ACCEPT
ORDER_003_ACCEPT --OrderCompleted(ORDER_003)--> ORDER_003_PASS
ORDER_003_PASS -> ORDER_004_ACCEPT
ORDER_004_ACCEPT --OrderCompleted(ORDER_004)--> ORDER_004_PASS_NORMAL
ORDER_004_ACCEPT --ScoreThresholdReached / completion--> ORDER_004_CLIMAX
ORDER_004_PASS_NORMAL or ORDER_004_CLIMAX -> CHAPTER_ENDING
```

No Phase3, Phase6, Phase8, scene, NavMesh, or ProjectSettings behavior is changed or certified by this report.

## Manual Recheck

- Re-run PlayMode manual acceptance to confirm visual text order and speaker labels are correct in the actual scene.
- Confirm Chinese glyph rendering quality on the target runtime font.
- Confirm no Phase3 / Phase6 scene or gameplay integration behavior was changed by this Phase10-only repair.
