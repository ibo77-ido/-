# mobile-转向方案

## 目标

为女主制作一套移动执行序列：

- 女主沿指定路径移动。
- 移动时身体面朝当前路径的前方方向。
- 使用已有的四方向移动动画：前、后、左、右。
- 方案适配移动端，避免复杂运行时计算和频繁状态切换。

## 核心定义

路径前方不是女主当前面朝方向，也不是直接朝终点方向，而是女主当前位置沿路径继续前进的方向。

推荐用当前位置和前方采样点计算：

```csharp
Vector3 current = path.GetPoint(t);
Vector3 ahead = path.GetPoint(t + lookAhead);
Vector3 pathForward = ahead - current;
```

这里要先统一量纲。`t` 可以是归一化进度 `[0, 1]`，也可以是沿路径的实际距离。两种方式不要混用：

- 如果 `t` 是归一化进度，`lookAhead` 也是归一化偏移，例如 `0.02` 表示路径总长度的 2%。
- 如果 `t` 是实际距离，`lookAhead` 也应该是实际距离，例如 `0.5f` 表示向前采样 0.5 米。

移动端推荐内部使用“实际距离”驱动，再通过预采样表把距离映射到路径位置。这样路径点间距不均匀时，移动速度和转向预判更稳定。

当路径是直线时，路径前方就是直线前进方向。

当路径是弯道时，路径前方是曲线当前位置的切线方向。

当路径反向播放时，路径前方也随播放方向反过来。

## 推荐结构

```text
HeroRoot
  ModelRoot
  CameraTarget
```

`HeroRoot` 负责：

- 沿路径移动。
- 平滑旋转到路径前方。
- 挂载移动序列控制脚本。

`ModelRoot` 负责：

- 挂载 Animator。
- 播放四方向移动动画。
- 处理模型自身朝向偏移。

这样可以把路径移动和动画播放拆开，避免动画 Root Motion 和路径控制互相抢位置。

## 执行序列方案

固定剧情或导演式演出推荐：

```text
Timeline
  Animation Track      -> 控制女主动作片段
  Control/Signal Track -> 触发对白、特效、镜头、事件
  Custom Move Track    -> 控制 HeroRoot 沿路径移动
```

`Custom Move Track` 建议后续单独实现为一个可复用轨道，输入和职责如下：

```text
输入：
  HeroRoot
  Path/Spline 引用
  起点距离或归一化起点
  终点距离或归一化终点
  速度曲线
  lookAhead
  rotateSpeed

输出：
  每帧设置 HeroRoot.position
  每帧设置 HeroRoot.rotation
  每帧写入 Animator 的 MoveX / MoveY / Speed
```

TODO：如果项目已经有执行序列系统，可以先不用自定义 Timeline Track，而是把同样的输入封装成一个 `MoveAlongPath` 执行节点。

路径推荐使用：

- Unity Splines
- 自定义 Waypoint Path
- 已有项目内路径组件

如果只是快速验证，可以先用 Waypoint Path。后续需要可视化编辑弯道时，再换成 Unity Splines。

## 四方向动画使用方式

Animator 使用 2D Blend Tree，参数建议：

```text
MoveX
MoveY
Speed
```

Blend Tree 摆点：

```text
Forward  -> ( 0,  1)
Backward -> ( 0, -1)
Left     -> (-1,  0)
Right    -> ( 1,  0)
Idle     -> ( 0,  0)
```

如果要求女主移动时始终面朝路径前方，大多数稳定移动阶段会播放 Forward。Left 和 Right 主要用于转弯过渡，Backward 主要用于特殊演出，例如身体还没转过去但路径已经反向，或者需要倒退演出。

## 朝向与动画参数

每帧计算路径前方：

```csharp
Vector3 current = EvaluatePath(progress);
Vector3 ahead = EvaluatePath(progress + lookAhead);
Vector3 pathForward = ahead - current;
pathForward.y = 0f;
```

如果路径前方有效，旋转 `HeroRoot`：

```csharp
if (pathForward.sqrMagnitude > 0.0001f)
{
    Quaternion targetRotation = Quaternion.LookRotation(pathForward.normalized, Vector3.up);
    heroRoot.rotation = Quaternion.Slerp(
        heroRoot.rotation,
        targetRotation,
        rotateSpeed * Time.deltaTime
    );
}
```

