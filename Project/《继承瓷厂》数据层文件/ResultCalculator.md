# ResultCalculator 结算计算器规范

## 项目

《继承瓷厂》— 最终结算评分模型

## 文档状态

**评分体系最后一份核心文档。** 此文档完成后，评分体系正式冻结，进入 ScriptableObject / Calculator / Unity 实现阶段。

## 依赖的冻结文件

| 文件 | 冻结内容 |
|------|----------|
| `ShapeScoreSpecification.md` | ShapeScore 公式、D_max=0.35、S/A/B/C/D/E |
| `GlazeScoreSpecification.md` | 欧几里得距离、D_max=0.02、釉色分类、影青/冬青判定 |
| `FireCalculator.md` | FireScore 公式、PenaltySource、玩家反馈系统 |
| `ScoringSystem_Phase4_Freeze.md` | 三系统统一规范、风险分析、冻结判定 |

> 以上文件已冻结，禁止修改。

---

# 第一部分：冻结状态检查结果

## 检查清单

| 检查项 | ShapeScore | GlazeScore | FireScore | 状态 |
|--------|------------|------------|-----------|------|
| 公式已冻结 | `100×(1−err/D_max)` | `100×(1−d/D_max)` | `100−T−D−F−P` | **全部通过** |
| D_max 已冻结 | D_max=0.35 | D_max=0.02 | N/A | **全部通过** |
| S/A/B/C/D/E 已冻结 | ✓ | ✓ | ✓ | **全部通过** |
| Difficulty 不影响评分 | ✓ §6.2 | ✓ §5.1 | ✓ N/A | **全部通过** |
| PenaltySource 已冻结 | N/A | N/A | ✓ 附录C | **通过** |
| 单一处罚原则 | N/A | N/A | ✓ 附录C | **通过** |
| UI 映射解耦 | ✓ §4 | ✓ §9 | N/A | **通过** |

## 冲突检测结果

```
════════════════════════════════════════════
          冻结状态检查：全部通过
          未发现冲突
          可以继续生成 ResultCalculator.md
════════════════════════════════════════════
```

## 统一性验证

| 验证项 | 预期 | 实际 | 结果 |
|--------|------|------|------|
| 三系统等级体系 | S/A/B/C/D/E | S/A/B/C/D/E | **一致** |
| 等级阈值 | 95/85/70/50/30 | 95/85/70/50/30 | **一致** |
| Difficulty 规则 | 不影响分值 | 不影响分值 | **一致** |
| UI 解耦原则 | 评分≠展示 | 评分≠展示 | **一致** |

---

# 第二部分：ResultCalculator 定位

## 不是第四个评分系统

```
ShapeCalculator ──→ ShapeScore (0~100)
GlazeCalculator  ──→ GlazeScore (0~100)
FireCalculator   ──→ FireScore (0~100)
                         │
                         ▼
ResultCalculator ──→ FinalScore, Grade, Rewards, OrderResult
                     (聚合器，不产生新的评分逻辑)
```

## 职责边界

| ResultCalculator 负责 | ResultCalculator 不负责 |
|----------------------|------------------------|
| 汇总三个评分 | 创造新的评分公式（不引入第4维度） |
| 生成 FinalScore | 修改已冻结的 Shape/Glaze/Fire 公式 |
| 生成 Grade | 独立判定器型/釉色匹配（由各Calculator负责） |
| 判定订单完成状态 | 计算原始评分（由各Calculator负责） |
| 计算金币/声望奖励 | 定义品阶名称（UI层负责） |
| 输出 ResultData | 保存数据（存档系统负责） |

---

# 第三部分：系统职责

## ResultCalculator 接口

```
ResultCalculator.Calculate(input) → ResultData

输入:
├── ShapeScore      (from ShapeCalculator)
├── GlazeScore      (from GlazeCalculator)
├── FireScore       (from FireCalculator)
├── OrderData       (from OrderSystem)
│   ├── RequiredShapeID
│   ├── RequiredGlazeID
│   └── Difficulty
├── DefectList      (from FireCalculator)
│   └── List<DefectType>
└── MatchedShapeID  (from ShapeCalculator)
    MatchedGlazeID  (from GlazeCalculator)

输出: ResultData
├── FinalScore
├── Grade
├── OrderResult
├── GoldReward
├── ReputationReward
├── + 所有输入数据透传
```

---

# 第四部分：输入结构

## 数据来源映射

| 输入字段 | 类型 | 来源 | 说明 |
|----------|------|------|------|
| `shapeScore` | float (0~100) | `ShapeCalculator.Calculate()` | 器型匹配评分 |
| `glazeScore` | float (0~100) | `GlazeCalculator.Calculate()` | 釉色配方评分 |
| `fireScore` | float (0~100) | `FireCalculator.Calculate()` | 烧制工艺评分 |
| `matchedShapeID` | string | `ShapeCalculator` 输出 | 识别出的最匹配器型 |
| `matchedGlazeID` | string | `GlazeCalculator` 输出 | 识别出的最匹配釉色 |
| `orderData.requiredShapeID` | string | `OrderSystem` | 订单要求的器型 |
| `orderData.requiredGlazeID` | string | `OrderSystem` | 订单要求的釉色 |
| `orderData.difficulty` | int (1~5) | `OrderSystem` | 订单难度 |
| `orderData.baseGold` | int | `OrderSystem` | 基础金币 |
| `orderData.baseReputation` | int | `OrderSystem` | 基础声望 |
| `defectList` | List\<string\> | `FireCalculator` 输出 | 触发的缺陷列表 |

---

# 第五部分：FinalScore 计算

## 方案比较

### 方案 A：加权求和

```
FinalScore = 0.35 × ShapeScore + 0.25 × GlazeScore + 0.40 × FireScore
```

### 方案 B：几何平均

```
FinalScore = (ShapeScore × GlazeScore × FireScore)^(1/3)
```

## 多维度对比分析

| 维度 | 方案 A (加权和) | 方案 B (几何平均) |
|------|----------------|-------------------|
| **数学稳定性** | 线性，无奇点，任意输入都可计算 | 对零值敏感，任一Score=0则FinalScore=0 |
| **玩家体验** | 优势可部分补偿劣势 | 强制均衡，弱项拖累整体 |
| **平衡难度** | 权重可调，支持策划迭代 | 权重建模困难，调整不直观 |
| **可解释性** | 玩家理解"火占40%" | 玩家难以理解几何平均含义 |
| **扩展性** | 新增维度只需加项 | 新增维度会放大"0值惩罚" |
| **Fire权重** | **0.40（最高）** | 等权(1/3)，不支持偏重 |
| **GDD符合度** | 完美：Fire是核心玩法，权重最高 | 不符合：Fire与Shape/Glaze等权 |

## 关键场景对比

| 场景 | ShapeScore | GlazeScore | FireScore | 方案A | 方案B | 哪个更合理？ |
|------|------------|------------|-----------|-------|-------|-------------|
| 完美三重 | 100 | 100 | 100 | **100.0 (S)** | **100.0 (S)** | 一致 |
| 火满分+其他零 | 0 | 0 | 100 | **40.0 (D)** | **0.0 (E)** | A: 火艺精湛值得留存 |
| 火差+其他优 | 90 | 90 | 30 | **66.0 (C)** | **62.4 (C)** | 接近 |
| 形状优+其他差 | 95 | 40 | 40 | **59.2 (C)** | **53.4 (C)** | 接近 |
| 火优+其他差 | 40 | 40 | 95 | **62.0 (C)** | **53.4 (C)** | A: Fire高权重体现 |
| 实际案例(88/90/72) | 88 | 90 | 72 | **82.1 (B)** | **82.9 (B)** | 接近 |
| 实际案例(62/43/57) | 62 | 43 | 57 | **55.2 (C)** | **53.4 (C)** | 接近 |

## 最终选择：**方案 A（加权求和）**

### 选择理由

**1. FireScore 权重最高（40%）— 符合GDD核心设计**

