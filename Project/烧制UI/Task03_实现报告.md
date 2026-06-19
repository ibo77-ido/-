# Task03 实现报告 — FiringPanelController 状态机

## 1. 任务目标

将 FiringPanelController 从"UI逻辑堆叠脚本"升级为状态机驱动 UI 系统：
- UI 状态统一入口（EnterState）
- Refresh 与状态控制解耦
- 视频播放状态流闭环
- 旧UI兼容隐藏处理

## 2. 文件变更

### Modified Files
- `Assets/Phase3/Scripts/UI/FiringPanelController.cs`

## 3. 核心架构变更

### 3.1 状态机入口统一
```csharp
EnterState(FiringUiState newState, bool force = false)
```
职责：UI 显隐 / 视频播放切换 / 按钮状态管理 / 状态缓存防重复触发（force bypass）

### 3.2 Refresh 职责拆分
- 仅负责：温度文本更新 / Zone 更新 / IsFiring 状态检测 / UI 数据同步
- 不再负责：状态切换 / 视频控制 / 按钮逻辑

### 3.3 视频播放系统（两段式）
```
FireStart → FiringLoop → ClosingKiln → ReadyToOpen
```
- loopPointReached 驱动下一状态
- Stop → Play 顺序切换
- 回调统一进入状态机

### 3.4 ResetPanel 优化
- `EnterState(FiringUiState.FiringActive, true)` force 模式
- 重置缓存数据（lastIsFiring / lastWholeTemperature / lastZone）

### 3.5 Legacy UI 处理
- Start 时执行 HideLegacyControls()
- 隐藏：windSlider / windowButton / zoneText / fireScoreText / statusText / temperatureText

## 4. Inspector 绑定新增

### VideoPlayer
- fireStartPlayer → FireStart
- firingLoopPlayer → FiringLoop
- closingKilnPlayer → ClosingKiln
- temperatureVideoPlayer → Temperature

### UI References
- firingVideoImage → FiringVideo_RawImage
- temperatureVideoImage → TemperatureVideo_RawImage
- staticFallbackImage → StaticFallback_Image
- fuelButtonRoot → Btn_Fuel
- stopButtonRoot → Btn_Stop
- openButtonRoot → Btn_OpenKiln
- windDoorImage → WindDoor_Image
- windDoorHoverArea → WindDoor_HoverArea
- windDoorFrames → 9 sprites (1.png~8.png + 满.png)

## 5. 状态机结构
```
FiringActive
    ↓ (Stop Fire)
ClosingKiln
    ↓ (关窑动画结束)
ReadyToOpen
```

## 6. Scene Mutation
- 无新增 GameObject
- 无层级结构变更
- 仅 Inspector 绑定 + 脚本更新

## 7. 验收结果

| # | Check | Result |
|---|-------|--------|
| 1 | 编译通过 | ✓ |
| 2 | 13 个新字段引用已绑定并验证 | ✓ |
| 3 | 风门 9 帧 Sprite 数组已绑定 | ✓ |
| 4 | EnterState 为唯一状态入口，带 force 参数 | ✓ |
| 5 | Refresh 只做连续刷新，不做状态切换 | ✓ |
| 6 | OnStopButtonClicked 只发起 EnterState(ClosingKiln) | ✓ |
| 7 | ResetPanel 用 force:true | ✓ |
| 8 | 旧控件在 Start 时隐藏 | ✓ |
| 9 | 视频回调 OnDestroy 清理 | ✓ |
| 10 | 运行时：起势→循环衔接 | ⏳ 待验证 |
| 11 | 运行时：停火→关窑动画→打开按钮 | ⏳ 待验证 |

## 8. 风险记录

| # | Risk | Mitigation |
|---|------|-----------|
| 1 | VideoPlayer loopPointReached 回调依赖 | 回调用 -= 再 += 防重复；OnDestroy 清理 |
| 2 | 3 个 VideoPlayer 共享 RT_Firing | Stop→Play 顺序切换已确保 |
| 3 | Legacy UI 残留 | 当前仅 inactive，后续可迁移至 DebugLayer |

## 9. 下一步
- Task04 风门滚轮交互（任务文件已定义：序列帧 + 鼠标滚轮 9 档切换）
- Task05 温度条视频驱动（temperatureBarProgress + 倒放 + 强制开窑）
