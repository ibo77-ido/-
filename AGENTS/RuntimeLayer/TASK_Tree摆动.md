# Task: Tree 树叶风摆动 Shader

## 当前执行

Task Tree摆动

## 目标

为 tree 物体下的所有树（V_Tree_Pine_01~04, V_Willow_01~02, V_Plum_Tree_01）添加风摆动效果，要求：
- **只有树叶可以摆动**（树干固定）
- **摆动强度忽大忽小**（阵风效果）

---

## Step 1 - Design

### 1. 现状分析

**Tree 结构**：
- 所有树都是单 GameObject + SpriteRenderer（无子对象）
- 树干和树叶绘制在同一张贴图上
- 场景中 `tree` 是容器，子对象包括：V_Willow_01, V_Willow_02, V_Tree_Pine_01

**贴图 UV 分布**（目测估算）：
- **Tree_Pine**: 树干占底部 ~40%，树冠占顶部 ~60%
- **Willow**: 树干占底部 ~30%，柳条树叶占顶部 ~70%
- **Plum_Tree**: 树干占底部 ~35%，树冠占顶部 ~65%

### 2. 技术方案选择

**方案：复用 SpriteBambooWind.shader + 参数调优**
- 利用现有 `_RootLock` 参数锁定底部（树干）
- `_NoiseStrength` 保持默认值附近（不强行提高），观察实际效果后再决定是否需要新建 Shader 做连续阵风
- 优点：无需新 Shader，与 Bamboo/Bush 保持一致，维护成本最低

### 3. Files Created

| 文件路径 | 类型 | 说明 |
|---------|------|------|
| `Assets/Phase9/Materials/M_Tree_Pine_01_Wind.mat` | Material | 松树 01 |
| `Assets/Phase9/Materials/M_Tree_Pine_02_Wind.mat` | Material | 松树 02 |
| `Assets/Phase9/Materials/M_Tree_Pine_03_Wind.mat` | Material | 松树 03 |
| `Assets/Phase9/Materials/M_Tree_Pine_04_Wind.mat` | Material | 松树 04 |
| `Assets/Phase9/Materials/M_Willow_01_Wind.mat` | Material | 柳树 01 |
| `Assets/Phase9/Materials/M_Willow_02_Wind.mat` | Material | 柳树 02 |
| `Assets/Phase9/Materials/M_Plum_Tree_01_Wind.mat` | Material | 梅树 01 |
| `Assets/Phase9/Materials/M_Maple_01_Wind.mat` | Material | 枫树 01 |
| `Assets/Phase9/Materials/M_Poplar_01_Wind.mat` | Material | 杨树 01 |

### 4. Files Modified

| 文件路径 | 修改内容 |
|---------|---------|
| `Assets/Phase9/Prefabs/Vegetation/V_Tree_Pine_01.prefab` | SpriteRenderer.m_Materials → M_Tree_Pine_01_Wind |
| `Assets/Phase9/Prefabs/Vegetation/V_Tree_Pine_02.prefab` | 同上 |
| `Assets/Phase9/Prefabs/Vegetation/V_Tree_Pine_03.prefab` | 同上 |
| `Assets/Phase9/Prefabs/Vegetation/V_Tree_Pine_04.prefab` | 同上 |
| `Assets/Phase9/Prefabs/Vegetation/V_Willow_01.prefab` | SpriteRenderer.m_Materials → M_Willow_01_Wind |
| `Assets/Phase9/Prefabs/Vegetation/V_Willow_02.prefab` | SpriteRenderer.m_Materials → M_Willow_02_Wind |
| `Assets/Phase9/Prefabs/Vegetation/V_Plum_Tree_01.prefab` | SpriteRenderer.m_Materials → M_Plum_Tree_01_Wind |
| `Assets/Phase9/Prefabs/Vegetation/V_Maple_01.prefab` | SpriteRenderer.m_Materials → M_Maple_01_Wind |
| `Assets/Phase9/Prefabs/Vegetation/V_Poplar_01.prefab` | SpriteRenderer.m_Materials → M_Poplar_01_Wind |

### 5. Data Flow

```
SpriteBambooWind.shader (复用)
       │
       ├── Tree_Pine_01~04 → _RootLock=0.35 (树干锁定底部35%)
       │                     _WindStrength=0.15 (与Bamboo 0.12相近，略高因Scale小)
       │
       ├── Willow_01~02 → _RootLock=0.25 (树干较细，锁定少)
       │                  _WindStrength=0.15 (保守值，看效果再调)
       │
       ├── Plum_Tree_01 → _RootLock=0.30
       │                  _WindStrength=0.15
       │
       ├── Maple_01 → _RootLock=0.30
       │               _WindStrength=0.15
       │
       └── Poplar_01 → _RootLock=0.35 (高树，树干比例大)
                        _WindStrength=0.15

UV.y → height → smoothstep(_RootLock, 1.0, height) → bendWeight
       底部(树干区域): bendWeight ≈ 0 (不摆动)
       顶部(树叶区域): bendWeight 从 0 逐渐过渡到 1
```

