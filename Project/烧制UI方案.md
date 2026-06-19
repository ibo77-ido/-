# Phase3 烧制 UI 接入方案

## 目标

将 `Assets/Phase3/UI/烧窑` 内的烧窑美术资源接入现有 Phase3 烧制流程，形成正式的烧制面板表现。

本轮目标是接入视觉和交互呈现，不重做烧制数值逻辑。现有 `FiringSystem`、`FiringPanelController`、`GameManager` 的主流程继续沿用。

## 当前资源

资源目录：

```text
Assets/Phase3/UI/烧窑
```

主要资源：

```text
底图 阶段1.png
底图 阶段2.png
底图 阶段3.png
循环火焰动画.mp4
关窑动画.mp4
添燃料按钮.png
停火按钮.png
打开按钮.png
确定按钮.png
温度条.png
进度条（空）.png
进度条（满）.png
提示框.png
参考图/1.png
参考图/2.png
参考图/3.png
```

资源判断：

- `循环火焰动画.mp4` 是完整烧制画面动画，包含窑内火焰和外部烟雾，不应裁成只显示窑口火焰。
- `关窑动画.mp4` 用于停火后的过渡动画。
- `底图 阶段1/2/3.png` 用于静态兜底、初始态、停火后状态，或视频播放前后的状态切换。
- 按钮图分别映射到添柴、停火、打开、确认等操作。

## 参考图与功能映射

### 参考图 1：烧制初始/操作态

画面特征：

- 窑门打开。
- 炉膛内无明显火焰。
- 左下为“添柴”操作。
- 右下为“停火”操作。
- 底部有温度/火候条。
- 右上有信息按钮。

功能映射：

```text
进入烧制模块         -> GameManager.EnterFiringModule()
开始烧制             -> FiringSystem.StartFiring()
添柴按钮             -> FiringSystem.AddFuel()
停火按钮             -> FiringSystem.StopFiring()
底部温度/火候条      -> FiringSystem.CurrentTemperature
火候区间             -> FiringSystem.GetCurrentZone()
```

### 参考图 2：烧制进行态

画面特征：

- 与参考图 1 同一操作界面。
- 窑内出现火焰。
- 外部烟雾也有动态表现。
- 添柴、停火仍然可操作。

功能映射：

```text
烧制中               -> FiringSystem.IsFiring == true
整屏动态画面         -> 循环火焰动画.mp4
视频播放方式         -> VideoPlayer + RenderTexture + RawImage
视频循环             -> VideoPlayer.isLooping = true
添柴                 -> currentTemperature += fuelBoost
温度条推进           -> CurrentTemperature 映射为 fillAmount
```

重要结论：

`循环火焰动画.mp4` 应作为烧制面板的动态主画面铺在主体区域，而不是只放在窑口局部。按钮、温度条和提示 UI 叠在视频上层。

### 参考图 3：停火/等待开窑态

画面特征：

- 窑门关闭。
- 整体画面变暗。
- 左下添柴和右下停火不再显示。
- 底部中央只显示“打开”按钮。

功能映射：

```text
点击停火             -> FiringSystem.StopFiring()
停火过渡动画         -> 关窑动画.mp4
关窑动画播放完成     -> 显示打开按钮
打开按钮             -> FiringPanelController.OnOpenKilnButtonClicked()
进入结果             -> GameManager.GoToResult()
```

## 推荐状态机

第一版建议收敛为三个 UI 状态：

```text
FiringReady / FiringActive
    进入烧制后显示烧制主画面
    播放循环火焰动画
    显示添柴、停火、温度条

ClosingKiln
    点击停火后进入
    停止循环火焰动画
    播放关窑动画
    暂时隐藏添柴、停火、打开按钮

ReadyToOpen
    关窑动画结束后进入
    显示阶段3底图或关窑动画最后一帧
    显示打开按钮
    点击打开进入结果面板
```

流程：

```text
进入烧制
-> 播放循环火焰动画
-> 玩家添柴/观察温度条
-> 玩家点击停火
-> 播放关窑动画
-> 显示打开按钮
-> 点击打开
-> GameManager.GoToResult()
```

