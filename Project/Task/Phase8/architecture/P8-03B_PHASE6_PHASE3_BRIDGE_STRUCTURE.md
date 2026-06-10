# P8-03B Bridge Structure

## Recommended Scene Structure

```text
Phase6 Scene
├─ WorldRoot
├─ LogicRoot
├─ ArtRoot
├─ BridgeRoot
│  ├─ GameplayBridgeManager
│  ├─ Phase3RuntimeContext
│  └─ BridgeCanvas
│     ├─ Order Module Instance
│     ├─ Shape Module Instance
│     ├─ Glaze Module Instance
│     ├─ Firing Module Instance
│     └─ Result Module Instance
└─ Workstation Roots
```

## Data Ownership

- `GameplayBridgeManager` owns session creation, start, complete, and abort.
- `GameplayModuleSession` stores the current module, area, runtime mode, and lock state.
- `Phase3ModuleAdapter` only binds, opens, closes, and unbinds module instances.
- `BridgeInputLock` only locks and restores Phase6 input.
- `BridgeCanvasController` only hosts and closes runtime UI.

## File Responsibility Matrix

- `GameplayBridgeManager.cs`: session lifecycle and bridge scheduling
- `GameplayModuleSession.cs`: session state container
- `Phase3ModuleAdapter.cs`: module open / close / bind / unbind
- `BridgeInputLock.cs`: movement and interaction lock
- `BridgeCanvasController.cs`: runtime UI root control
- `GameManager.cs`: minimal auto-advance gate only
- `ResultPanelController.cs`: completion relay only
