# P10D-15 Chapter 1 Narrative Content Scope Audit

## Source Documents Found

Primary story source:

```text
Project/StoryMain.md
```

This file exists and is a complete Chinese Chapter 1 story document for `第一章：《重燃窑火》`. It includes the chapter overview, database mapping summary, NPC table, chapter structure, node map, full prose/dialogue beats, order beats, climax, ending, system binding notes, trigger/unlock conditions, and chapter end conditions.

Phase10 planning and implementation sources found:

```text
Assets/Phase10_Narrative/Docs/P10_CH01_NarrativePlan.md
Assets/Phase10_Narrative/Docs/P10C_PlayableStoryMVPScope.md
Assets/Phase10_Narrative/Docs/P10C_CH01_PlayableFlowMap.md
Assets/Phase10_Narrative/Docs/P10D_InGameDeploymentContract.md
Assets/Phase10_Narrative/Docs/P10D_DialogueTextSurface.md
Assets/Phase10_Narrative/Docs/P10D_NPCInteractionEntry.md
Assets/Phase10_Narrative/Docs/P10D_ORDER001QuestSlice.md
Assets/Phase10_Narrative/Docs/P10D_DialogueLog.md
Assets/Phase10_Narrative/Docs/P10D_DialogueLogSaveSnapshotScope.md
Assets/Phase10_Narrative/Docs/P10D_DialogueLogSnapshotDecision.md
Assets/Phase10_Narrative/Docs/P10D_DialogueLogSnapshotExtensionDesign.md
Assets/Phase10_Narrative/Docs/P10D_DialogueLogManualPlayModeAcceptance.md
Assets/Phase10_Narrative/Docs/P10D_Chapter1FlowVerificationHarnessDesign.md
Assets/Phase10_Narrative/Docs/P10D_Chapter1FlowVerificationHarnessImplementation.md
Assets/Phase10_Narrative/Docs/Phase10_Narrative_Integration_Plan.md
Assets/Phase10_Narrative/Docs/P10_BridgeContract.md
Assets/Phase10_Narrative/Docs/P10_BridgeIntegrationPlan.md
Assets/Phase10_Narrative/Docs/P10_ReconciliationPlan.md
Assets/Phase10_Narrative/Docs/P10_R01_CoreBlueprintComplianceReview.md
```

The requested `Packages/Phase10_Narrative_Integration_Plan.md` was not found. A Phase10 integration plan copy exists under:

```text
Assets/Phase10_Narrative/Docs/Phase10_Narrative_Integration_Plan.md
```

Related non-Phase10 bridge/context documents found:

```text
Docs/Phase8/P8-01_BRIDGE_CONTRACT.md
Docs/Phase8/P8-07_DATA_OWNERSHIP.md
Docs/Phase8/P8-08_FILE_RESPONSIBILITY_MATRIX.md
```

These are bridge ownership context only. They are not Chapter 1 story sources.

## Existing Chapter 1 Content

The project currently has a complete authored Chapter 1 story source in `Project/StoryMain.md`.

It defines:

- Chapter title: `第一章：重燃窑火`.
- Estimated play time: 10 to 15 minutes.
- Core arc: reopening a dormant kiln, completing three orders, and earning renewed trust.
- Gameplay coverage: `ShapeScore`, `GlazeScore`, `FireScore`, and `ResultCalculator`.
- Story beats: prologue, tutorial, `ORDER_001`, `ORDER_003`, `ORDER_004`, climax, ending, system bindings, unlocks, and chapter end conditions.
- Chapter completion condition: complete `ORDER_001`, `ORDER_003`, and `ORDER_004`.

Phase10 has already reduced that source into a coarse technical Chapter 1 structure:

```text
None -> Prologue -> Tutorial -> Order001 -> Order003 -> Order004 -> Ending -> Completed
```

