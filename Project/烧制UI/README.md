# 烧制 UI 接入任务总览

> 依据 `Project/烧制UI方案.md` 拆分，共 6 个任务，按依赖顺序执行。

## 任务依赖图

```
Task01_FiringSystem扩展
   │
   ├──> Task02_场景层级搭建
   │       │
   │       ├──> Task03_FiringPanelController状态机
   │       │       │
   │       │       ├──> Task04_风门滚轮交互
   │       │       └──> Task05_温度条视频驱动
   │       │
   │       └──> Task06_Phase3SceneBuilder检测分支
   │
   └──> (Task01 完成后即可被 Task03/Task05 调用)
```

## 任务清单

| 序号 | 任务文件 | 职责 | 依赖 |
|------|---------|------|------|
| 01 | Task01_FiringSystem扩展.md | FiringSystem 新增低风门降温 + 3个接口 | 无 |
| 02 | Task02_场景层级搭建.md | Panel_Firing 下搭 ArtRoot_Firing + 4个视频 + 风门 + 按钮 | Task01 字段定义 |
| 03 | Task03_FiringPanelController状态机.md | 3态状态机 + 视频控制 + 起势→循环衔接 + Refresh缓存 | Task01 接口 + Task02 节点 |
| 04 | Task04_风门滚轮交互.md | 风门序列帧 + 鼠标滚轮检测 + 9档切换 | Task02 风门节点 + Task03 状态机 |
| 05 | Task05_温度条视频驱动.md | temperatureBarProgress 维护 + 视频 seek + 倒放 + 强制开窑 | Task01 接口 + Task03 状态机 |
| 06 | Task06_Phase3SceneBuilder检测分支.md | EnsureFiringPanelControls 加 ArtRoot_Firing 检测 | Task02 ArtRoot_Firing 节点 |

## 执行原则

- 每个任务按 `AGENTS/RuntimeLayer/run.md` 流程：Design → 审批 → 实现 → 验证
- 严格限制在当前 Task 范围，不提前开发后续 Task
- 每个任务完成后同步 STATE.md + DECISIONS.md + CODEBUDDY.md + memory
- 任务间通过 SerializeField / Inspector 引用解耦，避免代码耦合

## 验收顺序

1. Task01 完成 → FiringSystem 可独立编译运行（低风门降温可测）
2. Task02 完成 → 场景中可见美术节点层级（无逻辑）
3. Task03 完成 → 烧制主流程可跑通（起势→循环→停火→开窑）
4. Task04 完成 → 风门滚轮可操作
5. Task05 完成 → 温度条视频驱动 + 次品分支可测
6. Task06 完成 → Builder 不再污染美术面板

全部完成后按方案 §验收清单 做整体验收。
