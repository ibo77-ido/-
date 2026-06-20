# P10D-26 Selective Stage List Draft

## Draft Date

2026-06-20

## Basis

This draft is based on the P10D-25 closeout plus read-only review of:

- `git status --short`
- `git diff --name-status`
- `git ls-files --others --exclude-standard`

No `git add`, commit, push, revert, clean, or file deletion was performed.

## Recommended Include

Recommended Phase10D candidate files, pending explicit user approval of the final selective stage list.

### Phase10 Scripts

- `Assets/Phase10_Narrative/Scripts/Data/P10DialogueNodeSO.cs`
- `Assets/Phase10_Narrative/Scripts/Editor/P10NarrativePlayModeValidator.cs`
- `Assets/Phase10_Narrative/Scripts/Editor/P10D06DialogueLogValidator.cs`
- `Assets/Phase10_Narrative/Scripts/Editor/P10D06DialogueLogValidator.cs.meta`
- `Assets/Phase10_Narrative/Scripts/Editor/P10D14Chapter1FlowHarnessValidator.cs`
- `Assets/Phase10_Narrative/Scripts/Editor/P10D14Chapter1FlowHarnessValidator.cs.meta`
- `Assets/Phase10_Narrative/Scripts/Runtime/P10NarrativeManager.cs`
- `Assets/Phase10_Narrative/Scripts/Runtime/P10NarrativeSceneBindingHub.cs`
- `Assets/Phase10_Narrative/Scripts/Runtime/P10NarrativeSceneBindingHub.cs.meta`
- `Assets/Phase10_Narrative/Scripts/Runtime/P10NarrativeSceneTrigger.cs`
- `Assets/Phase10_Narrative/Scripts/Runtime/P10NarrativeSceneTrigger.cs.meta`
- `Assets/Phase10_Narrative/Scripts/State/P10NarrativeSnapshot.cs`
- `Assets/Phase10_Narrative/Scripts/State/P10NarrativeStateMachine.cs`
- `Assets/Phase10_Narrative/Scripts/UI/P10DialogueController.cs`

### Phase10 ScriptableObjects

- `Assets/Phase10_Narrative/ScriptableObjects/Chapters/P10_CH01_ChapterFlow.asset`
- `Assets/Phase10_Narrative/ScriptableObjects/Characters/P10_CH01_NPC_001_XuLaoBo_Placeholder.asset`
- `Assets/Phase10_Narrative/ScriptableObjects/Characters/P10_CH01_NPC_002_ZhouZhangGui_Placeholder.asset`
- `Assets/Phase10_Narrative/ScriptableObjects/Characters/P10_CH01_NPC_003_ChenShuYuan_Placeholder.asset`
- `Assets/Phase10_Narrative/ScriptableObjects/Characters/P10_CH01_NPC_004_LuKe_Placeholder.asset`
- `Assets/Phase10_Narrative/ScriptableObjects/Characters/P10_CH01_SPEAKER_Narrator.asset`
- `Assets/Phase10_Narrative/ScriptableObjects/Characters/P10_CH01_SPEAKER_Narrator.asset.meta`
- `Assets/Phase10_Narrative/ScriptableObjects/Characters/P10_CH01_SPEAKER_Player.asset`
- `Assets/Phase10_Narrative/ScriptableObjects/Characters/P10_CH01_SPEAKER_Player.asset.meta`
- `Assets/Phase10_Narrative/ScriptableObjects/Characters/P10_CH01_SPEAKER_SystemPrompt.asset`
- `Assets/Phase10_Narrative/ScriptableObjects/Characters/P10_CH01_SPEAKER_SystemPrompt.asset.meta`
- `Assets/Phase10_Narrative/ScriptableObjects/Characters/P10_CH01_SPEAKER_UIResult.asset`
- `Assets/Phase10_Narrative/ScriptableObjects/Characters/P10_CH01_SPEAKER_UIResult.asset.meta`
- `Assets/Phase10_Narrative/ScriptableObjects/Dialogues/P10_CH01_NODE_CHAPTER_ENDING.asset`
- `Assets/Phase10_Narrative/ScriptableObjects/Dialogues/P10_CH01_NODE_ORDER_001_ACCEPT.asset`
- `Assets/Phase10_Narrative/ScriptableObjects/Dialogues/P10_CH01_NODE_ORDER_001_FAIL.asset`
- `Assets/Phase10_Narrative/ScriptableObjects/Dialogues/P10_CH01_NODE_ORDER_001_PASS.asset`
- `Assets/Phase10_Narrative/ScriptableObjects/Dialogues/P10_CH01_NODE_ORDER_003_ACCEPT.asset`
- `Assets/Phase10_Narrative/ScriptableObjects/Dialogues/P10_CH01_NODE_ORDER_003_FAIL.asset`
- `Assets/Phase10_Narrative/ScriptableObjects/Dialogues/P10_CH01_NODE_ORDER_003_PASS.asset`
- `Assets/Phase10_Narrative/ScriptableObjects/Dialogues/P10_CH01_NODE_ORDER_004_ACCEPT.asset`
- `Assets/Phase10_Narrative/ScriptableObjects/Dialogues/P10_CH01_NODE_ORDER_004_CLIMAX.asset`
- `Assets/Phase10_Narrative/ScriptableObjects/Dialogues/P10_CH01_NODE_ORDER_004_FAIL.asset`
- `Assets/Phase10_Narrative/ScriptableObjects/Dialogues/P10_CH01_NODE_ORDER_004_PASS_NORMAL.asset`
- `Assets/Phase10_Narrative/ScriptableObjects/Dialogues/P10_CH01_NODE_PROLOGUE_01.asset`
- `Assets/Phase10_Narrative/ScriptableObjects/Dialogues/P10_CH01_NODE_TUTORIAL_01.asset`

