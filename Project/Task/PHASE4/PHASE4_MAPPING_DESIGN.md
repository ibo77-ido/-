# Phase3 数据层映射设计（修正版）

> **唯一事实来源**：《继承瓷厂》数据层文件（冻结）
> 
> **文档版本**：2.0  
> **修正日期**：2026-06-06  
> **修正原因**：对齐冻结数据层与项目现状

---

## 文档约定

| 标记 | 含义 |
|------|------|
|  **冻结** | 来自数据层文件，禁止修改 |
|  **实现** | 根据项目现状调整，可修改 |
|  **破坏性变更** | 会导致现有资产/代码不兼容 |

---

# 一、冻结数据层核心规则（不可修改）

## 1.1 ShapeScore 冻结规则 

**来源**：`ShapeScoreSpecification.md` V1.0

### 标准器型模板（5种）

| ShapeID | NameCN | Mouth | Neck | Shoulder | Belly | Foot | Difficulty |
|---------|--------|-------|------|----------|-------|------|------------|
| SHAPE_001 | 碗 | 0.85 | 0.00 | 0.00 | 0.60 | 0.35 | 1 |
| SHAPE_002 | 盘 | 0.90 | 0.00 | 0.00 | 0.30 | 0.55 | 2 |
| SHAPE_003 | 梅瓶 | 0.15 | 0.10 | 0.85 | 0.55 | 0.40 | 4 |
| SHAPE_004 | 玉壶春瓶 | 0.35 | 0.55 | 0.45 | 0.75 | 0.40 | 3 |
| SHAPE_005 | 罐 | 0.55 | 0.05 | 0.70 | 0.80 | 0.60 | 3 |

### 权重分配 
```
Mouth:   15%
Neck:    15%
Shoulder: 30%
Belly:   30%
Foot:    10%
```

### 评分公式 🔒

```
RawShapeError = Σ(w_k × |P_k - T_k|)
              = 0.15×|P_mouth - T_mouth|
              + 0.15×|P_neck - T_neck|
              + 0.30×|P_shoulder - T_shoulder|
              + 0.30×|P_belly - T_belly|
              + 0.10×|P_foot - T_foot|

ShapeScore = 100 × max(0, 1 - RawShapeError / D_max)

其中 D_max = 0.35
```

### 匹配逻辑 🔒

```
matchedShapeID = argmin(RawShapeError(i))  i ∈ {5种器型}
```

### 等级映射 🔒

| 分数区间 | 等级 |
|----------|------|
| 95 ~ 100 | S |
| 85 ~ 94 | A |
| 70 ~ 84 | B |
| 50 ~ 69 | C |
| 30 ~ 49 | D |
| 0 ~ 29 | E |

---

## 1.2 GlazeScore 冻结规则 🔒

**来源**：`GlazeScoreSpecification.md` V1.1 / `GlazeCalculator.md`

### 标准釉色模板（5种）🔒

| GlazeID | NameCN | Cu | Fe | Co | TempMin | TempMax | ColorType | Difficulty |
|---------|--------|----|----|----|---------|---------|-----------|------------|
| GLAZE_001 | 影青釉 | 0.00 | 0.020 | 0.00 | 1250 | 1280 | Oxidation | 2 |
| GLAZE_002 | 甜白釉 | 0.00 | 0.015 | 0.00 | 1250 | 1300 | Reduction | 2 |
| GLAZE_003 | 青花 | 0.00 | 0.000 | 0.02 | 1300 | 1330 | Reduction | 3 |
| GLAZE_004 | 霁红釉 | 0.01 | 0.000 | 0.00 | 1280 | 1320 | Oxidation | 3 |
| GLAZE_005 | 冬青釉 | 0.00 | 0.020 | 0.00 | 1250 | 1300 | Dual | 2 |

### 参数体系 🔒

```
参数：Cu (Copper), Fe (Iron), Co (Cobalt)
范围：[0, 0.02]
算法：欧几里得距离
```

### 评分公式 🔒

```
d(i) = sqrt((Cu_p - Cu_i)² + (Fe_p - Fe_i)² + (Co_p - Co_i)²)

d_min = min(d(i))  i ∈ {5种釉色}

GlazeScore = 100 × max(0, 1 - d_min / D_max)

其中 D_max = 0.02
```

