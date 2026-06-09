# P6-06 Scale System

## 逻辑与表现分离

```text
LogicRoot：碰撞、触发、导航、交互点
ArtRoot：模型、视觉、表现
```

规则：

1. 缩放只作用于 `ArtRoot`。
2. 不缩放 `LogicRoot`。
3. Prefab 原始尺寸不影响碰撞、导航和交互。
4. Phase6 地图视觉素材放在 `_MapRoot/ArtRoot`。
5. Phase6 地图可走区、障碍、区域触发和 NavMesh 放在 `_MapRoot/LogicRoot`。

## 统一缩放配置

### `AssetScaleConfigSO`

MVP 保持精简。

建议字段：

- `CharacterScale`
- `WorkstationScale`
- `BuildingScale`

### `ScaleManager`

职责：

1. 读取 `AssetScaleConfigSO`。
2. 对角色、工作台、建筑应用 MVP 缩放。
3. 支持测试时重新应用比例。

## 规则

1. 只缩放 `ArtRoot`。
2. 不动 `LogicRoot`。
3. 工作台刷新后仍符合比例。
