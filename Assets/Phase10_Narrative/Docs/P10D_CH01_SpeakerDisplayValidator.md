# P10D-20R3 Speaker Display Validator

Status: PASS by automation validation.

## Scope

P10D-20R3 verifies the bottom runtime dialogue speaker label, not only the Dialogue Log.

Covered:

- All current 13 Chapter 1 `P10_CH01_NODE_*` dialogue assets.
- Every existing `DialogueLines` entry in those assets.
- Runtime `P10DialogueRuntimeSurface` bottom `Speaker` text.
- Runtime `Dialogue` text beside the speaker label.
- Dialogue Log parity with the same displayed line.

Excluded:

- StoryMain R4 line-prefix asset import.
- Phase3 / Phase6 formal gameplay order systems.
- Workshop scene, NavMesh, ProjectSettings, or serialized scene references.
- Runtime code behavior changes.

## Validator Entry

Unity batchmode execute method:

```text
Phase10_Narrative.P10D14Chapter1FlowHarnessValidator.RunP10D20R3SpeakerDisplayValidation
```

Required pass log:

```text
P10D-20R3 Speaker Display validation passed.
```

## Acceptance

- Bottom speaker label equals the line-level `SpeakerCharacterId` resolved `P10CharacterDataSO.DisplayName`.
- Bottom dialogue text equals the current `DialogueLines[lineIndex].DialogueText`.
- One dialogue box contains one line only.
- Dialogue Log latest entry matches the same speaker and line shown at the bottom.
- Validator observes multiple speaker label transitions across Chapter 1.
- Existing P10D-19/P10D-20R2 flow and snapshot coverage remains reusable.

## Mutation Summary

- Runtime code modified: NO.
- Dialogue assets modified: NO.
- Character assets modified: NO.
- Scene mutation: NONE.
- Serialized references changed: NONE.

## Validation Evidence

- `dotnet build Phase10_Narrative.csproj`: PASS, 0 warnings / 0 errors.
- Unity batchmode validator:
  - Log: `Logs/P10D20R3_SpeakerDisplayValidator_latest.log`
  - Result: PASS
  - Required pass text: `P10D-20R3 Speaker Display validation passed.`
- Unity log caveat: licensing/access-token and curl shutdown warnings appeared, but the validator emitted the required pass message and the command exited with code 0.
