# Phase3 烧制 UI 接入方案

## 目标

将 `Assets/Phase3/UI/烧窑` 内的烧窑美术资源接入现有 Phase3 烧制流程，形成正式的烧制面板表现。

本轮目标是接入视觉和交互呈现。`FiringSystem` 温度计算做小幅扩展（新增低风门降温分支），评分逻辑不变；`FiringPanelController`、`GameManager` 主流程沿用。

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
火焰动画.mp4
循环.mp4
关窑动画.mp4
温度条变化.mp4
添燃料按钮.png
停火按钮.png
打开按钮.png
确定按钮.png
温度条.png
进度条（空）.png
进度条（满）.png
提示框.png
风门/1.png ~ 8.png
风门/满.png
参考图/1.png
参考图/2.png
参考图/3.png
```

资源判断：

- `火焰动画.mp4` 是烧制起势动画，进入烧制后先单次播放，表现火焰从无到有的起势过程，不循环。
- `循环.mp4` 是烧制主循环视频，包含窑内火焰和外部烟雾，在 `火焰动画.mp4` 播完后无缝衔接循环播放，不应裁成只显示窑口火焰。
- `关窑动画.mp4` 用于停火后的过渡动画。
- `温度条变化.mp4` 用于温度条动态表现，按视频自身帧数线性推进，风门状态决定正向/倒放。
- `底图 阶段1/2/3.png` 用于静态兜底、初始态、停火后状态，或视频播放前后的状态切换。
- `风门/1.png ~ 8.png + 满.png` 共 9 帧序列，对应风门 9 档开度，鼠标滚轮切换。
- 按钮图分别映射到添柴、停火、打开、确认等操作。

## 参考图与功能映射

### 参考图 1：烧制初始/操作态

画面特征：

- 窑门打开。
- 炉膛内无明显火焰。
- 左下为"添柴"操作。
- 右下为"停火"操作。
- 底部有温度/火候条。
- 右上有信息按钮。

功能映射：

```text
进入烧制模块         -> GameManager.EnterFiringModule()
开始烧制             -> FiringSystem.StartFiring()
添柴按钮             -> FiringSystem.AddFuel()
停火按钮             -> FiringSystem.StopFiring()
底部温度/火候条      -> 温度条变化.mp4（风门状态驱动方向，不绑温度数值）
火候区间             -> FiringSystem.GetCurrentZone()
风门调节             -> FiringSystem.SetWindValue()（滚轮交互）
```

### 参考图 2：烧制进行态

画面特征：

- 与参考图 1 同一操作界面。
- 窑内出现火焰。
- 外部烟雾也有动态表现。
- 添柴、停火、风门仍然可操作。

功能映射：

```text
烧制中               -> FiringSystem.IsFiring == true
烧制起势             -> 火焰动画.mp4（单次播放，不循环）
烧制主循环画面       -> 循环.mp4（起势播完后无缝衔接循环播放）
视频播放方式         -> VideoPlayer + RenderTexture + RawImage
视频循环             -> VideoPlayer.isLooping = true（仅 循环.mp4）
添柴                 -> currentTemperature += fuelBoost
温度条视频帧推进     -> 按视频自身帧率线性推进（风门正常时正向，低风门持续3s时倒放）
```

重要结论：

烧制主画面分两段播放：进入烧制先播 `火焰动画.mp4`（起势，单次），播完后无缝切换到 `循环.mp4`（循环播放）。两个视频共用同一个 `FiringVideo_RawImage`，通过切换 VideoPlayer.clip 或启用不同 VideoPlayer 实现。主画面应铺满主体区域，而不是只放在窑口局部。按钮、温度条和提示 UI 叠在视频上层。

### 参考图 3：停火/等待开窑态

画面特征：

- 窑门关闭。
- 整体画面变暗。
- 左下添柴和右下停火不再显示。
- 底部中央只显示"打开"按钮。

功能映射：

```text
点击停火             -> FiringSystem.StopFiring()
停火过渡动画         -> 关窑动画.mp4
关窑动画播放完成     -> 显示打开按钮
打开按钮             -> FiringPanelController.OnOpenKilnButtonClicked()
进入结果             -> GameManager.GoToResult()
```

## 温度条视频方案

温度条采用视频呈现，替换原 `进度条（空）.png + 进度条（满）.png` 的 Image fillAmount 方案。温度条视频按视频自身帧数线性推进，不绑定 `CurrentTemperature` 数值。

### 呈现方式

- 素材：`温度条变化.mp4`
- 链路：`VideoPlayer → RenderTexture → RawImage`
- 播放方式：不自动 Play，由 Controller 手动控制帧位置
- 帧推进按视频自身帧率线性进行，不映射温度数值

### 正向播放规则

- 触发条件：非倒放状态（windValue >= 0.3，或 windValue < 0.3 但未持续 3s）
- 视频按正常播放速度正向推进（帧 += 播放速率 * deltaTime）
- 推进到视频末帧后停止（不循环）

### 倒放规则

- 触发条件：`windValue < 0.3` 持续 3 秒以上
- 倒放速度 = 正常播放速度（帧 -= 播放速率 * deltaTime）
- 倒放到第 0 帧后停止
- 倒放到 0 帧 → 自动开窑，结果判定为次品
- `windValue` 恢复到 >= 0.3 时立即解除倒放，恢复正常正向

### 次品判定

倒放到 0 帧时触发强制开窑，`FiringSystem.StopFiring()` 被调用，并在进入结果前把烧制温度压到 0。这样 `CalculateScore()` 使用的 `stopFireTemp` 为 0，`GetFireScore()` 返回 0，`FireZone` 为 `Underfired`，结果自然走烧制失败/次品分支。

不要只依赖“低风门降温刚好降到 0”的时间巧合。实现上需要提供一个明确的强制欠烧入口，例如：

```csharp
public void ForceUnderfiredOpen()
{
    currentTemperature = 0f;
    StopFiring();
}
```

该入口只服务温度条倒放到 0 帧的失败分支，不修改 `ResultSystem` 的判定规则。

### 与温度数值的关系

- 温度条视频帧位置不与 `CurrentTemperature` 一一对应
- 视频是风门状态的视觉趋势反馈：风门正常→前进，风门低持续→倒退
- `CurrentTemperature` 数值仍由 `FiringSystem.Update()` 计算（升温/降温），用于评分
- Controller 内部维护一个独立 `temperatureBarProgress`，范围 0~1，再映射到视频帧

## 风门 UI 方案

风门重新暴露给玩家操作（推翻原"第一版不暴露"决定），采用序列帧 + 鼠标滚轮交互。

### 呈现方式

- 素材：`风门/1.png ~ 8.png + 满.png`（共 9 帧）
- 显示组件：`Image`，按 windValue 切换 sprite
- 摆放位置：画面右侧中下留白区，建议在停火按钮上方、信息按钮下方、窑门右侧之外
- 层级位置：位于主画面视频之上，按钮和提示层之下
- 热区位置：跟随风门视觉，不覆盖烟雾主体、窑口、温度条和左右按钮
- 帧映射：windValue 0~1 → frameIndex 0~8（9 档）
  - windValue = 0 → 1.png（第 0 帧，最小开度）
  - windValue = 1 → 满.png（第 8 帧，最大开度）

### 交互方式

- 鼠标中键滚轮在风门区域滚动 → 切换风门等级
- 滚轮向上 → windValue += 1/8
- 滚轮向下 → windValue -= 1/8
- Clamp 0~1，调用 `FiringSystem.SetWindValue()`
- 检测区域：`RectTransformUtility.RectangleContainsScreenPoint`
- 检测区域应覆盖风门图片本体，避免覆盖窑口、添柴、停火、温度条
- 切换丝滑（按 1/8 步进，共 9 档）

## FiringSystem 数值调整

原"不重做 FiringSystem 数值计算"调整为：**小幅扩展温度计算逻辑，支持低风门降温**。

### 新增字段

```csharp
[SerializeField] private float lowWindThreshold = 0.3f;
[SerializeField] private float lowWindDurationThreshold = 3f;
[SerializeField] private float temperatureDropPerSecond = 30f;
private float lowWindTimer;
private bool isTemperatureDropping;
```

### Update 逻辑

```csharp
if (!isFiring) return;
if (windValue < lowWindThreshold)
{
    lowWindTimer += Time.deltaTime;
    if (lowWindTimer >= lowWindDurationThreshold)
    {
        currentTemperature -= temperatureDropPerSecond * Time.deltaTime;
        currentTemperature = Mathf.Max(0f, currentTemperature);
        return;
    }
}
else
{
    lowWindTimer = 0f;
}
currentTemperature += temperatureRisePerSecond * windValue * Time.deltaTime;
```

### 新增只读接口

```csharp
public bool IsTemperatureDropping { get; }  // 供 Controller 判断倒放
public float WindValue { get; }             // 供 Controller 映射风门帧
public void ForceUnderfiredOpen()           // 温度条倒放到 0 帧时调用
```

### 不改部分

- `AddFuel`、`StopFiring`、`GetCurrentZone`、`GetFireScore`、`CalculateScore`、`StartFiring`、`ResetFiring` 接口签名不变
- `FireConfigSO` 配置不变
- 评分逻辑不变

## 推荐状态机

第一版收敛为三个主 UI 状态 + 一个强制开窑分支：

```text
FiringActive
    进入烧制后显示烧制主画面
    先播放 火焰动画.mp4（起势，单次，不循环）
    火焰动画播完后无缝切换到 循环.mp4（循环播放）
    温度条视频按帧 seek
    显示添柴、停火、风门
    风门滚轮可操作