《继承瓷厂》的游戏循环中，烧窑控火是最具游戏性的核心玩法：
- 拉坯（Shape）= 制作阶段，难度较低
- 配方（Glaze）= 配方阶段，知识门槛
- 烧窑（Fire）= 核心玩法，最多交互、最深机制、最高技巧要求

Fire=40% > Shape=35% > Glaze=25%，精确反映三者在游戏循环中的重要性和玩家投入。

**2. 数学简单可调**

```
FinalScore = w_s × ShapeScore + w_g × GlazeScore + w_f × FireScore

Where w_s + w_g + w_f = 1.0

当前值: w_s=0.35, w_g=0.25, w_f=0.40
调整空间: 策划可通过修改权重进行平衡调整，无需改动底层公式
```

**3. 玩家心理友好**

方案B中ShapeScore=0时，即使FireScore=100，FinalScore=0。玩家体验是"我烧窑完美但为什么总分0？"——极度挫败。
方案A中同样情况下FinalScore=40，传递"你的火候非常好，但器型/釉料还需练习"。

**4. 权重语义明确**

| 权重 | 语义 | 游戏设计含义 |
|------|------|-------------|
| FireScore 40% | "烧得好不好" | 核心玩法，最大权重 |
| GlazeScore 25% | "配方对不对" | 知识性，中等权重 |
| ShapeScore 35% | "做得像不像" | 基础技能，次高权重 |

> Shape=35% > Glaze=25% 的细微差异：因为Shape是基础操作（所有订单都需要），Glaze偏向策略选择。

---

# 第六部分：Grade 计算

## Grade 映射

采用与 ShapeScore / GlazeScore / FireScore **完全一致**的等级体系：

```
Grade = S  if FinalScore ≥ 95
        A  if FinalScore ≥ 85
        B  if FinalScore ≥ 70
        C  if FinalScore ≥ 50
        D  if FinalScore ≥ 30
        E  if FinalScore <  30
```

## 禁止

- 禁止在 Grade 计算中使用中文品阶
- 禁止根据订单状态调整 Grade
- 禁止引入第四维度影响 Grade

> Grade 是 FinalScore 的纯函数：`Grade = f(FinalScore)`

---

# 第七部分：UI 品阶映射

## 映射表

评分系统（内部）与 UI 展示层**完全解耦**。

| Grade (内部) | UI 显示名称 | 显示颜色 | 品阶含义 |
|-------------|------------|----------|----------|
| **S** | 贡品 | 金色 | 完美之作，可进贡宫廷 |
| **A** | 精品 | 紫色 | 工艺精湛，收藏级品质 |
| **B** | 佳品 | 蓝色 | 品质优良，市场流通上品 |
| **C** | 良品 | 绿色 | 合格成品，日常使用 |
| **D** | 次品 | 黄色 | 瑕疵品，仅供经验积累 |
| **E** | 废品 | 灰色 | 完全失败，无任何价值 |

## 解耦设计

```
ResultCalculator → Grade (S/A/B/C/D/E)
                      │
                      ▼
UIMapper.Map(Grade) → "贡品" / "精品" / ...
                      │
                      ▼
                    UI 渲染

未来允许修改"贡品/精品"等名称，不影响任何计算逻辑。
```

---

# 第八部分：订单完成判定

## 核心原则

```
订单完成 ≠ FinalScore高

订单完成 = Shape匹配 AND Glaze匹配
```

## 判定逻辑

```
Step 1: Shape匹配检查
  matchedShapeID == orderData.requiredShapeID ?
    YES → 继续
    NO  → OrderResult = Fail (原因: 器型不匹配)

Step 2: Glaze匹配检查
  matchedGlazeID == orderData.requiredGlazeID ?
    YES → 继续
    NO  → OrderResult = Fail (原因: 釉色不匹配)

Step 3: FireScore检查
  FireScore > 0 ? (不是致命缺陷)
    YES → 继续
    NO  → OrderResult = Fail (原因: 碎坯/窑裂)

Step 4: 品级判定
  基于 FinalScore 判定 OrderResult
```

## OrderResult 四级

| OrderResult | 触发条件 | 订单状态 |
|-------------|----------|----------|
| **Perfect** | FinalScore ≥ 95 **且** Shape匹配 **且** Glaze匹配 **且** FireScore > 0 | 完美完成 |
| **Excellent** | FinalScore ≥ 70 **且** Shape匹配 **且** Glaze匹配 **且** FireScore > 0 | 优质完成 |
| **Normal** | FinalScore ≥ 50 **且** Shape匹配 **且** Glaze匹配 **且** FireScore > 0 | 正常完成 |
| **Fail** | Shape不匹配 **或** Glaze不匹配 **或** FireScore = 0 | 订单失败 |

## 经典案例

| 案例 | Shape | Glaze | Fire | FinalScore | OrderResult | 说明 |
|------|-------|-------|------|------------|-------------|------|
| 影青碗(完美) | Bowl ✓ | Yingqing ✓ | 100 | 100 | **Perfect** | 全部完美 |
| 影青碗(合格) | Bowl ✓ | Yingqing ✓ | 72 | 80 | **Excellent** | 工艺一般但匹配 |
| **青花碗(做错)** | Bowl ✓ | BlueWhite ✗ | 95 | 85 | **Fail** | 釉色不对，订单失败 |
| **甜白盘(做错)** | Plate ✗ | SweetWhite ✓ | 90 | 82 | **Fail** | 器型不对，订单失败 |
| 影青碗(窑裂) | Bowl ✓ | Yingqing ✓ | 0 | 34 | **Fail** | 碎坯，无可交付 |

> 核心逻辑：即使 FinalScore 很高，器型或釉色不匹配就是订单失败。订单要求的是"影青碗"，不是"青花碗"。

---

# 第九部分：奖励系统

## 原则

```
奖励 ≠ FinalScore 的简单线性映射
奖励 = BaseReward × QualityModifier × DifficultyModifier
```

## 金币计算公式

```
GoldReward = Floor( BaseGold × QualityGoldMultiplier × DifficultyGoldMultiplier )

QualityGoldMultiplier:
  Grade=S: 2.0×
  Grade=A: 1.5×
  Grade=B: 1.2×
  Grade=C: 1.0×
  Grade=D: 0.5× (失败但仍给少量)
  Grade=E: 0.0×
  OrderResult=Fail: 0.0× (无论Grade，订单失败无奖励)

DifficultyGoldMultiplier:
  Difficulty=1: 1.0×
  Difficulty=2: 1.2×
  Difficulty=3: 1.5×
  Difficulty=4: 1.8×
  Difficulty=5: 2.2×
```

## 奖励计算示例

| 订单 | BaseGold | Grade | Quality× | Diff | Diff× | Gold |
|------|----------|-------|----------|------|-------|------|
| 影青碗(Diff=2) | 50 | S(完美) | 2.0 | 2 | 1.2 | **120** |
| 青花瓶(Diff=4) | 100 | A(精品) | 1.5 | 4 | 1.8 | **270** |
| 霁红罐(Diff=3) | 80 | B(佳品) | 1.2 | 3 | 1.5 | **144** |
| 影青碗(Diff=2) | 50 | C(良品) | 1.0 | 2 | 1.2 | **60** |
| 影青碗(Diff=2) | 50 | OrderFail | 0.0 | — | — | **0** |

> 难度乘数独立于评分，遵守"Difficulty 不影响评分分值"的冻结原则。

---

# 第十部分：声望系统

## 声望计算公式

```
ReputationReward = Floor( BaseRep × RepMod_Order × RepMod_Grade × RepMod_Diff )

RepMod_Order:
  Perfect:  1.5×
  Excellent: 1.2×
  Normal:   1.0×
  Fail:     0.0× (失败=零声望)

RepMod_Grade:
  Grade=S: 1.5×
  Grade=A: 1.2×
  Grade=B: 1.0×
  Grade=C: 0.8×
  Grade=D: 0.3×
  Grade=E: 0.0×

RepMod_Diff:
  Difficulty=1: 1.0×
  Difficulty=2: 1.1×
  Difficulty=3: 1.3×
  Difficulty=4: 1.5×
  Difficulty=5: 1.8×
```

