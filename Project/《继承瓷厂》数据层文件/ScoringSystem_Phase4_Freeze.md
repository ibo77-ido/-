# 《继承瓷厂》评分体系 Phase 4 冻结审查报告

## 审查日期

2026-06-05

## 审查范围

| 系统 | 文档 | 版本 | 状态 |
|------|------|------|------|
| ShapeScore | `ShapeScoreSpecification.md` | V1.0 正式版 | 本次创建 |
| GlazeScore | `GlazeScoreSpecification.md` | V1.1 | 本次更新 |
| FireScore | `FireCalculator.md` | V1.1 | 本次更新 |

---

# 第一部分：ShapeScore 优化路线图

## MVP (V1.0) — 已完成

- [x] 5部位加权绝对误差算法
- [x] D_max=0.35 线性分值映射
- [x] 18/18 测试案例全PASS
- [x] S/A/B/C/D/E 六级评级
- [x] ShapeConfig 模板库标准化
- [x] Excel验证公式

## V1.5 — 推荐优化（非阻塞）

| 优化项 | 优先级 | 工作量 | 说明 |
|--------|--------|--------|------|
| 单项部位雷达图UI | P1 | 2天 | 玩家直观看到口/颈/肩/腹/足各部位得分 |
| 拉坯实时预览评分 | P1 | 3天 | 拉坯过程中实时显示当前ShapeScore |
| 历史最佳排行榜 | P2 | 1天 | 本地存储最佳ShapeScore记录 |
| Difficulty经验值加乘 | P1 | 0.5天 | Difficulty不影响分值但影响经验值倍率 |

## V2.0 — 远期扩展（非MVP）

| 扩展项 | 说明 | 风险 |
|--------|------|------|
| 轮廓曲线拟合评分 | 加入 Hausdorff/Frechet 曲线相似度 | 计算量大，需性能分析 |
| 连续曲线Smoothness | 评估拉坯线条的平滑度 | 需要曲线插值算法 |
| 非对称器型支持 | 扁壶、方器、多棱器等 | 需要扩展模板数据结构 |
| 玩家自定义器型模板 | UGC 器型上传与分享 | 需要审核系统与数据扩维 |

---

# 第二部分：GlazeScore V1.1 优化建议

## 已实施

| 优化项 | 章节 | 说明 |
|--------|------|------|
| Difficulty应用方案 | GlazeScoreSpec §5 | Difficulty不影响分值，仅影响金币/声望/经验乘数 |
| 釉色分类系统 | GlazeScoreSpec §6 | Oxidation/Reduction/Dual 三分类，用于FireScore联动 |
| 影青/冬青三级判定 | GlazeScoreSpec §7 | Cu/Fe/Co→温度→UI展示，三级fallback机制 |

## 关键原则确认

- [x] 欧几里得距离算法**未修改**
- [x] d_min / D_max / GlazeScore 逻辑**未修改**
- [x] Difficulty 不影响评分分值
- [x] 数据库结构**未修改**
- [x] 测试案例全部保持PASS

---

# 第三部分：FireScore V1.1 优化建议

## 已实施

| 优化项 | 章节 | 说明 |
|--------|------|------|
| 单一错误单次处罚 | FireCalculator 附录C | PenaltySource机制，避免CP+缺陷双重扣分 |
| 玩家反馈系统 | FireCalculator 附录D | FireScoreReport模板，分级/渐进式反馈 |

## 双重处罚修正确认

| 场景 | V1.0(双重) | V1.1(单次取max) | 改善 |
|------|-----------|-----------------|------|
| 还原失败→阴黄 | -17 | -10 | +7 |
| 温度不足→欠烧 | -30 | -20 | +10 |
| 冷却过快→失透 | -13 | -10 | +3 |

> 所有致命缺陷(窑裂/严重过烧)保持直接归零，不受影响。

---

# 第四部分：三系统统一规范

## 4.1 等级体系统一

| 分数区间 | 等级 | ShapeScore | GlazeScore | FireScore | 统一 |
|----------|------|------------|------------|-----------|------|
| 95~100 | **S** | ✓ | ✓ | ✓ | **统一** |
| 85~94 | **A** | ✓ | ✓ | ✓ | **统一** |
| 70~84 | **B** | ✓ | ✓ | ✓ | **统一** |
| 50~69 | **C** | ✓ | ✓ | ✓ | **统一** |
| 30~49 | **D** | ✓ | ✓ | ✓ | **统一** |
| 0~29 | **E** | ✓ | ✓ | ✓ | **统一** |

> 三套系统均使用 S/A/B/C/D/E 等级，**完全统一**。

## 4.2 命名规范统一

