# P10D-16 CH01 Chinese Localization And Import Mapping

## Source of Truth

Frozen source-of-truth decision:

```text
Authoring source: Project/StoryMain.md
Runtime content source-of-truth: Assets/Phase10_Narrative/ScriptableObjects/Dialogues/*.asset
Speaker data source-of-truth: Assets/Phase10_Narrative/ScriptableObjects/Characters/*.asset
Chapter/order mapping source-of-truth: Assets/Phase10_Narrative/ScriptableObjects/Chapters/P10_CH01_ChapterFlow.asset
Hard-coded P10DialogueCatalog source-of-truth status: NO
```

`Project/StoryMain.md` remains the authored Chinese story source for Chapter 1. It must not be read at runtime.

For future import work, ScriptableObject assets should be the canonical runtime data. `P10DialogueCatalog` in `P10DialogueController.cs` is a technical fallback and must not become the authoritative content store.

P10D-16 is docs-only. It does not import text, edit ScriptableObject assets, edit `P10DialogueCatalog`, or change runtime behavior.

## Chinese Localization Rules

Frozen rules:

- Player-facing Chapter 1 text should be Chinese.
- Internal IDs stay unchanged, including `P10_CH01_NODE_*`, `P10_CH01_NPC_*`, `ORDER_*`, `SHAPE_*`, `GLAZE_*`, and `CODEX_*`.
- NPC display names should use Chinese names from `StoryMain.md`, not romanized placeholder names.
- Chapter title should be player-facing as `第一章《重燃窑火》`.
- Order names should use Chinese display names while keeping `ORDER_*` IDs for technical logs and validation.
- Dialogue should preserve the grounded, restrained tone in `StoryMain.md`.
- Chinese punctuation should follow the authored source style: `。` `，` `：` `？` `！` and Chinese quote marks where authored.
- Runtime/debug technical metadata may keep English labels only when it is not player-facing.
- Dialogue Log entries should store the same Chinese speaker name and Chinese dialogue text shown to the player.
- Snapshot save/load should preserve Chinese strings exactly.

Do not localize:

- Technical node IDs.
- Order IDs.
- Fact names such as `OrderCompleted` and `ScoreThresholdReached`.
- Bridge method names.
- Asset filenames in this first import pass.

## Speaker Name Mapping

Frozen speaker display mapping:

| Character ID | Current DisplayName | Chinese DisplayName | Role label for docs/import |
|---|---|---|---|
| `P10_CH01_NPC_001_XuLaoBo_Placeholder` | `Xu Lao Bo` | `徐老伯` | 老窑工 |
| `P10_CH01_NPC_002_ZhouZhangGui_Placeholder` | `Zhou Zhang Gui` | `周掌柜` | 茶馆老板 |
| `P10_CH01_NPC_003_ChenShuYuan_Placeholder` | `Chen Shu Yuan` | `陈书院` | 书院先生 |
| `P10_CH01_NPC_004_LuKe_Placeholder` | `Lu Ke` | `卢客` | 外地商人 |

Additional speaker labels from `StoryMain.md`:

| Speaker label | Use |
|---|---|
| `旁白` | Narration lines and environmental prose. |
| `系统提示` | Order accepted and tutorial prompt text if imported as dialogue/UI copy. |
| `UI结果` | Reward, unlock, and result text if imported as dialogue/UI copy. |
| `玩家` | Player choices only if a future choice system imports visible choice text. |

Current Phase10 has no `P10CharacterDataSO` asset for `旁白`, `系统提示`, `UI结果`, or `玩家`.

Resolved user decision on 2026-06-19:

```text
P10D-17 may add Phase10-only speaker assets for 旁白 / 系统提示 / UI结果 / 玩家.
These assets must stay under Assets/Phase10_Narrative/ScriptableObjects/Characters.
No runtime code is needed for this decision.
```

Reason:

```text
This is cleaner than forcing narration, system prompt, result, or player-choice text under NPC speaker names.
```

## UI Label Rules

Frozen player-facing UI label direction:

| Current label / concept | Chinese label |
|---|---|
| `Log` | `记录` |
| `Dialogue Log` | `对话记录` |
| `Next` | `继续` |
| `Close` | `关闭` |
| `No dialogue yet.` | `暂无对话记录。` |
| `Chapter 1` | `第一章` |
| `Chapter ending` | `第一章《重燃窑火》完成` |
| `Order accepted` | `订单接受` |
| `Order failed` | `订单失败` |
| `Reward` | `奖励` |
| `Unlock` | `解锁` |