### 匹配逻辑 🔒

```
Level 1: Cu/Fe/Co 距离判定
Level 2: 影青/冬青平局 → 温度判定
    - T_max ≤ 1280°C → 影青釉
    - T_max > 1280°C → 冬青釉
    - 无温度数据 → 标记需确认
```

### 釉色分类 🔒

**GlazeColorType 枚举定义**：
- Oxidation（氧化釉）：影青、霁红
- Reduction（还原釉）：青花、甜白
- Dual（双性釉）：冬青

---

## 1.3 FireScore 冻结规则 🔒

**来源**：`FireCalculator.md` V1.1

### 总公式 🔒

```
FireScore = max(0, 100 - T - D - F - P)

T = 温度误差扣分 (0~35)
D = 烧成时长扣分 (0~20)
F = 火焰状态扣分 (0~20)
P = 缺陷扣分 (0~25, 致命缺陷强制归零)
```

### 8 阶段配置 🔒

| StageID | 名称 | 温度范围 | 推荐时长 | 扣分上限 |
|---------|------|---------|---------|---------|
| S1 | 预热排水 | 常温→300°C | 4~6h | 3 |
| S2 | 晶体脱水 | 300→600°C | 4~6h | 3 |
| S3 | 氧化烧清 | 600→1000°C | 8~12h | 4 |
| S4 | 还原烧成 | 1000→1200°C | 6~8h | 5 |
| S5 | 强还原保温 | 1200→1280°C | 2~4h | 3 |
| S6 | 高温成瓷 | 1280→1320°C | 1~3h | 4 |
| S7 | 急冷段 | 1320→700°C | 4~6h | 2 |
| S8 | 缓冷段 | 700→常温 | 30~40h | 1 |

### 6 个温度检查点 🔒

| CP | 目标 | 扣分上限 |
|----|------|---------|
| CP1 | ≤100°C/h 升温速率 | 5 |
| CP2 | 573±20°C 石英转化 | 3 |
| CP3 | 1000±30°C 气氛切换 | 7 |
| CP4 | 1280~1320°C 成熟窗口 | 10 |
| CP5 | 573°C 冷却 | 3 |
| CP6 | 226°C 开窑时机 | 7 |

### 4 个火焰切换点 🔒

| FS | 从→到 | 扣分 |
|----|-------|------|
| FS1 | 亮红色→樱红色 | 3 |
| FS2 | 橘黄色→暗黄色 | 8 |
| FS3 | 暗黄色→亮黄色 | 3 |
| FS4 | 黄白色→青白色 | 6 |

### 11 种缺陷 🔒

| ID | 缺陷 | 扣分 | 致命 |
|----|------|------|------|
| D1 | 开裂/窑裂 | — | ✓ |
| D2 | 过烧(严重) | — | ✓ |
| D3 | 生烧/欠烧 | 20 | |
| D4 | 过烧(轻度) | 15 | |
| D5 | 变形 | 10 | |
| D6 | 阴黄 | 10 | |
| D7 | 起泡/釉泡 | 8 | |
| D8 | 窑粘 | 8 | |
| D9 | 烟熏 | 7 | |
| D10 | 无光/失透 | 10 | |
| D11 | 针孔 | 5 | |

### V1.1 PenaltySource 机制 🔒

```
同一PenaltySource的CP扣分与缺陷扣分不叠加
取 max(CP扣分, 缺陷扣分)
```

---

## 1.4 ResultScore 冻结规则 🔒

**来源**：`ResultCalculator.md` V1.1

### 总分公式 🔒

```
FinalScore = 0.35 × ShapeScore + 0.25 × GlazeScore + 0.40 × FireScore
```

### 等级映射 🔒

| FinalScore | Grade |
|------------|-------|
| ≥ 95 | S |
| ≥ 85 | A |
| ≥ 70 | B |
| ≥ 50 | C |
| ≥ 30 | D |
| < 30 | E |

### OrderResult 判定 🔒

```
Step 1: Shape匹配检查
    matchedShapeID == requiredShapeID ?
    
Step 2: Glaze匹配检查
    matchedGlazeID == requiredGlazeID ?
    
Step 3: FireScore检查
    fireScore > 0 ? (不是致命缺陷)
    
Step 4: 等级判定
    FinalScore ≥ 95 → Perfect
    FinalScore ≥ 70 → Excellent
    FinalScore ≥ 50 → Normal
```

