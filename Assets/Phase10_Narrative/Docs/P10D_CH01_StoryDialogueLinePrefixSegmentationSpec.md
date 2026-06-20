# P10D-20R4 StoryMain Dialogue Line Prefix Segmentation Spec

## Review Packet

Status: ready for user review.

Source: `Project/StoryMain.md`, 第一章《重燃窑火》。

Output: a docs-only segmentation specification that maps StoryMain dialogue into the 13 existing `P10_CH01_NODE_*` Chapter 1 nodes.

Document totals for review:

- 13 existing Chapter 1 node ids.
- 124 proposed `DialogueLine` rows.
- 8 supported display-prefix classes.

P10D-20R4 scope:

- Defines the authoring format for speaker-prefixed `DialogueLine.DialogueText`.
- Defines how StoryMain text is compressed into the existing 13-node Chapter 1 content shape.
- Does not implement runtime code.
- Does not modify dialogue assets, character assets, scenes, ProjectSettings, Phase3, Phase6, or Phase8.
- Does not add nodes.
- Does not add or run an automated validator.

Review decision needed from user: approve or request edits to the segmentation table before any later asset import, runtime repair, or validator work begins.

## 1. User-Confirmed Rule

P10D-20R4 explicitly follows the latest user rule:

- `DialogueLine.DialogueText` body text may contain a speaker display prefix as authored text.
- DialogueLine 正文可以包含 speaker 前缀。
- 格式：`SpeakerDisplayName：DialogueText`
- 每条 line 只能出现一个 speaker 前缀。
- 一个 dialogue box 每次只显示一条 line。
- When a speaker prefix is used, the authored form is `SpeakerDisplayName：DialogueText`.
- A speaker-prefixed line is valid when the prefix is at the start of the line and matches the intended speaker for that line.
- The forbidden cases are not "having a prefix"; the forbidden cases are wrong speaker ownership, multiple speaker prefixes inside one line, a speaker prefix that does not start at character 0, and one dialogue box displaying content from multiple speakers at once.
- This docs-only spec therefore keeps speaker prefixes in the proposed StoryMain lines instead of stripping them into body-only dialogue text.

## 2. Prefix Contract

For this P10D-20R4 StoryMain authoring spec, `DialogueLine.DialogueText` may contain one line-start display prefix. The proposed StoryMain segmentation table below keeps one prefix on every listed line.

Format: `SpeakerDisplayName：DialogueText`

The prefix is authored text. It is not a replacement for `SpeakerCharacterId`; a later implementation task must still map each line to the matching speaker asset.

| Speaker class | Display prefix when used | Intended speaker id |
|---|---|---|
| Narrator | `旁白：` | `P10_CH01_SPEAKER_Narrator` |
| Xu Laobo | `徐老伯：` | `P10_CH01_NPC_001_XuLaoBo_Placeholder` |
| Zhou Zhanggui | `周掌柜：` | `P10_CH01_NPC_002_ZhouZhangGui_Placeholder` |
| Chen Shuyuan | `陈书院：` | `P10_CH01_NPC_003_ChenShuYuan_Placeholder` |
| Lu Ke | `卢客：` | `P10_CH01_NPC_004_LuKe_Placeholder` |
| Player options | `玩家选项：` | `P10_CH01_SPEAKER_Player` |
| System prompt | `系统提示：` | `P10_CH01_SPEAKER_SystemPrompt` |
| UI result | `UI结果：` | `P10_CH01_SPEAKER_UIResult` |

Format requirements:

- If present, the prefix must start at character 0 of `DialogueLine.DialogueText`.
- The separator must be the full-width Chinese colon `：` (`U+FF1A`), not ASCII `:`.
- Each line may contain at most one speaker prefix.
- One dialogue box displays exactly one line at a time.
- Parenthetical action after the prefix belongs to the same speaker, for example `徐老伯：（没有抬头）“来了。”`.
- Player option alternatives may be separated with `/`; those alternatives are still one `玩家选项：` display line in this docs-only spec.
- `系统提示：` and `UI结果：` are display speaker classes for authored StoryMain text; they do not imply Phase3 / Phase6 gameplay UI integration in this task.

## 3. Rule Summary