再把世界移动方向转换为角色本地方向，喂给 Animator：

```csharp
Vector3 localMove = heroRoot.InverseTransformDirection(pathForward.normalized);

animator.SetFloat("MoveX", localMove.x, 0.12f, Time.deltaTime);
animator.SetFloat("MoveY", localMove.z, 0.12f, Time.deltaTime);
animator.SetFloat("Speed", 1f, 0.12f, Time.deltaTime);
```

这里 `localMove` 是 3D 向量，但四方向移动只关心水平面。Unity 的水平移动平面通常是 XZ，所以：

- `localMove.x` 对应左/右。
- `localMove.z` 对应前/后。
- `localMove.y` 是上下方向，不参与四方向地面移动动画。

停止时：

```csharp
animator.SetFloat("Speed", 0f, 0.12f, Time.deltaTime);
animator.SetFloat("MoveX", 0f, 0.12f, Time.deltaTime);
animator.SetFloat("MoveY", 0f, 0.12f, Time.deltaTime);
```

## 推荐参数

```text
lookAhead     = 0.02 到 0.08
rotateSpeed   = 8 到 14
animDampTime  = 0.10 到 0.18
```

`lookAhead` 越大，女主越早预判转弯。

`lookAhead` 越小，女主越贴近当前路径切线。

`rotateSpeed` 越大，身体越快对齐路径前方。

`rotateSpeed` 越小，转弯时左右动画过渡越明显。

如果希望旋转角速度更稳定，可以用 `RotateTowards` 替代 `Slerp`：

```csharp
float maxDegreesDelta = rotateDegreesPerSecond * Time.deltaTime;
heroRoot.rotation = Quaternion.RotateTowards(
    heroRoot.rotation,
    targetRotation,
    maxDegreesDelta
);
```

`Slerp(heroRoot.rotation, targetRotation, rotateSpeed * Time.deltaTime)` 写法简单，适合快速验证；`RotateTowards` 的含义更明确，`rotateDegreesPerSecond` 就是每秒最大旋转角度，更适合做可调参数。

## 移动端注意点

- 不建议每帧查找 GameObject 或组件，引用在初始化时缓存。
- 路径点较多时，可以预采样路径，运行时只做插值。
- Animator 参数使用阻尼，避免四方向动画频繁抖动切换。
- 角色位置由 `HeroRoot` 统一控制，除非明确需要 Root Motion。
- 如果使用 Root Motion，建议只取动画位移速度，不让动画直接决定最终路径位置。

## 常见问题

### 模型朝向不对

如果 `HeroRoot` 面朝路径前方，但模型看起来侧着走，说明模型默认朝向和 Unity 的 `+Z` 不一致。

处理方式：

```text
HeroRoot      -> 代码旋转
  ModelRoot   -> 固定旋转偏移，例如 Y = 90 或 Y = -90
```

不要在代码里反复给模型补旋转，固定偏移放在层级里更清楚。

### 弯道突然抽动

通常是采样点太近或路径点间距不均。

处理方式：

- 增大 `lookAhead`。
- 对路径做等距采样。
- 旋转使用 `Slerp` 或 `RotateTowards`。

### 四方向动画看不明显

如果 `rotateSpeed` 很高，角色会迅速面朝路径前方，于是基本只播 Forward。

处理方式：

- 适当降低 `rotateSpeed`。
- 增大 Animator 阻尼。
- 转弯段使用单独的转身动画或转弯状态。

## 推荐落地顺序

1. 搭建 `HeroRoot / ModelRoot` 层级。
2. 配置 Animator 2D Blend Tree。
3. 做一个 Waypoint Path 或 Spline Path。
4. 编写路径移动脚本，只控制 `HeroRoot`。
5. 加入 `lookAhead` 朝向计算。
6. 把本地移动方向传入 Animator。
7. 接入 Timeline 或执行序列系统。
8. 在直线、折线、曲线、反向路径上分别验证。

## 验收标准

- 女主沿路径移动时，身体始终朝向路径继续前进的方向。
- 直线路径没有左右摇摆。
- 弯道转向自然，不突然抽动。
- 四方向动画能在转弯和特殊移动中正确混合。
- 停止后能进入 Idle。
- 移动端运行时没有明显 GC 或频繁查找对象。