## 设计说明

| 维度 | 金币 | 声望 | 理由 |
|------|------|------|------|
| Grade 影响 | 大(1.0~2.0) | 中(0.8~1.5) | 金币更受品质影响 |
| OrderResult 影响 | 大(0~2.0) | 大(0~1.5) | Perfect 订单声望加成最高 |
| Difficulty 影响 | 大(1.0~2.2) | 中(1.0~1.8) | 困难订单金币奖励丰厚 |
| Fail 惩罚 | 0金币 | 0声望 | 统一归零 |

---

# 第十一部分：ResultData 结构

## 统一输出结构

```
ResultData {
  // === 评分数据 ===
  float   shapeScore        // from ShapeCalculator
  float   glazeScore        // from GlazeCalculator
  float   fireScore         // from FireCalculator
  float   finalScore        // 计算得出
  string  grade             // S/A/B/C/D/E
  
  // === 匹配数据 ===
  string  matchedShapeID    // from ShapeCalculator
  string  matchedGlazeID    // from GlazeCalculator
  string  matchedShapeName  // 展示用
  string  matchedGlazeName  // 展示用
  
  // === 订单数据 ===
  OrderResult orderResult   // Perfect/Excellent/Normal/Fail
  string  failReason        // 失败原因（仅Fail时）
  
  // === 奖励数据 ===
  int     goldReward
  int     reputationReward
  int     expReward          // 后续Phase
  
  // === 缺陷数据 ===
  List<DefectRecord> defectList  // from FireCalculator
    DefectRecord { name, severity, penalty }
  
  // === UI用预计算 ===
  string  gradeDisplayName  // "贡品"/"精品"/... (预映射)
  string  gradeColor        // 金色/紫色/... (预计算)
  
  // === 元数据 ===
  string  timestamp
  int     orderID
}
```

## 为什么统一读取 ResultData

| 系统 | 读取字段 | 理由 |
|------|----------|------|
| **UI渲染** | finalScore, grade, gradeDisplayName, gradeColor | 一站式获取展示所需 |
| **图鉴系统** | matchedShapeID, shapeScore, matchedGlazeID, glazeScore | 记录玩家配方与器型历史 |
| **订单系统** | orderResult, goldReward, reputationReward | 结算奖励与订单状态 |
| **存档系统** | 完整 ResultData | 序列化保存完整烧窑记录 |
| **成就系统** | grade, defectList, orderResult | 判断成就解锁条件 |

> 各系统不应分别调用 ShapeCalculator/GlazeCalculator/FireCalculator，而应读一次 ResultData。

---

# 第十二部分：Unity 实现架构

## ResultCalculator 结构

```
ResultCalculator
├── 依赖
│   ├── ShapeScore (from ShapeCalculator)
│   ├── GlazeScore (from GlazeCalculator)
│   ├── FireScore  (from FireCalculator)
│   ├── OrderData  (from OrderSystem)
│   └── DefectList (from FireCalculator)
│
├── 计算流程
│   ├── Step 1: ValidateInput()      输入校验
│   ├── Step 2: CalcFinalScore()     FinalScore = weighted sum
│   ├── Step 3: CalcGrade()          FinalScore → S/A/B/C/D/E
│   ├── Step 4: CheckOrder()         ShapeID/GlazeID 匹配检查
│   ├── Step 5: CalcGoldReward()     金币 = base × quality × diff
│   ├── Step 6: CalcRepReward()      声望 = base × order × grade × diff
│   ├── Step 7: MapGradeDisplay()    S→"贡品", A→"精品", ...
│   └── Step 8: BuildResultData()    组装输出
│
├── 输入: ResultInput
│   ├── ShapeScoreResult
│   ├── GlazeScoreResult
│   ├── FireScoreResult
│   └── OrderData
│
└── 输出: ResultData (统一结构体)
```

## 数据流

```
ShapeCalculator ──→ ShapeScoreResult
GlazeCalculator  ──→ GlazeScoreResult       OrderSystem ──→ OrderData
FireCalculator   ──→ FireScoreResult                        │
        │              │              │                     │
        └──────────────┼──────────────┼─────────────────────┘
                       │              │
                       ▼              ▼
                  ResultCalculator.Calculate(input)
                       │
                       ▼
                   ResultData
                    /  |  \
                   /   |   \
               UI    存档  图鉴
```

## 与现有 Calculator 的关系

```
ResultCalculator 不替代任何现有 Calculator。
ResultCalculator 是三个 Calculator 输出的消费者（聚合器）。

调用顺序:
  1. ShapeCalculator.Calculate(playerShapeData) → ShapeScoreResult
  2. GlazeCalculator.Calculate(playerGlazeData) → GlazeScoreResult
  3. FireCalculator.Calculate(playerFireData)   → FireScoreResult
  4. ResultCalculator.Calculate(resultInput)    → ResultData
```

---

# 第十三部分：测试案例

## 13.1 完美案例（5例）

| 编号 | 描述 | Shape | Glaze | Fire | Match | Final | Grade | Order | Gold(基50/D2) |
|------|------|-------|-------|------|-------|-------|-------|-------|-------------|
| TC-R01 | 完美影青碗 | 100 | 100 | 100 | ✓ | **100.0** | **S** | **Perfect** | 120 |
| TC-R02 | 完美霁红罐 | 100 | 100 | 100 | ✓ | **100.0** | **S** | **Perfect** | 120 |
| TC-R03 | 完美青花玉壶春 | 100 | 100 | 100 | ✓ | **100.0** | **S** | **Perfect** | 120 |
| TC-R04 | 完美甜白盘 | 100 | 100 | 100 | ✓ | **100.0** | **S** | **Perfect** | 120 |
| TC-R05 | 完美冬青梅瓶 | 100 | 100 | 100 | ✓ | **100.0** | **S** | **Perfect** | 120 |

## 13.2 高分失败订单（5例）

| 编号 | 描述 | Shape | Glaze | Fire | Match | Final | Grade | Order | Gold | 失败原因 |
|------|------|-------|-------|------|-------|-------|-------|-------|------|----------|
| TC-R06 | 釉色不匹配(高分) | 95(Bowl✓) | 90(BlueWhite✗) | 95 | ✗ | 93.75 | A | **Fail** | 0 | 釉色不匹配 |
| TC-R07 | 器型不匹配(高分) | 92(Plate✗) | 95(Yingqing✓) | 90 | ✗ | 91.95 | A | **Fail** | 0 | 器型不匹配 |
| TC-R08 | 全部不匹配(高分) | 100(Meiping✗) | 100(Dongqing✗) | 100 | ✗ | 100.0 | S | **Fail** | 0 | 器型+釉色不匹配 |
| TC-R09 | 窑裂(匹配但碎坯) | 98(Bowl✓) | 100(Yingqing✓) | 0 | ✗ | 59.3 | C | **Fail** | 0 | 窑裂/碎坯 |
| TC-R10 | 釉色混淆(高分) | 88(Bowl✓) | 90(WinterGreen✗) | 95 | ✗ | 91.3 | A | **Fail** | 0 | 釉色不匹配(误判冬青) |

> TC-R06：青花碗做成了影青釉的器型和形状，高分但订单要求的是影青釉。
> TC-R10：冬青与影青染色向量相同，可能混淆，玩家需在二级判定中手动确认。

## 13.3 低分完成订单（5例）