- `DialogueLine` 正文可以包含 line-start speaker 前缀。
- 若包含 speaker 前缀，格式为：`SpeakerDisplayName：DialogueText`
- 本 P10D-20R4 StoryMain 分割表中的 proposed lines 均保留一个 speaker 前缀，便于审核 speaker ownership。
- 每条 line 只能出现一个 speaker 前缀。
- speaker 前缀本身不是错误；错误是前缀归属不匹配、前缀不在行首、或同一行出现多个 speaker。
- 一个 dialogue box 每次只显示一条 `DialogueLine`。
- 本表中的旁白 line 以 `旁白：` 开头。
- 本表中的 NPC line 以对应 NPC 中文名开头：
  - `徐老伯：`
  - `周掌柜：`
  - `陈书院：`
  - `卢客：`
- 本表中的玩家选项 line 以 `玩家选项：` 开头。
- 本表中的系统提示 line 以 `系统提示：` 开头。
- 本表中的 UI结果 line 以 `UI结果：` 开头。
- 不允许一个 line 内混合两个 speaker。
- 不允许把旁白内容写成 NPC 前缀。
- 不允许把 NPC 台词写成旁白前缀。
- 不允许一框一次显示多段不同 speaker 内容。
- 本文档只定义 StoryMain 到 13 个既有 node 的文本分割规格；不实现 runtime code，不修改 dialogue assets，不新增 node。

## 4. Compression Rules

- StoryMain 中同一 speaker 的连续句子可以保留为连续 `DialogueLine`。
- StoryMain 中发生 speaker 切换时，必须拆成不同 `DialogueLine`。
- StoryMain 的玩家选项在当前 13-node 规格中压缩为一个 `玩家选项：A / B / C` line；选项后的分支回应不在本阶段实现。
- StoryMain 的精品、失败原因、重试循环、aftermath 等分支只在既有 13 node 内做默认路径或压缩记录；不得新增 branch node。
- 系统提示与 UI 结果必须使用 `系统提示：` / `UI结果：`，不得挂到 NPC speaker。
- 若一个 StoryMain 片段无法完整装入 13-node 形状，必须在该 node 的 `Notes` 中写明压缩方式。

## 5. Valid Examples

- `旁白：父亲走的那年，窑炉最后一次点火。`
- `旁白：这是两年后，你站在院子里的第一个清晨。`
- `徐老伯：（没有抬头，继续扫灰）“来了。”`
- `徐老伯：“昨晚睡得着？”`
- `玩家选项：没怎么睡。 / 还行，睡得挺沉。 / 没说话，看了一眼账本。`
- `系统提示：【新订单】周掌柜的甜白釉茶碗 — ORDER_001 已接受`

## 6. Allowed Display Forms

- `旁白：...`
- `徐老伯：...`
- `周掌柜：...`

## 7. Forbidden Examples

- `徐老伯：父亲走的那年，窑炉最后一次点火。`
  原因：旁白内容误挂 NPC。
- `旁白：徐老伯：“来了。”`
  原因：旁白 line 内混入 NPC 台词。
- `徐老伯：“来了。” 旁白：他没有抬头。`
  原因：同一 line 出现多个 speaker。
- `他说：旁白：窑炉最后一次点火。`
  原因：speaker 前缀不在 line 起始位置。
- `周掌柜：订单失败 — 请重新制作`
  原因：系统提示误挂 NPC。

## 8. StoryMain.md Segmentation Table

Source: `Project/StoryMain.md`, 第一章《重燃窑火》。

Implementation note: 当前运行时只有 13 个既有 `P10_CH01_NODE_*` 节点。本表不新增节点；StoryMain 中无法完整装入既有 13 node 的分支、精品/失败细分、玩家选项后续台词，在 `Notes` 中标明压缩或默认路径。

### 8.1 Node Summary