The current Phase10 implementation can form a technical pass-path vertical slice through the P10D-14 Editor-only harness. It is not yet a final narrative-content vertical slice because the full Chinese prose, detailed choices, failure/retry copy, score-tier feedback, UI prompt copy, rewards, and unlock copy from `StoryMain.md` have not been imported into Phase10 dialogue data.

## Existing NPCs / Speakers

`StoryMain.md` defines Chinese NPC roles:

```text
玩家 / 年轻继承人
徐老伯 / 老窑工
周掌柜 / 茶馆老板
陈书院 / 书院先生
卢客 / 外地商人
```

Current Phase10 data assets define four NPC placeholders:

```text
P10_CH01_NPC_001_XuLaoBo_Placeholder
  DisplayName: Xu Lao Bo
  Role: Elder craftsman and early mentor.
  DefaultNodeId: P10_CH01_NODE_PROLOGUE_01

P10_CH01_NPC_002_ZhouZhangGui_Placeholder
  DisplayName: Zhou Zhang Gui
  Role: Merchant contact and early order giver.
  DefaultNodeId: P10_CH01_NODE_ORDER_001_ACCEPT

P10_CH01_NPC_003_ChenShuYuan_Placeholder
  DisplayName: Chen Shu Yuan
  Role: Scholar customer who raises quality expectations.
  DefaultNodeId: P10_CH01_NODE_ORDER_003_ACCEPT

P10_CH01_NPC_004_LuKe_Placeholder
  DisplayName: Lu Ke
  Role: Visitor and later-stage observer.
  DefaultNodeId: P10_CH01_NODE_ORDER_004_ACCEPT
```

Gap: the current Phase10 speaker display names are romanized English placeholders, not the authored Chinese display names from `StoryMain.md`.

## Existing Dialogue Nodes

Current Phase10 dialogue node assets:

```text
P10_CH01_NODE_PROLOGUE_01
P10_CH01_NODE_TUTORIAL_01
P10_CH01_NODE_ORDER_001_ACCEPT
P10_CH01_NODE_ORDER_001_PASS
P10_CH01_NODE_ORDER_001_FAIL
P10_CH01_NODE_ORDER_003_ACCEPT
P10_CH01_NODE_ORDER_003_PASS
P10_CH01_NODE_ORDER_003_FAIL
P10_CH01_NODE_ORDER_004_ACCEPT
P10_CH01_NODE_ORDER_004_PASS_NORMAL
P10_CH01_NODE_ORDER_004_FAIL
P10_CH01_NODE_ORDER_004_CLIMAX
P10_CH01_NODE_CHAPTER_ENDING
```

These node IDs exist in:

```text
Assets/Phase10_Narrative/ScriptableObjects/Dialogues/*.asset
Assets/Phase10_Narrative/ScriptableObjects/Chapters/P10_CH01_ChapterFlow.asset
Assets/Phase10_Narrative/Scripts/UI/P10DialogueController.cs
Assets/Phase10_Narrative/Scripts/Runtime/P10NarrativeManager.cs
Assets/Phase10_Narrative/Scripts/Editor/P10D14Chapter1FlowHarnessValidator.cs
```

Current dialogue text status:

- `ScriptableObjects/Dialogues/*.asset` contain English one-line placeholder prose.
- `P10DialogueCatalog` in `P10DialogueController.cs` contains matching English fallback prose.
- `StoryMain.md` contains detailed Chinese dialogue and narrative prose, but that text is not imported into the runtime dialogue assets or fallback catalog.

Coarsening gap:

- `StoryMain.md` has finer beats such as `PROLOGUE_01/02/03`, `TUTORIAL_01/02/03/04`, detailed order setup, pass/fail branches, reward prompts, score-tier feedback, and player choices.
- Current Phase10 has 13 coarse nodes and no final choice model for the detailed authored branches.

## Existing Quest / Order IDs

Story and Phase10 both use these Chapter 1 order IDs:

```text
ORDER_001
ORDER_003
ORDER_004
```

`P10_CH01_ChapterFlow.asset` maps:

```text
ORDER_001
  Accept: P10_CH01_NODE_ORDER_001_ACCEPT
  Pass:   P10_CH01_NODE_ORDER_001_PASS
  Fail:   P10_CH01_NODE_ORDER_001_FAIL

ORDER_003
  Accept: P10_CH01_NODE_ORDER_003_ACCEPT
  Pass:   P10_CH01_NODE_ORDER_003_PASS
  Fail:   P10_CH01_NODE_ORDER_003_FAIL

ORDER_004
  Accept:    P10_CH01_NODE_ORDER_004_ACCEPT
  Pass:      P10_CH01_NODE_ORDER_004_PASS_NORMAL
  Fail:      P10_CH01_NODE_ORDER_004_FAIL
  NormalEnd: P10_CH01_NODE_ORDER_004_PASS_NORMAL
  Climax:    P10_CH01_NODE_ORDER_004_CLIMAX
```

Current runtime progression support:

- `OrderCompleted(ORDER_001)` advances only from `P10_CH01_NODE_ORDER_001_ACCEPT` to `P10_CH01_NODE_ORDER_001_PASS`.
- `OrderCompleted(ORDER_003)` advances only from `P10_CH01_NODE_ORDER_003_ACCEPT` to `P10_CH01_NODE_ORDER_003_PASS`.
- `ORDER_004` normal completion is currently covered through the approved `Order004PassNormal` trigger path in the Phase10 harness.
- `ScoreThresholdReached(95)` exists as a bridge fact path for the climax route, but the current P10D-14 harness validates the normal pass route.
- Failure node assets exist, but no formal `OrderFailed` runtime event type is currently implemented.

## Chinese Localization Status

Existing Chinese content:

- `Project/StoryMain.md` is the complete Chinese Chapter 1 source.
- Phase10 planning docs reference the Chinese chapter title and source document.
- P10D-14 uses temporary in-memory Chinese probe text only to verify rendering, Dialogue Log, and snapshot preservation. That probe is validation-only and not production content.

Current Phase10 runtime/data localization:

- Character display names are romanized English placeholders.
- Dialogue asset `DialogueText` fields are English placeholders.
- `P10DialogueCatalog` fallback text is English placeholder text.
- Runtime UI labels such as `Log`, `Next`, `Close`, `Dialogue Log`, and `No dialogue yet.` are English.

Judgment:

```text
Complete Chinese Chapter 1 story document exists: YES
Complete Chinese content imported into Phase10 runtime assets: NO
Current Chinese runtime coverage is production content: NO
Current Phase10 is content-localized to Chinese: NO
Chinese localization rules should be frozen before import: YES
Dialogue catalog/assets need a dedicated import/update task: YES
```

## Missing Content / Unknowns

Missing or not yet imported into Phase10:

- Full Chinese dialogue from `StoryMain.md`.
- Chinese speaker display names and possibly speaker title formatting.
- Fine-grained prologue and tutorial beats from the authored source.
- Player choice text and choice consequences.
- Score-tier response copy for pass, excellent, fail, and retry cases.
- UI prompt copy for order accepted, reward, unlock, chapter completed, and retry prompts.
- Exact mapping of StoryMain's detailed nodes to the current 13 Phase10 node IDs, or a decision to expand the node list.
- Failure/retry runtime progression rules for `ORDER_001`, `ORDER_003`, and `ORDER_004`.
- Whether `ORDER_004` climax should be triggered only by `ScoreThresholdReached(95)` or also by a future order-result fact.
- Final Chinese terminology rules for glaze, shape, order, score, reward, and codex text.
- Final ownership of `P10DialogueCatalog` versus ScriptableObject dialogue assets as the runtime source of truth.

Unknown / not audited in this docs-only pass:

- Actual Phase3 order asset semantic equivalence to `StoryMain.md` targets.
- Final Phase6 scene coordinates and staging for all NPC and prop beats.
- Any Yu-branch-specific assets or branch-only contracts, because this task intentionally did not read or modify Yu branch files.

## Binding Gaps

NPC binding:

- `P10NarrativeNpcInteractionBinder` can map four placeholder NPC object names to first-pass nodes.
- The binding is placeholder-name based and does not yet represent final authored Chinese NPC presentation.
- Current prefab/object names remain placeholder IDs, which is acceptable for Phase10 technical validation but not final content presentation.

Dialogue binding:

- `P10DialogueController` can resolve dialogue from ScriptableObject assets or `P10DialogueCatalog`.
- Both sources currently contain English placeholder lines.
- This creates a dual-source-of-truth risk unless the next import task chooses a canonical authoring source.

Quest binding:

- `ORDER_001` and `ORDER_003` have guarded `OrderCompleted` progression.
- `ORDER_004` normal/climax is technically represented but not fully bound to final gameplay result semantics.
- Failure paths are present as node assets but are not fully bound to formal gameplay failure facts.

Scene / bridge binding:

- Current Phase10 can validate a pass-path flow through Editor-only harness and existing Phase10 APIs.
- This task did not modify `Assets/Phase3/**`, `Assets/Phase6/**`, `Assets/Phase8/**`, `Assets/Scenes/**`, or `ProjectSettings/**`.
- No Yu branch bridge contract or asset was modified.
- Future Yu bridge integration should continue using neutral facts such as `OrderCompleted` and `ScoreThresholdReached`; Chapter 1 content import should not add Yu asset dependencies.

## Risks

- The complete authored source and current runtime assets diverge: `StoryMain.md` is Chinese and detailed; Phase10 assets/catalog are English and coarse.
- `P10DialogueCatalog` hard-coded fallback text can drift from ScriptableObject dialogue assets if both remain editable.
- Romanized NPC display names will fail Chinese presentation expectations unless localized.
- A naive import could overwrite placeholder node IDs or serialized references needed by the existing harness and bridge validators.
- Expanding node count without a mapping decision could break the current technical vertical slice.
- Failure/retry and score-tier content could imply progression rules that current runtime does not yet own.
- Editing scene bindings or Phase3/Phase6 data during content import would violate the current Phase10-only boundary and could disturb Yu bridge safety.

## Recommended Next Step

Recommended next task: freeze Chapter 1 Chinese localization and dialogue import rules before importing content.

Suggested sequence:

```text
1. Freeze localization/source-of-truth rules.
   Decide whether ScriptableObject dialogue assets or a generated catalog is canonical.
   Define Chinese speaker name, UI label, punctuation, and terminology rules.

2. Build a StoryMain-to-Phase10 node mapping table.
   Map each authored beat to the existing 13 nodes, or explicitly approve node expansion.
   Preserve existing node IDs unless a migration task is approved.

3. Import/update dialogue content as a Phase10-only task.
   Update dialogue assets/catalog within Assets/Phase10_Narrative only.
   Do not modify Phase3, Phase6, Phase8, scenes, or ProjectSettings.

4. Add binding only after content import gaps are known.
   Add NPC, quest, failure/retry, or score-tier bindings only where the existing Phase10 API is insufficient.
   Keep Yu bridge integration neutral-fact based.
```

Recommended immediate label:

```text
P10D-16 Chapter 1 Chinese Localization Rules and Dialogue Import Mapping
```

Expected scope for that next task should remain docs-first unless the user explicitly approves data asset import.

## P10D-15 Result

```text
Status: PASS
Docs-only: YES
Runtime code modified: NO
Serialized References Changed: NONE
Scene Mutation: NONE
Phase3 / Phase6 / Phase8 / Scenes / ProjectSettings modified: NO
Yu branch bridge impact: NO
Stage / commit / push: NO
```