| 规范 | ShapeScore | GlazeScore | FireScore | 统一 |
|------|------------|------------|-----------|------|
| 评分输出 | `ShapeScore` | `GlazeScore` | `FireScore` | Score |
| 等级输出 | `Grade` | `Grade` | `Grade` | Grade |
| 难度字段 | `Difficulty` | `Difficulty` | — | Difficulty |
| 修正系数 | — | `DifficultyMultiplier` | — | Modifier |
| 扣分项 | — | — | `Penalty` → T/D/F/P | Penalty |
| 原始误差 | `RawShapeError` | `d_min` | — | 各系统独立命名 |
| 参考阈值 | `D_max` | `D_max` | — | 各系统独立值 |

## 4.3 Difficulty规则统一

**统一原则（三系统一致）：**

```
Difficulty 不影响评分分值
Difficulty 仅影响收益与成长
```

| 影响维度 | Shape | Glaze | Fire |
|----------|-------|-------|------|
| 评分(Score) | ✗ | ✗ | ✗ |
| 金币倍率 | ✓ | ✓ | ✗ |
| 声望倍率 | ✓ | ✓ | ✗ |
| 经验倍率 | ✓ | ✓ | ✓ |
| 解锁条件 | ✓ | ✓ | ✗ |

> FireScore 无 Difficulty：烧窑工艺难度对所有器型/釉色相同，工艺本身即是挑战。

## 4.4 数据驱动规范统一

| 规范 | ShapeScore | GlazeScore | FireScore | 统一 |
|------|------------|------------|-----------|------|
| 数据来源 | ShapeConfig Sheet | GlazeConfig Sheet | FiringData.xlsx(4 Sheet) | Excel驱动 |
| 只读性 | 冻结禁止修改 | 冻结禁止修改 | 冻结禁止修改 | 统一 |
| ScriptableObject载体 | ShapeSO | GlazeSO | — | SO载体 |
| 配置与逻辑分离 | ✓ | ✓ | ✓ | 统一 |
| 阈值可配置 | D_max=0.35 | D_max=0.02 | 扣分值分布 | 统一支持参数化 |

## 4.5 评分逻辑与UI表现解耦

```
评分计算层 (Calculator)
    ↓ 输出: ScoreResult { score, grade, details }
UI展示层 (Renderer)
    ↓ 映射: grade → displayName
中文品阶层 (Localization)
    S→"天工" / A→"上品" / B→"中品" / C→"次品" / D→"废品" / E→"碎坯"
```

**原则：评分系统内部只使用 S/A/B/C/D/E。中文品阶仅存在于 UI 展示层。**

---

# 第五部分：三系统联动矩阵

| 场景 | ShapeScore | GlazeScore | FireScore | 联动规则 |
|------|------------|------------|-----------|----------|
| 订单完成判定 | ≥ 70(B) | ≥ 70(B) | ≥ 50(C) | AND逻辑 |
| 优质订单奖励 | ≥ 85(A) | ≥ 85(A) | ≥ 70(B) | 额外收益 |
| 完美订单成就 | ≥ 95(S) | ≥ 95(S) | ≥ 95(S) | 全部S级解锁成就 |
| 器型识别 | 输出ShapeID | — | — | 独立计算 |
| 釉色识别 | — | 输出GlazeID | — | 独立计算 |
| 釉色分类→烧窑提示 | — | GlazeColorType | CP强制要求 | 还原釉必须经历还原焰 |
| 影青/冬青判定 | — | Level2温度判定 | T_max提供 | FireScore输出温度数据 |

---

# 第六部分：风险分析

## 6.1 数学风险

| 风险 | 等级 | 说明 | 缓解措施 |
|------|------|------|----------|
| ShapeScore D_max 选择 | 低 | D_max=0.35在±0.05范围内等级变化≤1级 | 已做敏感性分析，D_max作为可配置参数 |
| GlazeScore D_max 选择 | 低 | D_max=0.02在±0.005范围内微量案例等级不变 | 已做敏感性分析，D_max作为可配置参数 |
| 影青/冬青重叠 | 低 | Cu/Fe/Co向量完全相同是数据事实 | 三级判定机制已解决，不影响GlazeScore |
| 权重合理性 | 低 | 肩30%/腹30%权重基于陶瓷鉴定学 | 18/18测试PASS验证 |
| 欧几里得距离线性映射 | 低 | 线性映射在d_min小范围合理 | 已通过19个案例验证 |

## 6.2 平衡风险

