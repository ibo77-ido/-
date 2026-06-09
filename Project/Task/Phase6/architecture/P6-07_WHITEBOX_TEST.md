# P6-07 WhiteBox Test

## 测试目标

验证 Phase6 的空间化闭环是否成立。

## 测试 UI

必须准备：

- `OrderUI_Test`
- `ShapeUI_Test`
- `GlazeUI_Test`
- `KilnUI_Test`
- `WarehouseUI_Test`
- `MaterialUI_Test` 可选

## 测试重点

- 场景打开
- 移动
- 连续点击
- 边界
- 摄像机跟随
- 区域进出
- 交互点
- E 键触发
- UI 锁定
- 工作台配置
- 外观刷新
- 缩放生效
- 逻辑与表现分离
- 完整流程

## 完成定义

只要以下闭环成立即可进入 Phase7：

```text
订单区 -> 拉坯区 -> 配釉区 -> 烧窑区 -> 返回订单区
```

并且：

- 无 Console Error
- 玩家可移动
- 摄像机跟随正常
- 区域检测正常
- E 键交互正常
- `RefreshVisual()` 正常
- `AssetScaleConfigSO` 正常
- `SO` 驱动正常
- `UIOpen` 锁定正常
