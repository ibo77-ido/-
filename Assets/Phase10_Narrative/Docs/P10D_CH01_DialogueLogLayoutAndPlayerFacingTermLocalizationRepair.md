# P10D-20R7 Dialogue Log Layout and Player-Facing Term Localization Repair

Status: PASS by automation; needs human PlayMode recheck.

## Manual Failure Cause

P10D-20R5 and P10D-20R6 fixed visible speaker prefixes and fuller bottom dialogue text, but manual PlayMode review still found two player-facing issues:

- The Dialogue Log panel content was too tight, with minimal padding and cramped entry spacing.
- Player-visible dialogue and log text exposed technical English identifiers such as `ShapeScore`, `GlazeScore`, `GLAZE_002`, and `CODEX_007`.

## Repair

Runtime Log layout in `P10DialogueController` now leaves more readable space:

- The title, close button, scroll area, viewport, and content area use larger margins.
- The Log content `VerticalLayoutGroup` has stronger left/right/top/bottom padding and wider entry spacing.
- Each entry uses larger internal spacing.
- The close button no longer shares the content viewport area.
- Log meta is weakened and now shows only `记录 #N`, avoiding player-facing node ids.

Player-visible term localization was handled in two layers:

- Chapter 1 dialogue `DialogueText` lines were localized from technical terms to Chinese player-facing names.
- Runtime bottom dialogue text and Dialogue Log line text pass through a player-facing localization helper, so old imported text or fallback text does not show bare technical identifiers.

Internal ids such as `NodeId`, `OrderId`, `ConditionKey`, and transition ids were not changed.

## Validator

Unity batchmode execute method:

```text
Phase10_Narrative.P10D14Chapter1FlowHarnessValidator.RunP10D20R7DialogueLogLayoutAndPlayerFacingTermLocalizationValidation
```

Required pass log:

```text
P10D-20R7 Dialogue Log Layout and Player-Facing Term Localization validation passed.
```

The validator checks:

- Dialogue Log panel, ScrollRect, viewport, content padding, and entry spacing.
- Close button does not overlap the content viewport.
- Log entry speaker, line, and meta Text objects are active, visible, non-zero size, wrapping horizontally, and not vertically truncated.
- Bottom visible dialogue text and Log entry visible text do not expose bare technical terms.
- Log still visibly records `说话人：` and `内容：`.
- P10D-20R5 speaker-visible and P10D-20R6 complete-visible-text regressions still pass.
- P10D-19 Chinese vertical slice coverage still passes through the Chapter 1 flow validation.

## Validation Evidence

- `dotnet build Phase10_Narrative.csproj`: PASS, 0 warnings / 0 errors.
- Unity batchmode validator:
  - Log: `Logs/P10D20R7_DialogueLogLayoutAndPlayerFacingTermLocalization.log`
  - Result: PASS
  - Required pass text present: `P10D-20R7 Dialogue Log Layout and Player-Facing Term Localization validation passed.`

Unity caveats:

- The first batchmode attempt was blocked by an already-open Unity Editor for this project.
- A later batchmode run refreshed/recompiled scripts and did not reach executeMethod output, so it was stopped and rerun.
- Final rerun emitted the required pass text and exited successfully.
- Licensing/access-token, Phase6 compile warnings, and mono shutdown warnings appeared, but no validator failure was logged.

## Manual Recheck

Human PlayMode recheck is still required:

- Confirm Log panel text has comfortable padding and entry spacing.
- Confirm close button does not cover Log content.
- Confirm bottom dialogue and Log content show Chinese player-facing terms instead of raw technical ids.
- Confirm dialogue progression remains one visible `DialogueLine` per box.
