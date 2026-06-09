# task-p6-04 Workstation

## 目标

实现工作台配置、工作台主体和外观刷新。

## 步骤

1. 创建 `WorkstationConfigSO`。
2. 创建 `Workstation`。
3. 创建 `WorkstationVisualController`。
4. 绑定 `LogicRoot / ArtRoot`。
5. 让工作台实现 `IInteractable`。
6. 让 `Interact(ICharacter actor)` 通过 `TestUIRouter` 打开对应 UI。
7. 实现 `RefreshVisual()`。
8. 确保 `RefreshVisual()` 不影响交互点和逻辑节点。

## 验收

- 工作台可配置
- 外观可刷新
- 逻辑和表现分离
- 刷新后仍可交互

## 依赖

- `task-p6-03-interaction.md`

## 交付物

- `WorkstationConfigSO`
- `Workstation`
- `WorkstationVisualController`
- `RefreshVisual()`
- `STATE.md` / `DECISIONS.md` 已更新
