# P10D-09 Dialogue Log Snapshot Extension Design

## Current Status

P10D-06 Dialogue Log / Galgame Log is accepted as runtime-session UI.

Current runtime behavior:

```text
Records only displayed dialogue lines.
Stores sequence, nodeId, speakerName, dialogueText.
Ignores consecutive duplicate nodeId + dialogueText entries.
Can be opened from the persistent runtime Log button, including non-dialogue state.
Displays speakerName for each entry.
Does not write disk data.
Does not connect to Phase3 or Phase6.
```

P10D-07 froze the save snapshot scope as docs-only. P10D-08 decided that snapshot persistence is acceptable only as a future Phase10-owned snapshot extension after versioning, restore semantics, duplicate prevention, and max-size policy are explicitly defined.

P10D-09 is a docs-only design freeze. It does not implement runtime snapshot code.

## Existing Snapshot Architecture

Phase10 already owns a small narrative snapshot surface:

```text
P10NarrativeSnapshot
P10NarrativeStateMachine.SaveSnapshot()
P10NarrativeStateMachine.LoadSnapshot(P10NarrativeSnapshot snapshot)
P10NarrativeManager.SaveSnapshot()
```

Current `P10NarrativeSnapshot` fields:

```text
SnapshotVersion
ChapterState
CurrentNodeId
PlayedNodeIds
NarrativeFlags
```

Current snapshot version:

```text
1
```

Dialogue Log is not currently part of `P10NarrativeSnapshot`. Runtime log entries currently live inside `P10DialogueController`, not in `P10NarrativeStateMachine`.

## Snapshot Version

If P10D-10 implements Dialogue Log snapshot persistence, the Phase10 snapshot version should change from:

```text
1 -> 2
```

Reason:

```text
The snapshot payload shape changes by adding dialogue log data.
Version 2 gives validators and future loaders a concrete compatibility branch.
```

Version 1 snapshots must remain loadable.

## V1 Snapshot Load Behavior

When loading a version 1 snapshot, or any snapshot with no dialogue log payload:

```text
Dialogue log restores as empty.
Narrative state, current node, played nodes, and flags restore with existing v1 behavior.
No dialogue nodes are replayed to reconstruct log history.
No synthetic log entries are created during load.
```

This preserves compatibility and prevents restore from mutating story progression.

## Dialogue Log Entry Schema

Future snapshot entry DTO:

```text
int sequence
string nodeId
string speakerName
string dialogueText
```

The schema mirrors the P10D-06 runtime entry contract. It should contain only displayed dialogue history and should not encode UI state or old-system gameplay state.

## Not Saved

The snapshot must not save:

```text
Log panel open / closed state: NO
Scroll position: NO
Button state: NO
Canvas visibility: NO
Dialogue panel visibility: NO
Transient layout state: NO
```

After restore, runtime UI should be rebuilt from normal runtime rules. The saved log data should only provide the list of historical entries.

## Restore Semantics

Restore should import dialogue log entries directly into the Phase10 dialogue log owner.

Required restore behavior:

```text
Clear the current runtime dialogue log before importing saved entries.
Import saved entries without sending narrative events.
Import saved entries without calling Dialogue Next.
Import saved entries without advancing state.
Import saved entries without rebuilding history by replaying nodes.
Allow empty dialogue log.
```

After import:

```text
nextDialogueLogSequence = max(restored sequence) + 1
```

If restored entries are empty:

```text
nextDialogueLogSequence = 1
```

If any restored entry has invalid or duplicate sequence values, the implementation should normalize the in-memory sequence order during import rather than failing load. The saved order should be the source of truth; normalized sequence values may become `1..N` in memory.

## Duplicate Prevention After Restore

P10D-06 duplicate prevention is consecutive `nodeId + dialogueText`.

Future implementation must keep that rule and apply it after restore:

```text
The last restored entry becomes the duplicate comparison baseline.
If the current restored node refreshes immediately after load and matches last restored nodeId + dialogueText, do not append a duplicate.
If a later displayed line differs by nodeId or dialogueText, append normally.
```

Implementation options for P10D-10:

```text
Option A: restore entries into dialogueLogEntries before the first post-load RefreshRuntimeSurface() call.
Option B: add an explicit import flag that suppresses one matching current-node append after snapshot load.
```

Preferred design:

```text
Option A, if lifecycle ordering is controllable.
Option B only if runtime load ordering causes an unavoidable current-node refresh before import.
```

## Max Log Size

Future snapshot persistence should cap saved dialogue log entries.

Frozen first implementation policy:

```text
Max saved entries: 200
Overflow behavior: keep newest entries, drop oldest entries.
```