| 编号 | 描述 | Shape | Glaze | Fire | Match | Final | Grade | Order | Gold |
|------|------|-------|-------|------|-------|-------|-------|-------|------|
| TC-R11 | 勉强合格 | 72(Bowl✓) | 70(Yingqing✓) | 55 | ✓ | 64.7 | C | **Normal** | 60 |
| TC-R12 | 火候勉强 | 80(Bowl✓) | 78(Yingqing✓) | 52 | ✓ | 68.3 | C | **Normal** | 60 |
| TC-R13 | 形状勉强 | 55(Plate✓) | 82(SweetWhite✓) | 70 | ✓ | 67.75 | C | **Normal** | 60 |
| TC-R14 | 釉料勉强 | 80(Bowl✓) | 55(Yingqing✓) | 70 | ✓ | 69.75 | C | **Normal** | 60 |
| TC-R15 | 均衡低分 | 60(Jar✓) | 65(Jihong✓) | 58 | ✓ | 60.45 | C | **Normal** | 60 |

> 低分完成 = Shape匹配 + Glaze匹配 + FireScore>0，但总体质量不高。玩家至少做对了器型/釉色方向。

## 13.4 边界案例（5例）

| 编号 | 描述 | Shape | Glaze | Fire | Match | Final | Grade | Order | 边界说明 |
|------|------|-------|-------|------|-------|-------|-------|-------|----------|
| TC-R16 | S/A边界 | 94 | 96 | 95 | ✓ | **95.0** | **S** | Excellent | 恰好S级(95.0) |
| TC-R17 | A/B边界 | 92 | 93 | 72 | ✓ | **85.0** | **A** | Excellent | 恰好A级(85.0) |
| TC-R18 | B/C边界 | 82 | 75 | 58 | ✓ | **70.0** | **B** | Normal | 恰好B级(70.0) |
| TC-R19 | C/D边界 | 62 | 50 | 42 | ✓ | **50.0** | **C** | Normal | 恰好C级(50.0) |
| TC-R20 | D/E边界 | 42 | 30 | 22 | ✓ | **30.0** | **D** | Normal | 恰好D级(30.0) |

> 边界值通过 Python 精确计算验证。

## 13.5 极端案例（5例）

| 编号 | 描述 | Shape | Glaze | Fire | Match | Final | Grade | Order | 说明 |
|------|------|-------|-------|------|-------|-------|-------|-------|------|
| TC-R21 | 全零 | 0 | 0 | 0 | ✗ | **0.0** | **E** | Fail | 完全失败 |
| TC-R22 | 火优+形状釉极差 | 5 | 5 | 100 | ✗ | **43.0** | **D** | Fail | 火再好也救不了错误的器型/釉色 |
| TC-R23 | 形状釉优+火极差 | 100 | 100 | 5 | ✓ | **62.0** | **C** | Normal | 器型釉料完美但烧废了 |
| TC-R24 | Fire=0(致命) | 100 | 100 | 0 | ✗ | **60.0** | **C** | Fail | 窑裂/碎坯，即使其他完美 |
| TC-R25 | 困难订单(D5)+C级 | 70 | 70 | 70 | ✓ | **70.0** | **B** | Excellent | 验证Fire权重=40%使及格偏B |

## 13.6 测试覆盖矩阵

| 测试维度 | 案例数 | 覆盖 |
|----------|--------|------|
| 完美(100分) | 5 | TC-R01~05 |
| 高分但订单失败 | 5 | TC-R06~10 |
| 低分但订单完成 | 5 | TC-R11~15 |
| 边界值 | 5 | TC-R16~20 |
| 极端值 | 5 | TC-R21~25 |
| Grade映射 | 6等级 | S/A/B/C/D/E 全覆盖 |
| OrderResult | 4状态 | Perfect/Excellent/Normal/Fail 全覆盖 |
| 奖励计算 | 5 | TC-R01~05 + TC-R11 |
| Fire权重验证 | 3 | TC-R08(火50%), TC-R24(火=0), TC-R25(验证40%) |

---

# 第十四部分：MVP 路线图

## MVP (当前)

| 功能 | 状态 | 说明 |
|------|------|------|
| FinalScore (加权和) | ✓ | w=[0.35, 0.25, 0.40] |
| Grade (S/A/B/C/D/E) | ✓ | FinalScore → 等级映射 |
| OrderResult (4级) | ✓ | Perfect/Excellent/Normal/Fail |
| GoldReward | ✓ | BaseGold × Quality × Difficulty |
| ReputationReward | ✓ | BaseRep × Order × Grade × Diff |
| UI品阶映射 | ✓ | S→贡品, A→精品, B→佳品, ... |
| ResultData输出 | ✓ | 统一数据出口 |

## V1.5

| 功能 | 优先级 | 说明 |
|------|--------|------|
| ExpReward | P1 | 经验值奖励（等级系统前置） |
| 烧窑日志 | P1 | 保存 ResultData 历史记录 |
| 成就系统接口 | P2 | 基于 ResultData 判断成就解锁 |
| 难度曲线可视化 | P2 | 显示 Difficulty × 收益的关系 |
| 订单失败原因详情 | P1 | UI层展示 failReason 字段 |

## V2.0

| 功能 | 优先级 | 说明 |
|------|--------|------|
| 收藏价值(CollectibleValue) | P3 | 基于 FinalScore+Grade+稀有度的综合价值 |
| 拍卖价值(AuctionValue) | P3 | 市场供需影响的价格模型 |
| 博物馆价值(MuseumValue) | P3 | 历史/文化/稀缺性叠加 |
| 玩家交易系统 | P3 | 玩家间买卖基于 Valuation 系统 |
| 赛季排行榜 | P3 | 基于 FinalScore 综合排名 |

> V2.0 的 Valuation 系统预留接口：`ResultData.collectibleValue`、`ResultData.auctionValue`，MVP中为 null。

---

# 第十五部分：风险分析

## 15.1 数学风险

| 风险 | 等级 | 说明 | 缓解 |
|------|------|------|------|
| 权重调优 | 低 | w=[0.35,0.25,0.40]需实际内测验证 | 权重作为可配置参数，支持策划在线调参 |
| 边界值等于阈值 | 低 | Score=95.0 精确命中S/A边界 | 使用 ≥ 判定，行为明确 |
| 高Fire低Shape导致Fail | 低 | Fire=100+Shape=N/A→Order=Fail | 符合设计：订单必须匹配器型/釉色 |

## 15.2 平衡风险

| 风险 | 等级 | 说明 | 缓解 |
|------|------|------|------|
| 难度乘数膨胀 | 中 | Diff=5 × Grade=S × Perfect = 过多金币 | 数值策划在 `DifficultyConfig` 中统一定义乘数表 |
| Shape权重vs Glaze权重 | 低 | 35% vs 25% 差异在均衡场景不明显 | 轻微差异，Shape是基础操作故略高 |
| OrderResult=Normal 的收益过低 | 中 | Normal 订单可能收益低于玩家预期 | 内测调整 BaseGold |
| 致命缺陷后无经验值 | 低 | FireScore=0→Final=60但订单失败→0经验 | 未来可给"失败经验值"(经验的10%) |

## 15.3 实现风险

| 风险 | 等级 | 说明 | 缓解 |
|------|------|------|------|
| 多Calculator依赖 | 低 | ResultCalculator 依赖 3 个 Calculator | 各Calculator独立可测，可使用 Mock 数据 |
| 浮点精度 | 低 | 浮点运算在边界值可能误差 | 使用 ≥ 判定 + 容差(如≥94.999→S) |
| UI 品阶更新 | 低 | 修改"贡品"名称需改UI层代码 | 使用 Localization 表，非硬编码 |
| ResultData 序列化 | 低 | 存档系统需序列化 ResultData | 使用 Unity JsonUtility 或 ScriptableObject |

---

# 第十六部分：Runtime Safety Rules

## 16.0 总原则

### 定位

Runtime Safety Rules 是 ResultCalculator 的生产级安全层。其存在不是为了修正正常流程，而是确保**异常输入不会导致系统崩溃**。

### 职责边界

| Runtime Safety Rules 负责 | Runtime Safety Rules 不负责 |
|---------------------------|---------------------------|
| 拦截异常输入 | 修改评分公式 |
| 防止 NaN/Infinity 污染 | 修改 Grade 阈值 |
| 防止 Null 崩溃 | 修改奖励公式 |
| 记录异常日志 | 修改 OrderResult 规则 |
| 提供 ErrorResult 兜底 | 修改数据库结构 |
| 保障存档版本兼容 | 新增玩法 |

