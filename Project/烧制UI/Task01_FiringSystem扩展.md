# Task01 — FiringSystem 扩展

## 目标

扩展 `FiringSystem` 温度计算逻辑，新增低风门降温分支 + 3 个只读/公开接口，为后续 Controller 的温度条视频驱动和次品判定提供数值支持。

## 依赖

- 无前置依赖

## 涉及文件

### Modified
- `Assets/Phase3/Scripts/Systems/Firing/FiringSystem.cs`

## 实施内容

### 1. 新增 SerializeField

```csharp
[SerializeField] private float lowWindThreshold = 0.3f;
[SerializeField] private float lowWindDurationThreshold = 3f;
[SerializeField] private float temperatureDropPerSecond = 30f;
```

### 2. 新增私有字段

```csharp
private float lowWindTimer;
private bool isTemperatureDropping;
```

### 3. 扩展 Update()

```csharp
private void Update()
{
    if (!isFiring) return;
    
    if (windValue < lowWindThreshold)
    {
        lowWindTimer += Time.deltaTime;
        if (lowWindTimer >= lowWindDurationThreshold)
        {
            isTemperatureDropping = true;
            currentTemperature -= temperatureDropPerSecond * Time.deltaTime;
            currentTemperature = Mathf.Max(0f, currentTemperature);
            return;
        }
    }
    else
    {
        lowWindTimer = 0f;
        isTemperatureDropping = false;
    }
    currentTemperature += temperatureRisePerSecond * windValue * Time.deltaTime;
}
```

### 4. 新增只读接口

```csharp
public bool IsTemperatureDropping { get { return isTemperatureDropping; } }
public float WindValue { get { return windValue; } }
```

### 5. 新增强制欠烧方法

```csharp
public void ForceUnderfiredOpen()
{
    currentTemperature = 0f;
    isTemperatureDropping = false;
    lowWindTimer = 0f;
    StopFiring();
}
```

### 6. 同步 ResetFiring()

`ResetFiring()` 需追加 `lowWindTimer = 0f; isTemperatureDropping = false;`

## 不改部分

- `AddFuel`、`StopFiring`、`GetCurrentZone`、`GetFireScore`、`CalculateScore`、`StartFiring`、`SetWindValue`、`ToggleWindow` 接口签名不变
- `FireConfigSO` 配置不变
- 评分逻辑不变

## Serialized References Changed

```
[NEW SerializeField] FiringSystem:
  - lowWindThreshold (float, 0.3)
  - lowWindDurationThreshold (float, 3)
  - temperatureDropPerSecond (float, 30)
[INSPECTOR] 无需重新拖引用，新增字段有默认值
```

## Scene Mutation

```
NONE
```

## Acceptance Check

1. FiringSystem 编译通过，无错误
2. windValue=1 时温度正常上升（50/秒）
3. windValue=0.2 持续 <3s，温度仍上升
4. windValue=0.2 持续 >=3s，温度下降（30/秒），`IsTemperatureDropping=true`
5. windValue 恢复 >=0.3，`IsTemperatureDropping=false`，温度恢复上升
6. `ForceUnderfiredOpen()` 调用后：currentTemperature=0、isFiring=false、IsTemperatureDropping=false
7. `ResetFiring()` 后 lowWindTimer 和 isTemperatureDropping 归零

## Risks

- `isTemperatureDropping` 状态需在 windValue 恢复时立即 false，否则 Controller 会误判倒放
- `ForceUnderfiredOpen()` 必须在 StopFiring 前清零温度，确保评分用 stopFireTemp=0

## Next Recommended Task

Task02_场景层级搭建