P10D-16 does not change UI labels because that would require runtime code edits. These labels are frozen as the desired Chinese target for a later UI-localization task.

Resolved user decision on 2026-06-19:

```text
Do not localize runtime UI labels in P10D-17.
Defer runtime UI label localization to P10D-18.
```

Chapter title UI rule:

```text
第一章《重燃窑火》 should enter player-facing UI.
```

If no dedicated chapter title UI exists at import time, include the title in `P10_CH01_NODE_CHAPTER_ENDING` and keep a note for a future chapter-title surface.

## Existing Node IDs

The current Phase10 Chapter 1 node set remains frozen for the next import pass:

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

Frozen node expansion decision for P10D-17:

```text
Do not expand the node list in P10D-17.
Map StoryMain.md content into the existing 13 node IDs first.
Preserve current node IDs so the P10D-14 harness, existing dialogue flow, and bridge validators remain stable.
```

Resolved user decision on 2026-06-19:

```text
Do not split ORDER_001 perfect or ORDER_003 excellent into independent branches in P10D-17.
Keep the existing 13-node structure for P10D-17.
Full-fidelity choices, tutorial substeps, score-tier branches, and retry loops remain future work.
```

## StoryMain.md Section to Node Mapping

P10D-17 should map the authored source into the existing 13 coarse nodes as follows.

| StoryMain.md section / beat | Existing Phase10 node | Import action |
|---|---|---|
| `一、第一章概述` | Chapter metadata, not a dialogue node | Use for title and summary notes only. |
| `二、数据库映射摘要` | Chapter/order metadata, not a dialogue node | Use for import notes only. Do not duplicate Phase3 gameplay data. |
| `三、NPC 表` | CharacterData assets | Use for Chinese display names, roles, and relationship summaries. |
| `四、章节结构图` | `P10_CH01_ChapterFlow.asset` | Use as validation reference only. |
| `五、剧情流程图（节点地图）` | All 13 nodes | Use as mapping reference. |
| `六、序章：废窑的清晨` | `P10_CH01_NODE_PROLOGUE_01` | Import compressed opening: narration, 徐老伯 first exchange, 周掌柜 order setup, and transition to tutorial. |
| `七、新手教学：第一次开炉` | `P10_CH01_NODE_TUTORIAL_01` | Import compressed tutorial: 拉坯, 施釉, 烧窑, 开窑, and system prompt tone. |
| `八、订单一：甜白釉茶碗` 8.2 | `P10_CH01_NODE_ORDER_001_ACCEPT` | Import 周掌柜 order acceptance and brief. |
| `八、订单一：甜白釉茶碗` 8.3 | `P10_CH01_NODE_ORDER_001_PASS` | Import ordinary pass as baseline; mention perfect reward only if the single node can carry compact extra text. |
| `八、订单一：甜白釉茶碗` 8.4 | `P10_CH01_NODE_ORDER_001_FAIL` | Import failure/retry text. |
| `九、订单二：影青釉茶碗` 9.2 | `P10_CH01_NODE_ORDER_003_ACCEPT` | Import 陈书院 order acceptance and brief. |
| `九、订单二：影青釉茶碗` 9.3 | `P10_CH01_NODE_ORDER_003_PASS` | Import ordinary pass as baseline; high-score line is pending full branch support. |
| `九、订单二：影青釉茶碗` 9.4 | `P10_CH01_NODE_ORDER_003_FAIL` | Import failure/retry text. |
| `十、订单三：祭红釉香筒` 10.2 | `P10_CH01_NODE_ORDER_004_ACCEPT` | Import 卢客 order pressure setup and warning. |
| `十、订单三：祭红釉香筒` 10.4 | `P10_CH01_NODE_ORDER_004_PASS_NORMAL` | Import normal completion text. |
| `十、订单三：祭红釉香筒` 10.5 | `P10_CH01_NODE_ORDER_004_FAIL` | Import failure/retry text. |
| `十、订单三：祭红釉香筒` 10.3 and `十一、第一章高潮：开窑仪式` | `P10_CH01_NODE_ORDER_004_CLIMAX` | Import climax path and compressed aftermath. |
| `十二、第一章结尾：窑火重燃` | `P10_CH01_NODE_CHAPTER_ENDING` | Import chapter completion, 徐老伯 recognition, and unlock summary. |
| `十三、系统绑定说明` | Import notes, not direct dialogue | Use to validate that localized copy preserves system meaning. |
| `十四、触发条件与解锁条件总表` | Import notes, not direct dialogue | Use to validate order/trigger mapping. |
| `十五、章节结束条件` | Chapter ending / import notes | Use to validate completion text and unlock summary. |

