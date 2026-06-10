# P8-03A Phase6 × Phase3 Gameplay Runtime Bridge v2.0
## Minimum Stable Runtime Bridge Design

## 0. 文档目标

这份设计文档定义 Phase6 与 Phase3 的正式运行时桥接方案。  
它的目标不是“把两个系统合并”，而是在不破坏 Phase3 原有玩法系统的前提下，让 Phase6 的区域交互成为 Phase3 玩法模块的进入入口，并且保证：

- Phase6 负责世界移动、区域识别、E 键交互
- Bridge 负责会话、输入、UI、生命周期、阻断、恢复
- Phase3 继续负责订单、器型、釉料、烧制、结果
- Phase3 原有自动串联流程在 Phase6 桥接运行时被阻断
- 玩家必须回到 Phase6 世界并移动到下一个区域，才能进入下一个模块

此设计遵循最小侵入原则：

- 新增文件为主
- Phase3 只保留约 15 行级别的最小门控
- 依赖方向严格保持 `Bridge -> Phase3`
- 禁止 `Phase3 -> Phase6`

---

## 1. 设计原则

### 1.1 不移动文件位置

本 v2.0 只升级设计内容，不要求改变现有文件路径。  
所有新增文件都应保持在现有 Phase6 / Bridge 目录体系内，现有 Phase3、Phase6 脚本文件不搬迁。

### 1.2 最小稳定实现

当前阶段不追求“框架化”或“平台化”，只追求：

- 单入口
- 会话隔离
- 自动串联阻断
- 最小侵入
- 可验证

### 1.3 Bridge 是唯一调度中心

Runtime 的唯一主调度对象必须是 `GameplayBridgeManager`。  
其他对象只能做数据、界面、输入或适配，不允许替代它做主流程决策。

### 1.4 Phase3 必须发事件

Bridge 不能靠“轮询猜测模块是否完成”，也不能靠“截断按钮回调”来替代 Phase3 的完成信号。  
Phase3 必须主动发出模块完成事件，Bridge 才能可靠地执行退出、恢复和下一次会话准备。

---

## 2. 总体架构

```text
Phase6 World Layer
    ↓
Bridge Runtime Layer
    ↓
Phase3 Gameplay Modules
```

### 2.1 Phase6 World Layer

职责：

- 玩家移动
- 区域检测
- E 键交互
- 工作站入口识别

不负责：

- 玩法推进
- 评分逻辑
- 模块退出
- 自动串联

### 2.2 Bridge Runtime Layer

职责：

- Session 创建、启动、结束、销毁
- 模块生命周期管理
- 输入接管与恢复
- UI 显示与关闭
- 自动串联阻断
- Phase6 恢复

### 2.3 Phase3 Gameplay Layer

职责：

- Order
- Shape
- Glaze
- Firing
- Result

原则：

- 不改玩法规则
- 不改评分规则
- 不让 Phase3 感知 Phase6

---

## 3. 自动串联阻断机制

这是本设计的核心。

### 3.1 为什么不能只在 Bridge 层“拦截”

如果只在 Bridge 层拦截：

- Phase3 会以为自己已经可以继续推进
- 面板按钮和内部状态可能已经进入下一模块
- Result、Next、Exit 等事件可能已经被触发
- 结果是“逻辑已前进，但世界未前进”

这会导致双重状态源：

- Phase3 认为流程继续了
- Phase6 认为玩家仍在当前站点

因此，**Phase3 必须自己知道当前是 Bridge 运行模式，并且在这一模式下停止自动推进**。

### 3.2 Phase3 GameManager 最小门控代码

下面是推荐的最小改动方式。  
目标不是重写 `GameManager`，而是加一个桥接模式门控和一个完成事件出口。

