# Phase11 UI 最终执行方案

> 适用范围：游戏主界面 UI、HUDQuickBar、PlayerStatusBar、Tooltip / 提示框、Phase11B 相机缩放与回位。
>
> 目标：把现有 Phase11 需求、参考图、美术素材和脚本逻辑统一到一份可执行方案里，方便直接进入制作。

## 1. 方案目标

Phase11 的 UI 目标不是单纯“做一个界面”，而是把主界面 UI 和交互入口一起搭起来，形成可用的游戏主 HUD。

最终要交付的内容分为三层：

1. **主界面美术层**
   - 参考图风格统一
   - 按钮、条形框、提示框、图标统一皮肤
   - 统一国风、浅米色、卷轴/宣纸质感

2. **主界面功能层**
   - 左上角玩家状态栏
   - 底部或右上快捷入口条
   - 提示框 / Tooltip
   - 当前订单、银两、声望等占位信息

3. **交互扩展层**
   - Phase11A：快捷按钮一键寻路 + 到达后自动交互
   - Phase11B：地图缩放 / 平移 / 智能回位

---

## 2. 现有资源扫描结论

### 2.1 已确认的 Phase11 结构

当前 `Assets/Phase11` 下已有的内容主要是逻辑脚本和配置脚手架：

- `Assets/Phase11/Data/HUDQuickBarConfigSO.cs`
- `Assets/Phase11/Scripts/HUDButtonData.cs`
- `Assets/Phase11/Scripts/HUDQuickBar.cs`
- `Assets/Phase11/Scripts/PlayerStatusBar.cs`
- `Assets/Phase11/Scripts/QuickBarButton.cs`

说明：

- 逻辑层已经有基础结构
- 还缺主 UI Prefab、按钮皮肤、状态栏皮肤、Tooltip 皮肤
- `Assets/Phase11/Prefabs` 目前为空，适合作为主 UI 的正式归档目录

### 2.2 已找到的主界面参考图

主界面参考图位于：

- `Assets/游戏主界面UI/参考图.png`

这张图是本次 UI 的核心美术依据，整体风格特点如下：

- 浅米色、淡绿色、低饱和
- 纸张、卷轴、手绘边框感
- 图标偏中式工坊 / 建筑 / 器具题材
- 界面留白很大，适合做“轻 UI”风格

### 2.3 已有可复用素材

主界面 UI 文件夹内已经有比较完整的现成素材，可直接进入拼装阶段：

- `Assets/游戏主界面UI/打开按钮.png`
- `Assets/游戏主界面UI/接单按钮.png`
- `Assets/游戏主界面UI/接下一单按钮.png`
- `Assets/游戏主界面UI/提示框.png`
- `Assets/游戏主界面UI/资金条.png`
- `Assets/游戏主界面UI/声望条.png`
- `Assets/游戏主界面UI/进度条（空）.png`
- `Assets/游戏主界面UI/进度条（满）.png`
- `Assets/游戏主界面UI/铜钱.png`
- `Assets/游戏主界面UI/图标/仓库.png`
- `Assets/游戏主界面UI/图标/拉胚.png`
- `Assets/游戏主界面UI/图标/接单.png`
- `Assets/游戏主界面UI/图标/施釉.png`
- `Assets/游戏主界面UI/图标/材料.png`
- `Assets/游戏主界面UI/图标/烧窑.png`
- `Assets/游戏主界面UI/按钮/按钮.png`
- `Assets/游戏主界面UI/按钮/高亮.png`
- `Assets/游戏主界面UI/按钮/按下.png`
- `Assets/游戏主界面UI/按钮/悬停.png`
- `Assets/游戏主界面UI/按钮/禁用.png`
- `Assets/游戏主界面UI/提示/成功.png`
- `Assets/游戏主界面UI/提示/失败.png`
- `Assets/游戏主界面UI/提示/气泡.png`

### 2.4 可参考的旧 UI 素材

Phase3 里也有一批可借用素材，适合作为风格对照或临时复用：

- `Assets/Phase3/UI/订单面板/接单按钮.png`
- `Assets/Phase3/UI/烧窑/提示框.png`
- `Assets/Phase3/UI/烧窑/温度条.png`
- `Assets/Phase3/UI/烧窑/确定按钮.png`
- `Assets/Phase3/UI/烧窑/添燃料按钮.png`
- `Assets/Phase3/UI/烧窑/底图 阶段1.png`
- `Assets/Phase3/UI/烧窑/底图 阶段2.png`
- `Assets/Phase3/UI/烧窑/底图 阶段3.png`

