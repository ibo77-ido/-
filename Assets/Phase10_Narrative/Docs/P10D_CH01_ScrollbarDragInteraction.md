# P10D-24T Scrollbar Drag Interaction for Log and Order Panels

## UI Structure

Dialogue Log keeps the existing scroll view shape and now binds a vertical scrollbar:

```text
DialogueLogPanel
└── DialogueLogScroll
    ├── Viewport
    │   └── Content
    └── DialogueLogScrollbar
        └── Sliding Area
            └── Handle
```

Current Order now uses the same scrollable panel structure:

```text
CurrentOrderPanel
└── CurrentOrderScroll
    ├── Viewport
    │   └── Content
    │       └── CurrentOrderContentText
    └── CurrentOrderScrollbar
        └── Sliding Area
            └── Handle
```

Both scroll views use `ScrollRect`, vertical-only movement, clamped bounds, mouse wheel sensitivity, and a linked `Scrollbar`. Content keeps padding through the content layout so body text does not sit under the bar.

## Log And Order Scrolling

- Dialogue Log supports mouse wheel scrolling and dragging `DialogueLogScrollbar/Sliding Area/Handle`.
- Current Order supports mouse wheel scrolling and dragging `CurrentOrderScrollbar/Sliding Area/Handle`.
- When content exceeds the viewport, dragging the handle changes `ScrollRect.verticalNormalizedPosition`.
- When content is short, the scrollbar can remain present through the prefab/runtime structure but is controlled by `ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport`.

## Art Slots

The scrollbar track and handle are reserved as normal `Image` components:

- track: `DialogueLogScrollbar` / `CurrentOrderScrollbar`
- handle: `Sliding Area/Handle`

The runtime/editor setup currently assigns placeholder image colors. Existing art can be connected by replacing those `Image` sprites without changing layout, scroll behavior, or validation assumptions.

## Click-Through Protection

Scroll views, tracks, and handles are raycast targets. Dragging or clicking the Log / Order scrollbars is handled by the overlay UI and does not advance the underlying dialogue.

The existing top-right panel mutual exclusion remains active:

- opening Dialogue Log hides `P10_TopRightActionBar`;
- opening Current Order hides `P10_TopRightActionBar`;
- closing either panel restores `P10_TopRightActionBar`.

No task progression rules, Dialogue Log snapshot schema, or current order state derivation rules were changed.

## Validation

| Check | Result |
|---|---|
| `dotnet build Phase10_Narrative.csproj` | PASS, 0 warnings, 0 errors |
| Unity batchmode validator | PASS |
| Method | `Phase10_Narrative.P10D14Chapter1FlowHarnessValidator.RunP10D24TScrollbarDragInteractionValidation` |
| Log | `Logs/P10D24T_ScrollbarDragInteraction.log` |
| Required PASS text | `P10D-24T Scrollbar Drag Interaction validation passed.` |

The validator covers:

- Dialogue Log panel has a `ScrollRect`.
- Dialogue Log scrollbar handle drag changes `verticalNormalizedPosition`.
- Dialogue Log content exceeds the viewport and scrolls.
- Current Order panel has a `ScrollRect`.
- Current Order scrollbar handle drag changes `verticalNormalizedPosition`.
- ScrollView, scrollbar track, and handle clicks do not advance dialogue.
- P10D-24 click-to-advance regression.
- P10D-21 current order panel regression.
- P10D-24S top-right action bar mutual exclusion regression.

## Manual Recheck

Still requires manual PlayMode review:

- verify Log and Order scrollbar visuals with final art sprites;
- drag the Log scrollbar handle through a long dialogue history;
- drag the Order scrollbar handle through a long current order hint;
- confirm mouse wheel scrolling on both panels;
- confirm repeated scroll, close, and reopen cycles do not leak clicks to the dialogue box.