### Phase10 Prefab And Scene

These are functionally required for accepted Phase10D UI behavior, but still appear in `Needs User Decision` because the user explicitly asked to decide whether overlay scene / prefab enter the submit.

- `Assets/Phase10_Narrative/Prefabs/UI/P10_CH01_DialogueUI_Placeholder.prefab`
- `Assets/Phase10_Narrative/Scenes/P10_CH01_NarrativeOverlay.unity`

### Phase10D Docs

- `Assets/Phase10_Narrative/Docs/P10D_CH01_ChineseDialogueAssetImport.md`
- `Assets/Phase10_Narrative/Docs/P10D_CH01_ChineseDialogueAssetImport.md.meta`
- `Assets/Phase10_Narrative/Docs/P10D_CH01_ChineseLocalizationAndImportMapping.md`
- `Assets/Phase10_Narrative/Docs/P10D_CH01_ChineseLocalizationAndImportMapping.md.meta`
- `Assets/Phase10_Narrative/Docs/P10D_CH01_ChineseVerticalSliceValidation.md`
- `Assets/Phase10_Narrative/Docs/P10D_CH01_ChineseVerticalSliceValidation.md.meta`
- `Assets/Phase10_Narrative/Docs/P10D_CH01_CurrentOrderPanelWithCraftingHints.md`
- `Assets/Phase10_Narrative/Docs/P10D_CH01_CurrentOrderPanelWithCraftingHints.md.meta`
- `Assets/Phase10_Narrative/Docs/P10D_CH01_DialogueBoxClickToAdvance.md`
- `Assets/Phase10_Narrative/Docs/P10D_CH01_DialogueBoxClickToAdvance.md.meta`
- `Assets/Phase10_Narrative/Docs/P10D_CH01_DialogueLogLayoutAndPlayerFacingTermLocalizationRepair.md`
- `Assets/Phase10_Narrative/Docs/P10D_CH01_DialogueLogLayoutAndPlayerFacingTermLocalizationRepair.md.meta`
- `Assets/Phase10_Narrative/Docs/P10D_CH01_DialogueSegmentationAndTriggerRepair.md`
- `Assets/Phase10_Narrative/Docs/P10D_CH01_DialogueSegmentationAndTriggerRepair.md.meta`
- `Assets/Phase10_Narrative/Docs/P10D_CH01_EditorVisibleFixedDialogueTextSlots.md`
- `Assets/Phase10_Narrative/Docs/P10D_CH01_EditorVisibleFixedDialogueTextSlots.md.meta`
- `Assets/Phase10_Narrative/Docs/P10D_CH01_FixedDialogueTextSlotsAndPrefixCleanup.md`
- `Assets/Phase10_Narrative/Docs/P10D_CH01_FixedDialogueTextSlotsAndPrefixCleanup.md.meta`
- `Assets/Phase10_Narrative/Docs/P10D_CH01_RuntimeDialogueSpeakerVisibleDisplayRepair.md`
- `Assets/Phase10_Narrative/Docs/P10D_CH01_RuntimeDialogueSpeakerVisibleDisplayRepair.md.meta`
- `Assets/Phase10_Narrative/Docs/P10D_CH01_RuntimeDialogueTextCompletenessRepair.md`
- `Assets/Phase10_Narrative/Docs/P10D_CH01_RuntimeDialogueTextCompletenessRepair.md.meta`
- `Assets/Phase10_Narrative/Docs/P10D_CH01_RuntimeUILabelLocalization.md`
- `Assets/Phase10_Narrative/Docs/P10D_CH01_RuntimeUILabelLocalization.md.meta`
- `Assets/Phase10_Narrative/Docs/P10D_CH01_ScrollbarDragInteraction.md`
- `Assets/Phase10_Narrative/Docs/P10D_CH01_ScrollbarDragInteraction.md.meta`
- `Assets/Phase10_Narrative/Docs/P10D_CH01_SpeakerDisplayValidator.md`
- `Assets/Phase10_Narrative/Docs/P10D_CH01_SpeakerDisplayValidator.md.meta`
- `Assets/Phase10_Narrative/Docs/P10D_CH01_StoryDialogueLinePrefixSegmentationSpec.md`
- `Assets/Phase10_Narrative/Docs/P10D_CH01_StoryDialogueLinePrefixSegmentationSpec.md.meta`
- `Assets/Phase10_Narrative/Docs/P10D_CH01_TopRightActionBarPanelMutualExclusion.md`
- `Assets/Phase10_Narrative/Docs/P10D_CH01_TopRightActionBarPanelMutualExclusion.md.meta`
- `Assets/Phase10_Narrative/Docs/P10D_ChangeScopeAudit.md`
- `Assets/Phase10_Narrative/Docs/P10D_ChangeScopeAudit.md.meta`
- `Assets/Phase10_Narrative/Docs/P10D_Chapter1FlowVerificationHarnessDesign.md`
- `Assets/Phase10_Narrative/Docs/P10D_Chapter1FlowVerificationHarnessDesign.md.meta`
- `Assets/Phase10_Narrative/Docs/P10D_Chapter1FlowVerificationHarnessImplementation.md`
- `Assets/Phase10_Narrative/Docs/P10D_Chapter1FlowVerificationHarnessImplementation.md.meta`
- `Assets/Phase10_Narrative/Docs/P10D_Chapter1NarrativeContentScopeAudit.md`
- `Assets/Phase10_Narrative/Docs/P10D_Chapter1NarrativeContentScopeAudit.md.meta`
- `Assets/Phase10_Narrative/Docs/P10D_Closeout_FinalCandidateReview.md`
- `Assets/Phase10_Narrative/Docs/P10D_Closeout_FinalCandidateReview.md.meta`
- `Assets/Phase10_Narrative/Docs/P10D_DialogueLog.md`
- `Assets/Phase10_Narrative/Docs/P10D_DialogueLog.md.meta`
- `Assets/Phase10_Narrative/Docs/P10D_DialogueLogManualPlayModeAcceptance.md`
- `Assets/Phase10_Narrative/Docs/P10D_DialogueLogManualPlayModeAcceptance.md.meta`
- `Assets/Phase10_Narrative/Docs/P10D_DialogueLogSaveSnapshotScope.md`
- `Assets/Phase10_Narrative/Docs/P10D_DialogueLogSaveSnapshotScope.md.meta`
- `Assets/Phase10_Narrative/Docs/P10D_DialogueLogSnapshotDecision.md`
- `Assets/Phase10_Narrative/Docs/P10D_DialogueLogSnapshotDecision.md.meta`
- `Assets/Phase10_Narrative/Docs/P10D_DialogueLogSnapshotExtensionDesign.md`
- `Assets/Phase10_Narrative/Docs/P10D_DialogueLogSnapshotExtensionDesign.md.meta`
- `Assets/Phase10_Narrative/Docs/P10D_DialogueLogValidatorRegressionTriage.md`
- `Assets/Phase10_Narrative/Docs/P10D_DialogueLogValidatorRegressionTriage.md.meta`
- `Assets/Phase10_Narrative/Docs/P10D_DialogueTextSurface.md`
- `Assets/Phase10_Narrative/Docs/P10D_DialogueTextSurface.md.meta`
- `Assets/Phase10_Narrative/Docs/P10D_FinalScopeAudit.md`
- `Assets/Phase10_Narrative/Docs/P10D_FinalScopeAudit.md.meta`
- `Assets/Phase10_Narrative/Docs/P10D_FinalValidationSweep.md`
- `Assets/Phase10_Narrative/Docs/P10D_FinalValidationSweep.md.meta`
- `Assets/Phase10_Narrative/Docs/P10D_InGameAcceptance.md`
- `Assets/Phase10_Narrative/Docs/P10D_InGameAcceptance.md.meta`
- `Assets/Phase10_Narrative/Docs/P10D_InGameDeploymentContract.md`
- `Assets/Phase10_Narrative/Docs/P10D_InGameDeploymentContract.md.meta`
- `Assets/Phase10_Narrative/Docs/P10D_NPCInteractionEntry.md`
- `Assets/Phase10_Narrative/Docs/P10D_NPCInteractionEntry.md.meta`
- `Assets/Phase10_Narrative/Docs/P10D_ORDER001QuestSlice.md`
- `Assets/Phase10_Narrative/Docs/P10D_ORDER001QuestSlice.md.meta`
- `Assets/Phase10_Narrative/Docs/P10D_RuntimeMountContract.md`
- `Assets/Phase10_Narrative/Docs/P10D_RuntimeMountContract.md.meta`
- `Assets/Phase10_Narrative/Docs/P10D_SelectiveStageListDraft.md`
- `Assets/Phase10_Narrative/Docs/P10D_SelectiveStageListDraft.md.meta`

