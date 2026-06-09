# P6-05 Area System

## 核心对象

### `AreaConfigSO`

保存区域类型、名称和基础范围。

建议字段：

```text
areaType
areaName
boundsSize
boundsCenter
```

### `AreaTrigger`

监听玩家进入和离开区域。

### `AreaManager`

管理场景中所有区域，并记录当前玩家所在区域。

## 规则

1. 进入区域有反馈。
2. 离开区域有反馈。
3. 反复进出不抖动。
4. 当前区域可查询。

## 范围

当前阶段只保留区域检测与管理，不做完整区域视觉升级。