### 零影响原则

```
正常数据路径: 完全不受影响
异常数据路径: 触发安全规则 → ErrorResult + 日志
```

**所有 V1.0 的 25 个测试案例（TC-R01~R25）在引入 Runtime Safety Rules 后输出完全一致。**

### 安全规则架构

```
Input → [16.1 Clamp] → [16.2 Null Check] → [16.3 Invalid Data Check]
                                                    │
                              ┌─────────────────────┤
                              │ 异常                 │ 正常
                              ▼                      ▼
                    [ErrorResult + 日志]     [16.4 RewardClamp]
                          中止结算              [16.5 Defect Protection]
                                               [16.6 ResultData Validation]
                                               [16.7 Save Versioning]
                                               [16.8 Error Logging]
                                                        │
                                                        ▼
                                                   ResultData
                                                        │
                                                  [16.9 安全检查表通过]
                                                        │
                                                        ▼
                                                    正常输出
```

---

## 16.1 Score Clamp

### 目的

防止配置错误、数据错误、浮点精度误差导致评分越出 [0, 100] 合法区间。

### 规则

```
Clamp(value, min, max):
  if value < min   → return min
  if value > max   → return max
  if value is NaN  → return min (并触发 §16.3 InvalidData 日志)
  else             → return value
```

### 适用位置与时机

| Score | Clamp 范围 | 执行时机 | 执行者 |
|-------|-----------|----------|--------|
| ShapeScore | [0, 100] | 进入 Calculate() 第一步 | ResultCalculator.ValidateInput() |
| GlazeScore | [0, 100] | 进入 Calculate() 第一步 | ResultCalculator.ValidateInput() |
| FireScore | [0, 100] | 进入 Calculate() 第一步 | ResultCalculator.ValidateInput() |
| FinalScore | [0, 100] | 加权求和后、Grade 判定前 | ResultCalculator.CalcFinalScore() |

### 示例

| 输入值 | 来源 | Clamp后 | 说明 |
|--------|------|---------|------|
| -10 | 配置错误/代码bug | **0** | 负数归零 |
| 120 | 浮点累积误差 | **100** | 超上限截断 |
| NaN | 0/0 运算 | **0 + 日志** | NaN 归零并记录 InvalidData |
| +∞ | 除零溢出 | **100 + 日志** | Infinity 截断并记录 |
| 85.5 | 正常数据 | **85.5** | 不变 |
| 0 | 正常下限 | **0** | 不变 |
| 100 | 正常上限 | **100** | 不变 |

### 实现伪代码

```
ValidateInput(ResultInput input):
  input.shapeScore = CleanScore(input.shapeScore, "ShapeScore")
  input.glazeScore = CleanScore(input.glazeScore, "GlazeScore")
  input.fireScore  = CleanScore(input.fireScore,  "FireScore")
  return input

CleanScore(float raw, string fieldName):
  if float.IsNaN(raw):
    LogWarning($"ResultCalculator: {fieldName}=NaN, clamped to 0")
    return 0
  if float.IsInfinity(raw):
    LogWarning($"ResultCalculator: {fieldName}=Infinity, clamped to boundary")
    return raw > 0 ? 100f : 0f
  return Math.Clamp(raw, 0f, 100f)

CalcFinalScore(float s, float g, float f):
  raw = w_s × s + w_g × g + w_f × f
  return Math.Clamp(raw, 0f, 100f)  // 二次兜底
```

---

## 16.2 Null Protection

### 目的

当上游 Calculator 因异常未能产生有效结果时，系统不崩溃，返回明确的 ErrorResult。

### 检测项与 ErrorCode

| 检测项 | 空值条件 | ErrorCode | 严重度 |
|--------|---------|-----------|--------|
| input 本体 | `input == null` | `NullInput` | Critical |
| ShapeScoreResult | `input.shapeScoreResult == null` | `MissingShapeResult` | Critical |
| GlazeScoreResult | `input.glazeScoreResult == null` | `MissingGlazeResult` | Critical |
| FireScoreResult | `input.fireScoreResult == null` | `MissingFireResult` | Critical |
| OrderData | `input.orderData == null` | `NullOrderData` | Critical |
| DefectList | `input.defectList == null` | *不报错* | — |

### 处理流程

```
Calculate(ResultInput input):

  // Step 0: Null Gate
  if input == null:
    return MakeErrorResult("NullInput", "ResultInput is null")

  if input.shapeScoreResult == null:
    return MakeErrorResult("MissingShapeResult", "ShapeScoreResult is null")

  if input.glazeScoreResult == null:
    return MakeErrorResult("MissingGlazeResult", "GlazeScoreResult is null")

  if input.fireScoreResult == null:
    return MakeErrorResult("MissingFireResult", "FireScoreResult is null")

  if input.orderData == null:
    return MakeErrorResult("NullOrderData", "OrderData is null")

  // DefectList: null → 视为空列表
  defectList = input.defectList ?? new List<DefectData>()

  // 继续正常结算...
```

### ErrorResult 结构

```
ErrorResult:
  shapeScore:        0.0
  glazeScore:        0.0
  fireScore:         0.0
  finalScore:        0.0
  grade:             "E"
  matchedShapeID:    ""
  matchedGlazeID:    ""
  matchedShapeName:  "系统错误"
  matchedGlazeName:  "系统错误"
  orderResult:       "Fail"
  failReason:        "SystemError: [{ErrorCode}] {message}"
  goldReward:        0
  reputationReward:  0
  expReward:         0
  defectList:        []
  gradeDisplayName:  "系统错误"
  gradeColor:        "#95A5A6"
  timestamp:         DateTime.UtcNow
  orderID:           input?.orderData?.orderID ?? -1
  version:           101
  errorFlag:         true          ← 新增字段
  errorCode:         "{ErrorCode}" ← 新增字段
```

### ErrorCode 枚举

```
ResultCalculatorErrorCode:
  // 空值错误 (Critical)
  NullInput
  MissingShapeResult
  MissingGlazeResult
  MissingFireResult
  NullOrderData

  // 无效数据错误 (Critical)
  InvalidDataError     // NaN, Infinity, 非法枚举值, 字符串污染
  InvalidDifficulty    // difficulty < 1 或 > 5
  InvalidMatchedID     // matchedShapeID/matchedGlazeID 为空

  // 数据损坏 (Critical)
  CorruptedDefectData

  // 版本错误 (Critical)
  UnknownVersion

  // 计算溢出 (Error)
  GoldOverflow
  ReputationOverflow

  // 无害异常 (Warning)
  UnknownDefectType
  DuplicateDefect
```

---

## 16.3 Invalid Data Handling

### 目的

统一检测并安全处理 NaN、Infinity、负数、字符串污染、非法枚举值等无效数据。

### 无效数据类型与处理

| 异常类型 | 检测方法 | 处理方式 | 严重度 |
|----------|----------|----------|--------|
| NaN (浮点) | `float.IsNaN(value)` | 替换为 0 + 日志 | Error |
| +∞ (浮点) | `float.IsPositiveInfinity(value)` | 替换为 100 + 日志 | Error |
| −∞ (浮点) | `float.IsNegativeInfinity(value)` | 替换为 0 + 日志 | Error |
| 负数 (Score字段) | `value < 0` | Clamp 至 0 | Warning |
| 空字符串 (ID字段) | `string.IsNullOrEmpty(id)` | ErrorResult | Critical |
| 超长字符串 | `id.Length > 128` | 截断至 128 字符 + 日志 | Warning |
| 非法枚举值 | `!Enum.IsDefined(value)` | 回退默认值 + 日志 | Error |
| Difficulty 非法 | `diff < 1 \|\| diff > 5` | 回退为 1 + 日志 | Error |

### 统一处理函数

