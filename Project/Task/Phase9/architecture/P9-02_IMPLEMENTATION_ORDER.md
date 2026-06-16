# P9-02 Implementation Order

## Goal

Keep Phase9 implementation ordered, reviewable, and reversible.

## Order

1. Audit map assets through UnityMCP.
2. Confirm reference-map build strategy.
3. Create or confirm `Phase9_GeneratedMap` hierarchy.
4. Normalize import settings for map textures when needed.
5. Assemble the `地图分层` image stack.
6. Add camera framing for inspection.
7. Capture screenshots.
8. Add optional object-level sprites only after visual approval.
9. Add colliders and interaction points only after object placement is approved.
10. Decide go / no-go for refinement.

## UnityMCP Use

Use UnityMCP for:

- asset discovery
- scene hierarchy inspection
- GameObject creation
- SpriteRenderer setup
- camera setup and screenshots
- console checks

Do not use UnityMCP to generate the map until the relevant task is approved.

## First Implementation Slice

The first executable slice should only create:

- `Phase9_GeneratedMap`
- child layer groups
- the seven `地图分层` sprites
- one inspection camera if needed

No colliders, no gameplay wiring, and no interaction markers in the first slice.