```csharp
public enum RuntimeMode
{
    StandalonePhase3,
    Phase6Bridge
}

public event System.Action OnModuleCompletedForBridge;

[SerializeField] private RuntimeMode runtimeMode = RuntimeMode.StandalonePhase3;
[SerializeField] private bool canAutoAdvance = true;

public void SetRuntimeMode(RuntimeMode mode)
{
    runtimeMode = mode;
    canAutoAdvance = mode == RuntimeMode.StandalonePhase3;
}

public void NotifyModuleCompletedForBridge()
{
    OnModuleCompletedForBridge?.Invoke();
}

private bool CanAutoAdvanceNow()
{
    return runtimeMode == RuntimeMode.StandalonePhase3 && canAutoAdvance;
}
```

### 3.3 在模块推进处加门控

Phase3 原本负责自动推进的地方，必须加上最小门控：

```csharp
public void GoToShape()
{
    if (!CanAutoAdvanceNow()) return;
    if (orderManager != null) orderManager.GetCurrentOrder();
    if (shapeSystem != null) shapeSystem.LoadTargetFromCurrentOrder();
    SetState(GameState.Shape);
}

public void GoToGlaze()
{
    if (!CanAutoAdvanceNow()) return;
    if (glazeSystem != null) glazeSystem.LoadTargetFromCurrentOrder();
    SetState(GameState.Glaze);
}

public void GoToFiring()
{
    if (!CanAutoAdvanceNow()) return;
    if (firingSystem != null) firingSystem.StartFiring();
    SetState(GameState.Firing);
}

public void GoToResult()
{
    if (!CanAutoAdvanceNow())
    {
        NotifyModuleCompletedForBridge();
        return;
    }

    if (resultPanelController != null) resultPanelController.ShowResult();
    advanceOrderOnNextStart = true;
    SetState(GameState.Result);
}
```

### 3.4 为什么要让 Phase3 发事件

因为 Phase3 才知道：

- 当前模块何时真正完成
- 结果页面何时触发退出
- 哪个按钮是“完成当前模块”的标准出口

Bridge 不应该猜测这些状态。  
Bridge 只应该接收事件并执行恢复。

### 3.5 Result 模块的特殊性

Result 是所有模块的终点，也是 Bridge 退出 Phase3 的最常见出口。  
因此：

- Result 既是结果页，也是退出出口
- 在 Phase6 桥接模式下，Result 不应继续自动发起新订单的原生串联
- Result 的“退出按钮”应作为 `OnModuleCompletedForBridge` 的触发来源

---

## 4. 数据跨模块持久化

### 4.1 为什么需要常驻数据

Phase6 桥接模式下，Phase3 的模块可能被多次进入：

- Order
- Shape
- Glaze
- Firing

如果每次进入都重新创建整个 Phase3 宿主对象，会产生：

- 状态丢失
- 订阅丢失
- 引用重建成本
- UI 与系统不同步

因此需要“常驻宿主 + 模块会话”结构。

### 4.2 常驻方案

推荐保留以下常驻对象：

- `GameplayBridgeManager`
- `Phase3RuntimeContext`
- `Phase3ModuleHost`
- `GameplayModuleSession`

其中：

- `GameplayBridgeManager` 负责会话调度
- `Phase3RuntimeContext` 负责运行模式和共享上下文
- `Phase3ModuleHost` 负责承载 Phase3 模块实例
- `GameplayModuleSession` 负责一次模块进入/退出过程的数据

### 4.3 不常驻的对象

不建议常驻：

- 单次模块 UI 的临时显隐状态
- 单次交互的输入遮罩状态
- 单次结果面板上的临时动画状态

这些属于会话内状态，应随 `Session` 销毁而销毁。

### 4.4 Result 模块在 Phase6 的特殊处理

Result 模块有两类职责：

1. 展示本次订单结果
2. 作为桥接退出点

在 Phase6 桥接模式下，Result 不应直接驱动新一轮 Phase3 原生循环。  
它应该：

- 记录结果
- 暴露完成事件
- 通知 Bridge 关闭当前会话
- 恢复 Phase6 地图控制

---

## 5. 场景架构决策

### 5.1 推荐方案 A：Phase3 模块 Prefab -> BridgeCanvas 实例化