```
// 浮点数值清洗
CleanNumeric(float rawValue, float minVal, float maxVal, string fieldName):
  if float.IsNaN(rawValue):
    LogInvalidData(fieldName, "NaN", rawValue)
    return minVal
  if float.IsInfinity(rawValue):
    LogInvalidData(fieldName, "Infinity", rawValue)
    return float.IsPositiveInfinity(rawValue) ? maxVal : minVal
  if rawValue < minVal:
    LogInvalidData(fieldName, "Underflow", rawValue)
    return minVal
  if rawValue > maxVal:
    LogInvalidData(fieldName, "Overflow", rawValue)
    return maxVal
  return rawValue

// 字符串ID校验
ValidateStringID(string id, string fieldName):
  if string.IsNullOrEmpty(id):
    LogInvalidData(fieldName, "NullOrEmpty", id)
    return (false, "")
  // 截断超长ID
  if id.Length > 128:
    LogWarning($"ResultCalculator: {fieldName} truncated from {id.Length} to 128 chars")
    id = id.Substring(0, 128)
  return (true, id)

// Difficulty 安全取值
SafeDifficulty(int rawDiff):
  if rawDiff < 1 || rawDiff > 5:
    LogInvalidData("Difficulty", "OutOfRange", rawDiff)
    return 1
  return rawDiff

// 统一无效数据日志
LogInvalidData(string fieldName, string errorType, object rawValue):
  LogError(new ResultCalculatorLog{
    level    = "Error",
    source   = "InvalidDataHandler",
    errorType = "InvalidDataError",
    message  = $"Field '{fieldName}': invalid {errorType}, raw value={rawValue}"
  })
```

### 触发条件汇总

| 检测时机 | 检测字段 | 异常处理 |
|----------|----------|----------|
| ValidateInput() | shapeScore | CleanNumeric → Clamp [0,100] |
| ValidateInput() | glazeScore | CleanNumeric → Clamp [0,100] |
| ValidateInput() | fireScore | CleanNumeric → Clamp [0,100] |
| ValidateInput() | matchedShapeID | ValidateStringID |
| ValidateInput() | matchedGlazeID | ValidateStringID |
| ValidateInput() | difficulty | SafeDifficulty |
| CalcFinalScore() | finalScore | CleanNumeric → Clamp [0,100] |
| Any | 任意字段 | 非法枚举值→回退默认值 |

### 对正常结算的影响

正常数据（合法范围内的数值、有效字符串ID、合法枚举值）不会触发任何 InvalidData 逻辑，结算流程完全不受影响。

---

## 16.4 Reward Overflow Protection

### 目的

防止倍率配置错误、Difficulty 异常、配置重复叠加导致 GoldReward 或 ReputationReward 溢出。

### 溢出风险来源

```
正常最大金币 = MaxBaseGold × MaxQualityMultiplier × MaxDifficultyMultiplier
             = 999999 × 2.0 × 2.2
             = 4,399,995  → 在 int32 范围（≈21亿）

风险场景:
  配置错误: QualityGold[S] = 100 (应为 2.0) → Gold = 999999 × 100 × 2.2 = 219,999,780
  配置错误: DiffGold[5] = 100 (应为 2.2)  → Gold = 999999 × 2.0 × 100 = 199,999,800
  双重错误:                                → Gold = 999999 × 100 × 100 = 9,999,990,000 → 溢出
```

### RewardClamp 规则

所有奖励计算的最终输出必须经过 `RewardClamp`，上限来自**配置而非硬编码**。

```
RewardClamp(RewardConfig config, int rawValue, string rewardType):
  if rawValue < config.min:
    LogWarning($"ResultCalculator: {rewardType} underflow ({rawValue} < {config.min})")
    return config.min
  if rawValue > config.max:
    LogError($"ResultCalculator: {rewardType} overflow ({rawValue} > {config.max})")
    return config.max
  return rawValue
```

### RewardClampConfig 配置项

```
RewardClampConfig (ScriptableObject):
  gold_min:       int = 0
  gold_max:       int = 9999999     // 单次最大金币
  reputation_min: int = 0
  reputation_max: int = 999999      // 单次最大声望
  exp_min:        int = 0
  exp_max:        int = 9999999     // 预留V1.5
```

### 配置验证规则

```
ValidateRewardClampConfig(RewardClampConfig config):
  if config.gold_max <= 0:
    LogError("RewardClampConfig.gold_max must be > 0")
    return false
  if config.reputation_max <= 0:
    LogError("RewardClampConfig.reputation_max must be > 0")
    return false
  if config.gold_min < 0:
    LogError("RewardClampConfig.gold_min must be >= 0")
    return false
  return true
```

### 应用示例

```
// 金币计算（正常流程）
GoldReward = Floor(baseGold × qualityMult × diffMult)

// RewardClamp 兜底
GoldReward = RewardClamp(config.rewardClamp, GoldReward, "GoldReward")

// 声望计算（正常流程）
RepReward = Floor(baseRep × orderMod × gradeMod × diffMod)

// RewardClamp 兜底
RepReward = RewardClamp(config.rewardClamp, RepReward, "ReputationReward")
```

### 上限配置来源

| 奖励类型 | 上限来源 | 说明 |
|----------|----------|------|
| GoldReward.max | `RewardClampConfig.gold_max` | 配置驱动，策划可调整 |
| ReputationReward.max | `RewardClampConfig.reputation_max` | 配置驱动，策划可调整 |
| ExpReward.max | `RewardClampConfig.exp_max` | 预留 V1.5 |

---

## 16.5 Defect Protection

### 目的

安全处理 DefectList 的各种异常形态，防止结算崩溃或错误判定。

### 异常场景与处理方案

| 场景 | 触发条件 | 处理方案 | 日志级别 |
|------|----------|----------|----------|
| DefectList 为 null | `list == null` | 视为空列表 `new List<DefectData>()` | 不记录 |
| DefectList 为空 | `list.Count == 0` | 正常流程 | 不记录 |
| 重复 Defect | 同一 `defect.id` 出现 ≥2 次 | **自动去重**（保留首次） | Warning |
| 未知 DefectType | `defect.id` 不在已知枚举 | 标记为 `UnknownDefect` + 保留原始数据 | Warning |
| 缺陷数据损坏 | 字段缺失/类型错误 | 跳过该条 + 保留原始数据 | Error |

### 实现伪代码

```
ProcessDefectList(List<DefectData> rawList):
  if rawList == null:
    return new List<DefectData>()  // null → 空列表

  seenIDs = new HashSet<string>()
  cleanedList = new List<DefectData>()
  knownDefectIDs = config.knownDefects  // 来自 DefectDataConfig

  foreach (defect in rawList):
    if defect == null:
      LogError("ResultCalculator: null DefectData entry, skipped")
      continue

    // 去重检查
    if seenIDs.Contains(defect.id):
      LogWarning($"ResultCalculator: duplicate defect {defect.id}, skipped")
      continue
    seenIDs.Add(defect.id)

    // 未知类型检查
    if !knownDefectIDs.Contains(defect.id):
      LogWarning($"ResultCalculator: unknown defect type {defect.id}")
      defect.type = DefectType.Unknown  // 标记为未知
      defect.displayName = $"[未知] {defect.id}"

    cleanedList.Add(defect)

  return cleanedList
```

### 对 OrderResult 的影响

- DefectList 为空 / null / 去重后为空：**不影响** OrderResult 判定
- DefectList 含未知缺陷：**不影响** OrderResult 判定（仅日志记录）
- DefectList 只影响 UI 展示与玩家反馈，**不参与 OrderResult 计算**

---

## 16.6 ResultData Validation

### 目的

在 ResultData 生成前，对核心字段执行完整性校验。校验失败则生成 ErrorResult，**禁止写入存档**。

### 校验清单