ClosingKiln
    点击停火后进入
    停止主画面视频（循环.mp4）
    播放关窑动画
    暂时隐藏添柴、停火、风门按钮

ReadyToOpen
    关窑动画结束后进入
    显示阶段3底图或关窑动画最后一帧
    显示打开按钮
    点击打开进入结果面板

强制开窑（次品分支）
    温度条视频倒放到 0 帧触发
    调用 FiringSystem.ForceUnderfiredOpen()
    跳过 ClosingKiln 动画
    直接进入 GameManager.GoToResult()
    结果次品（温度=0 → 分数=0 → 欠烧）
```

流程：

```text
进入烧制
-> 播放 火焰动画.mp4（起势）
-> 播完后无缝切换到 循环.mp4（循环）
-> 玩家添柴/调风门/观察温度条视频
-> 玩家点击停火
-> 播放关窑动画
-> 显示打开按钮
-> 点击打开
-> GameManager.GoToResult()

或：
-> 风门低进风量持续 3s
-> 温度条视频倒放
-> 倒放到 0 帧
-> 强制开窑
-> GameManager.GoToResult()（次品）
```

## 视频呈现方案

使用 Unity 原生视频链路：

```text
VideoPlayer
-> RenderTexture
-> RawImage
```

四个视频用途：

| 视频 | 用途 | 播放方式 |
|------|------|---------|
| 火焰动画.mp4 | 烧制起势 | 不循环，FiringActive 态进入时单次播放，播完后切到 循环.mp4 |
| 循环.mp4 | 烧制主循环画面 | isLooping=true，火焰动画播完后无缝衔接循环播放 |
| 关窑动画.mp4 | 停火过渡 | 不循环，ClosingKiln 态单次播放，结束触发 ReadyToOpen |
| 温度条变化.mp4 | 温度条动态 | 按 frame seek，不 Play，手动控制帧位置 |

推荐层级：

```text
Panel_Firing
├─ ArtRoot_Firing
│  ├─ StaticFallback_Image
│  ├─ FiringVideo_RawImage          (主画面：火焰动画 + 循环.mp4 共用)
│  ├─ TemperatureVideo_RawImage     (温度条)
│  └─ Overlay_Image 可选
├─ WindDoor_Image                    (风门序列帧，放在右侧中下留白区)
├─ WindDoor_HoverArea               (滚轮检测区，覆盖风门本体)
├─ TemperatureRoot 可选              (温度数值文本，若有)
├─ Btn_Fuel
├─ Btn_Stop
├─ Btn_Open
├─ Btn_Info 可选
└─ PromptRoot 可选
```

配置建议：

- `FiringVideo_RawImage` 铺满参考图主体区域，保持与底图相同宽高比例。
- 主画面两个视频（`火焰动画.mp4` + `循环.mp4`）共用同一个 `FiringVideo_RawImage`，可通过两个 VideoPlayer 共享 RenderTexture 实现，或单个 VideoPlayer 切换 clip。推荐双 VideoPlayer 共享 RenderTexture 方案，切换更无缝。
- `TemperatureVideo_RawImage` 放在底部温度条位置。
- `WindDoor_Image` 放在右侧中下留白区，建议位于停火按钮上方、信息按钮下方、窑门右侧之外；不要放到炉顶烟雾区，也不要遮挡窑口火焰主体。
- `WindDoor_HoverArea` 只覆盖风门视觉区域，避免滚轮操作误触发其它 UI。
- `StaticFallback_Image` 放在视频下方，视频未准备好或停止时显示。
- `火焰动画.mp4` 不循环，播放完成（loopPointReached）→ 停止自身 → 启动 `循环.mp4`。
- `循环.mp4` 设置循环播放。
- `关窑动画.mp4` 不循环，播放完成后切换到 `ReadyToOpen`。
- `温度条变化.mp4` 不自动播放，由 Controller 手动 seek frame。
- 按钮、温度条、风门、提示框全部位于视频上层。
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
public float WindValue { get; }
public bool IsTemperatureDropping { get; }
public void StartFiring()
public void StopFiring()
public void AddFuel()
public void SetWindValue(float value)
public void ForceUnderfiredOpen()
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

保留现有公开流程，扩展美术引用字段与状态机。

建议新增字段：

```csharp
[Header("Art - Videos")]
[SerializeField] private VideoPlayer fireStartPlayer;          // 火焰动画.mp4（起势，单次）
[SerializeField] private VideoPlayer firingLoopPlayer;        // 循环.mp4（起势播完后循环）
[SerializeField] private VideoPlayer closingKilnPlayer;       // 关窑动画.mp4
[SerializeField] private VideoPlayer temperatureVideoPlayer;  // 温度条变化.mp4
[SerializeField] private RawImage firingVideoImage;           // fireStart + firingLoop 共用
[SerializeField] private RawImage temperatureVideoImage;