| Node | Lines | Compression note |
|---|---:|---|
| `P10_CH01_NODE_PROLOGUE_01` | 11 | 序章选项压缩为展示 + 默认进入教学 |
| `P10_CH01_NODE_TUTORIAL_01` | 9 | Shape / Glaze / Fire 教学压缩为一个教学节点 |
| `P10_CH01_NODE_ORDER_001_ACCEPT` | 6 | 接单前对话与 brief 合并 |
| `P10_CH01_NODE_ORDER_001_PASS` | 5 | 精品路径暂不装入 |
| `P10_CH01_NODE_ORDER_001_FAIL` | 4 | 失败反馈保留，重试循环不实现 |
| `P10_CH01_NODE_ORDER_003_ACCEPT` | 13 | 精品挑战提示压缩到普通接单路径 |
| `P10_CH01_NODE_ORDER_003_PASS` | 4 | 高分 excellent 路径暂不装入 |
| `P10_CH01_NODE_ORDER_003_FAIL` | 6 | Glaze / Fire 失败原因压缩到同一失败节点 |
| `P10_CH01_NODE_ORDER_004_ACCEPT` | 15 | 三个选项后的不同回应压缩为默认 warning |
| `P10_CH01_NODE_ORDER_004_PASS_NORMAL` | 6 | 普通完成路径进入 Chapter Ending |
| `P10_CH01_NODE_ORDER_004_FAIL` | 8 | 失败反馈保留，重试循环不实现 |
| `P10_CH01_NODE_ORDER_004_CLIMAX` | 22 | `ORDER_004_CLIMAX` + `CLIMAX_AFTERMATH` 压缩合并 |
| `P10_CH01_NODE_CHAPTER_ENDING` | 15 | 结尾选项后回应压缩为默认认可文本 |
| **Total** | **124** | 13 existing nodes, no new node ids |

Not included in this docs-only table:

- Runtime branching for player choices.
- Retry loops after failed orders.
- Separate perfect / excellent branch nodes for `ORDER_001` or `ORDER_003`.
- A separate `CLIMAX_AFTERMATH` node.
- Any Phase3 / Phase6 formal order data binding.

### 8.2 Node Line Table

### P10_CH01_NODE_PROLOGUE_01

Line 01: `旁白：父亲走的那年，窑炉最后一次点火。`
Line 02: `旁白：这是两年后，你站在院子里的第一个清晨。`
Line 03: `徐老伯：（没有抬头，继续扫灰）“来了。”`
Line 04: `徐老伯：“昨晚睡得着？”`
Line 05: `玩家选项：没怎么睡。 / 还行，睡得挺沉。 / 没说话，看了一眼账本。`
Line 06: `徐老伯：“周掌柜昨天托人捎了话。”`
Line 07: `徐老伯：“他茶馆的碗缺了几只，问咱们能不能接。”`
Line 08: `徐老伯：（把扫帚靠在墙上，看着你）“你接不接？”`
Line 09: `玩家选项：接。 / 我……不太会。 / 就几只碗，值多少钱？`
Line 10: `系统提示：【新订单】周掌柜的甜白釉茶碗 — ORDER_001 已接受`
Line 11: `系统提示：当前资金：100 铜钱 | 当前声望：0`

Notes: StoryMain 的玩家选项分支需要后续实现选择结果；当前 13 node 规格可先压缩为选项展示 + 默认进入教学。

### P10_CH01_NODE_TUTORIAL_01

Line 01: `徐老伯：“先上坯台。”`
Line 02: `徐老伯：“碗的口沿要宽，碗底要稳。”`
Line 03: `徐老伯：“手感的事，说不清楚，你试着拉一只看看。”`
Line 04: `系统提示：器型评分（ShapeScore）：口沿比例 ×0.85 | 腹部比例 ×0.60 | 底足比例 ×0.35；合格线：≥ 80 分`
Line 05: `徐老伯：“施釉别急，太薄发灰，太厚会流。”`
Line 06: `系统提示：釉色评分（GlazeScore）启动。`
Line 07: `徐老伯：“烧窑更急不得，火候要稳。”`
Line 08: `系统提示：烧窑评分（FireScore）启动。`
Line 09: `徐老伯：“开窑的时候你自己来，手上没数，嘴上说没用。”`

Notes: StoryMain 教学包含多个系统步骤和分数反馈；当前 13 node 压缩为一个教学节点，不拆分 Shape/Glaze/Fire 子节点。

### P10_CH01_NODE_ORDER_001_ACCEPT

Line 01: `周掌柜：“我茶馆里的碗缺了几只，听说你们这窑又点火了，就来看看。”`
Line 02: `周掌柜：“要甜白釉茶碗，碗口宽一点，底要稳，别让客人端起来晃荡。”`
Line 03: `周掌柜：“釉色嘛，就是那种——白里带一点点暖，不要冷冰冰的。”`
Line 04: `系统提示：【订单接受】甜白釉茶碗 × 3`
Line 05: `系统提示：要求：ShapeScore ≥ 80 | GlazeScore ≥ 75`
Line 06: `系统提示：奖励：50 铜钱 + 声望 10`

