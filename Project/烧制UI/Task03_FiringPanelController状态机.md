# Task03 — FiringPanelController 状态机

## 目标

扩展 `FiringPanelController`，实现 3 态状态机（FiringActive / ClosingKiln / ReadyToOpen）+ 主画面视频两段式播放（起势→循环衔接）+ Refresh 缓存优化。本任务聚焦主流程，风门滚轮和温度条视频驱动分别由 Task04/Task05 实现。

## 依赖

- Task01 完成（FiringSystem 接口已就绪）
- Task02 完成（场景节点已搭建，Controller 可拖引用）

## 涉及文件

### Modified
- `Assets/Phase3/Scripts/UI/FiringPanelController.cs`

## 实施内容

### 1. 新增 SerializeField

```csharp
[Header("Art - Videos")]
[SerializeField] private VideoPlayer fireStartPlayer;          // 火焰动画.mp4
[SerializeField] private VideoPlayer firingLoopPlayer;        // 循环.mp4
[SerializeField] private VideoPlayer closingKilnPlayer;       // 关窑动画.mp4
[SerializeField] private VideoPlayer temperatureVideoPlayer;  // 温度条变化.mp4（Task05 用）
[SerializeField] private RawImage firingVideoImage;
[SerializeField] private RawImage temperatureVideoImage;       // Task05 用

[Header("Art - Static")]
[SerializeField] private Image staticFallbackImage;

[Header("Art - Wind Door")]
[SerializeField] private Image windDoorImage;                  // Task04 用
[SerializeField] private Sprite[] windDoorFrames;              // Task04 用
[SerializeField] private RectTransform windDoorHoverArea;      // Task04 用

[Header("Art - Buttons")]
[SerializeField] private GameObject fuelButtonRoot;
[SerializeField] private GameObject stopButtonRoot;
[SerializeField] private GameObject openButtonRoot;
```

> 注：风门和温度条相关字段在本 Task 中声明但暂不使用逻辑，留给 Task04/Task05 填充。Inspector 中可暂不拖引用，本 Task 验收不涉及。

### 2. 状态机枚举

```csharp
private enum FiringUiState { FiringActive, ClosingKiln, ReadyToOpen }
private FiringUiState currentState = FiringUiState.FiringActive;
```

### 3. 新增缓存字段

```csharp
private FiringUiState lastUiState = (FiringUiState)(-1);
private bool lastIsFiring = false;
```

### 4. 主画面视频两段式播放

```csharp
private void PlayFireStartSequence()
{
    if (fireStartPlayer == null || firingLoopPlayer == null) return;
    
    firingLoopPlayer.Stop();
    fireStartPlayer.frame = 0;
    fireStartPlayer.Play();
    
    // 注册起势播完回调
    fireStartPlayer.loopPointReached -= OnFireStartFinished;
    fireStartPlayer.loopPointReached += OnFireStartFinished;
}

private void OnFireStartFinished(VideoPlayer vp)
{
    fireStartPlayer.Stop();
    firingLoopPlayer.frame = 0;
    firingLoopPlayer.Play();
}

private void StopMainVideo()
{
    if (fireStartPlayer != null) fireStartPlayer.Stop();
    if (firingLoopPlayer != null) firingLoopPlayer.Stop();
}
```

### 5. 状态切换逻辑

```csharp
private void EnterState(FiringUiState newState)
{
    if (currentState == newState) return;
    currentState = newState;
    
    switch (newState)
    {
        case FiringUiState.FiringActive:
            fuelButtonRoot?.SetActive(true);
            stopButtonRoot?.SetActive(true);
            openButtonRoot?.SetActive(false);
            PlayFireStartSequence();
            break;
            
        case FiringUiState.ClosingKiln:
            fuelButtonRoot?.SetActive(false);
            stopButtonRoot?.SetActive(false);
            openButtonRoot?.SetActive(false);
            StopMainVideo();
            PlayClosingKiln();
            break;
            
        case FiringUiState.ReadyToOpen:
            openButtonRoot?.SetActive(true);
            break;
    }
}

private void PlayClosingKiln()
{
    if (closingKilnPlayer == null) return;
    closingKilnPlayer.frame = 0;
    closingKilnPlayer.Play();
    closingKilnPlayer.loopPointReached -= OnClosingKilnFinished;
    closingKilnPlayer.loopPointReached += OnClosingKilnFinished;
}

private void OnClosingKilnFinished(VideoPlayer vp)
{
    EnterState(FiringUiState.ReadyToOpen);
}
```

