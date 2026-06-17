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
