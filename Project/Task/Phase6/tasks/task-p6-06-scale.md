# task-p6-06 Scale

## 目标

实现统一缩放配置和应用机制。

## 步骤

1. 创建 `AssetScaleConfigSO`。
2. 创建 `ScaleManager`。
3. 约定 `LogicRoot / ArtRoot` 结构。
4. 只对 `ArtRoot` 做缩放。
5. 为角色、工作台、建筑应用统一比例。
6. 支持测试时重新应用比例。

## 验收

- 缩放统一管理
- 原始 Prefab 尺寸不污染逻辑
- 工作台刷新后仍符合比例

## 依赖

- `task-p6-04-workstation.md`

## 交付物

- `AssetScaleConfigSO`
- `ScaleManager`
- 统一缩放规则
- `STATE.md` / `DECISIONS.md` 已更新
