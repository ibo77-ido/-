# 项目长期记忆 — 景德镇·窑火千年

## 架构分层
- **Phase6**：世界层（玩家移动、NavMesh 手动驱动、E 键交互、工作台检测）
- **Phase8**：桥接层（世界层↔玩法层协调）
- **Phase3**：玩法层（Order→Shape→Glaze→Firing→Result 全流程）
- **Phase11**：HUD 交互层（纯引用，不修改下层代码）

## 核心设计原则
- HUD 层零侵入：通过 FindObjectOfType 查找 + 调用已有 public 方法，不修改 Phase3/6/8 任何文件
- 玩家移动：右键点地板 → PlayerCharacter.SetDestination() → MovementController 手动驱动 NavMesh
- 交互触发：InteractionController.TryInteract() → Workstation.Interact() → IInteractionEntryHandler

## 开发规范参考
- 文档路径：`C:/Users/lenovo/Desktop/景德镇御窑厂MVP开发实施规范.docx`
- 策划文档目录：`Task/` 文件夹
- Phase 规格文档格式：`Task/PhaseXX_功能名.md`

## Phase9 NavMesh / role-mobile memory - 2026-06-18
- Phase9 scene path: `Assets/Phase9/Scenes/SampleScene.unity`.
- Phase9 uses XZ-plane map alignment. `静态层` and `NavMesh-walkable` must stay fully overlapped in position/rotation/scale.
- Current Phase9 NavMesh bake source is `NavMeshWalkableBakeMesh_FromAlpha`, generated from `NavMesh-walkable.png` alpha pixels. Do not revert to Sprite Tight Mesh (`sprite.vertices` / `sprite.triangles`) for baking or click coverage.
- Legacy `NavMeshWalkableBakeMesh_FromSprite` should stay out of Road layer and its MeshCollider should stay disabled.
- Phase9 has a dedicated NavMesh agent type: `Phase9SmallAgent`, radius `0.06`, height `0.5`, voxel `0.01`.
- `Phase9_NavMeshSurface` and player `女主` use `Phase9SmallAgent`.
- Latest Phase9 NavMesh report: PASS, `2467` vertices, `1163` triangles, NavMesh Y `3.261`.
- `Phase9InteractionBridge.walkableAreaName` should be `NavMesh-walkable`, including the `_BridgeRoot` scene instance.
- Runtime click movement must be gated by renderer bounds coarse check + `NavMesh.SamplePosition` + `NavMesh.CalculatePath(PathComplete)`.
- Old `Phase9InteractionBridge` Sprite triangle click coverage was removed because it used Tight Mesh and could disagree with alpha-baked NavMesh.
- `Assets/Phase9/role-mobile.md` contains the step-by-step execution plan.
- role-mobile Step 1 completed: movement chain identified as `Phase9InteractionBridge -> PlayerCharacter -> MovementController`; player uses manual NavMesh path-corner movement.
- role-mobile Step 2 completed: click target parsing now resolves `NavMesh-walkable` and no longer depends on Sprite Tight Mesh coverage.
- Next role-mobile task: Step 3, verify `女主` has `PathComplete` routes to `Order-interact`, `Shape-interact`, `Glaze-interact`, and `Kiln-interact`.
