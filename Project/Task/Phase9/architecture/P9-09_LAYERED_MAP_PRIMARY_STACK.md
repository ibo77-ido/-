# P9-09 Layered Map Primary Stack

## Correction

Phase9 map construction must use the authored layered PNGs as the primary visual map, not a freehand Tilemap recreation.

The correct build order is:

1. `1地基层.png` as the base ground.
2. `2河岸过渡层.png` as the shoreline transition.
3. `3水体层.png` as the river and water layer.
4. `4道路层.png` as the road layer.
5. `5基础建筑层.png` as the building footprint/base layer.
6. `6建筑层.png` as the building layer.
7. `6装饰与道具.png` as the final decoration and props layer.

## Scene Result

Scene:

- `Assets/Phase9/Scenes/Phase9_MapPrototype.unity`

Screenshot:

- `Assets/Screenshots/screenshot-20260616-134444.png`

Applied changes:

- Set the seven layered SpriteRenderers to full opacity.
- Assigned visible sorting order from 100 to 700 in layer order.
- Disabled TilemapRenderer components for the old Tilemap reconstruction layers.
- Hid 32 manual `P9_` overlay objects so they do not duplicate the authored building and decoration layers.

## Rule Going Forward

The layered PNG stack is the visual source of truth.

Tilemap may be used later for invisible or semi-visible gameplay support only:

- collision masks
- navigation areas
- interaction regions
- local repair patches

Prefabs may be used later for interactive replacement or animated detail only after their position is matched against the authored layer stack.