[Header("Art - Static")]
[SerializeField] private Image staticFallbackImage;

[Header("Art - Wind Door")]
[SerializeField] private Image windDoorImage;                  // 风门序列帧显示
[SerializeField] private Sprite[] windDoorFrames;              // 1~8.png + 满.png (9帧)
[SerializeField] private RectTransform windDoorHoverArea;      // 鼠标滚轮检测区

[Header("Art - Buttons")]
[SerializeField] private GameObject fuelButtonRoot;
[SerializeField] private GameObject stopButtonRoot;
[SerializeField] private GameObject openButtonRoot;
```

状态机：

```text
FiringActive
    进入时先播放 火焰动画.mp4（fireStartPlayer，不循环）
    fireStartPlayer.loopPointReached → 停止自身，启动 循环.mp4（firingLoopPlayer，循环）
    温度条视频按帧 seek
    风门序列帧 + 滚轮交互
    显示添柴、停火、风门

ClosingKiln
    点击停火后进入
    停止循环火焰动画
    播放关窑动画
    隐藏添柴、停火、风门

ReadyToOpen
    关窑动画结束后进入
    显示阶段3底图或关窑动画最后一帧
    显示打开按钮
    点击打开 → GameManager.GoToResult()

强制开窑（次品）
    温度条视频倒放到 0 帧触发
    调用 FiringSystem.ForceUnderfiredOpen()
    跳过 ClosingKiln
    直接 GameManager.GoToResult()
    结果次品