## Include Only If User Approves

- `AGENTS/RuntimeLayer/MEMORY_Phase10.md`
- `Project/StoryMain.md`
- P10C planning / scope docs:
  - `Assets/Phase10_Narrative/Docs/P10C_CH01_PlayableFlowMap.md`
  - `Assets/Phase10_Narrative/Docs/P10C_CH01_PlayableFlowMap.md.meta`
  - `Assets/Phase10_Narrative/Docs/P10C_PlayableStoryMVPScope.md`
  - `Assets/Phase10_Narrative/Docs/P10C_PlayableStoryMVPScope.md.meta`
- P10R / reconciliation / broad integration docs:
  - `Assets/Phase10_Narrative/Docs/P10_R01_CoreBlueprintComplianceReview.md`
  - `Assets/Phase10_Narrative/Docs/P10_R01_CoreBlueprintComplianceReview.md.meta`
  - `Assets/Phase10_Narrative/Docs/P10_ReconciliationPlan.md`
  - `Assets/Phase10_Narrative/Docs/P10_ReconciliationPlan.md.meta`
  - `Assets/Phase10_Narrative/Docs/Phase10_Narrative_Integration_Plan.md`
  - `Assets/Phase10_Narrative/Docs/Phase10_Narrative_Integration_Plan.md.meta`