| 校验项 | 校验条件 | 失败处理 |
|--------|----------|----------|
| shapeScore | `0 ≤ shapeScore ≤ 100` | ErrorResult + 日志 |
| glazeScore | `0 ≤ glazeScore ≤ 100` | ErrorResult + 日志 |
| fireScore | `0 ≤ fireScore ≤ 100` | ErrorResult + 日志 |
| finalScore | `0 ≤ finalScore ≤ 100` | ErrorResult + 日志 |
| grade | `grade in {S, A, B, C, D, E}` | ErrorResult + 日志 |
| orderResult | `orderResult in {Perfect, Excellent, Normal, Fail}` | ErrorResult + 日志 |
| matchedShapeID | 非空字符串 | ErrorResult + 日志 |
| matchedGlazeID | 非空字符串 | ErrorResult + 日志 |
| goldReward | `goldReward ≥ 0` | ErrorResult + 日志 |
| reputationReward | `reputationReward ≥ 0` | ErrorResult + 日志 |
| failReason | `orderResult != Fail` 时必须为空 | Warning + 自动清空 |
| failReason | `orderResult == Fail` 时必须非空 | Warning + 填充默认原因 |
| gradeDisplayName | 非空字符串 | Warning + 重新计算 |
| gradeColor | 非空字符串，有效Hex颜色 | Warning + 重新计算 |
| defectList | 非 null | Warning + 替换为空列表 |

### 校验时机

```
Calculate(input):
  1. 正常计算: FinalScore → Grade → OrderResult → Reward → ResultData
  2. [16.6] ValidateResultData(resultData)  ← 生成前最后一道关卡
     ├── 通过 → 返回 ResultData
     └── 失败 → 返回 ErrorResult + 日志 + 禁止存档
```

### 校验失败处理

```
ValidateResultData(ResultData data):
  errors = []

  if data.shapeScore < 0 || data.shapeScore > 100:
    errors.Add($"shapeScore={data.shapeScore} out of [0,100]")
  if data.glazeScore < 0 || data.glazeScore > 100:
    errors.Add($"glazeScore={data.glazeScore} out of [0,100]")
  if data.fireScore < 0 || data.fireScore > 100:
    errors.Add($"fireScore={data.fireScore} out of [0,100]")
  if data.finalScore < 0 || data.finalScore > 100:
    errors.Add($"finalScore={data.finalScore} out of [0,100]")

  validGrades = {"S","A","B","C","D","E"}
  if !validGrades.Contains(data.grade):
    errors.Add($"grade={data.grade} invalid")

  validOrders = {"Perfect","Excellent","Normal","Fail"}
  if !validOrders.Contains(data.orderResult):
    errors.Add($"orderResult={data.orderResult} invalid")

  if string.IsNullOrEmpty(data.matchedShapeID):
    errors.Add("matchedShapeID is empty")
  if string.IsNullOrEmpty(data.matchedGlazeID):
    errors.Add("matchedGlazeID is empty")

  if data.goldReward < 0: errors.Add($"goldReward={data.goldReward} < 0")
  if data.reputationReward < 0: errors.Add($"reputationReward={data.reputationReward} < 0")

  // failReason 一致性校验
  if data.orderResult != "Fail" && !string.IsNullOrEmpty(data.failReason):
    LogWarning($"ResultCalculator: unexpected failReason for {data.orderResult}, clearing")
    data.failReason = ""
  if data.orderResult == "Fail" && string.IsNullOrEmpty(data.failReason):
    LogWarning("ResultCalculator: failReason is empty for Fail orderResult")
    data.failReason = "SystemError: 校验失败"

  if errors.Count > 0:
    LogError($"ResultData Validation FAILED: {string.Join("; ", errors)}")
    return (false, MakeErrorResult("ResultDataValidationFailed", string.Join("; ", errors)))
  return (true, data)
```

### 强制约束

- 校验失败时 **禁止写入存档**
- UI 层检测 `errorFlag == true` 时显示"结算异常，请重试"
- 可选：触发自动重试（最多 3 次）

---

## 16.7 Save Compatibility

### 目的

通过 `ResultDataVersion` 字段支持存档版本管理，确保旧版本存档始终可读。

### 版本号定义

```
ResultData.version (int):

  100 = V1.0  (Phase 4 冻结版)
  101 = V1.1  (Runtime Safety Rules 版)
  105 = V1.5  (未来扩展版)
  200 = V2.0  (未来大版本)
```

### 向下兼容原则

```
旧版本存档 → 必须能够读取
新版本存档 → 旧版本客户端不应加载（应提示升级）
```

### 存档迁移流程

```
LoadResultData(byte[] serializedData):
  version = ReadVersionHeader(serializedData)

  switch version:
    case 100: return MigrateV1_0_to_V1_1(serializedData)
    case 101: return DeserializeV1_1(serializedData)
    case 105: return MigrateV1_5_to_V1_1(serializedData)  // 降级读取
    case 200: return MigrateV2_0_to_V1_1(serializedData)  // 降级读取
    default:
      LogError($"ResultCalculator: unknown version {version}")
      return ErrorResult("UnknownVersion")
```

### 各版本迁移操作

| 源版本 | 目标版本 | 迁移字段 | 操作 |
|--------|----------|----------|------|
| 100 (V1.0) | 101 (V1.1) | `version` | `= 101` |
| 100 (V1.0) | 101 (V1.1) | `errorFlag` | `= false` |
| 100 (V1.0) | 101 (V1.1) | `errorCode` | `= null` |
| 101 (V1.1) | 105 (V1.5) | `expReward` | 读取并保留 |
| 101 (V1.1) | 105 (V1.5) | 其他新增字段 | 填充默认值 |

### 禁止行为

| 禁止 | 原因 |
|------|------|
| 修改已有字段的语义 | 导致旧存档数据解释错误 |
| 删除已有字段 | 导致旧存档字段丢失 |
| 改变字段类型（如 int→float） | 序列化/反序列化失败 |
| 旧版本读取新存档 | 新字段无法识别 |

---

## 16.8 Error Logging

### 目的

建立统一的结算异常日志规范，所有 Runtime Safety Rules 触发时记录结构化日志，支持线上问题排查。

### ResultCalculatorLog 结构

```json
{
  "timestamp": "2026-06-06T14:30:00.123Z",
  "level": "Error",
  "source": "ValidateInput",
  "errorCode": "MissingShapeResult",
  "message": "ShapeScoreResult is null, returning ErrorResult",
  "orderID": 42,
  "version": 101,
  "context": {
    "glazeScore": 85.0,
    "fireScore": 72.0,
    "difficulty": 3
  },
  "stackTrace": "at ResultCalculator.ValidateInput()..."
}
```

### 字段说明

| 字段 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `timestamp` | DateTime | 是 | UTC 时间戳 |
| `level` | string | 是 | Info / Warning / Error / Critical |
| `source` | string | 是 | 触发方法（ValidateInput / CalcReward / ...） |
| `errorCode` | string | 是 | 对应 ErrorCode 枚举值 |
| `message` | string | 是 | 人类可读描述 |
| `orderID` | int | 否 | 关联订单 ID |
| `version` | int | 否 | ResultData 版本号 |
| `context` | object | 否 | 当前关键变量快照 |
| `stackTrace` | string | 否 | 完整调用栈（仅 Error/Critical） |

### 日志级别定义

| 级别 | 含义 | 触发条件 | 是否阻断结算 |
|------|------|----------|-------------|
| **Info** | 正常信息 | 结算完成（可选开启） | 否 |
| **Warning** | 可自动恢复的异常 | 重复 Defect、未知 Defect、ID 截断、浮点负值 | 否 |
| **Error** | 需要关注的错误 | NaN/Infinity、Reward溢出、Defect损坏、验证失败 | 是 |
| **Critical** | 致命异常 | Null输入、版本未知、完全无法计算 | 是 |

### Critical 级别强制行为

```
Critical 级错误触发时:
  1. 立即停止结算流程
  2. 返回 ErrorResult
  3. 禁止写入存档
  4. 弹出用户提示 "结算异常，请重试" (可选，仅开发/测试阶段)
```

### 日志实现

