# P10D-08 Dialogue Log Snapshot Extension Decision

## Current Status

P10D-06 implemented an in-session Dialogue Log / Galgame Log surface.

The current dialogue log behavior is:

```text
Runtime-session only.
Records only displayed dialogue lines.
Stores sequence, nodeId, speakerName, dialogueText.
Ignores consecutive duplicate nodeId + dialogueText entries.
Can be opened from a persistent runtime Log button.
Does not write disk data.
Does not connect to save data.
Does not connect to Phase3 or Phase6.
```

P10D-07 froze the save snapshot scope as docs-only and identified the smallest candidate payload for any later snapshot extension.

## Existing Save/Snapshot Architecture

Phase10 already has a Phase10-owned snapshot type and state machine save / restore methods:

```text
P10NarrativeSnapshot
P10NarrativeStateMachine.SaveSnapshot()
P10NarrativeStateMachine.LoadSnapshot(P10NarrativeSnapshot snapshot)
P10NarrativeManager.SaveSnapshot()
```

The existing snapshot is versioned with `SnapshotVersion = 1`.

Current snapshot data:

```text
SnapshotVersion
ChapterState
CurrentNodeId
PlayedNodeIds
NarrativeFlags
```

The existing snapshot architecture is Phase10-owned and does not require Phase3 or Phase6 references. However, it has no dialogue log field, no restored dialogue log semantics, and no migration path for older snapshots that do not contain log data.

## Candidate Data

If dialogue log data is added later, the accepted candidate payload remains the P10D-06 line contract:

```text
sequence
nodeId
speakerName
dialogueText
```

The candidate data should represent displayed dialogue history only. It should not include future nodes, pending node choices, UI overlay state, scroll position, button state, or canvas visibility.

## Decision

P10D-08 decision: do not implement dialogue log snapshot persistence in this task.

Dialogue log snapshot persistence is suitable for a future Phase10-owned snapshot extension, but it should not be added immediately in P10D-08 because the current snapshot is version 1 and lacks explicit migration, restore, duplicate prevention, and maximum-size policy.

Recommended decision path:

```text
P10D-08: PASS as docs-only decision freeze.
P10D-09: implement only if versioning, restore semantics, duplicate prevention, and size limits are explicitly approved.
```

## Accepted Scope

Accepted for a future implementation:

```text
Add optional dialogue log payload to Phase10-owned snapshot.
Keep candidate fields to sequence, nodeId, speakerName, dialogueText.
Treat missing dialogue log data in old snapshots as an empty log.
Restore log entries without replaying narrative progression.
Preserve P10D-06 consecutive duplicate prevention for new runtime records.
Keep UI state out of snapshot.
Keep Phase3 and Phase6 out of the implementation.
```

## Rejected Scope

Rejected for P10D-08 and any first snapshot extension:

```text
Runtime code implementation in P10D-08.
Independent dialogue-log disk file.
Saving Log panel open / closed state.
Saving scroll position.
Saving button state.
Saving canvas or layout state.
Replaying dialogue nodes during restore to rebuild log.
Phase3 or Phase6 coupling.
ProjectSettings or scene mutation.
OrderFailed event type.
```

## Risks

| Risk | Assessment | Mitigation Required Before Implementation |
| --- | --- | --- |
| Snapshot compatibility | Existing snapshots are version 1 and have no dialogue log field. | Add optional field or version-gated migration before implementation. |
| Duplicate log records | Restored logs plus current node refresh may append duplicate current dialogue lines. | Define restore flag or duplicate guard before implementation. |
| Narrative progression side effects | Rebuilding log by replaying nodes could advance or mutate state. | Restore log data directly; never replay narrative events to rebuild history. |
| Save size growth | Long sessions can accumulate many dialogue lines. | Define cap, pruning, or compression policy before implementation. |
| UI state leakage | Saving overlay state can restore stale UI conditions. | Persist data only; rebuild UI from runtime state. |
| Phase3 / Phase6 coupling | Old gameplay systems must not own narrative log persistence. | Keep implementation inside Phase10 snapshot only. |

## Next Step Recommendation

Proceed to:

```text
P10D-09 Dialogue Log Snapshot Extension Implementation Decision / Design
```

P10D-09 should decide whether to implement the snapshot extension now. If approved, it should first define:

```text
Snapshot version or optional-field migration policy.
Dialogue log entry DTO shape.
Maximum retained log entries.
Restore behavior for old snapshots.
Duplicate prevention after restore.
Validation coverage for save / load round trip.
```

No runtime snapshot code should be changed by P10D-08.