---

## 3. 最终 UI 设计方向

### 3.1 整体风格

参考图已经明确了主界面的视觉方向，建议统一成：

- 国风工坊感
- 轻量卷轴 UI
- 浅米色底 + 浅绿边框
- 金色或淡棕色强调线
- 图标尽量扁平化、高识别度

### 3.2 颜色和材质建议

建议全 UI 遵循以下原则：

- 背景元素：浅米、淡绿、少量灰绿
- 重点交互：绿色高亮、金色描边
- 禁用态：灰度、降低透明度
- 按压态：阴影缩小、色彩压暗

### 3.3 字体建议

当前项目里未扫描到稳定可复用的中文字体资源，因此主方案建议：

- 先用系统可用的 TMP 中文字体替代
- 若后续找到项目内统一字体，再批量切换
- 文案尽量短，避免字体未统一时影响观感

---

## 4. 主界面最终结构

### 4.1 根节点建议

建议建立一个正式的主 UI 预制体：

- `Assets/Phase11/Prefabs/HUDCanvas.prefab`

Canvas 设置建议：

- `Screen Space - Overlay`
- 常驻 UI
- 不依赖 3D 场景摄像机

### 4.2 UI 层级建议

推荐层级如下：

```text
HUDCanvas
├─ PlayerStatusBar
├─ HUDQuickBar
├─ TooltipLayer
├─ PopupLayer
└─ DebugLayer
```

### 4.3 主界面视觉拆分

根据参考图，主界面建议拆成以下区块：

1. **左上角状态块**
   - 角色头像或身份牌
   - 银两
   - 声望
   - 当前订单

2. **顶部信息条**
   - 可放阶段标题、当前区域状态、任务状态
   - 当前阶段可先做装饰性占位

3. **快捷入口区**
   - 6 个功能按钮
   - 接单 / 拉胚 / 施釉 / 烧窑 / 仓库 / 材料

4. **提示与反馈区**
   - Tooltip
   - 成功 / 失败提示
   - 自动寻路提示

5. **辅助入口**
   - 打开按钮
   - 下一单按钮
   - 订单提示按钮

---

## 5. Phase11A 最终实现方案

Phase11A 的核心是：**点击快捷按钮后，角色自动寻路到对应建筑，到达后按配置自动交互或只寻路**。

### 5.1 逻辑关系

现有脚本已经支持这个流程：

- `HUDQuickBar`：负责按钮点击、寻路、到达后交互
- `QuickBarButton`：单个按钮 UI 组件
- `HUDButtonData`：按钮数据结构
- `HUDQuickBarConfigSO`：快捷栏配置资产
- `PlayerStatusBar`：状态栏占位显示

### 5.2 6 个快捷入口定义

建议固定为以下 6 项：

1. 接单
2. 拉胚
3. 施釉
4. 烧窑
5. 仓库
6. 材料

### 5.3 按钮交互规则

建议统一遵循以下行为：

- 若游戏当前不可移动，则按钮点击直接失效
- 若目标建筑不存在，则忽略
- 若角色已经在范围内，则可直接交互
- 若按钮属于自动交互项，则到达后自动触发
- 若按钮属于纯寻路项，则只走到目标点，不自动交互

### 5.4 建议配置表

| 索引 | areaType | 显示文案 | autoInteract | 说明 |
|---|---|---:|---:|---|
| 0 | Order | 接单台 | true | 到达后自动接单 |
| 1 | Wheel | 拉坯台 | true | 到达后自动交互 |
| 2 | Glaze | 施釉台 | true | 到达后自动交互 |
| 3 | Kiln | 烧窑 | true | 到达后自动交互 |
| 4 | Storage | 仓库 | false | 仅寻路 |
| 5 | Material | 材料库 | false | 仅寻路 |

### 5.5 HUDQuickBar 运行约束

建议保留这些规则：

- `Phase6GameManager.CanMove()` 作为总状态门禁
- `FindObjectsOfType<Workstation>()` 查找目标建筑
- 优先走 `InteractionPoint`
- 没有 `InteractionPoint` 时回退到建筑自身位置
- 到达检测超时保护建议保留，默认 15 秒

---

## 6. PlayerStatusBar 最终实现方案

### 6.1 当前定位

`PlayerStatusBar` 先做成占位状态栏，不直接接真实经济系统。

### 6.2 占位字段

建议先显示：

- 银两：`--`
- 声望：`--`
- 当前订单：`--`

### 6.3 后续扩展

