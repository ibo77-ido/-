# 项目长期记忆 — 景德镇·窑火千年

## 架构分层
- **Phase6**：世界层（玩家移动、NavMesh 手动驱动、E 键交互、工作台检测）
- **Phase8**：桥接层（世界层↔玩法层协调）
- **Phase3**：玩法层（Order→Shape→Glaze→Firing→Result 全流程）
- **Phase11**：HUD 交互层（纯引用，不修改下层代码）

## 核心设计原则
- HUD 层零侵入（设计目标）：应通过事件/接口通信。注意：2026-06-22 诊断发现实际已违规——Phase11 用 FindObjectOfType 硬引用了 Phase6/Phase9 具体类型，需改为事件通信
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

## 团队职责划分（用户2026-06-22澄清）
- **美术只负责素材创作（画图/导出序列帧/写Shader），不负责 Unity 内资源管理**
- 技术美术工作（图集/压缩/Prefab结构/Shader迁移/材质库）当前无专人负责，由程序兼任
- 团队没有 Tech Art 角色——这是美术管线缺失的根源，不是美术的错
- 程序需兼做技术美术：建图集、统一压缩是最高性价比的改进（半天工作量，包体砍半）

## 项目诊断结论 - 2026-06-22（中厂标准评估）
- 报告路径：`Docs/项目诊断报告_中厂标准评估.md`
- **架构决策**：值得继续做，需"局部重构+全面规范"，不推倒重做
- **P0 止血项**：①找回StoryMain.md ②统一双桥接(Phase8/9二选一) ③Phase11改事件通信 ④建Sprite Atlas ⑤统一PNG压缩 ⑥清理调试文件
- **三职位评分**：策划6.5 程序4.5 美术3.5（满分10，中厂基线8）
- **核心矛盾**：重流程轻内容（AGENTS体系18份规范但StoryMain丢失）、重框架轻落地
- **开发模式实况**：实质"1人+AI Agent"模式，非PPT所述3人协作（AI生成代码埋下反射/God Object隐患）
- **值得保留**：Phase3数值数据库+评分模型(9/10)、IGameplayProgressionAuthority接口解耦、6个水墨Shader、DECISIONS.md的68条ADR
- **需重构**：Phase9/8双桥接统一、Phase10反射改接口、God Object拆分、asmdef程序集隔离