这是当前推荐方案。

#### 结构

```text
Phase6 Scene
├─ WorldRoot
├─ LogicRoot
├─ ArtRoot
├─ BridgeRoot
│  ├─ GameplayBridgeManager
│  ├─ Phase3RuntimeContext
│  └─ BridgeCanvas
│     ├─ Order Module Prefab Instance
│     ├─ Shape Module Prefab Instance
│     ├─ Glaze Module Prefab Instance
│     ├─ Firing Module Prefab Instance
│     └─ Result Module Prefab Instance
└─ Workstation Roots
```

#### 优点

- 不依赖多场景加载
- 方便统一控制 UI
- 方便维护常驻上下文
- 调试成本低
- 对小团队最友好

#### 风险

- 需要确保模块 Prefab 的引用完整
- 需要明确哪些对象是常驻宿主，哪些是会话对象

### 5.2 方案 B：Additive 场景加载

#### 优点

- 理论上模块边界更强
- 适合大型项目拆分

#### 缺点

- 场景切换成本高
- 引用复杂
- 调试困难
- 容易出现重复管理器
- 会让当前原型阶段复杂度过高

### 5.3 结论

当前阶段推荐 **方案 A**。  
因为你要的是“稳定跑通、少改旧文件、方便后续开发”，不是先做最重的场景架构。

---

## 6. 完整时序流程

## 6.1 模块进入流程（9 步）

```text
1. 玩家移动到 Phase6 站点
2. 玩家按下 E
3. InputManager 将输入交给 InteractionController
4. InteractionController 确认当前工作站可交互
5. Workstation 将 AreaType 交给 GameplayBridgeManager
6. GameplayBridgeManager 创建 GameplayModuleSession
7. BridgeInputLock 锁定 Phase6 输入
8. BridgeCanvasController 打开对应 Phase3 模块
9. Phase3ModuleAdapter 设置 RuntimeMode = Phase6Bridge 并启动模块
```

## 6.2 模块运行中约束

模块运行期间，必须满足：

- 只允许当前模块的 UI 操作
- 只允许当前模块的数据更新
- 不允许再次 E 打开别的站点
- 不允许 Phase6 地图移动
- 不允许模块自动进入下一阶段
- 不允许 Phase3 直接访问 Phase6

## 6.3 模块完成退出流程（10 步）

```text
1. 模块内部完成条件达成
2. Phase3 发出 OnModuleCompletedForBridge
3. Bridge 接收到完成事件
4. Bridge 判断当前 Session 可退出
5. BridgeCanvasController 关闭对应模块
6. Phase3ModuleAdapter 解绑当前模块事件
7. GameplayBridgeManager 清理模块运行状态
8. BridgeInputLock 释放 Phase6 输入
9. GameplayModuleSession Dispose
10. Phase6 恢复移动与探索
```

## 6.4 异常退出流程

异常退出包括：

- 模块引用丢失
- UI 预制体未加载
- 运行时空引用
- 用户强制退出
- 会话状态异常

处理方式：

```text
1. 捕获异常或检测异常状态
2. Bridge 终止当前 Session
3. 关闭当前模块 UI
4. 解绑所有模块事件
5. 释放输入锁
6. 恢复 Phase6 状态
7. 记录错误日志
8. 允许玩家重新进入同一区域
```

---

## 7. 核心对象设计

### 7.1 `GameplayBridgeManager` 伪代码