Coarsening rule:

```text
When several StoryMain.md beats map to one existing node, import the strongest player-facing line sequence and preserve the detailed source section reference in the import notes.
Do not invent new plot content to fill gaps.
```

Choice text rule:

```text
Current Phase10 dialogue nodes do not model visible player choices.
P10D-17 should not import choices as functional choices.
Choice lines may be summarized into the node body only if needed for narrative continuity.
Full choice import requires a later approved runtime/data design.
```

## Order / Quest Mapping

Frozen order display mapping:

| Order ID | Chinese display name | Source section | Current Phase10 nodes |
|---|---|---|---|
| `ORDER_001` | `甜白釉茶碗` | `八、订单一：甜白釉茶碗（ORDER_001）` | Accept, Pass, Fail |
| `ORDER_003` | `影青釉茶碗` | `九、订单二：影青釉茶碗（ORDER_003）` | Accept, Pass, Fail |
| `ORDER_004` | `祭红釉香筒` | `十、订单三：祭红釉香筒（ORDER_004）` | Accept, PassNormal, Fail, Climax |

ORDER_004 naming note:

```text
Canonical task/order display name: 祭红釉香筒
Detailed target copy may say: 祭红釉直筒香罐
```

Frozen quest progression mapping for import:

```text
ORDER_001 accept text -> P10_CH01_NODE_ORDER_001_ACCEPT
OrderCompleted(ORDER_001) -> P10_CH01_NODE_ORDER_001_PASS

ORDER_003 accept text -> P10_CH01_NODE_ORDER_003_ACCEPT
OrderCompleted(ORDER_003) -> P10_CH01_NODE_ORDER_003_PASS

ORDER_004 accept text -> P10_CH01_NODE_ORDER_004_ACCEPT
Normal completion -> P10_CH01_NODE_ORDER_004_PASS_NORMAL
ScoreThresholdReached(95) / climax result -> P10_CH01_NODE_ORDER_004_CLIMAX
Chapter ending -> P10_CH01_NODE_CHAPTER_ENDING
```

Failure/retry import decision:

```text
Import failure/retry text for existing fail nodes: YES.
Add new OrderFailed runtime fact in P10D-17: NO.
Bind failure nodes to formal gameplay facts in P10D-17: NO, unless a later task explicitly allows runtime rule changes.
```

Score-tier import decision:

```text
ORDER_004 climax content: YES, import into P10_CH01_NODE_ORDER_004_CLIMAX.
ORDER_001 perfect branch: NO separate P10D-17 branch. Existing node set has no separate perfect node.
ORDER_003 excellent branch: NO separate P10D-17 branch. Existing node set has no separate excellent node.
P10D-17 should keep ordinary pass text as baseline for ORDER_001 and ORDER_003.
```

## Dialogue Import Strategy

Recommended P10D-17 import scope:

```text
Data/content import only under Assets/Phase10_Narrative.
Prefer updating ScriptableObject dialogue and character assets.
May add Phase10-only speaker assets for 旁白 / 系统提示 / UI结果 / 玩家.
Do not modify runtime code.
Do not modify Phase3, Phase6, Phase8, scenes, or ProjectSettings.
```

For each existing `P10DialogueNodeSO`:

- Preserve `NodeId`.
- Preserve `ChapterState`.
- Preserve `NodeKind`.
- Preserve `OrderId`.
- Preserve `NextNodeIds` unless a separate progression task approves changes.
- Update `SpeakerCharacterId` only if new speaker assets are approved and needed.
- Replace English placeholder `DialogueText` with Chinese text mapped from `StoryMain.md`.
- Keep `Conditions` only as current technical metadata unless a later task changes progression rules.

For each existing `P10CharacterDataSO`:

- Preserve `CharacterId`.
- Update `DisplayName` to Chinese.
- Update `Role` and `RelationshipSummary` to Chinese or bilingual import notes if approved.
- Preserve `DefaultNodeId`.