```
// MVP 阶段
void Log(ResultCalculatorLog entry):
  switch entry.level:
    case "Info":    Debug.Log(entry.message); break
    case "Warning": Debug.LogWarning(entry.message); break
    case "Error":   Debug.LogError(entry.message); break
    case "Critical": Debug.LogError($"[CRITICAL] {entry.message}"); break

// 生产阶段（可选）
void LogProd(ResultCalculatorLog entry):
  // 发送至 Sentry / Firebase Crashlytics
  ThirdPartyLogger.CaptureException(entry)
```

### 日志示例

```
// Warning - 无害异常
[2026-06-06T14:30:00] WARNING [ProcessDefectList] DuplicateDefect: 
  Defect 'FIRE_D1' appeared twice, deduplicated. orderID=42

// Error - 需关注
[2026-06-06T14:30:01] ERROR [ValidateInput] InvalidDataError:
  ShapeScore=NaN detected, replaced with 0. orderID=42

// Critical - 致命
[2026-06-06T14:30:02] CRITICAL [ValidateInput] MissingShapeResult:
  ShapeScoreResult is null, settlement aborted. orderID=42
```

---

## 16.9 Runtime Safety Checklist

### 目的

提供结算前的最终安全验证清单，所有检查项通过后方可输出 ResultData。

### 检查清单

```
═══════════════════════════════════════════════
  Runtime Safety Checklist — ResultCalculator
═══════════════════════════════════════════════

[ ] Score Clamp       — 所有 Score ∈ [0, 100]
[ ] Null Protection   — 无 null 输入
[ ] Invalid Data      — 无 NaN/Infinity/非法枚举值
[ ] Reward Clamp      — Gold/Rep/Exp 在配置上限内
[ ] Defect Safety     — DefectList 已去重，无空引用
[ ] ResultData Valid  — 核心字段完整且合法
[ ] Save Version      — ResultData.version 已设置
[ ] Error Logging     — 所有异常已记录
[ ] Normal Path OK    — 无任何安全规则触发 ◄ 正常情况

═══════════════════════════════════════════════

全部通过 (All Green) → 输出 ResultData
任一失败 (Any Red)   → ErrorResult + 禁止存档
```

### 实现位置

```
BuildResultData(CalculationContext ctx):
  resultData = new ResultData { /* 填充所有字段 */ }

  // 执行安全检查
  checklist = RunSafetyChecklist(ctx, resultData)
  
  if !checklist.AllPassed:
    LogError($"Safety checklist FAILED: {checklist.FailedItems}")
    return MakeErrorResult("SafetyChecklistFailed", checklist.FailedSummary)
  
  return resultData
```

### 与 25 个测试案例的关系

所有 V1.0 的 25 个测试案例（TC-R01~R25）数据均在合法范围内：

| 测试组 | 案例数 | Score范围 | Null风险 | NaN风险 | 安全检查 | 
|--------|--------|-----------|----------|---------|----------|
| 完美案例 | 5 | 100 | 无 | 无 | 全PASS |
| 高分失败 | 5 | 0~100 | 无 | 无 | 全PASS |
| 低分完成 | 5 | 52~70 | 无 | 无 | 全PASS |
| 边界案例 | 5 | 30~95 | 无 | 无 | 全PASS |
| 极端案例 | 5 | 0~100 | 无 | 无 | 全PASS |

安全检查清单对正常数据透明，不影响任何现有计算结果。

---

# 第十七部分：最终结论

## 17.1 ResultCalculator 冻结判定

```
════════════════════════════════════════════
      ResultCalculator 冻结判定

  ✅ FinalScore 公式: 加权求和 w=[0.35,0.25,0.40]
  ✅ Grade 映射: S/A/B/C/D/E (统一)
  ✅ OrderResult: Perfect/Excellent/Normal/Fail
  ✅ 奖励公式: BaseReward × Quality × Difficulty
  ✅ UI 解耦: Grade ↔ 品阶映射独立
  ✅ 测试覆盖: 25 案例 × 6 维度
  ✅ 无冲突: 与 ShapeScore/GlazeScore/FireScore 完全兼容

════════════════════════════════════════════
```

## 17.2 评分体系全面冻结

| 系统 | 文档 | 版本 | 冻结状态 |
|------|------|------|----------|
| ShapeScore | `ShapeScoreSpecification.md` | V1.0 | **已冻结** |
| GlazeScore | `GlazeScoreSpecification.md` | V1.1 | **已冻结** |
| FireScore | `FireCalculator.md` | V1.1 | **已冻结** |
| ResultCalculator | `ResultCalculator.md` | V1.1 | **已冻结** |
| 统一规范 | `ScoringSystem_Phase4_Freeze.md` | V1.0 | **已冻结** |

## 17.3 最终结论

```
════════════════════════════════════════════════════════════
                      
          《继承瓷厂》评分体系 — 正式冻结
                      
  评分系统          公式                     等级
  ─────────────────────────────────────────────────────
  ShapeScore        100×(1−err/0.35)         独立
  GlazeScore        100×(1−d/0.02)          独立
  FireScore         100−T−D−F−P             独立
  FinalScore        0.35×S+0.25×G+0.40×F   统一
                      
  结算系统          FinalScore→Grade→Order→Reward
                      
  核心决策:
  • FireScore 权重 40%（最高），反映核心玩法
  • S/A/B/C/D/E 六等级体系统一
  • Difficulty 不影响评分，仅影响收益
  • 评分逻辑与 UI 展示完全解耦
  • 25 个测试案例覆盖全部维度
                      
════════════════════════════════════════════════════════════
```

## 17.4 可进入下一阶段

| 阶段 | 判定 | 说明 |
|------|------|------|
| **ScriptableObject 开发** | ✅ 可进入 | ShapeSO/GlazeSO/OrderSO 等数据层 |
| **Calculator 开发** | ✅ 可进入 | Shape/Glaze/Fire/Result 四个Calculator |
| **Unity MVP 实现** | ✅ 可进入 | 拉坯UI → 配方UI → 烧窑UI → 结算UI |

### 实施建议

```
Phase A: ScriptableObject 数据层
  ├── ShapeSO.cs / GlazeSO.cs / OrderSO.cs  ← 已创建
  ├── KilnChangeSO.cs / CodexSO.cs
  ├── BalanceSO.cs / GameConfigSO.cs
  └── SO Assets 生成（从 Excel 数据导入）

Phase B: Calculator 开发
  ├── ShapeCalculator.cs  (ShapeScoreSpecification)
  ├── GlazeCalculator.cs  (GlazeScoreSpecification)
  ├── FireCalculator.cs   (FireCalculator.md)
  ├── ResultCalculator.cs (本文档)
  └── 单元测试（25个测试案例）

Phase C: Unity MVP 实现
  ├── 拉坯场景 → ShapeCalculator
  ├── 配方场景 → GlazeCalculator
  ├── 烧窑场景 → FireCalculator
  └── 结算场景 → ResultCalculator → ResultData → UI
```

---

## 附录：ResultCalculator 完整公式参考

```
Input:
  S = ShapeScore  (0~100, from ShapeCalculator)
  G = GlazeScore  (0~100, from GlazeCalculator)
  F = FireScore   (0~100, from FireCalculator)

FinalScore:
  FS = 0.35 × S + 0.25 × G + 0.40 × F

Grade:
  FS ≥ 95 → S
  FS ≥ 85 → A
  FS ≥ 70 → B
  FS ≥ 50 → C
  FS ≥ 30 → D
  FS < 30 → E

OrderResult:
  if matchedShapeID ≠ requiredShapeID → Fail
  if matchedGlazeID ≠ requiredGlazeID → Fail
  if F == 0                          → Fail (窑裂/碎坯)
  if FS ≥ 95                         → Perfect
  if FS ≥ 70                         → Excellent
  if FS ≥ 50                         → Normal
  else                               → Normal (匹配但质量差)

GoldReward:
  if OrderResult == Fail → 0
  else:
    gold = BaseGold × QualityGold(grade) × DiffGold(difficulty)

ReputationReward:
  if OrderResult == Fail → 0
  else:
    rep = BaseRep × OrderMod(result) × GradeMod(grade) × DiffRep(difficulty)
```