```csharp
public class GameplayBridgeManager : MonoBehaviour
{
    [SerializeField] private Phase3RuntimeContext runtimeContext;
    [SerializeField] private BridgeInputLock inputLock;
    [SerializeField] private BridgeCanvasController canvasController;
    [SerializeField] private Phase3ModuleAdapter moduleAdapter;

    private GameplayModuleSession currentSession;
    private bool isBusy;

    public void EnterArea(AreaType areaType, Workstation workstation)
    {
        if (isBusy) return;
        currentSession = CreateSession(areaType, workstation);
        StartSession(currentSession);
    }

    private GameplayModuleSession CreateSession(AreaType areaType, Workstation workstation)
    {
        return new GameplayModuleSession
        {
            RuntimeMode = RuntimeMode.Phase6Bridge,
            AreaType = areaType,
            Workstation = workstation,
            CanAutoAdvance = false,
            IsInputLocked = true,
            IsUIVisible = false
        };
    }

    private void StartSession(GameplayModuleSession session)
    {
        isBusy = true;
        inputLock.LockPhase6();
        canvasController.OpenModule(session.AreaType);
        moduleAdapter.BindSession(session);
        moduleAdapter.OpenModule(session.AreaType);
    }

    public void CompleteSession()
    {
        if (currentSession == null) return;
        moduleAdapter.CloseModule();
        canvasController.CloseCurrentModule();
        inputLock.UnlockPhase6();
        currentSession.Dispose();
        currentSession = null;
        isBusy = false;
    }

    public void AbortSession(string reason)
    {
        Debug.LogWarning($"[GameplayBridgeManager] AbortSession: {reason}");
        CompleteSession();
    }
}
```

### 7.2 `GameplayModuleSession` 字段定义

```csharp
public class GameplayModuleSession
{
    public RuntimeMode RuntimeMode;
    public BridgeModuleType ModuleType;
    public AreaType AreaType;
    public Workstation Workstation;
    public bool IsInputLocked;
    public bool IsUIVisible;
    public bool CanAutoAdvance;
    public bool IsCompleted;

    public void MarkCompleted()
    {
        IsCompleted = true;
    }

    public void Dispose()
    {
        Workstation = null;
        IsCompleted = false;
        IsUIVisible = false;
        IsInputLocked = false;
    }
}
```

### 7.3 `BridgeInputLock` 锁定范围

锁定范围建议包括：

- `InputManager`
- `MovementController`
- `InteractionController`
- `PlayerCharacter` 的移动输入
- Phase6 的点击地面移动

不建议锁定：

- 摄像机渲染
- 场景静态对象
- Phase3 模块内部自己的 UI 输入

### 7.4 完整文件清单

#### 新建 10 个

1. `Phase3RuntimeContext.cs`
2. `GameplayModuleSession.cs`
3. `GameplayBridgeManager.cs`
4. `Phase3ModuleAdapter.cs`
5. `BridgeInputLock.cs`
6. `BridgeCanvasController.cs`
7. `BridgeUIExitRelay.cs`
8. `IGameplayBridgeModule.cs`
9. `BridgeModuleType.cs`
10. `BridgeSessionLogger.cs`（可后置，非首发必需）

#### 改动 3 个

1. `Assets/Phase3/Scripts/Core/GameManager.cs`
2. `Assets/Phase3/Scripts/UI/ResultPanelController.cs`
3. `Assets/Phase6/Scripts/Systems/InputManager.cs`

#### 不动 N 个

原则上不动以下模块：

- `ShapeSystem`
- `GlazeSystem`
- `FiringSystem`
- `ResultSystem`
- `OrderManager`
- Phase6 的移动/区域/工作站核心结构

---

## 8. 执行计划与验收

### 8.1 12 步执行顺序

1. 创建 `RuntimeMode` 与 Session 结构
2. 新建 `GameplayModuleSession`
3. 新建 `GameplayBridgeManager`
4. 新建 `Phase3ModuleAdapter`
5. 新建 `BridgeInputLock`
6. 新建 `BridgeCanvasController`
7. 新建 `BridgeUIExitRelay`
8. 新建 `IGameplayBridgeModule`
9. 新建 `BridgeModuleType`
10. 在 `GameManager` 加最小门控
11. 在 `ResultPanelController` 接入完成事件
12. 在 `InputManager` 把 E 键交给 Bridge

### 8.4 实施优先级补充

为了降低首轮联调风险，建议采用“先打通一条链路，再扩展完整四段”的策略：

#### 第一阶段：只验证 Runtime 主链路

