# P6-04 Workstation System

## 标准结构

```text
Workstation
├ LogicRoot
│  ├ Collider
│  └ InteractionPoint
└ ArtRoot
```

组件：

- `Workstation`
- `WorkstationConfigSO`
- `WorkstationVisualController`
- `IInteractable`

## 职责

### `Workstation`

- 持有 `WorkstationConfigSO`
- 提供 `AreaType`
- 提供 `InteractionPoint`
- 接收 `Interact(ICharacter actor)`
- 通过 `TestUIRouter` 打开测试 UI

### `WorkstationConfigSO`

- 保存工作台类型
- 保存区域类型
- 保存交互范围
- 保存提示文本
- 保存默认视觉 Prefab

### `WorkstationVisualController`

- 刷新工作台外观
- 应用统一缩放

接口：

```csharp
void RefreshVisual();
void ApplyScale();
```

## 强制规则

1. `RefreshVisual()` 只能替换 `ArtRoot` 下的视觉对象。
2. `RefreshVisual()` 不允许移动、销毁或重新生成 `LogicRoot`。
3. `Collider`、`Trigger`、`InteractionPoint` 必须保持稳定。
