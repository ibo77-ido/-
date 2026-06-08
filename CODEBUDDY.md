# Project Conventions — 景德镇·窑火千年 (PHASE4)

## Code Style

- **Naming**: PascalCase for types/methods/properties, camelCase for fields/variables
- **Private fields**: `private camelCase` (no underscore prefix)
- **Serialized fields**: `[SerializeField] private camelCase`
- **Namespaces**: None (global namespace)
- **Indentation**: Tabs, Allman braces (opening brace on new line)
- **File naming**: One class per file, filename matches class name exactly

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
```

## Unity Patterns

- **ScriptableObject**: `[CreateAssetMenu(...)]`, `public camelCase` fields (no [SerializeField]), `[Range]`/`[Header]` attributes
- **MonoBehaviour**: Serialize dependencies with `[SerializeField] private`, use `Awake`/`Start`/`OnDestroy` lifecycle
- **Static Calculator**: Pure math class, input/output as `struct` value types, return new struct via object initializer
- **Debug logging**: `Debug.Log($"[ClassName] Message")` with square-bracket prefix
- **Unity version**: 2022.3.55f1c1, URP

## Agent Workflow

Follow `AGENTS/RuntimeLayer/run.md` for task execution: design → approve → implement → verify.
