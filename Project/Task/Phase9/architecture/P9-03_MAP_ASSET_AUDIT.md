# P9-03 Map Asset Audit

## UnityMCP Audit Result

UnityMCP search root:

`Assets/地图素材`

UnityMCP result:

- `Texture2D` total: `179`
- Page 1 returned: `179`
- Page 2 returned: `0`
- Query failures: `0`

This confirms Unity currently recognizes all 179 map image assets under the map source root.

## Asset Groups

| Group | Count | Phase9 Use |
| --- | ---: | --- |
| `地图分层` | 7 | First visual foundation / reference stack |
| `地表基础 Tile（Ground Tiles）` | 12 | Tilemap ground |
| `地形过渡 Tile` | 16 | Tilemap transition edges |
| `水体相关（Water）` | 9 | Tilemap water / shore / stream |
| `道路 Tile（Road System）` | 7 | Tilemap roads |
| `建筑（Prefabs  Props）` | 22 | Building prefabs |
| `植被（Vegetation）` | 32 | Vegetation prefabs |
| `装饰物（道具 Props）` | 44 | Prop prefabs |
| `天空与山体` | 6 | Atmosphere / optional background |
| `远景背景` | 9 | Reference / optional background |
| `水系氛围` | 7 | Reference / optional water mood |
| `建筑外壳` | 7 | Reference / optional building shell material |
| `建筑与地形的融合方式` | 1 | Reference only |

Total: `179`

## Build Classification

### Foundation Stack

Use first to establish the complete reference-style map:

1. `1地基层`
2. `2河岸过渡层`
3. `3水体层`
4. `4道路层`
5. `5基础建筑层`
6. `6建筑层`
7. `6装饰与道具`

These should be stacked with `SpriteRenderer` first. They provide the fastest visual match to the reference image.

### Tilemap Candidates

Use for editable terrain, roads, and water:

- `T_Grass_*`
- `T_Mud_*`
- `T_Stone_*`
- `T_Transition_*`
- `T_Water_*`
- `T_Road_*`

These are regularized tile assets and should not become individual prefabs unless a special collider or behavior is needed.

### Prefab Candidates

Use for reusable scene objects:

- `P_Bridge_Wood_01`
- `P_Building_*`
- `P_Kiln_*`
- `P_Watermill_01`
- `V_Bamboo_*`
- `V_Bush_*`
- `V_Grass_Tuft_01`
- `V_*_Flower_*`
- `V_Rock_*`
- `V_Tree_Pine_*`
- `V_Willow_*`
- `P_Prop_*`
- `P_Signpost_*`
- `P_Stele_01`

These should become prefab objects with `SpriteRenderer`, sorting rules, and later colliders or interaction points.

### Reference / Mood Only

Use for visual direction, not first-pass playable construction:

- `天空与山体`
- `远景背景`
- `水系氛围`
- `建筑外壳`
- `建筑与地形的融合方式`

These assets help judge style, depth, and atmosphere. They should not drive the first playable map unless the camera framing requires a backdrop.

## Decision

Phase9 should proceed with the three-layer method:

1. Layered map images as foundation.
2. Tilemap for ground, transitions, water, and roads.
3. Prefabs for buildings, vegetation, props, and later interaction objects.

## Next Task

Proceed to `task-p9-02-reference-map-strategy.md`.
