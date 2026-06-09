# task-p6-03 Interaction

## 目标

实现统一交互接口、交互点和测试 UI 路由。

## 步骤

1. 创建 `IInteractable`。
2. 创建 `InteractionPoint`。
3. 创建 `InteractionController`。
4. 创建 `TestUIRouter`。
5. 配置 `AreaType -> Test UI` 映射。
6. 让交互点触发测试 UI。
7. 让 UI 打开和关闭正确切换状态。

## 验收

- 玩家可交互
- 进入/离开范围反馈正常
- E 键触发稳定
- Workstation 不直接打开 UI
- 新增区域不改 Player 逻辑

## 依赖

- `task-p6-02-character.md`

## 交付物

- `IInteractable`
- `InteractionPoint`
- `InteractionController`
- `TestUIRouter`
- `STATE.md` / `DECISIONS.md` 已更新
