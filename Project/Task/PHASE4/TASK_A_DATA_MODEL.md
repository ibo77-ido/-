# Task-A: DataModel Layer

> **参考冻结规则**：见 [PHASE4_MAPPING_DESIGN.md](../PHASE4_MAPPING_DESIGN.md#一冻结数据层核心规则不可修改) §1.1 ShapeScore / §1.2 GlazeScore

---

## 目录结构 ⚙️

**决策**：继续使用 `Assets/Phase3/` 目录，不创建 Phase4

```
Assets/Phase3/
├── Data/
│   ├── Orders/              # OrderData 资产（保留，字段扩展）
│   ├── ShapeConfigs/        # 新增：ShapeConfigSO 资产
│   ├── GlazeConfigs/        # 新增：GlazeConfigSO 资产
│   ├── FireConfigs/         # 新增：FireConfigSO 资产
│   └── GameConfig/          # 新增：GameConfigSO 资产
│
├── Scripts/
│   ├── Data/
│   │   ├── OrderData.cs           # 保留，字段扩展
│   │   ├── ShapeType.cs           # 保留，扩展为5种
│   │   ├── ShapeConfigSO.cs       # 新增
│   │   ├── GlazeConfigSO.cs       # 新增
│   │   ├── FireConfigSO.cs        # 新增
│   │   └── GameConfigSO.cs        # 新增
│   │
│   ├── Calculators/               # 属于 Task-B，见 TASK_B_CALCULATOR.md
│   │
│   ├── Systems/                   # 保留（作为 MonoBehaviour 宿主）
│   │   ├── Order/
│   │   ├── Shape/
│   │   ├── Glaze/
│   │   ├── Firing/
│   │   └── Result/
│   │
│   └── UI/                        # 保留
│
└── Scenes/
    └── Phase3_Prototype.unity
```

---

## ScriptableObject 类设计 ⚙️

### ShapeConfigSO（新增）

**位置**：`Scripts/Data/ShapeConfigSO.cs`

ShapeConfigSO 继承自 ScriptableObject，包含以下字段：

**冻结字段**（来自 ShapeScoreSpecification）：
- shapeID（字符串）：器型唯一标识，值为 "SHAPE_001" ~ "SHAPE_005"
- nameCN（字符串）：中文名称
- nameEN（字符串）：英文名称
- mouthRatio（浮点数）：口部比例，范围 [0, 1]
- neckRatio（浮点数）：颈部比例，范围 [0, 1]
- shoulderRatio（浮点数）：肩部比例，范围 [0, 1]
- bellyRatio（浮点数）：腹部比例，范围 [0, 1]
- footRatio（浮点数）：足部比例，范围 [0, 1]
- difficulty（整数）：难度等级，范围 [1, 5]，不影响评分
- unlockLevel（整数）：解锁等级

**实现字段**（兼容现有代码）：
- shapeType（ShapeType 枚举）：映射到现有枚举

**ShapeType 枚举定义**（Scripts/Data/ShapeType.cs）：
- Bowl（碗）对应 SHAPE_001
- Plate（盘）对应 SHAPE_002
- Meiping（梅瓶）对应 SHAPE_003
- YuhuChun（玉壶春）对应 SHAPE_004
- Jar（罐）对应 SHAPE_005

---

### GlazeConfigSO（新增）⚠️ 破坏性变更

**位置**：`Scripts/Data/GlazeConfigSO.cs`

GlazeConfigSO 继承自 ScriptableObject，包含以下字段：

**冻结字段**（来自 GlazeScoreSpecification）：
- glazeID（字符串）：釉色唯一标识
- nameCN（字符串）：中文名称
- nameEN（字符串）：英文名称
- copper（浮点数）：铜元素含量，范围 [0, 0.02]
- iron（浮点数）：铁元素含量，范围 [0, 0.02]
- cobalt（浮点数）：钴元素含量，范围 [0, 0.02]
- temperatureMin（浮点数）：最低烧成温度
- temperatureMax（浮点数）：最高烧成温度
- colorType（GlazeColorType 枚举）：气氛分类
- difficulty（整数）：难度等级，范围 [1, 5]

**GlazeColorType 枚举定义**：
- Oxidation（氧化釉）：影青、霁红
- Reduction（还原釉）：青花、甜白
- Dual（双性釉）：冬青

**⚠️ 破坏性变更**：
- 现有 `GlazeRecipeData` 使用 kaolin/ash/copper/iron/cobalt（5材料，范围0~1）
- 新 `GlazeConfigSO` 使用 Cu/Fe/Co（3元素，范围0~0.02）
- **必须重新创建所有 Glaze 资产**

---

### OrderData（扩展）

**位置**：`Scripts/Data/OrderData.cs`（保留现有文件）

OrderData 继承自 ScriptableObject，仅保存需求参数，不包含玩家的配方引用：

**现有字段**（保留兼容）：
- orderName（字符串）：订单名称

**新增字段**（对齐冻结数据层）：
- orderID（字符串）：订单唯一标识

**需求参数字段**（仅保存需求，不引用玩家配方）：
- requiredShapeID（字符串）：订单要求的器型 ID（如 "SHAPE_001"）
- requiredGlazeID（字符串）：订单要求的釉色 ID（如 "GLAZE_001"）
- difficulty（整数）：订单难度，范围 [1, 5]
- baseGold（整数）：基础金币奖励
- baseReputation（整数）：基础声望奖励

**兼容旧字段**（已废弃）：
- rewardSilver（整数）：映射到 baseGold
- rewardReputation（整数）：映射到 baseReputation

---

### FireConfigSO（新增）

**位置**：`Scripts/Data/FireConfigSO.cs`

FireConfigSO 继承自 ScriptableObject，包含以下配置数组：

**配置字段**：
- stages（KilnStageConfig 数组，长度 8）：8 个烧成阶段配置
- flameSwitches（FlameSwitchConfig 数组，长度 4）：4 个火焰切换点
- defects（DefectConfig 数组，长度 11）：11 种缺陷定义

**KilnStageConfig 结构**：
- stageId（字符串）：阶段唯一标识
- stageNameCN（字符串）：中文名称
- stageNameEN（字符串）：英文名称
- tempStart（浮点数）：起始温度
- tempEnd（浮点数）：结束温度
- durationMin（浮点数）：最短时长
- durationMax（浮点数）：最长时长
- durationMedian（浮点数）：中位时长
- atmosphere（字符串）：气氛要求
- penaltyCap（整数）：扣分上限

**FlameSwitchConfig 结构**：
- switchId（字符串）：切换点 ID（FS1~FS4）
- fromColorCN（字符串）：起始火焰颜色中文名
- toColorCN（字符串）：目标火焰颜色中文名
- penaltyPoints（整数）：扣分值

**DefectConfig 结构**：
- defectId（字符串）：缺陷唯一标识
- nameCN（字符串）：中文名称
- nameEN（字符串）：英文名称
- isFatal（布尔值）：是否致命缺陷
- penaltyPoints（整数）：扣分值
- isStackable（布尔值）：是否可叠加
- triggerCondition（字符串）：触发条件

---

### GameConfigSO（新增）

**位置**：`Scripts/Data/GameConfigSO.cs`

GameConfigSO 继承自 ScriptableObject，包含全局配置参数（不含 UI 品阶映射）：

**Shape 评分参数**：
- shapeDMax（浮点数）：Shape 误差上限，默认 0.35
- shapeWeights（浮点数数组，长度 5）：五维权重，默认 [0.15, 0.15, 0.30, 0.30, 0.10]

**Glaze 评分参数**：
- glazeDMax（浮点数）：Glaze 距离上限，默认 0.02

**Result 权重**：
- shapeWeight（浮点数）：Shape 权重，默认 0.35
- glazeWeight（浮点数）：Glaze 权重，默认 0.25
- fireWeight（浮点数）：Fire 权重，默认 0.40

**Grade 阈值**：
- gradeS_Min（浮点数）：S 级最低分数，默认 95
- gradeA_Min（浮点数）：A 级最低分数，默认 85
- gradeB_Min（浮点数）：B 级最低分数，默认 70
- gradeC_Min（浮点数）：C 级最低分数，默认 50
- gradeD_Min（浮点数）：D 级最低分数，默认 30

**奖励乘数**：
- qualityGoldMultipliers（浮点数数组，长度 6）：各等级金币乘数
- qualityRepMultipliers（浮点数数组，长度 6）：各等级声望乘数
- orderRepMultipliers（浮点数数组，长度 4）：订单结果声望乘数
- difficultyGoldMultipliers（浮点数数组，长度 5）：各难度金币乘数
- difficultyRepMultipliers（浮点数数组，长度 5）：各难度声望乘数

**Runtime Safety**：
- goldMax（整数）：金币上限，默认 9999999
- repMax（整数）：声望上限，默认 999999

---

## Task-A 分解

| 子任务 | 内容 | 破坏性 |
|--------|------|--------|
| A1 | 创建 ShapeConfigSO + 5 个资产 | 新增 |
| A2 | 创建 GlazeConfigSO + 5 个资产 | ⚠️ 废弃旧资产 |
| A3 | 创建 FireConfigSO + 1 个资产 | 新增 |
| A4 | 创建 GameConfigSO + 1 个资产 | 新增 |
| A5 | 扩展 OrderData + 更新资产引用 | 中等 |
