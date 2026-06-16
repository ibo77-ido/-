# P9-01 Map Asset and Build Strategy

## Source Root

Phase9 uses this asset root:

`Assets/地图素材`

## Known Asset Groups

- `地图分层`: main layered map images.
- `地表基础 Tile（Ground Tiles）`: repeatable ground tiles.
- `地形过渡 Tile`: edge and transition tiles.
- `水体相关（Water）`: water, stream, and shore tiles.
- `道路 Tile（Road System）`: road shape tiles.
- `建筑（Prefabs  Props）`: houses, kiln, bridge, watermill.
- `植被（Vegetation）`: trees, bamboo, grass, flowers, rocks.
- `装饰物（道具 Props）`: barrels, jars, crates, fences, signs, tables.
- `天空与山体`, `远景背景`, `水系氛围`, `建筑外壳`: atmosphere and reference material.

## Build Strategy

The first implementation pass should use the layered map images as the visual foundation.

Priority:

1. Stack the `地图分层` images as the first full-map composition.
2. Align all layer sprites to the same origin.
3. Add object-level sprites only where they improve readability or interaction.
4. Add colliders and interaction markers only after visual approval.

## Initial Layer Stack

Recommended order:

1. `1地基层`
2. `2河岸过渡层`
3. `3水体层`
4. `4道路层`
5. `5基础建筑层`
6. `6建筑层`
7. `6装饰与道具`

## Scene Root

Use this root object:

`Phase9_GeneratedMap`

## Child Groups

```text
Phase9_GeneratedMap
├── 00_Background
├── 10_BaseGround
├── 20_ShoreTransition
├── 30_Water
├── 40_Road
├── 50_BuildingBase
├── 60_Buildings
├── 70_Vegetation
├── 80_Props
├── 90_Foreground
└── 99_Debug
```

## Sorting Order Contract

- Background: `0`
- Base ground: `100`
- Shore transition: `200`
- Water: `300`
- Road: `400`
- Building base: `500`
- Buildings: `600`
- Vegetation: `700`
- Props: `800`
- Foreground: `900`
- Debug: `1000`

## Validation

Every UnityMCP implementation pass must finish with:

- console check
- scene hierarchy check
- screenshot check
- short written result

## Reference Preservation Rule

The seven `地图分层` images must stay in the Phase9 prototype scene as reference layers.

They may be made transparent and moved behind editable content, but they should not be deleted during map construction. They are the visual guide for composition, style, and placement while Tilemap and Prefab layers become the editable playable map.
