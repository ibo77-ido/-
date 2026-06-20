# Phase11 主界面 UI 接入说明

## 铁律

本轮 Phase11 主界面 UI 只允许新增文件、Prefab、配置资产、Inspector 引用和桥接脚本。

禁止修改任何已有代码、已有场景、已有 Prefab、已有素材和已有文档。

如果后续发现必须改旧逻辑才能完成需求，应停止实施，改为设计新增桥接方案。

## 新增资产

| 类型 | 路径 | 用途 |
| --- | --- | --- |
| Prefab | `Assets/Phase11/Prefabs/HUDCanvas.prefab` | Phase11 主 UI 根 Prefab |
| Prefab | `Assets/Phase11/Prefabs/QuickBarButton.prefab` | 快捷栏按钮模板 |
| Config | `Assets/Phase11/Data/HUDQuickBarConfig.asset` | 快捷栏 6 个按钮配置 |
| Script | `Assets/Phase11/Scripts/Phase11PlayerEconomyLedger.cs` | 银两/声望累计账本 |
| Script | `Assets/Phase11/Scripts/Phase11EconomyStatusBinder.cs` | 银两/声望文本绑定 |
| Script | `Assets/Phase11/Scripts/Phase11ResultRewardBridge.cs` | 结算奖励累计桥 |
| Script | `Assets/Phase11/Scripts/MainPanelOutsideClickCloser.cs` | 主面板框外点击关闭 |

## 使用方式

1. 将 `Assets/Phase11/Prefabs/HUDCanvas.prefab` 手动拖入需要显示 Phase11 UI 的场景。
2. 运行场景后，`HUDQuickBar` 会根据 `HUDQuickBarConfig.asset` 自动生成 6 个快捷按钮。
3. 点击右下角 `OpenButton` 会打开 `MainPanel`。
4. `MainPanel` 打开后，点击面板外任意位置会关闭面板。
5. 面板外层的 `ModalClickShield` 会拦截点击，避免点击透明区域时误触地图移动。

## Prefab 结构

```text
HUDCanvas
├─ AlwaysOnHUD
│  ├─ PlayerBadge
│  │  └─ AvatarFrame
│  │     └─ Protagonist
│  ├─ EconomyBar
│  │  ├─ SilverIcon
│  │  ├─ SilverText
│  │  ├─ ReputationIcon
│  │  └─ ReputationText
│  ├─ CurrentOrderChip
│  │  └─ CurrentOrderText
│  ├─ HUDQuickBar
│  │  ├─ QuickBarBackplate
│  │  └─ ButtonContainer
│  └─ OpenButton
├─ MainPanel
│  ├─ ModalClickShield
│  └─ PanelWindow
│     ├─ Title
│     ├─ PortraitArea
│     ├─ OrderArea
│     └─ ActionArea
└─ Bridges
```

## Inspector 引用表

### HUDCanvas

组件：`MainPanelOutsideClickCloser`

| 字段 | 绑定对象 |
| --- | --- |
| `Panel Root` | `HUDCanvas/MainPanel` |
| `Panel Bounds` | `HUDCanvas/MainPanel/PanelWindow` |
| `Open Button` | `HUDCanvas/AlwaysOnHUD/OpenButton` |

### EconomyBar

组件：`Phase11EconomyStatusBinder`

| 字段 | 绑定对象 |
| --- | --- |
| `Silver Text` | `HUDCanvas/AlwaysOnHUD/EconomyBar/SilverText` |
| `Reputation Text` | `HUDCanvas/AlwaysOnHUD/EconomyBar/ReputationText` |
| `Economy Ledger` | `HUDCanvas/Bridges` 上的 `Phase11PlayerEconomyLedger` |

### HUDQuickBar

组件：`HUDQuickBar`

| 字段 | 绑定对象 |
| --- | --- |
| `Config` | `Assets/Phase11/Data/HUDQuickBarConfig.asset` |
| `Button Container` | `HUDCanvas/AlwaysOnHUD/HUDQuickBar/ButtonContainer` |
| `Button Prefab` | `Assets/Phase11/Prefabs/QuickBarButton.prefab` |

