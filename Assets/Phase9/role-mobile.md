# role-mobile

## 目标

让 Phase9 中的女主移动逻辑匹配当前 NavMesh 烘焙结果：

- 女主只能在蓝色 NavMesh 烘焙区域内移动。
- 女主能够覆盖所有与起点连通的蓝色区域。
- 女主移动后能够完成一轮 Phase3 的 4 个核心交互玩法。
- 移动过程中的前后遮挡关系正确。

## 当前基础

- Phase9 场景：`Assets/Phase9/Scenes/SampleScene.unity`
- 主地图对象：`静态层`
- 可走遮罩对象：`NavMesh-walkable`
- 当前烘焙来源：`NavMeshWalkableBakeMesh_FromAlpha`
- 当前 NavMesh agent：`Phase9SmallAgent`
- agent 半径：`0.06`
- agent 高度：`0.5`
- voxel size：`0.01`

## 执行阶段

### 1. 盘点现有移动链路

先只读检查这些对象和脚本：

- `女主`
- `PlayerCharacter`
- `MovementController`
- `Phase9InteractionBridge`
- `CameraFollow2D`
- `Order-interact`
- `Shape-interact`
- `Glaze-interact`
- `Kiln-interact`

需要确认：

- 点击输入在哪里处理。
- 点击点如何从屏幕坐标转换到 XZ 世界坐标。
- 是否调用 `NavMesh.SamplePosition`。
- 是否调用 `NavMesh.CalculatePath`。
- 女主实际移动是通过 `NavMeshAgent`，还是手动更新 `transform`。
- Phase9 当前 XZ 坐标映射是否已经接入移动逻辑。

### 2. 限制移动只能发生在 NavMesh 上

标准移动链路应为：

```text
点击屏幕
-> 转换为 XZ 世界坐标
-> NavMesh.SamplePosition
-> NavMesh.CalculatePath
-> 只有 PathComplete 才允许移动
-> 移动过程中持续贴合 NavMesh
```

关键规则：

- `NavMesh.SamplePosition` 只负责找到附近 NavMesh 点，不代表女主一定能走到。
- 必须再用 `NavMesh.CalculatePath` 验证路径。
- 只有 `path.status == NavMeshPathStatus.PathComplete` 才能设置移动目标。
- 如果移动过程中女主偏离 NavMesh，需要用最近 NavMesh 点拉回。

### 3. 验证蓝色连通区域覆盖

写一个 Editor 验证工具或临时验证函数：

```text
读取当前 NavMesh triangulation
从女主当前位置采样起点
对蓝色区域采样多个测试点
逐个 CalculatePath
输出可达/不可达报告
```

报告应包含：

- NavMesh 总采样点数量。
- 从女主起点可达采样点数量。
- 不可达采样点数量。
- 不可达点世界坐标。

如果存在不可达点，优先判断：

- `NavMesh-walkable` 图片是否存在断开的蓝色区域。
- 窄路是否被 agent 半径切断。
- 交互点是否放在不可达孤岛上。

### 4. 验证 Phase3 四个核心交互

默认验证这 4 个交互点：

- `Order-interact`
- `Shape-interact`
- `Glaze-interact`
- `Kiln-interact`

每个交互点验证：

```text
女主当前位置
-> 交互点附近 NavMesh.SamplePosition
-> CalculatePath
-> PathComplete
-> 距离满足交互范围
```

输出报告格式建议：

```text
Order-interact: PASS, reachable, distance 0.32
Shape-interact: PASS, reachable, distance 0.28
Glaze-interact: PASS, reachable, distance 0.40
Kiln-interact: PASS, reachable, distance 0.35
```

如果失败，不直接扩大 NavMesh，先检查：

- 交互点位置是否在可走区域边缘外。
- 交互半径是否太小。
- 是否需要给交互点增加独立的 `interactionReachPoint`。

### 5. 匹配运行时移动逻辑

移动脚本需要保证：

- `NavMeshAgent.agentTypeID` 使用 `Phase9SmallAgent`。
- `NavMeshAgent.radius = 0.06`。
- `NavMeshAgent.height = 0.5`。
- 点击不可达区域时不移动。
- 点击非 NavMesh 区域时，最多只吸附到合理距离内的 NavMesh 点。
- 路径不完整时不移动。
- 到达交互点附近后，交互系统能识别当前目标。

### 6. 处理移动遮挡关系

先检查当前资源结构：

- 女主 SpriteRenderer 在哪个子物体上。
- 场景建筑、墙、树是否是独立 SpriteRenderer。
- 是否使用 SortingGroup。
- `静态层` 是否是一整张合成大图。

推荐遮挡规则：

```text
角色越靠画面下方，sortingOrder 越高
角色越靠画面上方，sortingOrder 越低
```

可选实现：

```text
每帧根据角色脚底点的 screenY 或 worldZ 更新 sortingOrder
```

如果场景建筑和地面都在同一张 `静态层` 图片里，无法只靠 sortingOrder 做真实遮挡。此时需要拆出前景遮挡层：

```text
静态层_Background
女主
静态层_ForegroundOccluder
```

女主在中间，前景遮挡层覆盖角色应该被遮挡的部分。

## 推荐实施顺序

1. 只读检查 `MovementController`、`PlayerCharacter`、`Phase9InteractionBridge`。
2. 修移动目标选择：必须 `SamplePosition + CalculatePath(PathComplete)`。
3. 确认女主和 Surface 都使用 `Phase9SmallAgent`。
4. 写 Editor 验证：女主到 4 个核心交互点全部可达。
5. 运行验证：点击蓝色区域可移动，点击非蓝色区域不越界。
6. 检查遮挡资源结构。
7. 如果遮挡对象是独立 SpriteRenderer，加入动态 sortingOrder。
8. 如果只有整张静态图，制作或接入前景遮挡层。

## 验收标准

- 女主无法移动到蓝色 NavMesh 区域外。
- 女主可以覆盖所有与起点连通的蓝色区域。
- `Order-interact`、`Shape-interact`、`Glaze-interact`、`Kiln-interact` 全部可达。
- 完成一轮 Phase3 核心交互流程不需要手动拖动物体。
- 女主经过建筑、墙、树等遮挡区域时，前后层级符合画面表现。