```

按钮逻辑：

```text
点击添柴
    调用 FiringSystem.AddFuel()

点击停火
    调用 FiringSystem.StopFiring()
    进入 ClosingKiln 状态
    播放 关窑动画.mp4

关窑动画结束
    进入 ReadyToOpen 状态
    显示 打开按钮

点击打开
    调用 GameManager.GoToResult()

温度条倒放到 0 帧
    调用 FiringSystem.ForceUnderfiredOpen()
    跳过 ClosingKiln
    直接 GameManager.GoToResult()
    结果次品（温度=0 → 分数=0 → 欠烧）
```

## Refresh 性能策略

接入美术后不建议每帧全量刷新所有 UI。

当前 `Update()` 每帧调用 `Refresh()`，第一版保留驱动，但控制刷新粒度：

- 温度条视频帧由 `temperatureBarProgress` 驱动，每帧计算目标 frame，但只在目标 frame 改变时 seek
- 风门帧只在档位变化时换 sprite
- 温度文本如果保留，只在整数温度变化时更新
- 背景图、按钮显隐、视频播放状态只在状态切换时更新
- 不要每帧反复设置 `Image.sprite`
- 不要每帧反复调用 `VideoPlayer.Play()` / `Stop()`
- 不要每帧切换 `RawImage.texture`

建议内部缓存：

```text
lastWholeTemperature
lastWindFrame
lastUiState
lastIsFiring
lastIsDropping
lastTemperatureBarFrame
```

## 风门/窗口功能处理

- 风门重新暴露，采用序列帧 + 鼠标滚轮交互（见"风门 UI 方案"章节）
- 旧 `Slider_Wind` 隐藏不删除（保留兼容 Phase3SceneBuilder）
- `Btn_Window` / `ToggleWindow()` 第一版仍不暴露（参考图无对应控件）
- `FiringSystem.SetWindValue()` 接口继续沿用

## Phase3SceneBuilder 同步策略

当前构建器：

```text
Assets/Phase3/Editor/Phase3SceneBuilder.cs
```

其中 `EnsureFiringPanelControls()` 会创建文字版占位 UI，包括 Text、Slider、Button。

接入美术面板后采用方案 A：

检测到美术版节点后，跳过旧占位控件创建。

识别节点：

```text
Panel_Firing/ArtRoot_Firing
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
- `FiringSystem.Update()` 的温度计算逻辑（新增低风门降温分支）。
- `FiringSystem` 新增低风门相关 SerializeField、只读接口与 `ForceUnderfiredOpen()`。
- `Phase3SceneBuilder.EnsureFiringPanelControls()` 的美术面板检测逻辑。
- 烧制按钮的 `Image.sprite` 和 RectTransform。
- 温度条改为视频帧映射表现。
- 风门改为序列帧 + 滚轮交互。
- 视频播放链路和 RenderTexture 资源。

