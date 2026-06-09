# P6-03 Interaction System

## 统一交互接口

### `IInteractable`

建议接口：

```csharp
public interface IInteractable
{
    void Interact(ICharacter actor);
}
```

作用：

- 统一工作台、NPC、箱子、告示牌等交互入口
- 避免 `InteractionController` 依赖具体类型

## 交互控制

### `InteractionController`

职责：

1. 查找可交互目标。
2. 判断距离和状态。
3. 调用 `IInteractable.Interact(ICharacter actor)`。
4. 不直接打开具体 UI。

## 交互点

### `InteractionPoint`

职责：

- 定义交互入口
- 作为工作台固定交互点
- 避免贴脸交互和穿模

建议参数：

| 参数 | 建议值 |
| --- | --- |
| Interaction Distance | 1.5m |
| Stopping Distance | 0.5m |

## 测试 UI 路由

### `TestUIRouter`

职责：

1. 根据 `AreaType` 打开测试 UI。
2. 打开 UI 时设置 `CharacterState = UIOpen`。
3. 关闭 UI 时恢复 `Idle`。

建议映射：

| AreaType | Test UI |
| --- | --- |
| Order | OrderUI_Test |
| Wheel / Shape | ShapeUI_Test |
| Glaze | GlazeUI_Test |
| Kiln | KilnUI_Test |
| Storage / Warehouse | WarehouseUI_Test |
| Material | MaterialUI_Test 或 WarehouseUI_Test |

## 规则

1. Workstation 不直接打开 UI。
2. PlayerCharacter 不直接打开 UI。
3. 新增区域只改映射，不改玩家逻辑。
