# P10D-20R6 Runtime Dialogue Text Completeness Repair

Status: PASS by automation; needs human PlayMode recheck.

## Manual Failure Cause

P10D-20R5 passed automation, but human PlayMode acceptance failed because the bottom dialogue box did not present the current `DialogueLine` completely enough for reading.

R5 fixed visible speaker prefixes, but the runtime bottom dialogue body still used a short fixed text region and `VerticalWrapMode.Truncate`, so long Chinese lines could be visually cut even when the assigned Text value was complete.

## Repair

The runtime bottom dialogue UI was adjusted in `P10DialogueController`:

- Bottom `DialoguePanel` height increased from 22% to 34% of screen height.
- Speaker label moved upward to reserve more space for body text.
- Dialogue body Text region expanded vertically.
- Dialogue body font size reduced from 20 to 18.
- `Text.horizontalOverflow` remains `Wrap` for Chinese line wrapping.
- `Text.verticalOverflow` changed from `Truncate` to `Overflow`.
- Visible body text still uses `SpeakerDisplayName：DialogueText`.

The repair keeps the existing line model:

- One dialogue box displays one `DialogueLine`.
- No multiple lines are merged into one box.
- No dialogue assets or node definitions were changed.
- Task progression rules were not changed.

## Validator

Unity batchmode execute method:

```text
Phase10_Narrative.P10D14Chapter1FlowHarnessValidator.RunP10D20R6RuntimeDialogueTextCompletenessValidation
```

Required pass log:

```text
P10D-20R6 Runtime Dialogue Text Completeness validation passed.
```

Validation checks visible bottom dialogue body Text, not Dialogue Log text:

- Exact visible text equals `SpeakerDisplayName：DialogueLine`.
- Visible text contains the complete dialogue body.
- Visible text starts with the expected speaker prefix.
- Text GameObject is active in hierarchy.
- Text color alpha is greater than 0.
- RectTransform width and height are greater than 0.
- `horizontalOverflow == Wrap`.
- `verticalOverflow != Truncate`.
- Current box does not contain other `DialogueLines` from the same node.

Covered text cases:

- Prologue narrator first line.
- Prologue Xu Laobo line.
- Prologue system prompt line.
- Tutorial Xu Laobo long line.
- ORDER_001_ACCEPT Zhou Zhanggui long line.
- ORDER_001_ACCEPT system prompt.
- ORDER_003_ACCEPT Chen Shuyuan long line.
- ORDER_003_ACCEPT system prompt.
- ORDER_004_ACCEPT Lu Ke long line.
- ORDER_004_ACCEPT Xu Laobo warning line.
- ORDER_004_ACCEPT system prompt.

## Validation Evidence

- `dotnet build Phase10_Narrative.csproj`: PASS, 0 warnings / 0 errors.
- Unity batchmode validator:
  - Log: `Logs/P10D20R6_RuntimeDialogueTextCompleteness.log`
  - Result: PASS
  - Required pass text present: `P10D-20R6 Runtime Dialogue Text Completeness validation passed.`

Unity caveats:

- First batchmode run refreshed/recompiled changed scripts and then failed once because the validator initially treated ordinary Chinese colons inside system prompt text as multiple speaker prefixes.
- The validator was corrected to detect known speaker prefixes rather than every full-width colon.
- Final rerun emitted the required pass text and exited with code 0.
- Licensing/access-token and curl shutdown warnings appeared, but no validator failure was logged.

## Mutation Summary

- Runtime code modified: YES, limited to bottom dialogue UI layout and Text overflow behavior.
- Dialogue assets modified: NO.
- Character assets modified: NO.
- New node added: NO.
- Task progression rules changed: NO.
- Scene mutation: NONE.
- Serialized references changed: NONE.
- Stage / commit / push: NO.

## Manual Recheck

Human PlayMode recheck is still required:

- Confirm bottom dialogue body shows complete speaker-prefixed long lines.
- Confirm Continue advances one `DialogueLine` at a time.
- Confirm no bottom dialogue box combines multiple speakers or multiple lines.
