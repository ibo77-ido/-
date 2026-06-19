# Phase 11 UI 美术素材需求汇总

基于 `Phase11_GameHUD.md`、`Phase11A_EditorSetup.md` 以及当前项目已有资源，整理出 Phase11 需要的 UI 美术素材清单。

## 1. 结论

Phase11 的 UI 需求主要分为两块：

1. `HUDQuickBar` 快捷入口栏
2. `PlayerStatusBar` 左上角状态栏

其中 Phase11B 的相机缩放 / 平移 / 智能复位，基本不新增 UI 美术素材。

## 2. 当前项目可复用资源

项目里已经有一批可直接复用或参考的 UI 资源，优先级如下：

### 2.1 主界面 UI

- `Assets/游戏主界面UI/接单按钮.png`
- `Assets/游戏主界面UI/接下一单按钮.png`
- `Assets/游戏主界面UI/打开按钮.png`
- `Assets/游戏主界面UI/提示框.png`
- `Assets/游戏主界面UI/资金条.png`
- `Assets/游戏主界面UI/声望条.png`
- `Assets/游戏主界面UI/铜钱.png`
- `Assets/游戏主界面UI/声望.png`
- `Assets/游戏主界面UI/进度条（空）.png`
- `Assets/游戏主界面UI/进度条（满）.png`

### 2.2 Phase3 UI

- `Assets/Phase3/UI/订单面板/订单面板 空.png`
- `Assets/Phase3/UI/订单面板/订单面板 文字.png`
- `Assets/Phase3/UI/订单面板/接单按钮.png`
- `Assets/Phase3/UI/烧窑/提示框.png`
- `Assets/Phase3/UI/烧窑/温度条.png`
- `Assets/Phase3/UI/烧窑/确定按钮.png`
- `Assets/Phase3/UI/烧窑/添燃料按钮.png`
- `Assets/Phase3/UI/烧窑/底图 阶段1.png`
- `Assets/Phase3/UI/烧窑/底图 阶段2.png`
- `Assets/Phase3/UI/烧窑/底图 阶段3.png`

### 2.3 地图素材可借用做图标参考

- `Assets/地图素材/建筑（Prefabs  Props）/仓库区.png`
- `Assets/地图素材/建筑（Prefabs  Props）/原料区.png`
- `Assets/地图素材/建筑（Prefabs  Props）/器型区.png`
- `Assets/地图素材/建筑（Prefabs  Props）/烧制区.png`
- `Assets/地图素材/建筑（Prefabs  Props）/订单区.png`
- `Assets/地图素材/建筑（Prefabs  Props）/釉料区.png`

## 3. Phase11 必须补的素材

### 3.1 快捷栏底板

用途：底部居中的 `HUDQuickBar` 背景皮肤。

建议素材：

- 快捷栏底板背景
- 边框 / 描边
- 按钮槽位背景
- 选中高亮框
- 禁用态灰化背景

说明：

- 目前项目里没有专门为 Phase11 快捷栏准备的统一底板。
- 可以先临时用现有按钮或提示框风格拼装，但正式版建议单独出一套。

### 3.2 快捷栏按钮皮肤

用途：6 个功能按钮的统一外观。

需要的状态：

- 正常态
- 悬停态
- 按下态
- 高亮态
- 禁用态

建议分辨率：

- 单个按钮皮肤建议按 `128x128` 或 `256x256` 规格制作，便于后期缩放。

### 3.3 6 个快捷入口图标

对应 `HUDQuickBarConfigSO` 中的 6 个按钮：

1. 接单
2. 拉坯
3. 施釉
4. 烧窑
5. 仓库
6. 材料

建议图标风格：

- 扁平化
- 高对比
- 识别度优先
- 适合小尺寸显示

### 3.4 左上角状态栏皮肤

用途：`PlayerStatusBar` 的背景和信息框。

建议素材：

- 状态栏底板
- 数值标签底图
- 分隔线
- 小图标槽位

当前需要显示的占位项：

- 银两
- 声望
- 当前订单

### 3.5 状态图标

建议补齐：

- 银两图标
- 声望图标
- 当前订单图标

当前项目里已经有：

- `Assets/游戏主界面UI/铜钱.png`
- `Assets/游戏主界面UI/声望.png`

但“当前订单”还没有明显可复用图标，建议单独补一个。

### 3.6 Tooltip / 提示框体系

用途：快捷按钮悬停说明、操作提示、自动寻路反馈提示。

建议素材：

- Tooltip 气泡底板
- 说明框
- 短提示条
- 成功 / 失败反馈条

当前可先复用：

- `Assets/游戏主界面UI/提示框.png`
- `Assets/Phase3/UI/烧窑/提示框.png`

## 4. 建议的优先级

### P0

- 6 个快捷入口图标
- 快捷栏按钮统一皮肤
- 左上角状态栏底板
- TMP 中文字体资产

### P1

- 高亮 / 按下 / 禁用态图
- Tooltip 气泡
- 当前订单图标

### P2

- 自动寻路中 / 到达 / 取消等轻反馈图形
- 更细的状态栏装饰

## 5. 额外注意点

1. 当前 `HUDQuickBar` 和 `PlayerStatusBar` 都是偏“纯 UI 逻辑”，美术素材最好保持同一套风格。
2. Phase11A 允许按钮先不放图标，但正式版建议补齐，避免功能入口不直观。
3. 项目里暂时没扫到 `.ttf/.otf/.ttc` 字体文件，中文 TMP 字体需要确认来源。
4. 如果沿用现有主界面风格，建议直接参考 `Assets/游戏主界面UI/` 的配色和描边方式。

## 6. 推荐交付清单

如果要发给美术，建议按下面这组最小清单推进：

- `HUDQuickBar` 底板 1 套
- 6 个按钮图标
- 6 个按钮皮肤状态图
- `PlayerStatusBar` 底板 1 套
- 银两 / 声望 / 当前订单图标各 1 个
- Tooltip 气泡 1 套
- 中文 TMP 字体 1 套

---

最后更新：2026-06-20