- `Assets/Phase10_Narrative/Scripts/Editor/P10C05SaveStateSafetyValidator.cs`
- `Assets/Phase10_Narrative/Scripts/Editor/P10C05SaveStateSafetyValidator.cs.meta`
- Art source directory shown by git as the non-ASCII `美术素材/**` tree.
- Other abnormal non-Phase10 untracked path shown as `e10 narrative workspace contracts...`.

## Exclude / Do Not Stage

- `AGENTS/RuntimeLayer/MEMORY_Phase10.md`, unless explicitly approved.
- `AGENTS/WorkFlowLayerV1.3/STATE.md`
- `AGENTS/WorkFlowLayerV1.3/DECISIONS.md`
- `Assets/Phase3/**`
- `Assets/Phase6/**`
- `Assets/Phase8/**`
- `Assets/Scenes/**`
- `ProjectSettings/**`
- `Logs/**`
- `obj/**`
- `.vs/**`
- `P10_04_PlayMode.log`
- `Assets/Phase6/Scenes/Workshop_TestScene.meta`
- `Assets/Phase6/Scenes/Workshop_TestScene.unity`
- `Assets/Phase6/Scenes/Workshop_TestScene/NavMesh.asset`
- `Assets/Phase6/Scenes/Workshop_TestScene/NavMesh.asset.meta`
- `Assets/Workshop_TestScene.meta`
- `Assets/Workshop_TestScene/NavMesh.asset`
- `Assets/Workshop_TestScene/NavMesh.asset.meta`
- `Assets/Workshop_TestScene/**`
- temporary or abnormal untracked files outside the confirmed Phase10D candidate scope

