# P10D-07 Dialogue Log Save Snapshot Scope

## Goal

P10D-07 freezes the scope for whether the P10D-06 dialogue log should become part of a Phase10-owned save snapshot.

This task is docs-only. It does not implement persistence, does not write runtime dialogue history to disk, and does not connect the dialogue log to Phase3 or Phase6 systems.

## Current State

P10D-06 dialogue log is runtime-session only.

It records dialogue lines displayed during the current runtime session and is cleared when the runtime session ends. The log exists for player review while playing and is not currently part of any save payload.

## Snapshot Candidate Data

If a later implementation includes dialogue log data in a Phase10 snapshot, the first version should save only the minimum line data:

```text
sequence
nodeId
speakerName
dialogueText
```

This mirrors the P10D-06 runtime log entry contract and avoids adding UI or gameplay ownership to the persistence surface.

## Not Saved

The snapshot must not save runtime UI state:

```text
Log panel open / closed state
Scroll position
Button state
Canvas visibility state
Transient layout state
```

The log UI should reconstruct itself from saved line data only if a future snapshot implementation explicitly adds that data.

## Boundaries

```text
Docs-only scope freeze: YES
Runtime code implementation: NO
Disk writes: NO
Independent dialogue-log file: NO
Save integration in P10D-07: NO
Phase3 coupling: NO
Phase6 coupling: NO
Scene mutation: NO
Serialized references changed: NONE
```

If implemented later, dialogue log persistence should be added only through a Phase10-owned snapshot extension. It must not require Phase3 order data, Phase6 scene state, or any old-system ownership of narrative log entries.

## Risks

| Risk | Impact | Scope Note |
| --- | --- | --- |
| Save size growth | Long play sessions can accumulate many dialogue lines. | A later implementation may need caps or compression rules. |
| Duplicate records | Re-saving or replaying restored entries could duplicate log lines. | Runtime duplicate rules must remain explicit during restore. |
| Old save compatibility | Existing snapshots may not contain dialogue log data. | New fields must be optional or version-gated. |
| Snapshot version migration | Adding log data changes Phase10 snapshot shape. | P10D-08 must decide versioning before code changes. |

## Recommendation

P10D-07 should remain a docs-only scope freeze.

Recommended next task:

```text
P10D-08 Dialogue Log Snapshot Extension Decision
```

P10D-08 should decide whether to implement a Phase10-owned snapshot extension, and if yes, define versioning, restore semantics, duplicate prevention, and maximum log size before touching runtime code.
