# P10D-21 Current Order Panel with Crafting Hints

Status: PASS by automation; needs human PlayMode recheck.

## Scope

This feature is Phase10-only. It displays the current narrative order and crafting hints from Phase10 state/node context only.

It does not read Phase3 order data, does not connect to the formal Phase3 order system, does not add nodes, does not change task progression, and does not extend the snapshot schema.

## UI Entry

The Phase10 runtime dialogue surface now has a persistent `PersistentOrderButton` labelled `订单`.

Clicking it opens `CurrentOrderPanel`. Closing the panel only hides the panel. It does not advance narrative state and does not affect Dialogue Log entries.

Prefab path:

```text
Assets/Phase10_Narrative/Prefabs/UI/P10_CH01_DialogueUI_Placeholder.prefab
```

Prefab objects:

```text
P10_Runtime_DialogueSurface/PersistentOrderButton
P10_Runtime_DialogueSurface/CurrentOrderPanel
P10_Runtime_DialogueSurface/CurrentOrderPanel/CurrentOrderTitleText
P10_Runtime_DialogueSurface/CurrentOrderPanel/CurrentOrderContentText
P10_Runtime_DialogueSurface/CurrentOrderPanel/CurrentOrderCloseButton
```

## Display Rules

No active order:

```text
当前订单
当前无订单
```

Active order fields:

```text
订单名称
来源 NPC
当前目标
器型提示
釉色提示
烧成温度
评分要求
奖励
状态
```

## Lifecycle

- `P10_CH01_NODE_ORDER_001_ACCEPT` shows ORDER_001.
- After ORDER_001 pass/fail node, the panel returns to no active order until ORDER_003 accept.
- `P10_CH01_NODE_ORDER_003_ACCEPT` shows ORDER_003.
- After ORDER_003 pass/fail node, the panel returns to no active order until ORDER_004 accept.
- `P10_CH01_NODE_ORDER_004_ACCEPT` shows ORDER_004.
- After ORDER_004 result and chapter completion, the panel shows no active order or completion text.

## Order Content

ORDER_001:

```text
订单名称：甜白釉茶碗
来源 NPC：周掌柜
当前目标：制作 3 只甜白釉茶碗
器型提示：碗口要宽，碗底要稳
釉色提示：白里带一点暖，不要冷冰冰
烧成温度：1250°C - 1300°C
评分要求：器型评分 ≥ 80，釉色评分 ≥ 75
奖励：50 铜钱，声望 +10
状态：进行中
```

ORDER_003:

```text
订单名称：影青釉茶碗
来源 NPC：陈书院
当前目标：制作 1 只影青釉茶碗
器型提示：碗口要正，腹部要收
釉色提示：半透半不透，像雾里的月
烧成温度：1250°C - 1280°C
评分要求：器型评分 ≥ 80，釉色评分 ≥ 75
奖励：55 铜钱，声望 +10
状态：进行中
```

ORDER_004:

```text
订单名称：祭红釉香筒
来源 NPC：卢客
当前目标：制作 1 件祭红釉直筒香罐
器型提示：直筒型，高一点，口径约十厘米，高约十三厘米
釉色提示：正红，不能暗，不能偏紫，不能发黑
烧成温度：1250°C - 1280°C
评分要求：烧窑评分 ≥ 70，精品综合评分 ≥ 95
奖励：70 铜钱，声望 +10；精品可获得额外奖励
状态：进行中
```

## Validation

Unity batchmode execute method:

```text
Phase10_Narrative.P10D14Chapter1FlowHarnessValidator.RunP10D21CurrentOrderPanelWithCraftingHintsValidation
```

Required pass log:

```text
P10D-21 Current Order Panel with Crafting Hints validation passed.
```

The validator checks:

- Persistent `订单` button exists in the Phase10 UI prefab.
- Current order panel title/content/close controls exist.
- No-order state displays `当前无订单`.
- ORDER_001, ORDER_003, and ORDER_004 display every required field.
- Completing each order hides/removes the active order display until the next accept node.
- Opening/closing the order panel does not advance narrative state.
- Opening/closing the order panel does not change Dialogue Log count and does not open Dialogue Log.
- P10D-20R9B/R8/R7/R6/R5 and P10D-19 regressions still pass.

## Evidence

- `dotnet build Phase10_Narrative.csproj`: PASS, 0 warnings / 0 errors.
- Unity validator:
  - Log: `Logs/P10D21_CurrentOrderPanelWithCraftingHints.log`
  - PASS text present: `P10D-21 Current Order Panel with Crafting Hints validation passed.`

## Manual Recheck

Still needs human PlayMode recheck:

- Confirm `订单` button is visible and opens the current order panel.
- Confirm closing the panel does not progress dialogue or narrative.
- Confirm Dialogue Log remains independent.
- Confirm the active order and crafting hints are readable at each accept node.