Notes: StoryMain 中接单前对话和 brief 合并到当前 accept node；不新增 `ORDER_001_BRIEF` 节点。

### P10_CH01_NODE_ORDER_001_PASS

Line 01: `周掌柜：（拿起碗，在手里掂了掂）“嗯，稳。”`
Line 02: `周掌柜：“釉色……差不多，够用了。”`
Line 03: `周掌柜：“行，五十铜钱。”`
Line 04: `旁白：甜白釉在清晨的光线里泛着温润的暖白。这笔账，算是过了。`
Line 05: `UI结果：获得 50 铜钱 | 声望 +10 | 甜白釉 GLAZE_002 解锁 | 图鉴 CODEX_007 解锁`

Notes: 精品路径 `order001_pass_perfect` 暂不装入当前 13 node；后续可作为分支扩展。

### P10_CH01_NODE_ORDER_001_FAIL

Line 01: `周掌柜：（拿起碗，翻了个底）“口沿歪了。”`
Line 02: `周掌柜：“我茶馆的桌子不平，碗再歪，客人会骂的。”`
Line 03: `周掌柜：（把碗递回来）“重做吧。材料钱先挂账，做好了结清。”`
Line 04: `系统提示：订单失败 — 请重新制作 | 已消耗材料将从资金中扣除`

Notes: 当前 13 node 保留失败反馈；重试循环不在本 docs-only 阶段实现。

### P10_CH01_NODE_ORDER_003_ACCEPT

Line 01: `陈书院：“这里……还开着。”`
Line 02: `陈书院：“我姓陈，在城东书院教书。”`
Line 03: `陈书院：“听说这里的窑……重新点了。”`
Line 04: `陈书院：“我想要一只茶碗。”`
Line 05: `徐老伯：“陈先生，您说。”`
Line 06: `陈书院：“影青釉。”`
Line 07: `陈书院：“碗口要正，腹部要……有一种收着的感觉。”`
Line 08: `陈书院：“不是大碗，是喝茶用的小碗，拿在手里要像握着一捧水光。”`
Line 09: `玩家选项：影青釉我有。做多大？ / 您说的“水光”，是指釉色的透明感？ / 这个……有点难度。`
Line 10: `系统提示：【订单接受】影青釉茶碗 × 1`
Line 11: `系统提示：要求：GlazeScore ≥ 75 | ShapeScore ≥ 80`
Line 12: `系统提示：影青釉温度区间：1250°C ~ 1280°C`
Line 13: `系统提示：奖励：55 铜钱 + 声望 10`

Notes: StoryMain 的 `order003_brief_bonus` 精品挑战提示暂压缩到普通接单路径。

### P10_CH01_NODE_ORDER_003_PASS

Line 01: `陈书院：（捧起碗，慢慢转了一圈）“釉色……”`
Line 02: `陈书院：“尚可。”`
Line 03: `旁白：“尚可”——你后来才明白，对陈先生来说，这已经算是夸了。`
Line 04: `UI结果：获得 55 铜钱 | 声望 +10 | 图鉴 CODEX_006 解锁`

Notes: StoryMain 的高分 `order003_pass_excellent` 暂不装入当前 13 node；后续可作为分支扩展。

### P10_CH01_NODE_ORDER_003_FAIL

Line 01: `陈书院：（拿起碗，沉默了一会儿）`
Line 02: `陈书院：“釉色……偏了。”`
Line 03: `陈书院：“不像水光，更像——灰雨。”`
Line 04: `陈书院：“无妨，再试。我明日再来。”`
Line 05: `系统提示：订单失败 — GlazeScore 未达标（需 ≥ 75）| 请重新配釉施釉`
Line 06: `系统提示：订单失败 — 烧窑温度超出 1250-1280°C 区间 | 影青釉对温度极为敏感`

Notes: StoryMain 区分 GlazeScore 与 FireScore 失败原因；当前 13 node 可压缩为同一失败节点，具体原因由后续系统事实决定显示哪一条系统提示。

### P10_CH01_NODE_ORDER_004_ACCEPT

