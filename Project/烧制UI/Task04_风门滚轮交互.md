# Task04 — 风门滚轮交互

## 目标

实现风门序列帧切换 + 鼠标中键滚轮交互。玩家鼠标悬停在风门热区滚动滚轮时，按 1/8 步进切换 windValue（9 档），同步切换风门序列帧 sprite。

## 依赖

- Task02 完成（WindDoor_Image + WindDoor_HoverArea 节点已搭建）
- Task03 完成（FiringPanelController 已有 windDoorImage / windDoorFrames / windDoorHoverArea 字段）

## 涉及文件

### Modified
- `Assets/Phase3/Scripts/UI/FiringPanelController.cs`

## 实施内容

### 1. 字段填充（Inspector）

在 Task02 搭建的节点基础上，将引用拖入 Task03 已声明的字段：

- `windDoorImage` ← WindDoor_Image
- `windDoorFrames` ← 风门/1.png ~ 8.png + 满.png（共 9 个 Sprite，按顺序）
- `windDoorHoverArea` ← WindDoor_HoverArea

### 2. 新增缓存字段

```csharp
private int lastWindFrame = -1;
```

### 3. 滚轮检测逻辑

```csharp
private void UpdateWindDoorScroll()
{
    if (windDoorHoverArea == null || firingSystem == null) return;
    if (currentState != FiringUiState.FiringActive) return;
    
    // 检测鼠标是否在风门热区
    bool isOverWindDoor = RectTransformUtility.RectangleContainsScreenPoint(
        windDoorHoverArea, 
        Input.mousePosition,
        null  // ScreenSpaceOverlay 模式传 null；若 Camera 模式需传对应 Camera
    );
    
    if (!isOverWindDoor) return;
    
    float scroll = Input.mouseScrollDelta.y;
    if (Mathf.Abs(scroll) < 0.01f) return;
    
    // 步进 1/8 = 0.125
    float currentWind = firingSystem.WindValue;
    float step = 1f / 8f;
    if (scroll > 0)
    {
        currentWind += step;
    }
    else
    {
        currentWind -= step;
    }
    currentWind = Mathf.Clamp01(currentWind);
    firingSystem.SetWindValue(currentWind);
}
```

### 4. 风门帧切换（带缓存）

```csharp
private void RefreshWindDoorFrame()
{
    if (windDoorImage == null || windDoorFrames == null || windDoorFrames.Length == 0) return;
    if (firingSystem == null) return;
    
    // windValue 0~1 → frameIndex 0~8
    int frameIndex = Mathf.RoundToInt(firingSystem.WindValue * 8f);
    frameIndex = Mathf.Clamp(frameIndex, 0, windDoorFrames.Length - 1);
    
    if (frameIndex != lastWindFrame)
    {
        windDoorImage.sprite = windDoorFrames[frameIndex];
        lastWindFrame = frameIndex;
    }
}
```

### 5. 集成到 Update

在 `Update()` 中调用（Task03 的 Update 基础上追加）：

```csharp
private void Update()
{
    Refresh();
    UpdateWindDoorScroll();   // 新增
    RefreshWindDoorFrame();   // 新增
}
```

### 6. Canvas 模式适配

若 Canvas 为 ScreenSpaceCamera 模式，需将 `RectangleContainsScreenPoint` 的第三个参数改为对应 Camera：

```csharp
// 在 Controller 中新增字段或从 Canvas 获取
Camera uiCamera = GetComponentInParent<Canvas>().worldCamera;
bool isOver = RectTransformUtility.RectangleContainsScreenPoint(
    windDoorHoverArea, Input.mousePosition, uiCamera
);
```

第一版按 ScreenSpaceOverlay（uiCamera=null）实现，若场景为 Camera 模式再适配。

## Serialized References Changed

```
[INSPECTOR REBIND] FiringPanelController:
  - windDoorImage → 拖入 WindDoor_Image
  - windDoorFrames → 拖入 9 个风门 Sprite（1.png~8.png + 满.png，按顺序）
  - windDoorHoverArea → 拖入 WindDoor_HoverArea
```

## Scene Mutation

```
NONE
```

## Acceptance Check

1. 编译通过
2. 鼠标移到风门热区，滚轮向上 → windValue 增加 0.125，风门帧切换到更大开度
3. 滚轮向下 → windValue 减少 0.125，风门帧切换到更小开度
4. windValue=0 → 显示 1.png；windValue=1 → 显示 满.png
5. 9 档切换丝滑，无跳帧
6. 鼠标移出热区，滚轮无效
7. 非 FiringActive 态（ClosingKiln/ReadyToOpen）滚轮无效
8. 风门帧只在档位变化时切换 sprite（不每帧设置）

## Risks

- Canvas 模式若为 ScreenSpaceCamera，`RectangleContainsScreenPoint` 需传 uiCamera，否则检测失效
- 风门 9 帧 Sprite 顺序必须严格按 1~8 + 满，否则视觉错乱
- `Input.mouseScrollDelta` 在 Editor 和打包后行为一致，但触屏设备无滚轮，需后续适配（第一版不处理）

## Next Recommended Task

Task05_温度条视频驱动
