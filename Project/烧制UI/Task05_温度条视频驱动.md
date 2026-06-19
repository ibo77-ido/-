# Task05 — 温度条视频驱动

## 目标

实现温度条视频帧驱动：Controller 内部维护独立的 `temperatureBarProgress`（0~1），按视频自身帧率线性推进；风门低进风持续 3s 时倒放；倒放到 0 帧触发强制开窑次品分支。

## 依赖

- Task01 完成（FiringSystem.IsTemperatureDropping / ForceUnderfiredOpen 接口已就绪）
- Task03 完成（FiringPanelController 状态机已就绪，temperatureVideoPlayer 字段已声明）

## 涉及文件

### Modified
- `Assets/Phase3/Scripts/UI/FiringPanelController.cs`

## 实施内容

### 1. 新增字段

```csharp
[Header("Temperature Bar")]
[SerializeField] private float temperatureBarForwardSpeed = 1f;   // 正向推进速度（progress/秒）
[SerializeField] private float temperatureBarReverseSpeed = 1f;   // 倒放速度（progress/秒）

private float temperatureBarProgress = 0f;     // 0~1，独立于 CurrentTemperature
private bool isReversing = false;
private int lastTemperatureBarFrame = -1;
private bool hasTriggeredForceOpen = false;
```

### 2. 字段填充（Inspector）

- `temperatureVideoPlayer` ← TemperatureVideoPlayer（Task02 已搭建）
- `temperatureVideoImage` ← TemperatureVideo_RawImage

### 3. 温度条视频初始化

在 `Start()` 或 `EnterState(FiringActive)` 时：

```csharp
private void InitTemperatureVideo()
{
    if (temperatureVideoPlayer == null) return;
    temperatureVideoPlayer.Stop();
    temperatureVideoPlayer.frame = 0;
    temperatureBarProgress = 0f;
    isReversing = false;
    hasTriggeredForceOpen = false;
    lastTemperatureBarFrame = -1;
}
```

### 4. 温度条视频驱动（每帧）

```csharp
private void UpdateTemperatureVideo()
{
    if (temperatureVideoPlayer == null || firingSystem == null) return;
    if (currentState != FiringUiState.FiringActive) return;
    if (hasTriggeredForceOpen) return;
    
    long totalFrames = temperatureVideoPlayer.frameCount;
    if (totalFrames <= 0) return;
    
    // 判断倒放状态
    bool shouldReverse = firingSystem.IsTemperatureDropping;
    
    if (shouldReverse)
    {
        isReversing = true;
        temperatureBarProgress -= temperatureBarReverseSpeed * Time.deltaTime;
        temperatureBarProgress = Mathf.Max(0f, temperatureBarProgress);
    }
    else
    {
        isReversing = false;
        temperatureBarProgress += temperatureBarForwardSpeed * Time.deltaTime;
        temperatureBarProgress = Mathf.Min(1f, temperatureBarProgress);
    }
    
    // 映射到视频帧
    long targetFrame = (long)(temperatureBarProgress * totalFrames);
    targetFrame = Mathf.Clamp(targetFrame, 0, totalFrames - 1);
    
    if (targetFrame != lastTemperatureBarFrame)
    {
        temperatureVideoPlayer.frame = targetFrame;
        lastTemperatureBarFrame = (int)targetFrame;
    }
    
    // 倒放到 0 帧 → 强制开窑次品
    if (isReversing && temperatureBarProgress <= 0f && !hasTriggeredForceOpen)
    {
        TriggerForceUnderfiredOpen();
    }
}

private void TriggerForceUnderfiredOpen()
{
    hasTriggeredForceOpen = true;
    Debug.Log("[FiringPanel] Temperature bar reversed to 0, force underfired open.");
    firingSystem.ForceUnderfiredOpen();
    
    // 跳过 ClosingKiln 动画，直接进入结果
    StopMainVideo();
    if (closingKilnPlayer != null) closingKilnPlayer.Stop();
    
    gameManager?.GoToResult();
}
```

### 5. 集成到 Update

在 `Update()` 中调用（Task03/Task04 基础上追加）：

```csharp
private void Update()
{
    Refresh();
    UpdateWindDoorScroll();
    RefreshWindDoorFrame();
    UpdateTemperatureVideo();   // 新增
}
```

### 6. ResetPanel 同步

在 `ResetPanel()` 中调用 `InitTemperatureVideo()`。

### 7. 进入 FiringActive 状态时初始化

在 `EnterState(FiringUiState.FiringActive)` 分支中调用 `InitTemperatureVideo()`。

## 关键逻辑说明

### temperatureBarProgress 与 CurrentTemperature 的关系

- **完全解耦**
- `temperatureBarProgress` 是 Controller 内部独立维护的 0~1 进度，驱动视频帧
- `CurrentTemperature` 是 FiringSystem 的数值，用于评分
- 两者都受 windValue 影响（通过 IsTemperatureDropping 间接关联），但不一一对应

### 倒放触发链

```
windValue < 0.3 持续 3s
  → FiringSystem.IsTemperatureDropping = true
  → Controller.UpdateTemperatureVideo() 检测到 shouldReverse
  → temperatureBarProgress 减少
  → 视频 frame 反向 seek
  → progress <= 0 → TriggerForceUnderfiredOpen()
  → FiringSystem.ForceUnderfiredOpen()（温度压0 + StopFiring）
  → GameManager.GoToResult()
  → 评分 stopFireTemp=0 → 分数=0 → 欠烧次品
```

### 次品判定的可靠性

不依赖"降温刚好降到0"的时间巧合，而是：
1. `temperatureBarProgress` 倒放到 0 触发
2. `ForceUnderfiredOpen()` 明确把 `currentTemperature = 0`
3. 评分链路自然走欠烧分支

## Serialized References Changed

```
[NEW SerializeField] FiringPanelController:
  - temperatureBarForwardSpeed (float, 1)
  - temperatureBarReverseSpeed (float, 1)
[INSPECTOR REBIND] FiringPanelController:
  - temperatureVideoPlayer → 拖入 TemperatureVideoPlayer
  - temperatureVideoImage → 拖入 TemperatureVideo_RawImage
```

## Scene Mutation

```
NONE
```

## Acceptance Check

1. 编译通过
2. 进入烧制 → 温度条视频从 0 帧开始
3. 风门 >= 0.3 → 温度条视频正向推进（frame 增加）
4. 风门 < 0.3 持续 3s → 温度条视频倒放（frame 减少）
5. windValue 恢复 >= 0.3 → 倒放立即停止，恢复正向
6. 倒放到 frame 0 → 自动触发强制开窑 → 进入结果面板
7. 强制开窑后 `CurrentTemperature = 0`，`GetFireScore() = 0`，结果为次品
8. 温度条视频帧只在 targetFrame 变化时 seek（不每帧重复 seek）
9. 非 FiringActive 态温度条视频不更新
10. ResetPanel 后温度条视频归零

## Risks

- `VideoPlayer.frame` 手动 seek 每帧调用可能有性能开销，若卡顿可考虑降低 seek 频率（如每 2 帧 seek 一次）
- `temperatureBarProgress` 正向到 1 后停止（不循环），需确认末帧表现是否合适
- `hasTriggeredForceOpen` 需在 ResetPanel 时重置，否则重复进入烧制无法再次触发次品分支
- 倒放速度和正向速度默认都为 1（progress/秒），实际视频时长不同会导致视觉速度差异，需实测调整

## Next Recommended Task

Task06_Phase3SceneBuilder检测分支