### 暂不改

- 不重做 `FiringSystem` 评分计算（`GetFireScore` / `CalculateScore` / `GetCurrentZone`）。
- 不改 `GameManager.UpdatePanels()` 面板切换方式。
- 不改 Phase9 通过窑口进入 `EnterFiringModule()` 的桥接方式。
- 不改 `ResultSystem` 次品判定（强制欠烧时由 `FiringSystem` 明确把温度压到 0，再沿用现有评分链路）。
- 不拆分 mp4 为序列帧。
- 不把循环火焰视频裁成局部火焰层。
- 不删除旧的风门 Slider 和窗口按钮（隐藏即可）。

## 实施步骤

1. 在 `Panel_Firing` 下搭建 `ArtRoot_Firing` 层级。
2. 添加 `StaticFallback_Image`，绑定 `底图 阶段1.png` 或 `底图 阶段3.png`。
3. 添加 `FiringVideo_RawImage`，铺满烧窑主体画面，作为 `火焰动画.mp4` 与 `循环.mp4` 共用输出。
4. 添加 `TemperatureVideo_RawImage`，绑 `温度条变化.mp4`。
5. 创建 RenderTexture，分配给视频输出（`火焰动画` 与 `循环` 可共享同一 RenderTexture）。
6. 添加或配置 4 个 VideoPlayer（fireStart / firingLoop / closingKiln / temperature）。
7. 添加 `WindDoor_Image` + `WindDoor_HoverArea`，放在右侧中下留白区，拖入 9 帧风门序列。
8. 将按钮替换为美术按钮图：
   - `Btn_Fuel` 使用 `添燃料按钮.png`
   - `Btn_Stop` 使用 `停火按钮.png`
   - `Btn_OpenKiln` 使用 `打开按钮.png`