## 视频呈现方案

使用 Unity 原生视频链路：

```text
VideoPlayer
-> RenderTexture
-> RawImage
```

推荐层级：

```text
Panel_Firing
├─ ArtRoot_Firing
│  ├─ StaticFallback_Image
│  ├─ FiringVideo_RawImage
│  └─ Overlay_Image 可选
├─ TemperatureRoot
│  ├─ ProgressEmpty_Image
│  ├─ ProgressFull_Image
│  └─ TemperatureNeedle / TemperatureMask 可选
├─ Btn_Fuel
├─ Btn_Stop
├─ Btn_Open
├─ Btn_Info 可选
└─ PromptRoot 可选
```

配置建议：

- `FiringVideo_RawImage` 铺满参考图主体区域，保持与底图相同宽高比例。
- `StaticFallback_Image` 放在视频下方，视频未准备好或停止时显示。
- `循环火焰动画.mp4` 设置循环播放。
- `关窑动画.mp4` 不循环，播放完成后切换到 `ReadyToOpen`。
- 按钮、温度条、提示框全部位于视频上层。
- 如果视频透明度不可用，就让视频作为完整背景；不要再叠同一张底图遮挡视频主体。

## 现有代码绑定点

烧制逻辑：

```text
Assets/Phase3/Scripts/Systems/Firing/FiringSystem.cs
```

当前可复用接口：

```csharp
public float CurrentTemperature { get; }
public bool IsFiring { get; }
public void StartFiring()
public void StopFiring()
public void AddFuel()
public FireZone GetCurrentZone()
public float GetFireScore()
```

烧制 UI：

```text
Assets/Phase3/Scripts/UI/FiringPanelController.cs
```

当前可复用按钮逻辑：

```text
fuelButton       -> OnFuelButtonClicked()
stopButton       -> OnStopButtonClicked()
openKilnButton   -> OnOpenKilnButtonClicked()
```

流程入口：

```text
GameManager.EnterFiringModule()
GameManager.GoToFiring()
GameManager.GoToResult()
```

## FiringPanelController 调整建议

保留现有公开流程，扩展美术引用字段。

建议新增字段：

```csharp
[Header("Art")]
[SerializeField] private Image staticFallbackImage;
[SerializeField] private RawImage firingVideoImage;
[SerializeField] private VideoPlayer firingLoopPlayer;
[SerializeField] private VideoPlayer closingKilnPlayer;
[SerializeField] private Image progressEmptyImage;
[SerializeField] private Image progressFullImage;
[SerializeField] private GameObject fuelButtonRoot;
[SerializeField] private GameObject stopButtonRoot;
[SerializeField] private GameObject openButtonRoot;
[SerializeField] private GameObject promptRoot;
```

按钮逻辑：

```text
点击添柴
    调用 FiringSystem.AddFuel()
    可选：触发轻微火焰增强反馈

点击停火
    调用 FiringSystem.StopFiring()
    进入 ClosingKiln 状态
    播放 关窑动画.mp4

关窑动画结束
    进入 ReadyToOpen 状态
    显示 打开按钮

点击打开
    调用 GameManager.GoToResult()
```

## Refresh 性能策略

接入美术后不建议每帧全量刷新所有 UI。

当前 `Update()` 每帧调用 `Refresh()`，第一版可以保留驱动，但需要控制刷新粒度：

- 温度条 `fillAmount` 可以每帧更新。
- 温度文本如果保留，只在整数温度变化时更新。
- 背景图、按钮显隐、视频播放状态只在状态切换时更新。
- 不要每帧反复设置 `Image.sprite`。
- 不要每帧反复调用 `VideoPlayer.Play()` / `Stop()`。
- 不要每帧切换 `RawImage.texture`。

建议内部缓存：

```text
lastWholeTemperature
lastFireZone
lastUiState
lastIsFiring
```

## 风门/窗口功能处理

现有原型里有：

```text
windSlider
windowButton
FiringSystem.SetWindValue()
FiringSystem.ToggleWindow()
```

但烧窑参考图没有对应的风门或窗口控件。

