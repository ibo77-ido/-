# P10D-20R8 Fixed Dialogue Text Slots and Speaker Prefix Cleanup

Status: PASS by automation; needs human PlayMode recheck.

## User Requirement

The runtime bottom dialogue box must no longer write speaker prefixes into the body text.

The bottom dialogue UI uses two fixed text slots shared by all dialogue lines:

- `P10_DialogueSpeakerText`: speaker display name only, such as `徐老伯`, `旁白`, `系统提示`, or `UI结果`.
- `P10_DialogueBodyText`: pure dialogue body only, without `徐老伯：`, `旁白：`, `系统提示：`, or `UI结果：`.

These objects are created once by the runtime dialogue surface and reused for every `DialogueLine`. C# updates only their `.text` values when the current node or line changes.

## Repair

`P10DialogueController` now treats the bottom dialogue box as a structured speaker/body surface:

- `Build()` creates stable fixed objects named `P10_DialogueSpeakerText` and `P10_DialogueBodyText`.
- `SetDialogue()` assigns `speakerText.text` from the current line speaker display name.
- `SetDialogue()` assigns `dialogueText.text` from the current line body after player-facing term localization and speaker-prefix cleanup.
- The visible body slot no longer concatenates `SpeakerDisplayName + "：" + DialogueText`.
- Dialogue Log remains structured as `说话人：...` and `内容：...`, while the Log content body also avoids duplicated speaker prefixes.

The prologue first line was corrected to keep one pure body line:

```text
父亲走的那年，窑炉最后一次点火。
```

No node ids, transition ids, task gates, or Chapter 1 flow rules were changed.

## Prefix Cleanup Rule

Runtime visible body text is normalized as pure body text:

- If imported dialogue text starts with a known speaker prefix, the prefix is stripped for the bottom body slot.
- Known prefixes include `旁白：`, `徐老伯：`, `周掌柜：`, `陈书院：`, `卢客：`, `系统提示：`, `UI结果：`, and `玩家：`.
- Half-width `:` variants are also stripped.
- Player-facing term localization from P10D-20R7 remains active, but it does not re-add speaker prefixes.

Dialogue assets were checked for speaker-prefixed `DialogueText` values. No broad node expansion or progression edits were made.

## Editor Manual Movement

The two runtime Text objects are stable fixed slots, so they can be moved later in Editor or migrated to serialized prefab/scene references:

- Move `P10_DialogueSpeakerText` to adjust the speaker anchor.
- Move `P10_DialogueBodyText` to adjust the body anchor.
- Runtime refreshes update text content only and do not recreate these objects during Next/Continue.
- If the surface is later converted to prefab or scene binding, keep these references serialized so user-authored RectTransform placement is preserved.

## Validator

Unity batchmode execute method:

```text
Phase10_Narrative.P10D14Chapter1FlowHarnessValidator.RunP10D20R8FixedDialogueTextSlotsAndSpeakerPrefixCleanupValidation
```

Required pass log:

```text
P10D-20R8 Fixed Dialogue Text Slots and Speaker Prefix Cleanup validation passed.
```

The validator checks:

- Bottom visible speaker slot exists, is active, visible, and named `P10_DialogueSpeakerText`.
- Bottom visible body slot exists, is active, visible, and named `P10_DialogueBodyText`.
- The same Text component instances are reused across Next/Continue and direct node display.
- Prologue line 1 shows speaker `旁白` and body `父亲走的那年，窑炉最后一次点火。`.
- Xu Laobo, system prompt, and UI result lines update the fixed slots without body speaker prefixes.
- Body text does not contain known speaker prefixes.
- Dialogue Log entries keep structured speaker/body data without duplicated prefixes in body content.
- P10D-19 Chinese vertical slice, P10D-20R5/R6 visible dialogue, and P10D-20R7 Log layout/localization regressions still pass.

## Validation Evidence

- `dotnet build Phase10_Narrative.csproj`: PASS, 0 warnings / 0 errors.
- Unity batchmode validator:
  - Log: `Logs/P10D20R8_FixedDialogueTextSlotsAndPrefixCleanup.log`
  - Result: PASS
  - Required pass text present: `P10D-20R8 Fixed Dialogue Text Slots and Speaker Prefix Cleanup validation passed.`

Unity caveats:

- The first batchmode attempt was blocked because the project was already open in a visible Unity Editor.
- After closing that visible Editor, batchmode recompiled scripts and emitted the required pass text.
- Existing Phase6 warnings appeared during Unity script compilation, but no R8 validator failure was logged.
- A residual no-window Unity process remained visible to `Get-Process`, but it did not block the final validator pass.

## Manual Recheck

Human PlayMode recheck is still required:

- Confirm the bottom speaker text is visibly separate from the body text.
- Confirm the bottom body text has no speaker prefix.
- Confirm Next/Continue reuses the same speaker/body slots.
- Confirm Log remains readable and structured.
- Confirm one bottom dialogue box still displays only one `DialogueLine`.
