# P10D-17 CH01 Chinese Dialogue Asset Import

## Import Scope

P10D-17 imported Chapter 1 Chinese dialogue and speaker display data into Phase10 ScriptableObject assets only.

Allowed data sources:

```text
Project/StoryMain.md
Assets/Phase10_Narrative/Docs/P10D_CH01_ChineseLocalizationAndImportMapping.md
Assets/Phase10_Narrative/ScriptableObjects/Dialogues/**
Assets/Phase10_Narrative/ScriptableObjects/Characters/**
Assets/Phase10_Narrative/ScriptableObjects/Chapters/P10_CH01_ChapterFlow.asset
```

Runtime code was not modified. `P10DialogueCatalog` was not synchronized.

## Data Source

Authoring source:

```text
Project/StoryMain.md
```

Import mapping source:

```text
Assets/Phase10_Narrative/Docs/P10D_CH01_ChineseLocalizationAndImportMapping.md
```

Runtime source-of-truth after this pass:

```text
Assets/Phase10_Narrative/ScriptableObjects/Dialogues/*.asset
Assets/Phase10_Narrative/ScriptableObjects/Characters/*.asset
```

`P10DialogueCatalog` remains a technical fallback and is a known later alignment risk.

## Modified Character Assets

The four existing Chapter 1 NPC assets keep their `CharacterId` and `DefaultNodeId`, but now use Chinese player-facing names and Chinese role summaries.

| Asset | DisplayName |
|---|---|
| `P10_CH01_NPC_001_XuLaoBo_Placeholder.asset` | `徐老伯` |
| `P10_CH01_NPC_002_ZhouZhangGui_Placeholder.asset` | `周掌柜` |
| `P10_CH01_NPC_003_ChenShuYuan_Placeholder.asset` | `陈书院` |
| `P10_CH01_NPC_004_LuKe_Placeholder.asset` | `卢客` |

## New Speaker Assets

P10D-17 added Phase10-only speaker assets for non-NPC presentation text.

| Asset | CharacterId | DisplayName | Use |
|---|---|---|---|
| `P10_CH01_SPEAKER_Narrator.asset` | `P10_CH01_SPEAKER_Narrator` | `旁白` | Narration and chapter framing. |
| `P10_CH01_SPEAKER_SystemPrompt.asset` | `P10_CH01_SPEAKER_SystemPrompt` | `系统提示` | Order prompts and requirement text. |
| `P10_CH01_SPEAKER_UIResult.asset` | `P10_CH01_SPEAKER_UIResult` | `UI结果` | Rewards, unlocks, and completion results. |
| `P10_CH01_SPEAKER_Player.asset` | `P10_CH01_SPEAKER_Player` | `玩家` | Future player-facing choice text placeholder. |

`P10_CH01_ChapterFlow.asset` was updated only to include these speaker `CharacterIds`. The node list remains unchanged.

## Dialogue Asset Import

All 13 existing dialogue node assets were updated with Chinese `DialogueText` mapped from `StoryMain.md`.

| Dialogue asset | Node ID | Speaker | Order |
|---|---|---|---|
| `P10_CH01_NODE_PROLOGUE_01.asset` | `P10_CH01_NODE_PROLOGUE_01` | `旁白` | none |
| `P10_CH01_NODE_TUTORIAL_01.asset` | `P10_CH01_NODE_TUTORIAL_01` | `徐老伯` | none |
| `P10_CH01_NODE_ORDER_001_ACCEPT.asset` | `P10_CH01_NODE_ORDER_001_ACCEPT` | `周掌柜` | `ORDER_001` |
| `P10_CH01_NODE_ORDER_001_PASS.asset` | `P10_CH01_NODE_ORDER_001_PASS` | `周掌柜` | `ORDER_001` |
| `P10_CH01_NODE_ORDER_001_FAIL.asset` | `P10_CH01_NODE_ORDER_001_FAIL` | `周掌柜` | `ORDER_001` |
| `P10_CH01_NODE_ORDER_003_ACCEPT.asset` | `P10_CH01_NODE_ORDER_003_ACCEPT` | `陈书院` | `ORDER_003` |
| `P10_CH01_NODE_ORDER_003_PASS.asset` | `P10_CH01_NODE_ORDER_003_PASS` | `陈书院` | `ORDER_003` |
| `P10_CH01_NODE_ORDER_003_FAIL.asset` | `P10_CH01_NODE_ORDER_003_FAIL` | `陈书院` | `ORDER_003` |
| `P10_CH01_NODE_ORDER_004_ACCEPT.asset` | `P10_CH01_NODE_ORDER_004_ACCEPT` | `卢客` | `ORDER_004` |
| `P10_CH01_NODE_ORDER_004_PASS_NORMAL.asset` | `P10_CH01_NODE_ORDER_004_PASS_NORMAL` | `卢客` | `ORDER_004` |
| `P10_CH01_NODE_ORDER_004_FAIL.asset` | `P10_CH01_NODE_ORDER_004_FAIL` | `卢客` | `ORDER_004` |
| `P10_CH01_NODE_ORDER_004_CLIMAX.asset` | `P10_CH01_NODE_ORDER_004_CLIMAX` | `旁白` | `ORDER_004` |
| `P10_CH01_NODE_CHAPTER_ENDING.asset` | `P10_CH01_NODE_CHAPTER_ENDING` | `旁白` | none |