## Needs User Decision

- Whether to submit Phase10D now.
- Target branch for the Phase10D submit.
- Whether Phase3 / Phase6 dirty state must be isolated before staging Phase10D.
- Whether to include `Assets/Phase10_Narrative/Scenes/P10_CH01_NarrativeOverlay.unity`.
- Whether to include `Assets/Phase10_Narrative/Prefabs/UI/P10_CH01_DialogueUI_Placeholder.prefab`.
- Whether to include the art source directory shown as `美术素材/**`.
- Whether to include `Project/StoryMain.md`.
- Whether to include `AGENTS/RuntimeLayer/MEMORY_Phase10.md`.
- Whether to include P10C / P10R / broad planning docs.

## Suggested git add commands

These commands are suggestions only. They were not executed. They intentionally avoid `git add .`, Phase3, Phase6, Logs, obj, memory, and workflow state.

### Core Phase10D Runtime And Validators

```powershell
git add -- Assets/Phase10_Narrative/Scripts/Data/P10DialogueNodeSO.cs
git add -- Assets/Phase10_Narrative/Scripts/Editor/P10NarrativePlayModeValidator.cs
git add -- Assets/Phase10_Narrative/Scripts/Editor/P10D06DialogueLogValidator.cs Assets/Phase10_Narrative/Scripts/Editor/P10D06DialogueLogValidator.cs.meta
git add -- Assets/Phase10_Narrative/Scripts/Editor/P10D14Chapter1FlowHarnessValidator.cs Assets/Phase10_Narrative/Scripts/Editor/P10D14Chapter1FlowHarnessValidator.cs.meta
git add -- Assets/Phase10_Narrative/Scripts/Runtime/P10NarrativeManager.cs
git add -- Assets/Phase10_Narrative/Scripts/Runtime/P10NarrativeSceneBindingHub.cs Assets/Phase10_Narrative/Scripts/Runtime/P10NarrativeSceneBindingHub.cs.meta
git add -- Assets/Phase10_Narrative/Scripts/Runtime/P10NarrativeSceneTrigger.cs Assets/Phase10_Narrative/Scripts/Runtime/P10NarrativeSceneTrigger.cs.meta
git add -- Assets/Phase10_Narrative/Scripts/State/P10NarrativeSnapshot.cs
git add -- Assets/Phase10_Narrative/Scripts/State/P10NarrativeStateMachine.cs
git add -- Assets/Phase10_Narrative/Scripts/UI/P10DialogueController.cs
```

### Phase10D Data Assets