### QuickBarButton

组件：`QuickBarButton`

| 字段 | 绑定对象 |
| --- | --- |
| `Icon Image` | `QuickBarButton/Icon` |
| `Label Text` | `QuickBarButton/Label` |
| `Button` | `QuickBarButton` 根节点上的 `Button` |

### Bridges

组件：

| 组件 | 作用 |
| --- | --- |
| `Phase11PlayerEconomyLedger` | 保存累计银两和累计声望 |
| `Phase11ResultRewardBridge` | 读取结算结果并写入累计账本 |

## 快捷按钮桥接表

| 按钮 | AreaType | 行为 |
| --- | --- | --- |
| 接单 | `Order` | 寻路，到达后自动交互 |
| 拉胚 | `Wheel` | 寻路，到达后自动交互 |
| 施釉 | `Glaze` | 寻路，到达后自动交互 |
| 烧窑 | `Kiln` | 寻路，到达后自动交互 |
| 仓库 | `Storage` | 只寻路，不自动交互 |
| 材料 | `Material` | 只寻路，不自动交互 |

注意：主面板素材 `接单按钮.png` 和 `接下一单按钮.png` 本轮只作为视觉储备，不桥接功能逻辑。

## 银两与声望累计链路

```text
ResultSystem.LastResult
    ├─ goldReward
    └─ reputationReward
        ↓
Phase11ResultRewardBridge
        ↓
Phase11PlayerEconomyLedger.AddRewards(...)
        ↓
Phase11EconomyStatusBinder
        ↓
EconomyBar/SilverText
EconomyBar/ReputationText
```

说明：

| 项目 | 处理方式 |
| --- | --- |
| 银两来源 | `ResultSystem.LastResult.goldReward` |
| 声望来源 | `ResultSystem.LastResult.reputationReward` |
| 累计方式 | 每次进入结算状态时读取最终奖励并累加 |
| 显示方式 | `Phase11EconomyStatusBinder` 监听账本变化并刷新文本 |

## 本轮明确不做

| 项目 | 状态 |
| --- | --- |
| 修改已有场景 | 不做 |
| 修改已有代码 | 不做 |
| 修改已有素材导入设置 | 不做 |
| 主面板关闭按钮 | 不添加 |
| 提示框显示逻辑 | 不添加 |
| 气泡提示逻辑 | 不添加 |
| 日志条目显示逻辑 | 不添加 |
| 主面板两个接单按钮功能桥接 | 不添加 |

## UI 与地图点击隔离

`MainPanel` 内新增了全屏透明节点 `ModalClickShield`。

它的职责：

| 职责 | 说明 |
| --- | --- |
| 拦截点击 | 主面板打开时，透明区域会接收 UI 点击 |
| 关闭面板 | 点击面板外任意区域会触发关闭 |
| 避免误触地图 | 透明区域不会把点击穿透给地图移动逻辑 |

这个方案不需要修改地图输入代码。

## 后续微调建议

| 区域 | 建议 |
| --- | --- |
| `AlwaysOnHUD` | 调整常驻 HUD 的屏幕边距和缩放 |
| `EconomyBar` | 微调银两/声望文本位置，使其贴合新增框架 |
| `HUDQuickBar/ButtonContainer` | 调整快捷按钮间距和底部位置 |
| `MainPanel/PanelWindow` | 按参考图调整主面板大小和内部区域比例 |
| `ModalClickShield` | 保持全屏，不建议缩小 |

## 验证清单

| 检查项 | 期望 |
| --- | --- |
| Prefab 能拖入场景 | `HUDCanvas` 正常显示 |
| 打开按钮 | 点击后显示 `MainPanel` |
| 框外点击 | 点击面板外关闭 `MainPanel` |
| 面板打开时点击地图 | 不触发地图移动 |
| 6 个快捷按钮 | 运行时自动生成 |
| 接单/拉胚/施釉/烧窑 | 到达后自动交互 |
| 仓库/材料 | 只寻路不自动交互 |
| 结算后银两/声望 | 显示累计值 |