### 6. Acceptance Strategy

**验收标准1**: 材质创建无错误，Shader 编译通过
**验收标准2**: 树干区域（底部 25-35%）保持静止
**验收标准3**: 树叶区域有明显轻微摆动
**验收标准4**: 所有 Tree Prefab 材质引用正确

---

## Design Approved 后执行

### 参数设计

| 参数 | Tree_Pine | Willow | Plum_Tree | Maple | Poplar |
|------|-----------|--------|-----------|-------|--------|
| `_WindStrength` | 0.15 | 0.15 | 0.15 | 0.15 | 0.15 |
| `_WindSpeed` | 1.2 | 1.5 | 1.2 | 1.2 | 1.0 |
| `_RootLock` | 0.35 | 0.25 | 0.30 | 0.30 | 0.35 |
| `_NoiseStrength` | 0.35 | 0.35 | 0.35 | 0.35 | 0.35 |
| `_WaveFrequency` | 4 | 6 | 4 | 5 | 3 |
| `_PhaseOffset` | 0,1,2,3 | 4,5 | 6 | 7 | 8 |

### 关于"忽大忽小"的处理

现有 Shader 的噪声项 `Hash21(floor(v.texcoord * 16.0) + floor(t * 2.0))` 每 0.5 秒突变一次，而非连续渐变。强行提高 `_NoiseStrength` 会产生跳变/卡顿感，不是自然的阵风效果。

**方案：先按以上参数实现，观察实际效果**：
- 如果现有的 `sin` + `leafFlutter` + 少量 `noise` 已经足够"自然"，就不需要额外处理
- 如果确实需要"忽大忽小"的阵风感，再考虑新建 Shader（添加连续时间的 `gust envelope`：`sin(time * interval)^2` 做包络线）

---

## 实现后输出

### 1. Files Created
- 9 个 Tree Wind 材质文件（在 `Assets/Phase9/Materials/` 下）

### 2. Files Modified
- 9 个 Tree Prefab 文件

### 3. Serialized References Changed

```
Serialized References Changed:
- [NEW FILE] M_Tree_Pine_01_Wind.mat ~ M_Tree_Pine_04_Wind.mat
- [NEW FILE] M_Willow_01_Wind.mat, M_Willow_02_Wind.mat
- [NEW FILE] M_Plum_Tree_01_Wind.mat
- [NEW FILE] M_Maple_01_Wind.mat
- [NEW FILE] M_Poplar_01_Wind.mat
- [INSPECTOR REBIND] V_Tree_Pine_01~04.prefab → 对应 Tree Wind 材质
- [INSPECTOR REBIND] V_Willow_01~02.prefab → 对应 Willow Wind 材质
- [INSPECTOR REBIND] V_Plum_Tree_01.prefab → M_Plum_Tree_01_Wind
- [INSPECTOR REBIND] V_Maple_01.prefab → M_Maple_01_Wind
- [INSPECTOR REBIND] V_Poplar_01.prefab → M_Poplar_01_Wind
```

### 4. Scene Mutation Declaration

```
Scene Mutation: NONE
（仅修改 Prefab + 创建材质，不修改 Scene 文件）
```

### 5. Acceptance Check

- [ ] 9 个材质创建成功
- [ ] 树干区域静止
- [ ] 树叶区域轻微摆动
- [ ] Prefab 材质引用正确

### 6. Risks

| 风险 | 可能性 | 缓解措施 |
|------|--------|---------|
| 树干/树叶 UV 分界不精确 | 中 | `_RootLock` 可在 Inspector 实时微调 |
| 摆动幅度不合适 | 低 | `_WindStrength` 可调 |
| "忽大忽小"效果不足 | 中 | 观察后决定是否需要新建 Shader 添加连续阵风包络 |

### 7. Next Recommended Task

- 场景中 Play Mode 观察效果
- 如需更精确的阵风效果，新建 SpriteTreeWind.shader 添加连续 gust envelope

---

## WF_03_VERIFICATION

### PASS / FAIL

等待实现后验证

### Evidence

* 验收标准1: 材质 Inspector 截图
* 验收标准2: Play Mode 树干静止观察
* 验收标准3: Play Mode 树叶摆动观察
* 验收标准4: Prefab 材质引用 Inspector 截图