```powershell
git add -- Assets/Phase10_Narrative/ScriptableObjects/Chapters/P10_CH01_ChapterFlow.asset
git add -- Assets/Phase10_Narrative/ScriptableObjects/Characters/P10_CH01_NPC_001_XuLaoBo_Placeholder.asset
git add -- Assets/Phase10_Narrative/ScriptableObjects/Characters/P10_CH01_NPC_002_ZhouZhangGui_Placeholder.asset
git add -- Assets/Phase10_Narrative/ScriptableObjects/Characters/P10_CH01_NPC_003_ChenShuYuan_Placeholder.asset
git add -- Assets/Phase10_Narrative/ScriptableObjects/Characters/P10_CH01_NPC_004_LuKe_Placeholder.asset
git add -- Assets/Phase10_Narrative/ScriptableObjects/Characters/P10_CH01_SPEAKER_Narrator.asset Assets/Phase10_Narrative/ScriptableObjects/Characters/P10_CH01_SPEAKER_Narrator.asset.meta
git add -- Assets/Phase10_Narrative/ScriptableObjects/Characters/P10_CH01_SPEAKER_Player.asset Assets/Phase10_Narrative/ScriptableObjects/Characters/P10_CH01_SPEAKER_Player.asset.meta
git add -- Assets/Phase10_Narrative/ScriptableObjects/Characters/P10_CH01_SPEAKER_SystemPrompt.asset Assets/Phase10_Narrative/ScriptableObjects/Characters/P10_CH01_SPEAKER_SystemPrompt.asset.meta
git add -- Assets/Phase10_Narrative/ScriptableObjects/Characters/P10_CH01_SPEAKER_UIResult.asset Assets/Phase10_Narrative/ScriptableObjects/Characters/P10_CH01_SPEAKER_UIResult.asset.meta
git add -- Assets/Phase10_Narrative/ScriptableObjects/Dialogues/P10_CH01_NODE_CHAPTER_ENDING.asset
git add -- Assets/Phase10_Narrative/ScriptableObjects/Dialogues/P10_CH01_NODE_ORDER_001_ACCEPT.asset
git add -- Assets/Phase10_Narrative/ScriptableObjects/Dialogues/P10_CH01_NODE_ORDER_001_FAIL.asset
git add -- Assets/Phase10_Narrative/ScriptableObjects/Dialogues/P10_CH01_NODE_ORDER_001_PASS.asset
git add -- Assets/Phase10_Narrative/ScriptableObjects/Dialogues/P10_CH01_NODE_ORDER_003_ACCEPT.asset
git add -- Assets/Phase10_Narrative/ScriptableObjects/Dialogues/P10_CH01_NODE_ORDER_003_FAIL.asset
git add -- Assets/Phase10_Narrative/ScriptableObjects/Dialogues/P10_CH01_NODE_ORDER_003_PASS.asset
git add -- Assets/Phase10_Narrative/ScriptableObjects/Dialogues/P10_CH01_NODE_ORDER_004_ACCEPT.asset
git add -- Assets/Phase10_Narrative/ScriptableObjects/Dialogues/P10_CH01_NODE_ORDER_004_CLIMAX.asset
git add -- Assets/Phase10_Narrative/ScriptableObjects/Dialogues/P10_CH01_NODE_ORDER_004_FAIL.asset
git add -- Assets/Phase10_Narrative/ScriptableObjects/Dialogues/P10_CH01_NODE_ORDER_004_PASS_NORMAL.asset
git add -- Assets/Phase10_Narrative/ScriptableObjects/Dialogues/P10_CH01_NODE_PROLOGUE_01.asset
git add -- Assets/Phase10_Narrative/ScriptableObjects/Dialogues/P10_CH01_NODE_TUTORIAL_01.asset
```

### Phase10D Prefab And Scene

```powershell
git add -- Assets/Phase10_Narrative/Prefabs/UI/P10_CH01_DialogueUI_Placeholder.prefab
git add -- Assets/Phase10_Narrative/Scenes/P10_CH01_NarrativeOverlay.unity
```

### Phase10D Docs

