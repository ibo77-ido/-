# P9-00 Phase9 Overview

## Goal

Phase9 uses UnityMCP to turn the existing map assets into a playable map layout that matches the reference image style.

## Target Result

- All map assets under `Assets/地图素材` are audited first.
- The map is built from the layered reference maps, not from a blank scene.
- The first pass produces a clear, readable, same-style map composition.
- Roads, water, buildings, vegetation, and props are organized into stable layers.
- The result can be inspected and adjusted in Unity without re-planning.

## Non-Goals

- Do not start with a full procedural map generator.
- Do not rebuild art assets.
- Do not merge gameplay systems into this phase.
- Do not expand scope beyond map construction and verification.

## Success Criteria

- Every usable map texture is accounted for.
- The reference map strategy is written down before implementation.
- The scene layer structure is fixed before objects are placed.
- A first-pass map can be generated and visually checked in Unity.

## Execution Rule

1. Audit assets with UnityMCP.
2. Write the map construction plan.
3. Lock the scene hierarchy and layer order.
4. Normalize import settings where needed.
5. Assemble the first map pass.
6. Validate with screenshots and console checks.