## Node Mapping Result

The import preserves the existing 13-node Chapter 1 structure.

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

No node was added, deleted, renamed, or split.

## Order Mapping Result

| Order ID | Chinese display text used in imported copy | Existing node coverage |
|---|---|---|
| `ORDER_001` | `甜白釉茶碗` | Accept, Pass, Fail |
| `ORDER_003` | `影青釉茶碗` | Accept, Pass, Fail |
| `ORDER_004` | `祭红釉香筒` | Accept, PassNormal, Fail, Climax |

The import does not change order progression rules. Existing pass/fail/climax nodes keep their current technical IDs and conditions.

## Not Changed

P10D-17 intentionally did not change:

```text
Assets/Phase10_Narrative/Scripts/**
P10DialogueCatalog fallback text
runtime UI labels
formal task progression rules
Phase3 assets
Phase6 assets or scenes
Phase8 assets
Assets/Scenes
ProjectSettings
Yu branch bridge contracts
```

## Known Remaining Items

- `P10DialogueCatalog` still contains English fallback text and should be handled only by a later approved task.
- Runtime UI labels such as `Log`, `Next`, `Close`, and `Dialogue Log` remain English and are deferred to P10D-18.
- Player choice text from `StoryMain.md` was not imported as functional choices because the current 13-node structure has no choice model.
- `ORDER_001` perfect and `ORDER_003` excellent remain compressed into existing pass nodes and were not split into new branches.
- Failure/retry text is present in existing fail nodes, but no new `OrderFailed` runtime fact or progression rule was added.

## Validation Evidence

Checks completed for this data-only pass:

```text
dotnet build Phase10_Narrative.csproj: PASS, 0 warnings / 0 errors
Text scan: PASS, 13 dialogue assets have Chinese DialogueText
Chapter flow scan: PASS, 13 NodeIds remain and 4 Phase10-only speaker IDs are present
P10DialogueCatalog sync scan: PASS, no script edit performed
Unity batchmode import check: PASS, exit code 0
Unity log: Logs/P10D17_UnityImport.log
git diff / status review: no Scene or ProjectSettings edits attributable to P10D-17
```

Unity log notes:

- The log ends with `Exiting batchmode successfully now!` and return code `0`.
- The log contains Unity licensing/access-token and curl shutdown/network warnings, but no fatal import or compile failure for this pass.

## P10D-17 Result

```text
Status: PASS
Runtime code modified: NO
P10DialogueCatalog synchronized: NO
Node expansion: NO
Validation: dotnet build PASS; Unity batchmode import PASS
Serialized References Changed: P10_CH01_ChapterFlow.asset CharacterIds list only; scene/prefab references unchanged.
Scene Mutation: NONE
Yu bridge impact: NO
Stage / commit / push: NO
```

## Recommended Next Step

Recommended next task:

```text
P10D-18 Chapter 1 Runtime UI Label Localization / Fallback Alignment Decision
```

The next task should decide whether to localize runtime UI labels first, synchronize or generate `P10DialogueCatalog` fallback, or add a validator that asserts ScriptableObject dialogue assets are present before falling back to hard-coded English copy.