优先验证：

- `Order -> Shape`
- Session 生命周期
- 输入锁
- UI 打开与关闭
- 自动推进阻断
- 回到 Phase6 后恢复移动

这一阶段的目标不是一次性覆盖全部模块，而是先确认：

- Bridge 能否稳定接管 Phase3
- Phase3 能否在桥接模式下停止自动串联
- Phase6 能否在退出后可靠恢复

#### 第二阶段：扩展完整玩法链路

在第一阶段稳定后，再依次扩展：

- `Glaze`
- `Firing`
- `Result`

#### 第三阶段：体验层完善

最后再补充：

- Fade
- 动画
- Camera
- 音效
- 演出
- 提示

#### 可后置内容

以下内容不建议阻塞首发联调：

- `BridgeSessionLogger`
- Analytics
- Runtime 统计
- 通用模块注册系统
- 高级 Runtime 框架
- 泛化事件总线

这类内容更适合作为二期增强项，而不是首轮稳定性验证的前置条件。

### 8.5 Go / No-Go 验收标准

以下条件全部满足后，才认为本桥接方案达到可继续扩展的状态：

#### Go 标准

- Phase6 四个区域能分别进入对应 Phase3 模块
- 每次只进入一个模块
- Bridge 模式下 Phase3 不再自动进入下一模块
- Result 退出后能恢复 Phase6 移动与探索
- Phase3 不直接引用 Phase6

#### No-Go 标准

- 出现重复 UI 打开
- 出现输入锁死
- 出现 Session 串台
- 出现 Phase3 自动串联未阻断
- 出现 Result 退出重复触发
- 出现 Phase3 -> Phase6 反向依赖

### 8.2 预计工时

在当前原型规模下，建议估算：

- 架构落地：2~4 小时
- 引用绑定：1~2 小时
- PlayMode 验证：1~2 小时
- 修复边界问题：1~2 小时

总计建议预留：

**5~10 小时**

### 8.3 三维验收标准

#### 功能验收
- Phase6 四个区域分别进入对应 Phase3 模块
- 每次只进入一个模块
- 模块完成后不自动推进下一模块
- 回到 Phase6 后能继续移动和探索

#### 稳定验收
- 无 `NullReferenceException`
- 无重复 UI 打开
- 无输入锁死
- 无 Session 泄漏
- 无 Phase3 直接依赖 Phase6

#### 数据验收
- Phase3 常驻数据在多次进入模块后仍稳定
- 当前订单和模块结果不会因重新进入而丢失
- Result 模块能稳定作为退出出口

---

## 9. 风险与对策

### 风险 1：Phase3 自动串联仍然生效

对策：
- 必须在 `GameManager` 中加入 `RuntimeMode` 门控
- 必须在模块推进路径处加入 `CanAutoAdvanceNow()`
- 必须由 Phase3 发出完成事件，而不是 Bridge 猜测状态

### 风险 2：常驻对象过多导致状态串台

对策：
- 只有 Bridge 管理器和运行上下文常驻
- 单次模块的 UI/输入状态放入 Session
- Session 退出即清理

### 风险 3：Result 模块和退出逻辑冲突

对策：
- 明确 Result 是“结果页 + 退出出口”
- Result 的完成按钮直接触发 `OnModuleCompletedForBridge`

### 风险 4：适配器变成第二个 GameManager

对策：
- Adapter 只能做翻译和绑定
- 不允许写流程决策
- 不允许决定下一模块

---

## 10. 最终结论

这份 v2.0 设计已经收敛到一个可实施的最小稳定形态：

> **Single Entry + Session Control + Minimal Intrusion**

它的核心不是“把 UI 打开”，而是“把 Phase3 模块作为 Bridge 管理下的运行时实例进入”。  
这样可以同时满足：

- Phase6 世界探索节奏
- Phase3 模块完整功能
- 自动串联阻断
- 数据持久化
- 最小侵入
- 后续可扩展性

这就是当前阶段最稳的正式架构。
