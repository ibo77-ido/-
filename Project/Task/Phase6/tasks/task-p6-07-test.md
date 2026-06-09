# task-p6-07 WhiteBox Test

## 目标

完成白盒测试、记录结果，并确认 Phase7 准入。

## 步骤

1. 准备测试 UI。
2. 执行移动测试。
3. 执行区域测试。
4. 执行交互测试。
5. 执行 `RefreshVisual()` 测试。
6. 执行状态机测试。
7. 执行 `NavMesh` 测试。
8. 执行 2D 斜俯视相机测试。
9. 执行 `UIOpen` 锁定测试。
10. 执行完整流程测试。
11. 执行地图规则测试。
12. 记录 T01~T20 结果。

## 地图规则测试

地图白盒方案新增检查项：

- 地图尺寸为 `80 x 60`
- 地图以 Orthographic 2D 俯视/斜俯视方式呈现
- `_MapRoot` 下存在 `LogicRoot` 与 `ArtRoot`
- 玩家最终视觉来自 `ArtRoot` 的 2D 地图素材层
- `WalkableRoot` / `StaticBlockerRoot` / `WallRoot` 位于 `LogicRoot`
- `Camera_2D_Oblique` 能完整覆盖核心地图
- `Ground_Base` 不作为 `NavMesh` 来源
- 主通道宽度不小于 `4m`
- 次通道宽度不小于 `2.5m`
- 可移动区域全部使用 `Mat_Walkable_Green`
- 静止障碍全部使用 `Mat_Static_Gray`
- 所有障碍都有 `Collider`
- 所有核心交互点位于可移动区域内
- 所有核心区域之间 `NavMesh` 连通
- 点击灰色障碍不可进入
- 点击围墙外不可进入
- 点击移动通过斜俯视相机正确转换到 `X/Z` 目标点
- 隐藏逻辑层关闭可见性后，不影响移动、交互和区域检测

## 验收

- 全部 PASS
- 无 Console Error
- 可完整走通：
  `订单区 -> 拉坯区 -> 配釉区 -> 烧窑区 -> 返回订单区`

## 依赖

- `task-p6-01-scene.md`
- `task-p6-02-character.md`
- `task-p6-03-interaction.md`
- `task-p6-04-workstation.md`
- `task-p6-05-area.md`
- `task-p6-06-scale.md`

## 交付物

- 白盒测试记录
- Phase7 准入结论
- `STATE.md` / `DECISIONS.md` 已更新
