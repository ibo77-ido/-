# P10D-20R5 Runtime Dialogue Speaker Visible Display Repair

Status: PASS by automation; needs human PlayMode recheck.

## Manual Failure Cause

P10D-20R3 passed automation, but human PlayMode acceptance failed:

- Dialogue Log speaker names were correct.
- The in-game bottom dialogue box still did not visibly show who was speaking.
- R3 validated internal `Speaker` Text values and log parity, but did not require the bottom dialogue body Text to visibly contain the speaker.

## Repair

Chosen repair: Option A, visible dialogue body includes the speaker prefix.

Runtime display now formats the bottom dialogue body as:

```text
SpeakerDisplayName：DialogueLine
```

Examples:

```text
旁白：父亲走的那年，窑炉最后一次点火。
徐老伯：来了。昨晚睡得着？
系统提示：新订单：周掌柜的甜白釉茶碗（ORDER_001）已接受。
UI结果：获得 50 铜钱，声望 +10。
```

The separate speaker label is still assigned, but manual acceptance no longer depends on that label being noticed or unobscured.

The runtime surface root `RectTransform` now also receives a stable fallback size of `1280 x 720` so validator checks can prove visible text objects have non-zero geometry.

## Validator

Unity batchmode execute method:

```text
Phase10_Narrative.P10D14Chapter1FlowHarnessValidator.RunP10D20R5RuntimeDialogueSpeakerVisibleDisplayValidation
```

Required pass log:

```text
P10D-20R5 Runtime Dialogue Speaker Visible Display validation passed.
```

Validation covers visible bottom dialogue UI, not only log data:

- Prologue line 0 starts with `旁白：`.
- Prologue line 1 starts with `徐老伯：`.
- `ORDER_001_ACCEPT` starts with `周掌柜：`.
- `ORDER_003_ACCEPT` starts with `陈书院：`.
- `ORDER_004_ACCEPT` starts with `卢客：`.
- The checked Text GameObject is active in hierarchy.
- Text alpha is greater than 0.
- RectTransform width and height are greater than 0.
- Text is read from `P10_Runtime_DialogueSurface/DialoguePanel/Panel/Dialogue`, not from Dialogue Log UI.
- Each visible dialogue text contains exactly one full-width speaker separator.

## Validation Evidence

- `dotnet build Phase10_Narrative.csproj`: PASS, 0 warnings / 0 errors.
- Unity batchmode validator:
  - Log: `Logs/P10D20R5_RuntimeDialogueSpeakerVisibleDisplay.log`
  - Result: PASS
  - Required pass text present: `P10D-20R5 Runtime Dialogue Speaker Visible Display validation passed.`

Unity caveats:

- One batchmode run was blocked by an open Unity Editor instance and produced a project-already-open fatal error.
- The blocking Unity Editor process was closed, then the validator was rerun.
- One compile-refresh run was stopped after script reload did not reach executeMethod output.
- Final rerun emitted the required pass text and exited with code 0.
- Licensing/access-token and curl shutdown warnings appeared, but no validator failure was logged.

## Mutation Summary

- Runtime code modified: YES, limited to `P10DialogueController` runtime surface display formatting and root RectTransform fallback size.
- Dialogue assets modified: NO.
- Character assets modified: NO.
- New node added: NO.
- Task progression rules changed: NO.
- Scene mutation: NONE.
- Serialized references changed: NONE.
- Stage / commit / push: NO.

## Manual Recheck

Human PlayMode recheck is still required because the original failure was visual:

- Confirm the bottom dialogue box body visibly shows `旁白：...`, `徐老伯：...`, `周掌柜：...`, `陈书院：...`, and `卢客：...`.
- Confirm one click of Continue shows one line only.
- Confirm no bottom dialogue box combines multiple speakers.
