# Phase11 主界面 UI 在 Phase9scene 的接入说明

## 目标

Phase11 游戏主界面 UI 最终在 `Phase9scene` 中表现。

在当前铁律下，不直接修改或保存 `Phase9scene` 场景文件。接入方式改为新增一个安装入口 Prefab，由你手动拖入 `Phase9scene`。

## 新增安装入口

| 类型 | 路径 | 用途 |
| --- | --- | --- |
| Script | `Assets/Phase11/Scripts/Phase11UIBootstrapper.cs` | 运行时实例化 HUDCanvas |
| Prefab | `Assets/Phase11/Prefabs/Phase11UIBootstrapper.prefab` | 可拖入 Phase9scene 的安装入口 |

## Phase9scene 接入方式

1. 打开 `Phase9scene`。
2. 将 `Assets/Phase11/Prefabs/Phase11UIBootstrapper.prefab` 拖入场景根节点。
3. 运行场景。
4. `Phase11UIBootstrapper` 会在 `Awake` 时实例化 `Assets/Phase11/Prefabs/HUDCanvas.prefab`。
5. 如果场景里已经存在同名 `HUDCanvas`，安装器会复用已有对象，不重复创建。

## Bootstrapper Inspector

| 字段 | 默认值 | 说明 |
| --- | --- | --- |
| `Hud Canvas Prefab` | `Assets/Phase11/Prefabs/HUDCanvas.prefab` | 要实例化的 UI 根 Prefab |
| `Instantiate On Awake` | true | 进入场景时自动生成 UI |
| `Skip If Hud Already Exists` | true | 避免重复生成 `HUDCanvas` |

## 为什么不直接改 Phase9scene

当前规则是只新增文件、Prefab、配置和桥接引用，不改已有场景。

直接把 `HUDCanvas` 或 `Phase11UIBootstrapper` 写入 `Phase9scene` 会导致已有场景文件发生修改，因此本轮不自动保存场景变更。

## 推荐做法

| 情况 | 推荐 |
| --- | --- |
| 你想手动控场 | 直接拖 `HUDCanvas.prefab` 进 `Phase9scene` |
| 你想保持场景更干净 | 拖 `Phase11UIBootstrapper.prefab` 进 `Phase9scene` |
| 你想后续自动接入 | 继续新增独立安装工具，但仍不修改旧场景 |

## 验证点

| 检查项 | 期望 |
| --- | --- |
| Phase9scene 运行 | 自动出现 Phase11 主 UI |
| Hierarchy | 出现 `HUDCanvas` |
| 重复运行或已有 HUDCanvas | 不重复生成多个 HUDCanvas |
| 打开按钮 | 可以打开主面板 |
| 主面板外点击 | 可以关闭主面板且不穿透地图 |
| 快捷按钮 | 6 个按钮运行时生成 |
