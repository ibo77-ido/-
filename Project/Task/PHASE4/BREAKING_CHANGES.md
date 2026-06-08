# 破坏性变更清单

> 涉及 Task-A（DataModel）和 Task-D（UI）的资产/代码/数据变更。
>
> **迁移前必须阅读本文件。**

---

## 资产重建

| 类型 | 现有资产 | 处理方式 |
|------|---------|---------|
| GlazeRecipeData | 3 个资产 | **废弃**，创建新 GlazeConfigSO |
| ShapeRecipeData | 1 个资产 | **废弃**，创建新 ShapeConfigSO |
| OrderData | 3 个资产 | 扩展字段，更新引用 |

## 代码重构

| 模块 | 变更类型 |
|------|---------|
| GlazeSystem | **重写**（参数体系变更：5材料→3元素） |
| FiringSystem | **重写**（评分架构变更：委托 FireCalculator） |
| ResultSystem | **重写**（等级+OrderResult+奖励） |
| Panel_Glaze | **重构**（Slider 数量 5→3） |
| Panel_Firing | **重构**（仅火候评分适配，暂不新增阶段/火焰/缺陷 UI） |
| Panel_Result | **重构**（新增 OrderResult 展示 + S/A/B/C/D/E 品阶映射） |

## 数据迁移

| 数据 | 迁移方案 |
|------|---------|
| Glaze 参数 | 无迁移（参数体系根本改变） |
| Shape 参数 | 扩展字段（mouth→mouthRatio 等） |
| Order 奖励 | rewardSilver → baseGold，rewardReputation → baseReputation |

## 兼容性注意事项

1. 旧资产（GlazeRecipeData/ShapeRecipeData）已移至 `Assets/Phase3/Data/Legacy/`，不删除
2. OrderData 旧引用断裂后，需在场景 Inspector 中手动重新绑定新资产
3. ShapeSystem/GlazeSystem 新增的 shapeTemplates/glazeTemplates SerializeField 数组需在 Inspector 中手动赋值
4. ResultSystem 新增的 gameConfig 引用需在 Inspector 中赋值