Line 01: `卢客：“你们这里……是窑厂吧？”`
Line 02: `卢客：（看了看窑炉）“还能接活儿？”`
Line 03: `徐老伯：“能。什么活儿？”`
Line 04: `卢客：“祭红釉的罐子。”`
Line 05: `卢客：“直筒型，高一点，口径十厘米，高十三。”`
Line 06: `卢客：（声音略低）“家里老人的祭祀用，不是摆件，是真要上香的。”`
Line 07: `卢客：“——要正红，不能暗，不能偏紫，不能发黑。”`
Line 08: `玩家选项：祭红釉，我来做。 / 祭红釉难度很高，我没把握。 / 看向徐老伯。`
Line 09: `徐老伯：“祭红釉靠铜发色，铜在高温下容易挥发。”`
Line 10: `徐老伯：“温度要稳在 1250 到 1280，早一分晚一分都容易出问题。”`
Line 11: `徐老伯：“这是本章到现在最难的一次烧窑。别急。”`
Line 12: `系统提示：【订单接受】祭红釉直筒香罐`
Line 13: `系统提示：目标釉色：祭红釉 GLAZE_004 | 难度：★★★★★`
Line 14: `系统提示：温度区间：1250-1280°C | 铜基釉对温度极为敏感`
Line 15: `系统提示：奖励：70 铜钱 + 声望 10 + 若精品则额外解锁`

Notes: StoryMain 中三个玩家选项后的不同回应暂压缩为默认进入 `order004_warning`。

### P10_CH01_NODE_ORDER_004_PASS_NORMAL

Line 01: `卢客：（拿起罐子，看了好一会儿）`
Line 02: `卢客：“红色……偏了一点，往暗了走。”`
Line 03: `卢客：“但能用。”`
Line 04: `卢客：“七十铜钱，说好的。”`
Line 05: `卢客：“家里老人的事，先用着。等你们手艺再好一些，再来。”`
Line 06: `UI结果：获得 70 铜钱 | 声望 +10 | 图鉴 CODEX_009 解锁`

Notes: 普通完成路径进入 `P10_CH01_NODE_CHAPTER_ENDING`。

### P10_CH01_NODE_ORDER_004_FAIL

Line 01: `卢客：（拿起罐子，罐壁上有一道细裂纹）`
Line 02: `卢客：“开裂了。”`
Line 03: `卢客：“我等得起，但老人等不起了。”`
Line 04: `卢客：“再试一次。这次我在这里等。”`
Line 05: `徐老伯：“温度没稳住。祭红釉的铜成分，差一点点就变色甚至开裂。”`
Line 06: `徐老伯：“再来一次。这次我在旁边告诉你哪里要调。”`
Line 07: `系统提示：订单失败 — FireScore 不合格 | 祭红釉需严格控温在 1250-1280°C`
Line 08: `系统提示：提示：温度超过 1280°C → 铜挥发，颜色变黑；低于 1250°C → 釉面发哑`

Notes: 当前 13 node 保留失败反馈；重试循环不在本 docs-only 阶段实现。

### P10_CH01_NODE_ORDER_004_CLIMAX

Line 01: `卢客：（站在窑炉前，双手环抱，眼神盯着窑门）`
Line 02: `徐老伯：（递给你开窑钩）“你开。”`
Line 03: `旁白：窑门打开的瞬间，一道深红从暗处透出来。`
Line 04: `旁白：不是暗沉的砖红，不是淡薄的粉红。`
Line 05: `旁白：是那种——你以前只在父亲账本里见过描述的颜色。`
Line 06: `旁白：正红，鲜亮，像血，又像云。`
Line 07: `卢客：（沉默。走近，弯下腰，把罐子捧起来，就那样举着，不说话。）`
Line 08: `卢客：（过了很长一会儿）“……成了。”`
Line 09: `卢客：（放下罐子，转向你）“你叫什么名字？”`
Line 10: `玩家选项：告诉他你的名字。 / 没说名字，只是点了点头。`
Line 11: `卢客：“说好七十，给你一百四。”`
Line 12: `卢客：“码头那边的活，过两天叫人来谈。”`
Line 13: `UI结果：首次精品！综合评分 ≥ 95`
Line 14: `UI结果：获得 140 铜钱（精品双倍奖励）| 声望 +20`
Line 15: `UI结果：图鉴 CODEX_009 祭红釉解锁`
Line 16: `UI结果：成就：第一件精品`
Line 17: `旁白：卢客离开后，院子里只剩下你和徐老伯。`
Line 18: `徐老伯：“祭红釉……你父亲最后一次烧，也是这个颜色。”`
Line 19: `徐老伯：“那时候他也是你这个年纪。”`
Line 20: `玩家选项：他……后来为什么离开？ / 他一定是个很好的瓷匠。 / 没有说话，看着那个罐子的印记。`
Line 21: `旁白：窑火已经熄了。但炉壁还是热的。`
Line 22: `旁白：你知道，明天它会再次燃起来。`