```powershell
git add -- Assets/Phase10_Narrative/Docs/P10D_CH01_ChineseDialogueAssetImport.md Assets/Phase10_Narrative/Docs/P10D_CH01_ChineseDialogueAssetImport.md.meta
git add -- Assets/Phase10_Narrative/Docs/P10D_CH01_ChineseLocalizationAndImportMapping.md Assets/Phase10_Narrative/Docs/P10D_CH01_ChineseLocalizationAndImportMapping.md.meta
git add -- Assets/Phase10_Narrative/Docs/P10D_CH01_ChineseVerticalSliceValidation.md Assets/Phase10_Narrative/Docs/P10D_CH01_ChineseVerticalSliceValidation.md.meta
git add -- Assets/Phase10_Narrative/Docs/P10D_CH01_CurrentOrderPanelWithCraftingHints.md Assets/Phase10_Narrative/Docs/P10D_CH01_CurrentOrderPanelWithCraftingHints.md.meta
git add -- Assets/Phase10_Narrative/Docs/P10D_CH01_DialogueBoxClickToAdvance.md Assets/Phase10_Narrative/Docs/P10D_CH01_DialogueBoxClickToAdvance.md.meta
git add -- Assets/Phase10_Narrative/Docs/P10D_CH01_DialogueLogLayoutAndPlayerFacingTermLocalizationRepair.md Assets/Phase10_Narrative/Docs/P10D_CH01_DialogueLogLayoutAndPlayerFacingTermLocalizationRepair.md.meta
git add -- Assets/Phase10_Narrative/Docs/P10D_CH01_DialogueSegmentationAndTriggerRepair.md Assets/Phase10_Narrative/Docs/P10D_CH01_DialogueSegmentationAndTriggerRepair.md.meta
git add -- Assets/Phase10_Narrative/Docs/P10D_CH01_EditorVisibleFixedDialogueTextSlots.md Assets/Phase10_Narrative/Docs/P10D_CH01_EditorVisibleFixedDialogueTextSlots.md.meta
git add -- Assets/Phase10_Narrative/Docs/P10D_CH01_FixedDialogueTextSlotsAndPrefixCleanup.md Assets/Phase10_Narrative/Docs/P10D_CH01_FixedDialogueTextSlotsAndPrefixCleanup.md.meta
git add -- Assets/Phase10_Narrative/Docs/P10D_CH01_RuntimeDialogueSpeakerVisibleDisplayRepair.md Assets/Phase10_Narrative/Docs/P10D_CH01_RuntimeDialogueSpeakerVisibleDisplayRepair.md.meta
git add -- Assets/Phase10_Narrative/Docs/P10D_CH01_RuntimeDialogueTextCompletenessRepair.md Assets/Phase10_Narrative/Docs/P10D_CH01_RuntimeDialogueTextCompletenessRepair.md.meta
git add -- Assets/Phase10_Narrative/Docs/P10D_CH01_RuntimeUILabelLocalization.md Assets/Phase10_Narrative/Docs/P10D_CH01_RuntimeUILabelLocalization.md.meta
git add -- Assets/Phase10_Narrative/Docs/P10D_CH01_ScrollbarDragInteraction.md Assets/Phase10_Narrative/Docs/P10D_CH01_ScrollbarDragInteraction.md.meta
git add -- Assets/Phase10_Narrative/Docs/P10D_CH01_SpeakerDisplayValidator.md Assets/Phase10_Narrative/Docs/P10D_CH01_SpeakerDisplayValidator.md.meta
git add -- Assets/Phase10_Narrative/Docs/P10D_CH01_StoryDialogueLinePrefixSegmentationSpec.md Assets/Phase10_Narrative/Docs/P10D_CH01_StoryDialogueLinePrefixSegmentationSpec.md.meta
git add -- Assets/Phase10_Narrative/Docs/P10D_CH01_TopRightActionBarPanelMutualExclusion.md Assets/Phase10_Narrative/Docs/P10D_CH01_TopRightActionBarPanelMutualExclusion.md.meta
git add -- Assets/Phase10_Narrative/Docs/P10D_ChangeScopeAudit.md Assets/Phase10_Narrative/Docs/P10D_ChangeScopeAudit.md.meta
git add -- Assets/Phase10_Narrative/Docs/P10D_Chapter1FlowVerificationHarnessDesign.md Assets/Phase10_Narrative/Docs/P10D_Chapter1FlowVerificationHarnessDesign.md.meta
git add -- Assets/Phase10_Narrative/Docs/P10D_Chapter1FlowVerificationHarnessImplementation.md Assets/Phase10_Narrative/Docs/P10D_Chapter1FlowVerificationHarnessImplementation.md.meta
git add -- Assets/Phase10_Narrative/Docs/P10D_Chapter1NarrativeContentScopeAudit.md Assets/Phase10_Narrative/Docs/P10D_Chapter1NarrativeContentScopeAudit.md.meta
git add -- Assets/Phase10_Narrative/Docs/P10D_Closeout_FinalCandidateReview.md Assets/Phase10_Narrative/Docs/P10D_Closeout_FinalCandidateReview.md.meta
git add -- Assets/Phase10_Narrative/Docs/P10D_DialogueLog.md Assets/Phase10_Narrative/Docs/P10D_DialogueLog.md.meta
git add -- Assets/Phase10_Narrative/Docs/P10D_DialogueLogManualPlayModeAcceptance.md Assets/Phase10_Narrative/Docs/P10D_DialogueLogManualPlayModeAcceptance.md.meta
git add -- Assets/Phase10_Narrative/Docs/P10D_DialogueLogSaveSnapshotScope.md Assets/Phase10_Narrative/Docs/P10D_DialogueLogSaveSnapshotScope.md.meta
git add -- Assets/Phase10_Narrative/Docs/P10D_DialogueLogSnapshotDecision.md Assets/Phase10_Narrative/Docs/P10D_DialogueLogSnapshotDecision.md.meta
git add -- Assets/Phase10_Narrative/Docs/P10D_DialogueLogSnapshotExtensionDesign.md Assets/Phase10_Narrative/Docs/P10D_DialogueLogSnapshotExtensionDesign.md.meta
git add -- Assets/Phase10_Narrative/Docs/P10D_DialogueLogValidatorRegressionTriage.md Assets/Phase10_Narrative/Docs/P10D_DialogueLogValidatorRegressionTriage.md.meta
git add -- Assets/Phase10_Narrative/Docs/P10D_DialogueTextSurface.md Assets/Phase10_Narrative/Docs/P10D_DialogueTextSurface.md.meta
git add -- Assets/Phase10_Narrative/Docs/P10D_FinalScopeAudit.md Assets/Phase10_Narrative/Docs/P10D_FinalScopeAudit.md.meta
git add -- Assets/Phase10_Narrative/Docs/P10D_FinalValidationSweep.md Assets/Phase10_Narrative/Docs/P10D_FinalValidationSweep.md.meta
git add -- Assets/Phase10_Narrative/Docs/P10D_InGameAcceptance.md Assets/Phase10_Narrative/Docs/P10D_InGameAcceptance.md.meta
git add -- Assets/Phase10_Narrative/Docs/P10D_InGameDeploymentContract.md Assets/Phase10_Narrative/Docs/P10D_InGameDeploymentContract.md.meta
git add -- Assets/Phase10_Narrative/Docs/P10D_NPCInteractionEntry.md Assets/Phase10_Narrative/Docs/P10D_NPCInteractionEntry.md.meta
git add -- Assets/Phase10_Narrative/Docs/P10D_ORDER001QuestSlice.md Assets/Phase10_Narrative/Docs/P10D_ORDER001QuestSlice.md.meta
git add -- Assets/Phase10_Narrative/Docs/P10D_RuntimeMountContract.md Assets/Phase10_Narrative/Docs/P10D_RuntimeMountContract.md.meta
git add -- Assets/Phase10_Narrative/Docs/P10D_SelectiveStageListDraft.md Assets/Phase10_Narrative/Docs/P10D_SelectiveStageListDraft.md.meta
```

## Risk Notes

- This draft intentionally excludes Phase3 and Phase6 dirty state.
- Phase6 Workshop_TestScene / NavMesh dirty and deleted files remain out of scope.
- `AGENTS/RuntimeLayer/MEMORY_Phase10.md` documents the work but should not be staged unless explicitly approved.
- Prefab and overlay scene are probably required for the accepted UI behavior, but they remain a user decision because the closeout requested explicit confirmation.
- Art source files are numerous and not under `Assets/Phase10_Narrative`; they need separate import, naming, compression, and 9-slice decisions.
- The final staging operation should be performed only after the user confirms the exact list.

## Next Step

Ask the user to approve one of these staging policies:

- `Core Phase10D`: stage recommended scripts, ScriptableObjects, prefab, scene, and P10D docs.
- `Code/data only`: stage scripts and ScriptableObjects, hold prefab/scene/docs.
- `Docs only`: stage P10D docs after review.
- `Custom`: user provides an exact inclusion/exclusion list.
