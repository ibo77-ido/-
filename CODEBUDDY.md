# Project Conventions — 景德镇·窑火千年 (Phase7 COMPLETE)

## Code Style

- **Naming**: PascalCase for types/methods/properties, camelCase for fields/variables
- **Private fields**: `private camelCase` (no underscore prefix)
- **Serialized fields**: `[SerializeField] private camelCase`
- **Namespaces**: None (global namespace)
- **Indentation**: Tabs, Allman braces (opening brace on new line)
- **File naming**: One class per file, filename matches class name exactly
- **Phase6 naming conflict avoidance**: `Phase6GameManager` (not `GameManager`), `Phase6GameState` (not `GameState`)

## Project Structure

```
Assets/Phase3/
  Data/          — ScriptableObject configs, structs, enums
  Scripts/
    Calculators/ — Static pure-logic classes
    Core/        — Central orchestrator (GameManager)
    Systems/     — MonoBehaviour game systems (Order, Shape, Glaze, Firing, Result)
    UI/          — MonoBehaviour panel controllers
  Editor/        — Editor-only tools (not under Scripts/)
  Scenes/        — Scene files

Assets/Phase6/
  Scenes/        — Workshop_TestScene
  Scripts/       — Phase6 scripts (Phase6GameManager, etc.)
  Prefabs/       — Prefabs
  Data/          — ScriptableObject configs (AreaConfigSO, WorkstationConfigSO, etc.)
  Materials/     — Whitebox materials (Mat_Walkable_Green, Mat_Static_Gray)
```

## Unity Patterns

- **ScriptableObject**: `[CreateAssetMenu(...)]`, `public camelCase` fields (no [SerializeField]), `[Range]`/`[Header]` attributes
- **MonoBehaviour**: Serialize dependencies with `[SerializeField] private`, use `Awake`/`Start`/`OnDestroy` lifecycle
- **Static Calculator**: Pure math class, input/output as `struct` value types, return new struct via object initializer
- **Debug logging**: `Debug.Log($"[ClassName] Message")` with square-bracket prefix
- **Unity version**: 2022.3.55f1c1, URP
- **Phase6 LogicRoot/ArtRoot separation**: Collider/Trigger/InteractionPoint on LogicRoot (never scaled), visual on ArtRoot (scaled by ScaleManager)
- **Phase6 RefreshVisual()**: Only replaces ArtRoot children, never moves/destroys LogicRoot
- **Phase7 layer boundary**: Phase3 scripts must NOT reference Phase6 types; cross-layer communication via UnityEvent (scene Inspector binding), never direct SerializeField references
- **Phase7 state authority**: Phase6GameManager is sole authority for World State (Playing/UIOpen); Phase3 GameManager is sole authority for Gameplay State (Order/Shape/Glaze/Firing/Result); cross-domain control prohibited

## Agent Workflow

Follow `AGENTS/RuntimeLayer/run.md` for task execution: design → approve → implement → verify.

After each task step, sync: STATE.md + DECISIONS.md + CODEBUDDY.md + memory files.

## Suggestion Evaluation Rule

When the user proposes a suggestion or direction, do NOT immediately execute it. First evaluate feasibility against the current project's concrete information (code state, architecture, existing decisions, scene bindings, etc.). If feasible, execute. If not feasible, explain the specific reason with evidence from the project. Never blindly execute without assessment.