第一版建议：

- 不在正式美术面板中暴露 `Slider_Wind`。
- 不在正式美术面板中暴露 `Btn_Window`。
- 核心操作收敛为 `添柴`、`停火`、`打开`。
- 如果数值设计仍需要风门，可后续改成隐藏参数、调试控件或信息面板中的高级操作。

## Phase3SceneBuilder 同步策略

当前构建器：

```text
Assets/Phase3/Editor/Phase3SceneBuilder.cs
```

其中 `EnsureFiringPanelControls()` 会创建文字版占位 UI，包括 Text、Slider、Button。

接入美术面板后建议采用方案 A：

检测到美术版节点后，跳过旧占位控件创建。

建议识别节点：

```text
Panel_Firing/ArtRoot_Firing
```

或：

```text
Panel_Firing/Image_StageBg
```

推荐逻辑：

```text
if Panel_Firing 下存在 ArtRoot_Firing
    说明烧制美术面板已经接入
    不再创建 Text_Zone、Text_FireScore、Text_Status、Slider_Wind、Btn_Window 等旧占位控件
    只确保 FiringPanelController 存在
else
    保持当前旧逻辑，补齐程序占位控件
```

这样不会影响 Order、Shape、Glaze、Result 其它面板，也不会破坏 Phase3 原型场景的自动修复能力。

## 接入边界

### 可以改

- `Panel_Firing` 的视觉层级。
- `FiringPanelController` 的美术引用和状态切换。
- `Phase3SceneBuilder.EnsureFiringPanelControls()` 的美术面板检测逻辑。
- 烧制按钮的 `Image.sprite` 和 RectTransform。
- 温度条从 Slider 改为图片填充表现。
- 视频播放链路和 RenderTexture 资源。

### 暂不改

- 不重做 `FiringSystem` 数值计算。
- 不改 `GameManager.UpdatePanels()` 面板切换方式。
- 不改 Phase9 通过窑口进入 `EnterFiringModule()` 的桥接方式。
- 不拆分 mp4 为序列帧。
- 不把循环火焰视频裁成局部火焰层。
- 不强行保留旧的风门 Slider 和窗口按钮。

## 实施步骤

1. 在 `Panel_Firing` 下搭建 `ArtRoot_Firing`。
2. 添加 `StaticFallback_Image`，绑定 `底图 阶段1.png` 或 `底图 阶段3.png`。
3. 添加 `FiringVideo_RawImage`，尺寸铺满烧窑主体画面。
4. 创建 RenderTexture，用于 `循环火焰动画.mp4` 和 `关窑动画.mp4` 输出。
5. 添加或配置 VideoPlayer。
6. 将按钮替换为美术按钮图：
   - `Btn_Fuel` 使用 `添燃料按钮.png`
   - `Btn_Stop` 使用 `停火按钮.png`
   - `Btn_OpenKiln` 使用 `打开按钮.png`
7. 温度条使用 `进度条（空）.png` + `进度条（满）.png` 或 `温度条.png` 组合表现。
8. 扩展 `FiringPanelController` 状态机和视频控制。
9. 优化 `Refresh()`，避免每帧全量设置 UI。
10. 修改 `Phase3SceneBuilder`，检测 `ArtRoot_Firing` 后跳过旧占位控件创建。
11. 在 Phase3 场景测试进入烧制、添柴、停火、打开、进入结果。
12. 在 Phase9 桥接场景测试从窑口进入烧制流程。

## 验收清单

- 进入烧制面板后，循环视频正常播放。
- 视频以整屏烧制画面呈现，内部火焰和外部烟雾都可见。
- 添柴按钮可点击，并能影响当前温度。
- 停火按钮可点击，点击后停止升温。
- 点击停火后播放关窑动画。
- 关窑动画播放完成后显示打开按钮。
- 点击打开后进入结果面板。
- 温度/火候条能随烧制过程推进。
- 美术面板存在时，`Phase3SceneBuilder` 不再生成旧文字版烧制控件。
- 控制台无脚本编译错误。
- Phase3 单独场景流程正常。
- Phase9 窑口进入烧制流程正常。