Notes: 当前 13 node 的高潮节点需要承载 StoryMain `ORDER_004_CLIMAX` 与 `CLIMAX_AFTERMATH` 的压缩默认路径；不新增 aftermath node。

### P10_CH01_NODE_CHAPTER_ENDING

Line 01: `旁白：三笔订单，交出去的时候你的手还有点抖。`
Line 02: `旁白：账本上第一次出现了正数。`
Line 03: `徐老伯：“周掌柜那边说，上次的碗卖完了，要再来一批。”`
Line 04: `徐老伯：“书院那边，陈先生叫人来问，能不能做盘？”`
Line 05: `徐老伯：“能接吗？”`
Line 06: `玩家选项：接。 / 盘……我还没做过。`
Line 07: `徐老伯：“行。这是你的活，你管。”`
Line 08: `旁白：窑炉在身后发出轻微的热气声。`
Line 09: `旁白：这一次，是你自己点上的火。`
Line 10: `UI结果：第一章《重燃窑火》完成。`
Line 11: `UI结果：获得经验 → 等级 1 → 2`
Line 12: `UI结果：解锁器型：盘 SHAPE_002`
Line 13: `UI结果：解锁釉色：甜白釉 GLAZE_002`
Line 14: `UI结果：已完成图鉴：CODEX_006 / CODEX_007 / CODEX_009`
Line 15: `系统提示：第二阶段已开放 — 新器型、新订单、更高难度`

Notes: StoryMain 的两个玩家选项后徐老伯回应可在后续实现选择分支；当前 13 node 可先走默认认可文本。

## 9. Order UI Design Appendix

- UI 上有 `订单` 按钮。
- 点击 `订单` 按钮打开订单列表。
- 关闭订单列表不推进剧情。
- 接取 `ORDER_001` 后，订单列表显示 `甜白釉茶碗`。
- 完成 `ORDER_001` 后，从订单列表删除或隐藏。
- 接取 `ORDER_003` 后，订单列表更新为 `影青釉茶碗`。
- 完成 `ORDER_003` 后删除或隐藏。
- 接取 `ORDER_004` 后，订单列表更新为 `祭红釉香筒`。
- 完成第一章后订单列表为空，或显示 `第一章已完成`。
- 本阶段不实现订单 UI。
- 本阶段不接 Phase3 / Phase6 正式订单系统。
- Phase10 只保存叙事订单展示状态，不拥有 Phase3 gameplay order data。

## 10. Document Review Acceptance Criteria

- The document clearly states P10D-20R4 is docs-only.
- The document explicitly supports the latest user rule that speaker display prefixes are valid authored text.
- The prefix contract lists all supported display prefixes and intended speaker ids.
- Every line in the 13-node segmentation table has one line-start display prefix.
- The forbidden cases distinguish invalid prefix usage from valid prefixed dialogue.
- Speaker switches are represented as separate lines.
- Player choices are compressed into `玩家选项：...` lines and marked as future branch work.
- System prompts and UI result text use `系统提示：` / `UI结果：`, not NPC prefixes.
- Each node has notes for omitted or compressed StoryMain branches.
- No runtime, validator, asset, scene, ProjectSettings, Phase3, Phase6, or Phase8 completion is claimed.

## 11. Scope Boundary

- P10D-20R4 is submitted for user review only as a documentation artifact.
- This document is not proof that runtime speaker display works.
- This document is not proof that current dialogue assets have been imported or changed.
- This document is not proof that any automated validator exists or has run.
- Later implementation tasks must explicitly choose whether to import these prefixed lines into assets, update runtime display behavior, or add validators.
