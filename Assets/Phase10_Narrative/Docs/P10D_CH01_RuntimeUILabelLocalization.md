# P10D-18 CH01 Runtime UI Chinese Label Localization

## Scope

P10D-18 localized Phase10 runtime UI text that is directly player-facing in the dialogue surface and Dialogue Log.

Runtime code was changed only under:

```text
Assets/Phase10_Narrative/Scripts/UI/P10DialogueController.cs
```

Editor validation was updated under:

```text
Assets/Phase10_Narrative/Scripts/Editor/P10D06DialogueLogValidator.cs
```

No Phase3, Phase6, Phase8, scene, or ProjectSettings file was modified by this task.

## Localized UI Text

| Previous text | Chinese text | Surface |
|---|---|---|
| `Log` | `记录` | Bottom dialogue panel button and persistent top-right log button |
| `Next` | `继续` | Bottom dialogue panel advance button |
| `Close` | `关闭` | Bottom dialogue close button and Dialogue Log close button |
| `Dialogue Log` | `对话记录` | Dialogue Log panel title |
| `No dialogue yet.` | `暂无对话记录` | Empty Dialogue Log state |
| empty speaker fallback `Narrative` | `旁白` | Runtime speaker fallback for missing/blank speaker names |
| log entry speaker field | `说话人：...` | Dialogue Log entry view |
| log entry line field | `内容：...` | Dialogue Log entry view |
| log entry technical meta prefix | `记录 #... 节点 ...` | Dialogue Log entry meta line |

The runtime object names remain stable, including `LogLabel`, `NextLabel`, `CloseLabel`, `Speaker`, `Line`, and `Meta`, so existing validator hierarchy paths remain usable.

## Technical Fields Kept

These fields remain technical and are intentionally not fully localized:

```text
NodeId values such as P10_CH01_NODE_TUTORIAL_01
OrderId values such as ORDER_001
Asset/object hierarchy names such as DialogueLogPanel and LogButton
Validator/internal exception messages
P10NarrativeDebugPanel dev labels
P10DialogueCatalog hard-coded fallback text
```

`P10DialogueCatalog` was not synchronized in P10D-18. The validator now compares visible restored dialogue against the runtime log entry generated from the active dialogue asset, not against `P10DialogueCatalog.GetDialogueText`.

## Runtime Behavior Boundaries

P10D-18 does not change:

```text
Dialogue node IDs
Dialogue node count
NextNodeIds
Order progression rules
Dialogue Log snapshot schema
Snapshot version
Save/load semantics
UI open/closed state persistence
Phase3 / Phase6 / Phase8 / scene bindings
Yu bridge contracts or facts
```

Opening or closing the localized Dialogue Log remains UI-only and does not advance narrative state.

## Font Risk

The runtime UI still uses the existing built-in font resolution path:

```text
LegacyRuntime.ttf
Arial.ttf fallback
```

P10D-18 does not modify ProjectSettings and does not add an external font package. The P10D18 validator asserts localized text objects have a non-null font. Manual PlayMode visual review is still recommended for final Chinese glyph quality on target machines.

## Validation

Completed checks:

```text
dotnet build Phase10_Narrative.csproj
  PASS, 0 warnings / 0 errors

Unity batchmode validator:
  Method: Phase10_Narrative.P10D06DialogueLogValidator.RunP10D18RuntimeUILabelLocalizationValidation
  Log: Logs/P10D18_RuntimeUILabelLocalization.log
  Result: PASS

Unity batchmode snapshot/log regression:
  Method: Phase10_Narrative.P10D06DialogueLogValidator.RunP10D10DialogueLogSnapshotValidation
  Log: Logs/P10D18_P10D10SnapshotRegression.log
  Result: PASS
```

Validator coverage:

- Bottom dialogue panel labels are `记录`, `继续`, and `关闭`.
- Persistent log button label is `记录`.
- Dialogue Log close button label is `关闭`.
- Dialogue Log title is `对话记录`.
- Empty state is `暂无对话记录`.
- Dialogue Log entry fields use `说话人：` and `内容：`.
- Technical meta uses the localized prefix `记录 #` and `节点`.
- Persistent Log button remains available.
- Opening and closing Log does not advance narrative state.
- Snapshot/log behavior remains compatible with P10D-10.

Unity log caveat:

- The logs contain Unity licensing/access-token and curl shutdown/network warnings.
- The requested validators emitted the expected pass messages and the commands returned exit code `0`.

## Unhandled Items

- `P10DialogueCatalog` still contains English fallback copy and remains a separate technical-debt item.
- `P10NarrativeDebugPanel` is dev/debug UI and remains English.
- Final Chinese font appearance should still be inspected manually in PlayMode before final release acceptance.

## P10D-18 Result

```text
Status: PASS
Runtime code modified: YES
Progression rules changed: NO
P10DialogueCatalog synchronized: NO
Dialogue node expansion: NO
Serialized References Changed: NONE
Scene Mutation: NONE
Stage / commit / push: NO
```