Reason:

```text
The log is player-facing recent history. Keeping newest entries avoids unbounded snapshot growth and preserves the most relevant review window.
```

When pruning, sequence values may remain as originally recorded in saved data. On restore, in-memory sequence may either preserve saved sequence values or normalize to `1..N`, but `nextDialogueLogSequence` must still be greater than the highest active in-memory sequence.

## Speaker Name Rules

Empty log is allowed.

Missing speakerName should be tolerated during load but normalized for runtime display:

```text
If speakerName is missing or whitespace, use "Narrative".
If nodeId can resolve through P10CharacterDataSO or P10DialogueCatalog in a future implementation, that resolved value may replace the fallback.
```

The snapshot should store `speakerName` as displayed at the time the line was recorded. It should not require Phase3 or Phase6 lookup to restore.

## Accepted Scope

Accepted for P10D-10 implementation if approved:

```text
Add a Phase10-owned dialogue log payload to P10NarrativeSnapshot.
Bump snapshot version to 2.
Load v1 snapshots with empty dialogue log.
Save only sequence, nodeId, speakerName, dialogueText.
Cap saved log entries at 200.
Keep newest entries when over capacity.
Restore log entries directly without replaying narrative progression.
Keep duplicate prevention based on consecutive nodeId + dialogueText.
Add validators for v1 load, v2 round trip, duplicate prevention, and max-size pruning.
```

## Rejected Scope

Rejected for the first snapshot extension:

```text
Saving UI state.
Saving scroll position.
Saving Log panel open / closed state.
Saving button state.
Saving Phase3 order data.
Saving Phase6 scene or NavMesh state.
Writing an independent dialogue-log disk file.
Replaying dialogue nodes during restore.
Adding OrderFailed event type.
Changing ORDER_001 progression rules.
```

## Phase Boundaries

Dialogue Log snapshot persistence must remain Phase10-owned.

```text
Phase3 coupling: NO
Phase6 coupling: NO
Independent disk file: NO
Scene mutation: NO
Serialized reference changes in P10D-09: NONE
Runtime code implementation in P10D-09: NO
```

## Risks

| Risk | Design Response |
| --- | --- |
| Old snapshot compatibility | Bump to version 2 and treat v1 missing log as empty. |
| Duplicate current-node record after restore | Last restored entry is duplicate baseline; suppress matching immediate refresh if needed. |
| Save size growth | Save max 200 entries; keep newest entries. |
| Speaker name missing in older or malformed data | Normalize to `Narrative` or resolved Phase10 speaker name during import. |
| UI state leakage | UI state is explicitly excluded. |
| Accidental Phase3 / Phase6 coupling | Snapshot data is Phase10-owned and requires no old-system lookup. |

## Decision

P10D-09 design result:

```text
PASS
Docs-only design freeze.
Runtime implementation: NO
Recommended Next Step: P10D-10 Dialogue Log Snapshot Extension Implementation
```

P10D-10 may implement the extension if approved, using snapshot version 2 and the restore / capacity / duplicate rules frozen in this document.

## P10D-10 Implementation Note

P10D-10 implemented the approved Phase10-owned snapshot extension.

Implemented behavior:

```text
P10NarrativeSnapshot.SnapshotVersion = 2.
P10NarrativeSnapshot includes DialogueLogEntries.
DialogueLogEntries save sequence, nodeId, speakerName, dialogueText.
P10DialogueController exports at most 200 newest log entries.
P10DialogueController imports snapshot entries without UI state.
P10NarrativeManager.SaveSnapshot() attaches dialogue log entries.
P10NarrativeManager.LoadSnapshot() restores narrative state and imports dialogue log entries.
Version 1 snapshots restore dialogue log as empty.
Restore does not replay nodes, send narrative events, trigger Dialogue Next, or advance state.
Restored last entry remains the duplicate baseline for the next displayed line.
```

Validation:

```text
dotnet build Phase10_Narrative.csproj: 0 warnings / 0 errors
Unity validator: Phase10_Narrative.P10D06DialogueLogValidator.RunP10D10DialogueLogSnapshotValidation
Unity log path: D:\UnityGame\director-true-Gong\Logs\P10D10_DialogueLogSnapshotValidation.log
Unity log contains: P10D-10 Dialogue Log Snapshot Extension validation passed.
```

Boundaries:

```text
Independent disk file: NO
Phase3 coupling: NO
Phase6 coupling: NO
UI state saved: NO
Scroll position saved: NO
Log panel open state saved: NO
Scene mutation: NONE
Serialized reference changes: NONE
```
