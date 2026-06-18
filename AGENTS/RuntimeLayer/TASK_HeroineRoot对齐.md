# Task: 女主 HeroineRoot 脚底对齐(文档化现有工具)

## 当前执行

Task HeroineRoot对齐 — DESIGN(等待审批)

## 目标

把 Phase9 世界场景中的 `女主` 重构为 `HeroineRoot (父) + GirlModel (子)` 结构,使:

- `HeroineRoot` 是真正走路的逻辑点,pivot 在脚底地面,挂 NavMeshAgent + 移动脚本
- `GirlModel` 是纯视觉层,挂 SpriteRenderer + Animator,可在 HeroineRoot 下微调位置
- NavMeshAgent 永远在蓝色 NavMesh 区域内,GirlModel 负责显示

**本任务不新写代码,直接使用已存在的 Editor 工具 `Phase9FixPlayerVisibility.cs` 完成。**

---

## 现有工具盘点

| 项 | 值 |
|---|---|
| 工具脚本 | `Assets/Phase9/Scripts/Editor/Phase9FixPlayerVisibility.cs` |
| 菜单入口 | `Phase9 > Fix > Heroine Root Restructure` |
| 目标场景 | `Assets/Phase9/Scenes/SampleScene.unity` |
| 目标对象 | 场景中名为 `女主` 的 GameObject |
| 幂等性 | 已存在 `HeroineRoot > GirlModel` 时自动跳过(`:39-44`) |

### 工具已实现的行为(对照贴出的 7 步)

| 手动步骤 | 工具实现 | 行号 |
|---|---|---|
| 1. 新建 HeroineRoot 放脚底 | 创建 HeroineRoot,position = 女主当前位置 | `:59-63` |
| 2. 模型拖到 HeroineRoot 下 | SetParent(heroineRoot),重命名为 GirlModel | `:66-67` |
| 3. 调 GirlModel 本地位置 | localPosition=Vector3.zero,localEuler.x=90 面向相机,保留 localScale | `:70-72` |
| 4. NavMeshAgent 挂到 HeroineRoot | MoveComponent\<NavMeshAgent\> 从 GirlModel 移到 HeroineRoot | `:75` |
| 5. 移动脚本控制 HeroineRoot | PlayerCharacter + MovementController 也移过去 | `:78-79` |
| 6. 调 NavMeshAgent 参数 | updatePosition=false, updateRotation=false, baseOffset=0 | `:85-87` |
| 7. (额外) 恢复 SpriteRenderer/Animator | 若被误删,从 `Assets/人物素材/女主/正面.png` 恢复 | `:136-165` |

工具还会清理旧 `VisualRoot` 方案残留(`:103-134`),保证新旧切换不冲突。

---

## Step 1 - Design

### 1. Files Created

```
NONE
（Phase9FixPlayerVisibility.cs 已存在,本任务不新增任何文件）
```

### 2. Files Modified

| 文件路径 | 修改内容 | 改动者 |
|---|---|---|
| `Assets/Phase9/Scenes/SampleScene.unity` | 场景层级变更:新增 HeroineRoot,女主本体改名 GirlModel 并成为其子物体,组件迁移 | 运行菜单命令时由工具修改 |

### 3. Data Flow

**重构前:**

```text
女主 (GameObject)
  ├─ Transform (pivot 可能在身体中心,不在脚底)
  ├─ NavMeshAgent
  ├─ PlayerCharacter
  ├─ MovementController
  ├─ SpriteRenderer
  └─ Animator
```

**重构后:**

```text
HeroineRoot (GameObject)            ← 逻辑层,pivot 在脚底地面
  ├─ Transform position = 女主原脚底世界位置
  ├─ NavMeshAgent (updatePosition=false, updateRotation=false, baseOffset=0)
  ├─ PlayerCharacter                 ← 移动入口,agent 本地取
  ├─ MovementController              ← 手动路径移动,改 transform.position
  └─ GirlModel (原"女主")            ← 视觉层
       ├─ localPosition = 0,0,0
       ├─ localEuler = 90,0,0        ← Sprite 面向相机
       ├─ localScale = 原女主 scale
       ├─ SpriteRenderer
       └─ Animator
```

**移动数据流(重构后不变):**

