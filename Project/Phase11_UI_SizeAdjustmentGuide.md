# Phase11 主界面 UI 大小调整指南

## 为什么主要在 PlayMode/Game 视图调整

`HUDCanvas` 使用的是 `Screen Space - Overlay`。

这类 UI 不依赖世界坐标，也不是地图里的 Sprite 物件。它最终显示在 PlayMode 的 Game 视图里，因此大小判断应以 Game 视图为准。

## 推荐调整方式

1. 打开 `Assets/Phase9/Scenes/SampleScene.unity`。
2. 选中 Hierarchy 里的 `HUDCanvas`。
3. 打开 Game 视图，设置常用分辨率，例如 16:9 的 1280x720 或 1920x1080。
4. 在不进入 PlayMode 时，可以看 `EditorPreviewIconRow` 的右上预览图标。
5. 进入 PlayMode 后，`EditorPreviewIconRow` 会自动隐藏，真实按钮由 `HUDQuickBar` 动态生成。

## 调整入口

| 想调什么 | 调哪个对象 |
| --- | --- |
| 整体 UI 缩放 | `HUDCanvas` 上的 `CanvasScaler` |
| 左上头像大小 | `HUDCanvas/AlwaysOnHUD/PlayerBadge` |
| 顶部资金/声望条 | `HUDCanvas/AlwaysOnHUD/TopStatusBars` |
| 左侧银两/声望框 | `HUDCanvas/AlwaysOnHUD/EconomyBar` |
| 左侧订单打开按钮 | `HUDCanvas/AlwaysOnHUD/OpenButton` |
| 右上快捷栏位置 | `HUDCanvas/AlwaysOnHUD/HUDQuickBar` |
| 右上预览图标大小 | `HUDCanvas/AlwaysOnHUD/HUDQuickBar/EditorPreviewIconRow/*Preview` |
| 运行时真实按钮大小 | `Assets/Phase11/Prefabs/QuickBarButton.prefab` |

## 图标大小建议

| 元素 | 推荐屏幕观感 |
| --- | --- |
| 头像框 | 明显可辨，但不要超过左上角 15% 宽度 |
| 银两/声望图标 | 能看清图案，约为头像宽度的 25%-30% |
| 右上快捷图标 | 单个按钮约 76-84 屏幕像素较合适 |
| 打开订单按钮 | 比快捷图标略小或相近，不要抢主视觉 |

## 当前尺寸基准

当前 `CanvasScaler` 参考分辨率是 `1920x1080`，Game 视图如果是 `1280x720`，视觉上会按约 `0.666` 缩放。

因此：

| 设计尺寸 | 1280x720 下大约显示 |
| --- | --- |
| 120 px | 80 px |
| 104 px | 69 px |
| 58 px | 39 px |
| 202 px | 135 px |

## 快捷按钮的两个层

| 层 | 作用 |
| --- | --- |
| `EditorPreviewIconRow` | 编辑态预览尺寸，PlayMode 自动隐藏 |
| `HUDQuickBar` 运行时按钮 | PlayMode 中由 `HUDQuickBarConfig.asset` 自动生成，负责真实点击逻辑 |

注意：如果只调了 `EditorPreviewIconRow`，运行时真实按钮大小不会跟着变。运行时按钮大小要调 `Assets/Phase11/Prefabs/QuickBarButton.prefab`。

## 我建议的微调顺序

1. 先调 `HUDQuickBar` 的位置，让右上图标靠参考图。
2. 再调 `QuickBarButton.prefab` 的整体按钮大小。
3. 然后调 `Icon` 子节点大小，避免图案贴边。
4. 再调 `PlayerBadge` 与 `EconomyBar`。
5. 最后进 PlayMode 看真实按钮是否与预览一致。

## 不建议

| 操作 | 原因 |
| --- | --- |
| 直接缩放 `HUDCanvas` Transform | 容易影响所有 UI 与 CanvasScaler 的换算 |
| 只在 Scene 视图判断大小 | Overlay UI 最终以 Game 视图为准 |
| 只调预览图标 | 预览层 PlayMode 会隐藏，不影响真实按钮 |
| 把图标做太透明 | 容易和地图混在一起 |