### 奖励公式 🔒

```
GoldReward = Floor(BaseGold × QualityMult(Grade) × DiffMult(Difficulty))

ReputationReward = Floor(BaseRep × OrderMod(Result) × GradeMod(Grade) × DiffRepMult(Difficulty))
```

### 质量乘数 🔒

| Grade | GoldMult | RepMult |
|-------|----------|---------|
| S | 2.0 | 1.5 |
| A | 1.5 | 1.2 |
| B | 1.2 | 1.0 |
| C | 1.0 | 0.8 |
| D | 0.5 | 0.3 |
| E | 0.0 | 0.0 |

### 难度乘数 🔒

| Difficulty | GoldMult | RepMult |
|------------|----------|---------|
| 1 | 1.0 | 1.0 |
| 2 | 1.2 | 1.1 |
| 3 | 1.5 | 1.3 |
| 4 | 1.8 | 1.5 |
| 5 | 2.2 | 1.8 |

### OrderResult 乘数 🔒

| OrderResult | RepMult |
|-------------|---------|
| Perfect | 1.5 |
| Excellent | 1.2 |
| Normal | 1.0 |
| Fail | 0.0 |

---

## 1.5 统一规则 🔒

**来源**：`ScoringSystem_Phase4_Freeze.md`

```
Difficulty 不影响评分分值
Difficulty 仅影响收益与成长

评分系统内部只使用 S/A/B/C/D/E
中文品阶仅存在于 UI 展示层
```

---

# 五、冻结约束与实现决策对照表

| 规则 | 来源 | 状态 |
|------|------|------|
| 5 种器型模板 | ShapeScoreSpec | 🔒 冻结 |
| 权重 [0.15, 0.15, 0.30, 0.30, 0.10] | ShapeScoreSpec | 🔒 冻结 |
| D_max = 0.35 | ShapeScoreSpec | 🔒 冻结 |
| 5 种釉色模板 | GlazeScoreSpec | 🔒 冻结 |
| Cu/Fe/Co 三元素，范围 0~0.02 | GlazeScoreSpec | 🔒 冻结 |
| 欧几里得距离算法 | GlazeScoreSpec | 🔒 冻结 |
| D_max = 0.02 | GlazeScoreSpec | 🔒 冻结 |
| 影青/冬青三级判定 | GlazeScoreSpec | 🔒 冻结 |
| T+D+F+P 四维扣分 | FireCalculator | 🔒 冻结 |
| 8 阶段 + 6CP + 4FS + 11缺陷 | FireCalculator | 🔒 冻结 |
| PenaltySource 机制 | FireCalculator V1.1 | 🔒 冻结 |
| 权重 35%/25%/40% | ResultCalculator | 🔒 冻结 |
| S/A/B/C/D/E 六等级 | ResultCalculator | 🔒 冻结 |
| OrderResult 四级判定 | ResultCalculator | 🔒 冻结 |
| 奖励公式 | ResultCalculator | 🔒 冻结 |
| Difficulty 不影响评分 | ScoringSystem_Phase4 | 🔒 冻结 |
| Phase3 目录 | PROJECT_STRUCTURE | ⚙️ 可调整 |
| SO 类名 | PROJECT_STRUCTURE | ⚙️ 可调整 |
| 目录组织 | PROJECT_STRUCTURE | ⚙️ 可调整 |

---

**实现决策按 Task 拆分**：
- [Task-A DataModel Layer](PHASE4/TASK_A_DATA_MODEL.md) — 目录结构 + SO 类设计
- [Task-B Calculator Layer](PHASE4/TASK_B_CALCULATOR.md) — Calculator 输入/输出结构
- [Task-C System Layer](PHASE4/TASK_C_SYSTEM.md) — System 宿主设计
- [Task-D UI Layer](PHASE4/TASK_D_UI.md) — UI 参数调整
- [Task-E E2E Testing](PHASE4/TASK_E_TESTING.md) — 测试验证
- [破坏性变更清单](PHASE4/BREAKING_CHANGES.md) — 资产重建/代码重构/数据迁移

---

*文档结束。主文件仅包含冻结约束规则。实现决策详见各 Task 子文件。*