```text
Phase9InteractionBridge.HandleWorldClickMove
  -> TryResolveMoveTarget(screenPos, out navTarget)
  -> playerCharacter.SetDestination(navTarget)
     -> movementController.SetDestination(target)
        -> NavMesh.SamplePosition + CalculatePath(PathComplete)
        -> 每帧 Vector3.MoveTowards 改 HeroineRoot.transform.position
        -> 同步 navMeshAgent.nextPosition
```

HeroineRoot 移动 → GirlModel 作为子物体跟随 → 视觉表现正确。
NavMeshAgent 永远贴在 HeroineRoot 上,不会因 Sprite pivot 偏移而漂出蓝区。

### 4. Acceptance Strategy

**验收标准1 — 层级结构正确**
- 场景中存在 `HeroineRoot`,其下有且仅有一个子物体 `GirlModel`(原 `女主`)

**验收标准2 — 组件挂载位置正确**
- `HeroineRoot` 上挂:NavMeshAgent、PlayerCharacter、MovementController
- `GirlModel` 上挂:SpriteRenderer、Animator
- `GirlModel` 上**不应**再有 NavMeshAgent/PlayerCharacter/MovementController

**验收标准3 — NavMeshAgent 参数正确**
- `updatePosition = false`
- `updateRotation = false`
- `baseOffset = 0`
- `radius / height` 与 `Phase9SmallAgent` 约定一致(radius=0.06, height=0.5,见 `role-mobile.md`)

**验收标准4 — 视觉朝向正确**
- `GirlModel.localEulerAngles.x = 90`(Sprite 面向相机)
- Play Mode 下女主 Sprite 可见,不侧躺、不背对相机

**验收标准5 — 移动功能正常**
- Play Mode 下点击蓝色 NavMesh 区域,HeroineRoot 移动,GirlModel 跟随
- 点击蓝区外,女主不移动
- 女主不漂出 NavMesh 覆盖范围

**验收标准6 — 幂等**
- 重复执行菜单命令,工具输出 "HeroineRoot already exists with GirlModel. Skip.",不产生重复结构

---

## 执行步骤(审批后)

1. 打开场景 `Assets/Phase9/Scenes/SampleScene.unity`
2. 确认场景中存在名为 `女主` 的 GameObject
3. 执行菜单 `Phase9 > Fix > Heroine Root Restructure`
4. 查看 Console,确认 `[Phase9FixPlayerVisibility] Done. HeroineRoot=... GirlModel localEuler=...`
5. 在 Hierarchy 中按验收标准 1-3 逐项核对
6. 进入 Play Mode,按验收标准 4-5 验证视觉与移动
7. 保存场景

---

## Risks

| 风险 | 可能性 | 说明 | 缓解措施 |
|------|--------|------|---------|
| **命名冲突(已知)** | 高 | `PlayerCharacter.cs:24-25` 有 `transform.Find("LogicRoot")` / `transform.Find("ArtRoot")` fallback,但工具创建的子物体叫 `GirlModel`,fallback 会失败。CODEBUDDY.md 里 Phase6 约定也是 LogicRoot/ArtRoot。 | 当前 `logicRoot`/`artRoot` 字段在 PlayerCharacter 可见代码路径中未被使用,暂不致功能故障。**建议后续单独起 Task 统一命名**:要么改工具为 LogicRoot/ArtRoot,要么删掉 PlayerCharacter 里的 Find fallback。**本任务不处理。** |
| NavMeshAgent 参数需复核 | 中 | 工具只设了 updatePosition/updateRotation/baseOffset,radius/height 需手动核对 `role-mobile.md` 约定(0.06/0.5) | 执行后在 Inspector 核对,不符则手动改 |
| GirlModel localPosition 需微调 | 中 | 工具设为 (0,0,0),若模型 pivot 不在脚底,视觉上会偏离 | 若脚没踩在 HeroineRoot 上方,手动调 GirlModel.localPosition.y |
| 场景文件被修改 | 高 | 工具会修改 SampleScene.unity 层级 | 执行前确保场景已提交或备份,执行后保存并 review diff |

---

## Next Recommended Task

- **命名统一**:把 Phase9 的 `HeroineRoot/GirlModel` 与 Phase6 的 `LogicRoot/ArtRoot` 约定统一,修正 `PlayerCharacter.cs` 的 Find fallback 或改 `Phase9FixPlayerVisibility.cs` 命名。这是已知的技术债,应在接入更多角色系统前处理。

---

## 等待审批

以上为 Design,未执行任何场景修改或代码改动。

审批通过后按"执行步骤"操作。