### 6. 按钮回调调整

```csharp
private void OnStopButtonClicked()
{
    if (firingSystem == null || !firingSystem.IsFiring) return;
    firingSystem.StopFiring();
    EnterState(FiringUiState.ClosingKiln);
}

private void OnOpenKilnButtonClicked()
{
    if (firingSystem == null || firingSystem.IsFiring) return;
    
    var shapeResult = shapeSystem?.LastResult;
    var glazeResult = glazeSystem?.LastResult;
    Debug.Log($"[OpenKiln] Shape: {shapeResult?.overallScore ?? 0f:F1}% | Glaze: {glazeResult?.overallScore ?? 0f:F1}% | Fire: {firingSystem.GetFireScore():F1}");
    
    openKilnButton.interactable = false;
    gameManager?.GoToResult();
}
```

### 7. Refresh 缓存优化

```csharp
private void Refresh()
{
    if (firingSystem == null) return;
    
    // 状态切换检测
    if (currentState != lastUiState)
    {
        EnterState(currentState);
        lastUiState = currentState;
    }
    
    // IsFiring 变化检测
    if (firingSystem.IsFiring != lastIsFiring)
    {
        lastIsFiring = firingSystem.IsFiring;
        if (lastIsFiring && currentState == FiringUiState.FiringActive)
        {
            PlayFireStartSequence();
        }
    }
    
    // 保留现有 temperatureText/zoneText/fireScoreText/statusText 更新逻辑
    // （带缓存，只在整数温度变化时更新）
    // ... 原有 Refresh 内容
}
```

### 8. 重置面板

```csharp
public void ResetPanel()
{
    if (windSlider != null) windSlider.value = 0f;
    if (stopButton != null) stopButton.interactable = true;
    if (openKilnButton != null) openKilnButton.interactable = false;
    
    StopMainVideo();
    if (closingKilnPlayer != null) closingKilnPlayer.Stop();
    EnterState(FiringUiState.FiringActive);
}
```

### 9. 隐藏旧控件

`Start()` 中将旧 `windSlider`、`windowButton`、`zoneText`、`fireScoreText`、`statusText` 的 GameObject 设为 inactive（若引用非空）。

## Serialized References Changed

```
[NEW SerializeField] FiringPanelController:
  - fireStartPlayer, firingLoopPlayer, closingKilnPlayer, temperatureVideoPlayer
  - firingVideoImage, temperatureVideoImage
  - staticFallbackImage
  - windDoorImage, windDoorFrames, windDoorHoverArea (本 Task 不拖引用)
  - fuelButtonRoot, stopButtonRoot, openButtonRoot
[INSPECTOR REBIND] 需拖入 4 个 VideoPlayer + 2 个 RawImage + staticFallbackImage + 3 个按钮根节点
```

## Scene Mutation

```
NONE（仅改脚本，引用拖拽在 Editor 完成）
```

## Acceptance Check

1. 编译通过，无错误
2. 进入烧制 → 播放 火焰动画.mp4（起势，单次）
3. 火焰动画播完 → 无缝切换到 循环.mp4 循环播放
4. 点击停火 → 进入 ClosingKiln → 播放关窑动画.mp4
5. 关窑动画播完 → 进入 ReadyToOpen → 显示打开按钮
6. 点击打开 → 进入结果面板
7. Refresh 不每帧全量设置视频/按钮状态
8. 旧控件已隐藏

## Risks

- `fireStartPlayer.loopPointReached` 回调可能重复注册，需 `-=` 再 `+=`
- 3 个主画面 VideoPlayer 共享 RenderTexture，切换时需确保前一个 Stop 后再 Play 下一个，否则画面闪烁
- `OnEnable` 调用 `Refresh` 时 firingSystem 可能为空，需空判断

## Next Recommended Task

Task04_风门滚轮交互 或 Task05_温度条视频驱动（可并行）