Recommended `DialogueText` format for P10D-17:

```text
Use compact multi-line Chinese text when a coarse node represents several authored beats.
Keep each node readable in the current dialogue surface and Dialogue Log.
Avoid importing huge full sections into a single node if it would make runtime review unusable.
```

P10D-17 should include a post-import validation doc or checklist that confirms:

- All 13 node assets have Chinese `DialogueText`.
- All four NPC assets have Chinese `DisplayName`.
- `ORDER_001`, `ORDER_003`, and `ORDER_004` names are recorded.
- P10D-14 harness node IDs remain unchanged.
- Serialized scene references are not changed.

## Hard-coded Catalog vs Asset Source Risk

Current risk:

```text
P10DialogueController can resolve text from ScriptableObject dialogue assets.
If no assets are bound, it falls back to hard-coded P10DialogueCatalog.
Both currently contain English placeholder text.
```

Frozen ownership rule:

```text
ScriptableObject dialogue assets are canonical.
P10DialogueCatalog is a fallback only.
```

Risk if P10D-17 updates assets only:

- Editor-bound or serialized dialogue controllers may show Chinese.
- Runtime-created fallback-only dialogue controllers may still show English from `P10DialogueCatalog`.
- The visible runtime result may appear partially localized even though canonical assets are Chinese.

Resolved user decision on 2026-06-19:

```text
Option A, docs/data only:
  Update ScriptableObject assets only.
  Record that P10DialogueCatalog remains English fallback and needs a later runtime alignment task.

Do not synchronize P10DialogueCatalog in P10D-17.
```

English fallback decision:

```text
Player-facing English fallback should not be retained as final Chapter 1 content.
Temporary English fallback may remain only as a documented technical debt in P10D-17.
```

## Resolved User Decisions

Resolved on 2026-06-19 before P10D-17 execution:

| Decision | P10D-17 ruling | Why |
|---|---|---|
| Add new Phase10-only speaker assets for `旁白`, `系统提示`, `UI结果`, and `玩家`? | YES | Cleaner than putting non-NPC presentation under NPC names. |
| P10D-17 node strategy | Use existing 13 nodes | Preserves harness and bridge stability. |
| P10D-17 source-of-truth | ScriptableObject assets | Avoids hard-coded content as canonical data. |
| Update `P10DialogueCatalog` in P10D-17? | NO | ScriptableObject assets are authoritative; fallback sync is deferred. |
| Keep English fallback player-facing? | NO | Final CH01 should be Chinese. |
| Import failure/retry text? | YES for existing fail nodes | Existing nodes already support fail content data. |
| Import ORDER_004 climax text? | YES | Existing climax node already exists. |
| Import ORDER_001 perfect and ORDER_003 excellent as separate branches? | NO | Keep the existing 13 nodes and avoid runtime flow expansion. |
| Localize runtime UI labels `Log`, `Next`, `Close` in P10D-17? | NO | P10D-17 focuses on dialogue/character assets; UI labels move to P10D-18. |

## Recommended Next Step

Recommended next task:

```text
P10D-17 Chapter 1 Chinese Dialogue Asset Import
```

Recommended P10D-17 scope:

```text
Allowed:
  Assets/Phase10_Narrative/ScriptableObjects/Characters/**
  Assets/Phase10_Narrative/ScriptableObjects/Dialogues/**
  Assets/Phase10_Narrative/Docs/**
  AGENTS/RuntimeLayer/MEMORY_Phase10.md

Default forbidden:
  Assets/Phase10_Narrative/Scripts/**
  Assets/Phase3/**
  Assets/Phase6/**
  Assets/Phase8/**
  Assets/Scenes/**
  ProjectSettings/**
```

P10D-17 should not synchronize `P10DialogueCatalog` and should not localize runtime UI labels. It should record the remaining fallback/UI-label mismatch for a later task, recommended as P10D-18.

## P10D-16 Result

```text
Status: PASS
Docs-only: YES
Runtime code modified: NO
Dialogue import performed: NO
User decisions recorded: YES, on 2026-06-19
Serialized References Changed: NONE
Scene Mutation: NONE
Phase3 / Phase6 / Phase8 / Scenes / ProjectSettings modified: NO
Yu branch bridge impact: NO
Stage / commit / push: NO
```
