# P10D Dialogue Log / Galgame Log

## Goal

P10D-06 adds a galgame-style dialogue history log to the Phase10 runtime dialogue surface.

The first pass is runtime-session only. It does not write dialogue history to disk, does not connect to save data, and does not bind to Phase3 or Phase6 systems.

## Runtime Behavior

The dialogue UI includes a `Log` button. When the player opens it, the runtime surface shows a scrollable dialogue history panel for the current session.

Each displayed dialogue line records:

```text
sequence
nodeId
speakerName
dialogueText
```

The log ignores consecutive duplicate entries with the same `nodeId` and `dialogueText`.

## Runtime Layout

`P10_Runtime_DialogueSurface` uses a full-screen RectTransform root so overlay panels can be laid out against the whole screen. The bottom dialogue box is hosted under a separate `DialoguePanel` child.

`DialogueLogPanel` is a direct child of the full-screen runtime surface. It must not inherit the bottom dialogue box height; otherwise the galgame-style history panel becomes compressed and the scrollable review area is too small.

The log panel is a full-screen / large overlay runtime UI. It uses a large upper-screen review area, anchored near full width and above the bottom dialogue box:

```text
anchorMin: (0.06, 0.18)
anchorMax: (0.94, 0.96)
```

When opened, the panel exposes `DialogueLogScroll` as a vertical-only `ScrollRect` with `Viewport` and `Content` bindings so the player can review accumulated dialogue history. Closing the log returns to the currently active dialogue UI without advancing or clearing the current line.

Manual acceptance found that runtime button labels were not visible. P10D-06 now treats button text visibility as an acceptance item: `Log`, `Next`, bottom `Close`, and Dialogue Log `Close` labels must have non-null font, visible color, centered full-stretch RectTransform labels, and overflow settings that do not truncate the button text.

Manual acceptance also found that the Log could only be opened while the dialogue UI was active. The repaired behavior is galgame-style: a persistent runtime `Log` button remains available on the full-screen dialogue surface, so the player can open the history overlay even when the bottom dialogue box is hidden. Opening or closing the log does not advance narrative state, clear history, or create duplicate entries.

The log records only dialogue lines that have been displayed through `SetCurrentNode` / current node refresh. Each entry must clearly show the speaker NPC / role name above the dialogue text. Speaker resolution uses `P10CharacterDataSO.DisplayName` when available, then falls back to `P10DialogueCatalog.GetSpeakerNameForNode(nodeId)`. The meta line retains sequence and `nodeId`.

Expected runtime hierarchy:

```text
P10_Runtime_DialogueSurface
├─ DialoguePanel
│  └─ Panel / Speaker / Dialogue / Log / Next / Close
└─ DialogueLogPanel
   └─ DialogueLogScroll / Viewport / Content
```

## Boundaries

```text
Serialized References Changed: NONE
Scene Mutation: NONE
Disk writes: NONE
Save integration: NONE
Phase3 / Phase6 coupling: NONE
OrderFailed event type: NOT ADDED
```

## Validation

P10D-06 adds an Editor validation entry:

```text
Phase10_Narrative.P10D06DialogueLogValidator.RunP10D06DialogueLogValidation
```

The validator creates temporary Editor objects only. It does not save or mutate a Unity scene.

Validation covers:

```text
Prologue display records one log entry.
Prologue -> Tutorial records two log entries.
Repeated SetCurrentNode for the same node/text does not duplicate the log.
Dialogue log panel open and close methods are callable.
```
