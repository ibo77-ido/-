# Phase11 主界面 UI 最终交付清单

## 当前接入状态

Phase11 主界面 UI 已按预期排版接入 Phase9 场景。

| 项目 | 状态 |
| --- | --- |
| 目标场景 | `Assets/Phase9/Scenes/SampleScene.unity` |
| UI 根对象 | 已添加 `HUDCanvas` |
| UI 点击系统 | 已添加 `EventSystem` |
| 场景保存 | 已保存 |
| 允许例外 | 本次按用户授权修改 Phase9 场景，只添加 Phase11 UI 实例与 EventSystem |

## 场景新增对象

| 对象 | 位置 | 说明 |
| --- | --- | --- |
| `HUDCanvas` | Phase9 场景根节点 | Phase11 主界面 UI |
| `EventSystem` | Phase9 场景根节点 | 让 uGUI Button 可点击 |

## 新增 Phase11 资产

| 类型 | 路径 |
| --- | --- |
| Prefab | `Assets/Phase11/Prefabs/HUDCanvas.prefab` |
| Prefab | `Assets/Phase11/Prefabs/QuickBarButton.prefab` |
| Prefab | `Assets/Phase11/Prefabs/Phase11UIBootstrapper.prefab` |
| Config | `Assets/Phase11/Data/HUDQuickBarConfig.asset` |
| Script | `Assets/Phase11/Scripts/MainPanelOutsideClickCloser.cs` |
| Script | `Assets/Phase11/Scripts/Phase11EconomyStatusBinder.cs` |
| Script | `Assets/Phase11/Scripts/Phase11PlayerEconomyLedger.cs` |
| Script | `Assets/Phase11/Scripts/Phase11ResultRewardBridge.cs` |
| Script | `Assets/Phase11/Scripts/Phase11UIBootstrapper.cs` |
| Screenshot | `Assets/Phase11/Screenshots/Phase11_Phase9_UI_Layout_Check.png` |

## 初版排版预期

| 区域 | 初版位置 |
| --- | --- |
| 头像/主角 | 左上角 |
| 当前订单 | 顶部中间 |
| 银两/声望 | 右上角 |
| 快捷按钮栏 | 底部中间 |
| 打开按钮 | 右下角 |
| 主面板 | 屏幕中央，默认隐藏 |

## 快捷按钮桥接

| 按钮 | AreaType | 行为 |
| --- | --- | --- |
| 接单 | `Order` | 寻路，到达后自动交互 |
| 拉胚 | `Wheel` | 寻路，到达后自动交互 |
| 施釉 | `Glaze` | 寻路，到达后自动交互 |
| 烧窑 | `Kiln` | 寻路，到达后自动交互 |
| 仓库 | `Storage` | 只寻路 |
| 材料 | `Material` | 只寻路 |

## 银两/声望桥接

```text
ResultSystem.LastResult.goldReward
ResultSystem.LastResult.reputationReward
        ↓
Phase11ResultRewardBridge
        ↓
Phase11PlayerEconomyLedger
        ↓
Phase11EconomyStatusBinder
        ↓
HUDCanvas/AlwaysOnHUD/EconomyBar
```

## 主面板交互

| 交互 | 结果 |
| --- | --- |
| 点击 `OpenButton` | 打开或关闭 `MainPanel` |
| 点击主面板外 | 关闭 `MainPanel` |
| 主面板打开时点击透明区域 | 被 `ModalClickShield` 拦截，不应穿透地图 |

## 本轮保留限制

| 项目 | 状态 |
| --- | --- |
| 主面板接单按钮逻辑 | 未桥接 |
| 接下一单按钮逻辑 | 未桥接 |
| 关闭按钮 | 未添加 |
| 提示框显示 | 未添加 |
| 气泡显示 | 未添加 |
| 日志条目显示 | 未添加 |

## 已验证

| 检查项 | 结果 |
| --- | --- |
| Phase9 场景存在 | 通过 |
| `HUDCanvas` 在 Phase9 场景中 | 通过 |
| `EventSystem` 在 Phase9 场景中 | 通过 |
| `HUDQuickBarConfig` 6 个按钮 | 通过 |
| `HUDCanvas` 关键桥接组件 | 通过 |
| `MainPanel` 默认隐藏 | 通过 |
| `ModalClickShield` 存在 | 通过 |

## 当前风险

| 风险 | 说明 |
| --- | --- |
| TMP 中文缺字 | 默认 `LiberationSans SDF` 不含中文，运行时可能显示方块 |
| 旧 PPtr 报错 | 控制台仍有 `PPtr cast failed when dereferencing! Casting from Sprite to Texture!`，来源待查 |
| 场景已有 dirty 状态 | 接入前 Phase9 场景已处于 dirty 状态，本次保存会把当前场景状态一起落盘 |

## 后续建议

1. 给 TMP 配置中文字体资产，解决中文方块。
2. 打开截图 `Assets/Phase11/Screenshots/Phase11_Phase9_UI_Layout_Check.png` 检查初版布局。
3. 在 Phase9 场景中手动微调 `HUDCanvas` 下各 RectTransform。
4. 运行场景，实际点击 6 个快捷按钮验证寻路/交互。
5. 完成一次结算流程，验证银两和声望累计显示。