| 风险 | 等级 | 说明 | 缓解措施 |
|------|------|------|----------|
| 订单难度vs奖励平衡 | 中 | Difficulty×1.0~1.8是否公平 | 需数值策划验证，当前公式为初版 |
| 三系统阈值一致性 | 低 | S/A/B/C/D/E各阈值在三系统中语义一致 | 已统一检查 |
| FireScore扣分手感 | 中 | 玩家是否感知扣分公平 | V1.1已消除双重处罚，需内测反馈 |
| 难度与评分解耦的玩家感知 | 低 | 玩家可能困惑"为什么梅瓶和碗评分标准一样" | UI提示"评分仅基于精度，难度影响奖励" |

## 6.3 实现风险

| 风险 | 等级 | 说明 | 缓解措施 |
|------|------|------|----------|
| Calculator性能 | 低 | 三个Calculator均为O(n)小数据量 | 不需要优化 |
| SO数据同步 | 低 | 仅5条Shape+5条Glaze配置 | 数据量极小 |
| 多系统集成测试 | 中 | Shape+Glaze+Fire联动逻辑需端到端测试 | 独立Calculator便于单元测试 |
| UI层映射复杂度 | 低 | Grade→中文品阶映射是纯函数 | 实现简单 |

---

# 第七部分：最终结论

## 7.1 评分体系冻结判定

| 检查项 | ShapeScore | GlazeScore | FireScore | 判断 |
|--------|------------|------------|-----------|------|
| 算法定义完整 | ✓ V1.0 | ✓ V1.1 | ✓ V1.1 | **通过** |
| 测试案例充分 | 18/18 PASS | 19/19 PASS | 5例验证 | **通过** |
| 边界案例覆盖 | ✓ | ✓ | ✓ | **通过** |
| 等级体系统一 | ✓ | ✓ | ✓ | **通过** |
| Excel验证方法 | ✓ | ✓ | ✓ | **通过** |
| 数据库冻结 | ✓ | ✓ | ✓ | **通过** |
| 文档规范完整 | ✓ | ✓ | ✓ | **通过** |
| V1.1优化完成 | N/A | ✓ | ✓ | **通过** |

## 7.2 最终结论

```
════════════════════════════════════════════
            
    评分体系已达到冻结标准
            
    ✅ ShapeScore V1.0 — 完整规范 + 18案例全PASS
    ✅ GlazeScore V1.1 — 完整规范 + 19案例全PASS  
    ✅ FireScore V1.1 — 完整规范 + 5案例验证PASS
    ✅ 三系统等级体系统一 (S/A/B/C/D/E)
    ✅ Difficulty不影响评分的统一原则
    ✅ 评分逻辑与UI表现完全解耦
    ✅ V1.1消除双重处罚 + 玩家反馈系统
    
════════════════════════════════════════════
```

### 可以进入以下阶段：

| 阶段 | 前置条件 | 状态 |
|------|----------|------|
| **ScriptableObject 开发** | 评分体系冻结 | ✅ 可进入 |
| **Calculator 开发** | SO数据层就绪 + 评分规范完成 | ✅ 可进入 |
| **Unity MVP 实现** | Calculator就绪 | ✅ 可进入 |

### 建议实施顺序

```
Phase A: ScriptableObject数据层
  ├── ShapeSO.cs / GlazeSO.cs / OrderSO.cs  ← 已完成
  ├── CodexSO.cs / KilnChangeSO.cs
  ├── BalanceSO.cs / GameConfigSO.cs
  └── SO Assets 创建（基于Excel数据导入）

Phase B: Calculator开发
  ├── ShapeCalculator.cs (ShapeScoreSpecification)
  ├── GlazeCalculator.cs (GlazeScoreSpecification)
  ├── FireCalculator.cs (FireCalculator.md)
  └── 单元测试

Phase C: Unity MVP实现
  ├── 拉坯UI → ShapeCalculator
  ├── 配方UI → GlazeCalculator
  ├── 烧窑UI → FireCalculator
  └── ResultCalculator (三系统综合评分)
```

---

## 附录：文档清单

| 文件 | 版本 | 路径 |
|------|------|------|
| GameDatabase_Refactored.xlsx | 冻结 | `正式游戏文件/` |
| ShapeScore_验证系统.xlsx | 冻结 | `正式游戏文件/` |
| FiringData.xlsx | 冻结 | `正式游戏文件/` |
| GlazeCalculator.md | 冻结 | `正式游戏文件/` |
| GlazeScoreSpecification.md | V1.1 | `正式游戏文件/` |
| ShapeScoreSpecification.md | V1.0 | `正式游戏文件/` |
| FireCalculator.md | V1.1 | `正式游戏文件/` |
| ScoringSystem_Phase4_Freeze.md | V1.0 | `正式游戏文件/` (本文档) |