后续如果 Phase13 接入真实数据，再把显示源切到：

- 银两系统
- 声望系统
- 当前订单系统

这样不会影响当前 UI 的结构和版式。

---

## 7. Tooltip 与反馈体系

### 7.1 建议保留的提示类型

- 鼠标悬停说明
- 操作指引提示
- 自动寻路反馈
- 成功 / 失败提示

### 7.2 建议素材优先级

可优先复用：

- `Assets/游戏主界面UI/提示框.png`
- `Assets/游戏主界面UI/提示/气泡.png`
- `Assets/游戏主界面UI/提示/成功.png`
- `Assets/游戏主界面UI/提示/失败.png`

### 7.3 提示层级

建议所有提示统一放在：

- `TooltipLayer`
- `PopupLayer`

避免和常驻 HUD 混在一起，后续维护更简单。

---

## 8. Phase11B 最终实现方案

Phase11B 的目标是给地图视角增加：

- 滚轮缩放
- 右键拖拽平移
- 主角移动时自动回位

### 8.1 约束原则

建议保持以下原则不变：

- 不修改 `CameraFollow2D.cs`
- 不依赖其私有字段
- 通过 `CameraFollow2D.enabled = false/true` 切换模式
- Phase11B 自己管理缩放和平移参数

### 8.2 三态机建议

#### 1. 跟随模式

- `CameraFollow2D.enabled = true`
- 摄像机正常跟随主角

#### 2. 手动浏览模式

- 鼠标滚轮触发
- 右键拖拽触发
- `CameraFollow2D.enabled = false`
- 由 `CameraZoomController` 接管

#### 3. 智能回位模式

- 主角开始移动时触发
- 摄像机平滑回到默认 framing
- 回位完成后恢复跟随模式

### 8.3 推荐的自定义参数

建议 `CameraZoomController` 自持以下参数：

- `defaultOrthographicSize`
- `defaultFramingOffset`
- `resetDuration`
- `resetCurve`
- `minZoom`
- `maxZoom`
- `zoomSensitivity`
- `panSpeed`

这样不会碰 `CameraFollow2D` 的内部实现。

---

## 9. 最终资源清单

### 9.1 本次必须交付

1. `Assets/Phase11/Prefabs/HUDCanvas.prefab`
2. `HUDQuickBar` 主体美术
3. `PlayerStatusBar` 美术
4. `QuickBarButton` 皮肤
5. `Tooltip` 皮肤
6. `HUDQuickBarConfigSO` 配置资产
7. 主界面占位图标和条形素材整理

### 9.2 建议补充交付

1. `CameraZoomController.cs`
2. 成功 / 失败提示弹层
3. 订单相关图标
4. 当前订单专用图标

---

## 10. 文件归档建议

为了方便管理，建议 Phase11 统一保持以下结构：

```text
Assets/Phase11/
├─ Data/
├─ Prefabs/
├─ Scripts/
└─ Phase11_UI_FinalPlan.md
```

如果后续要继续细分，还可以增加：

- `Assets/Phase11/UI/`
- `Assets/Phase11/Materials/`
- `Assets/Phase11/Sprites/`

但当前阶段先不要过度拆分，避免资源分散。

---

## 11. 推荐实施顺序

### 阶段 1：主 UI 骨架

1. 创建 `HUDCanvas.prefab`
2. 搭好 `PlayerStatusBar`、`HUDQuickBar`、`TooltipLayer`
3. 把参考图版式对齐

### 阶段 2：素材拼装

1. 统一按钮皮肤
2. 统一状态条皮肤
3. 统一图标风格
4. 补 Tooltip / 提示框

### 阶段 3：逻辑接线

1. 配置 `HUDQuickBarConfigSO`
2. 把 6 个按钮和建筑类型绑定
3. 验证寻路和自动交互

### 阶段 4：扩展功能

1. 加入 Phase11B 相机控制
2. 加入智能回位
3. 做边界和异常处理

---

## 12. 最终结论

Phase11 的正确做法不是单独“补几个 UI 图”，而是一次性建立一套完整的主界面系统：

- 参考图定视觉
- 现有素材定风格
- Phase11 脚本定交互
- `HUDCanvas.prefab` 定结构
- `HUDQuickBarConfigSO` 定数据

这样做的好处是：

- 后续维护成本低
- UI 风格统一
- 逻辑和美术不会互相打架
- Phase11A / Phase11B 可以分步落地

> 建议把这份文档作为 Phase11 的最终执行依据，后续如果要补充，也只在这份文档上迭代，不再分散到多份方案里。