9. 隐藏旧 `Slider_Wind`、`Btn_Window`、文字占位控件（不删除）。
10. 扩展 `FiringSystem.Update()`：新增低风门降温逻辑 + `IsTemperatureDropping` / `WindValue` 接口 + `ForceUnderfiredOpen()`。
11. 扩展 `FiringPanelController`：状态机 + 视频控制（含起势→循环衔接）+ 风门滚轮 + 温度条帧映射 + Refresh 缓存。
12. 修改 `Phase3SceneBuilder`，检测 `ArtRoot_Firing` 后跳过旧占位控件创建。
13. 在 Phase3 场景测试进入烧制、起势动画、循环切换、添柴、风门调节、停火、打开、进入结果。
14. 测试低风门 3s 倒放 → 自动开窑次品流程。
15. 在 Phase9 桥接场景测试从窑口进入烧制流程。

## 验收清单

- 进入烧制面板后，先播放 `火焰动画.mp4`（起势，单次）。
- `火焰动画.mp4` 播完后无缝切换到 `循环.mp4` 循环播放。
- 主画面视频以整屏烧制画面呈现，内部火焰和外部烟雾都可见。
- 添柴按钮可点击，并能影响当前温度。
- 风门序列帧随滚轮丝滑切换（9 档）。
- 风门视觉和滚轮热区位于右侧中下留白区，不遮挡烟雾主体、窑口火焰和底部按钮。
- 风门 >= 0.3 时温度上升，温度条视频帧前进。
- 风门 < 0.3 持续 3s 时温度下降，温度条视频倒放。
- 温度条视频倒放到 0 帧后自动开窑，结果为次品。
- 温度条视频按视频自身帧数线性推进，不绑温度数值。
- 停火按钮可点击，点击后停止升温。
- 点击停火后播放关窑动画。
- 关窑动画播放完成后显示打开按钮。
- 点击打开后进入结果面板。
- 美术面板存在时，`Phase3SceneBuilder` 不再生成旧文字版烧制控件。
- 控制台无脚本编译错误。
- Phase3 单独场景流程正常。
- Phase9 窑口进入烧制流程正常。

## 待确认参数

| 参数 | 暂定值 | 说明 |
|------|--------|------|
| 倒放速度 | 正常播放速度 | 帧 -= 播放速率 * deltaTime |
| temperatureDropPerSecond | 30 | 低风门降温速率（仅影响 CurrentTemperature 数值与评分） |
| lowWindThreshold | 0.3 | 低风门判定阈值 |
| lowWindDurationThreshold | 3 | 低风门持续触发倒放的时间 |
| 风门档位映射 | windValue*8 取整 | 0~8 共 9 档 |
