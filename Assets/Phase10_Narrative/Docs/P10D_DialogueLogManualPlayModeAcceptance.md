# P10D-11 Dialogue Log Manual PlayMode Acceptance Plan

## Goal

P10D-11 defines the manual PlayMode acceptance plan for the P10D-06 through P10D-10 Dialogue Log feature set.

This task is docs-only. It does not add runtime UI, scene wiring, save/load buttons, or Phase3 / Phase6 integration.

## Preconditions

- Use Unity `2022.3.55f1c1`, matching `ProjectSettings/ProjectVersion.txt`.
- Open the `Gong` branch workspace.
- Do not stage, commit, or push during this acceptance pass.
- Do not repair or handle existing Phase3 / Phase6 dirty files, including `Workshop_TestScene` / NavMesh state.
- The Phase10 runtime dialogue surface must be available through the current Phase10D setup:
  - `P10NarrativeManager`
  - `P10DialogueController`
  - `P10NarrativeSceneBindingHub`
  - approved NPC / scene trigger path
- Current project limitation:
  - There is no dedicated player-facing snapshot save/load UI for Dialogue Log.
  - Snapshot save/load behavior must be verified through the existing Unity validator or a temporary Editor-only hook.
  - Do not add runtime debug UI for P10D-11.

## Test Scene / Entry Point

Primary manual entry:

```text
Workshop_TestScene with Phase10D runtime mount available
```

Acceptable alternate entry:

```text
Any PlayMode scene or temporary test scene containing:
- P10NarrativeManager
- P10DialogueController
- P10NarrativeSceneBindingHub
- at least one approved NPC / trigger entry into CH01
```

Snapshot-specific entry:

```text
Unity batchmode validator:
Phase10_Narrative.P10D06DialogueLogValidator.RunP10D10DialogueLogSnapshotValidation
```

Validator log path:

```text
D:\UnityGame\director-true-Gong\Logs\P10D10_DialogueLogSnapshotValidation.log
```

Required validator success marker:

```text
P10D-10 Dialogue Log Snapshot Extension validation passed.
```

## Manual Steps

### 1. Start PlayMode

1. Open the accepted Phase10D manual scene entry.
2. Enter PlayMode.
3. Wait for the Phase10 runtime dialogue surface to initialize.

Expected:

- A persistent `Log` button is visible near the top-right of the runtime surface.
- The `Log` button text is visible and readable.
- No Phase3 / Phase6 behavior is changed by opening PlayMode for this test.

### 2. Open Log Before Dialogue

1. Without starting a dialogue line, click the persistent `Log` button.
2. Inspect the dialogue log overlay.
3. Click the log panel `Close` button.

Expected:

- Log opens even when the bottom DialoguePanel is hidden or no dialogue is active.
- Empty state is displayed as `No dialogue yet.`
- Log panel `Close` text is visible.
- Closing Log does not start dialogue, advance story, or create a log entry.

### 3. Trigger First Dialogue Through NPC / Approved Entry

1. Trigger the approved Prologue entry through an NPC or approved scene trigger.
2. Confirm the bottom DialoguePanel appears.
3. Confirm speaker and dialogue text are visible.
4. Open Log.

Expected:

- One log entry is created.
- Entry sequence is visible.
- Entry `nodeId` is visible in the meta line.
- Entry speaker name is visible, such as `Xu Lao Bo` for Prologue.
- Entry dialogue text matches the currently displayed line.

### 4. Advance Multiple Dialogue Lines

1. Close Log.
2. Use `Next` to advance Prologue to Tutorial.
3. Continue to the approved next dialogue node where allowed.
4. Open Log again.

Expected:

- Multiple entries are shown in display order.
- Earlier entries remain visible above later entries.
- Each entry includes NPC / speaker name and dialogue text.
- `Next`, `Close`, and `Log` button text remains visible.

### 5. Duplicate Prevention

1. While on the same current node, force a surface refresh by closing and reopening Log or otherwise reselecting the same node without advancing.
2. Open Log.

Expected:

- The same `nodeId + dialogueText` is not appended twice consecutively.
- Log count remains unchanged after repeated refresh of the same current line.

### 6. Open Log After Closing DialoguePanel

1. Close the bottom DialoguePanel with the dialogue `Close` button.
2. Confirm the current dialogue panel is hidden.
3. Click the persistent top-right `Log` button.

Expected:

- Log still opens from non-dialogue state.
- Existing entries remain visible.
- Opening / closing Log does not reopen or advance dialogue unless the dialogue was visible before opening Log.
- No duplicate log entry is created by opening Log.

### 7. Snapshot Save / Load Restoration

Manual limitation:

```text
No direct player-facing snapshot save/load UI exists in the current project.
```

Use one of these accepted verification routes:

```text
Route A: Run the Unity batchmode validator RunP10D10DialogueLogSnapshotValidation.
Route B: Use a temporary Editor-only hook to call P10NarrativeManager.SaveSnapshot(), then P10NarrativeManager.LoadSnapshot(snapshot), without saving scene changes.
```

Expected:

- Snapshot version is `2`.
- Snapshot includes `DialogueLogEntries`.
- Loading snapshot restores the saved log entries.
- Loading snapshot restores narrative state and current node.
- Loading snapshot does not call Dialogue Next.
- Loading snapshot does not replay dialogue nodes.
- Loading snapshot does not publish narrative events for imported log entries.
- Loading snapshot does not advance story state.
- Refreshing the current restored node does not duplicate the last restored line.

### 8. Snapshot Capacity

Use validator or Editor-only hook to seed more than 200 dialogue log entries.

Expected:

- Exported snapshot keeps exactly the newest 200 entries.
- Oldest overflow entries are dropped.
- Restored in-memory sequence order is normalized and remains readable.

### 9. Excluded UI State

1. Open Log.
2. Scroll to a non-default position.
3. Save snapshot through validator or Editor-only hook.
4. Load snapshot.

Expected:

- Log entry data is restored.
- Log panel open / closed state is not restored as saved state.
- Scroll position is not restored.
- Button state is not restored.
- Runtime UI returns according to normal runtime rules.

## Expected Results

The manual pass is accepted when:

- Persistent `Log` button is visible after PlayMode startup.
- `Log`, `Next`, and `Close` button labels are visible.
- Log opens from non-dialogue state.
- Empty log displays `No dialogue yet.`
- Dialogue trigger creates log entries.
- Entries show speaker name, text, sequence, and node id.
- Multiple entries preserve display order.
- Consecutive duplicate current-node refresh does not append duplicates.
- Log opens after bottom DialoguePanel is closed.
- Snapshot save/load restores dialogue log entries through validator or Editor-only hook.
- Snapshot load does not advance story and does not duplicate current line.
- More than 200 entries are pruned to newest 200 on export.
- Log open state, scroll position, and button state are not saved.
- Phase3 / Phase6 systems are not changed by the test.

## Pass / Fail Checklist

| Check | PASS / FAIL | Notes |
| --- | --- | --- |
| Persistent top-right `Log` button visible after startup |  |  |
| `Log` button text visible |  |  |
| Empty Log opens before dialogue |  |  |
| Empty state shows `No dialogue yet.` |  |  |
| NPC / approved trigger starts dialogue |  |  |
| First displayed line creates one log entry |  |  |
| Log entry shows speaker name |  |  |
| Log entry shows dialogue text |  |  |
| Multiple dialogue lines appear in order |  |  |
| Same node refresh does not duplicate current line |  |  |
| Persistent Log opens after bottom DialoguePanel is closed |  |  |
| Snapshot load restores log entries |  | Validator or Editor-only hook required |
| Snapshot load does not advance narrative state |  | Validator or Editor-only hook required |
| Snapshot load does not duplicate restored current line |  | Validator or Editor-only hook required |
| Snapshot export keeps newest 200 entries only |  | Validator or Editor-only hook required |
| Log open / closed state is not saved |  | Validator or Editor-only hook required |
| Scroll position is not saved |  | Validator or Editor-only hook required |
| Phase3 / Phase6 behavior is unaffected |  |  |

## Regression Watchlist

- Runtime surface appears but top-right persistent `Log` button is missing.
- Button labels are invisible due to font, color, alignment, or overflow settings.
- Log panel is parented under the bottom DialoguePanel and becomes vertically compressed.
- Opening Log while no dialogue is active creates a fake log entry.
- Closing Log advances dialogue or clears current text.
- Speaker name falls back unexpectedly to `Narrative` for known CH01 NPC nodes.
- Repeated `SetCurrentNode` or UI refresh appends duplicate entries.
- Snapshot load replays nodes instead of importing log entries.
- Snapshot load calls Dialogue Next or moves from `ORDER_001_ACCEPT` into pass state.
- Snapshot saves Log panel open state or scroll position.
- Snapshot export grows without the 200-entry cap.
- Any test step mutates Phase3 / Phase6 scenes, data assets, or ProjectSettings.

## Known Out-of-Scope Items

- No new runtime snapshot save/load UI in P10D-11.
- No runtime debug panel for snapshot testing in P10D-11.
- No independent dialogue-log disk file.
- No Phase3 order data persistence.
- No Phase6 scene, player, NavMesh, or workstation state persistence.
- No `OrderFailed` event type.
- No changes to ORDER_001 progression rules.
- No scene or prefab mutation.
- No ProjectSettings mutation.

## Acceptance Record

P10D-11 is PASS when this document exists and the current P10D-06 through P10D-10 Dialogue Log manual acceptance coverage is reviewable.

Implementation code:

```text
NO
```

Serialized References Changed:

```text
NONE
```

Scene Mutation:

```text
NONE
```
