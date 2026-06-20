# P10D-20R9B Clearly Visible Dialogue Text Slot Objects

Status: PASS by automation; needs human Editor/PlayMode recheck.

## Manual Failure Cause

P10D-20R9 passed automation, but human review still failed because the reported `P10_DialogueSpeakerText` and `P10_DialogueBodyText` path was too deep and did not match the user's operation expectation. The slots were not clearly discoverable as manual placement objects.

The R9B setup also exposed an automation gap: Unity reported a prefab save problem caused by a missing script component, but the earlier setup/validator did not turn that into a hard failure. R9B now removes missing scripts from the Phase10 UI prefab before save and checks the saved YAML on disk.

## Implementation

R9B uses solution A: Phase10-owned prefab / overlay scene visible UI slots.

Prefab asset:

```text
Assets/Phase10_Narrative/Prefabs/UI/P10_CH01_DialogueUI_Placeholder.prefab
```

Prefab Mode hierarchy:

```text
P10_CH01_DialogueUIRoot
  P10_CH01_DialogueManualSlots
    P10_DialogueSpeakerText
    P10_DialogueBodyText
```

Overlay scene:

```text
Assets/Phase10_Narrative/Scenes/P10_CH01_NarrativeOverlay.unity
```

Scene Hierarchy path when the overlay scene is open:

```text
P10_CH01_NarrativeRoot/P10_CH01_DialogueUIRoot/P10_CH01_DialogueManualSlots/P10_DialogueSpeakerText
P10_CH01_NarrativeRoot/P10_CH01_DialogueUIRoot/P10_CH01_DialogueManualSlots/P10_DialogueBodyText
```

`P10_CH01_DialogueManualSlots` is a direct child of the prefab root, has its own Screen Space Overlay Canvas, and contains the two fixed Text objects directly. Both Text objects have `UnityEngine.UI.Text`, `CanvasRenderer`, and `RectTransform`.

## Manual Movement

To move the fixed slots in Editor:

1. Open `Assets/Phase10_Narrative/Prefabs/UI/P10_CH01_DialogueUI_Placeholder.prefab`.
2. Search for `P10_DialogueSpeakerText` or `P10_DialogueBodyText`.
3. Move or resize the selected object's `RectTransform`.
4. Save the prefab.

The overlay scene uses the Phase10 prefab instance, so saved prefab placement is preserved outside PlayMode. Runtime code updates only `Text.text`; it does not reset `RectTransform` anchor, offset, size, scale, or anchored position during `SetCurrentNode`, refresh, Next, or Continue.

## Runtime Binding

`P10DialogueController` now prefers the serialized/editor-authored slots:

- `speakerText` is bound to `P10_CH01_DialogueManualSlots/P10_DialogueSpeakerText`.
- `bodyText` is bound to `P10_CH01_DialogueManualSlots/P10_DialogueBodyText`.
- If serialized references are missing, the controller searches the shallow manual slot root first.
- Runtime-created fallback is allowed only if prefab/slots are missing, logs a warning, and is not considered a replacement for manual acceptance.

## Validator

Unity batchmode execute method:

```text
Phase10_Narrative.P10D14Chapter1FlowHarnessValidator.RunP10D20R9BClearlyVisibleDialogueTextSlotObjectsValidation
```

Required pass log:

```text
P10D-20R9B Clearly Visible Dialogue Text Slot Objects validation passed.
```

The validator checks:

- Prefab has shallow direct-child root `P10_CH01_DialogueManualSlots`.
- Exactly one `P10_DialogueSpeakerText` and exactly one `P10_DialogueBodyText` exist.
- Both slots are active, not hidden by `HideFlags`, have `Text`, and have movable, non-zero-scale `RectTransform`s.
- Saved prefab YAML contains the shallow root and both slot names, and contains no `m_Script: {fileID: 0}` missing script component.
- Runtime uses prefab/editor-visible slots, not runtime-created fallback slots.
- Refresh and Next keep the same Text references.
- Refresh and Next do not reset moved `RectTransform.anchoredPosition`.
- Speaker slot displays the speaker name.
- Body slot displays pure body text without speaker prefix.
- R8 fixed-slot behavior, R7 Log/layout localization, R6 completeness, R5 visible speaker, and P10D-19 Chinese vertical slice regressions still pass.

## Validation Evidence

- `dotnet build Phase10_Narrative.csproj`: PASS, 0 warnings / 0 errors.
- Unity setup:
  - Method: `Phase10_Narrative.P10D14Chapter1FlowHarnessValidator.SetupP10D20R9BClearlyVisibleDialogueTextSlotObjects`
  - Log: `Logs/P10D20R9B_SetupClearlyVisibleDialogueTextSlots.log`
  - Result: PASS
- Unity validator:
  - Method: `Phase10_Narrative.P10D14Chapter1FlowHarnessValidator.RunP10D20R9BClearlyVisibleDialogueTextSlotObjectsValidation`
  - Log: `Logs/P10D20R9B_ClearlyVisibleDialogueTextSlots.log`
  - Result: PASS
  - Required pass text present: `P10D-20R9B Clearly Visible Dialogue Text Slot Objects validation passed.`

## Manual Recheck

Still requires human Editor/PlayMode recheck:

- Confirm both Text objects are directly searchable in Prefab Mode.
- Confirm the overlay scene instance expands to the same manual slot path.
- Confirm moving/saving the prefab Text `RectTransform`s changes runtime placement.
- Confirm bottom body text remains prefix-free and one dialogue box still shows one `DialogueLine`.
