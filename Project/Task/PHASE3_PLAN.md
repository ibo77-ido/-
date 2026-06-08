# PHASE3_PLAN

## Phase3 Unity灰盒原型阶段

原则：

- 单个 Task 控制在 1~3 小时
- 可独立开发
- 可独立验收
- 严格遵守 MVP 开发规范
- 不涉及正式UI
- 不涉及美术资源
- 不涉及地图/NPC/剧情

---

# 模块A：项目初始化

## Task A1：创建Phase3灰盒场景
- 创建测试场景
- 创建Canvas
- 创建基础布局区域

验收：运行后可看到测试界面，无报错

依赖：无

## Task A2：创建核心数据结构（ScriptableObject）

定义以下三个 ScriptableObject：

**OrderData**
- string orderName
- ShapeType targetShape（enum：Bowl / Vase / Plate）
- float targetMouth, targetNeck, targetShoulder, targetBelly, targetFoot
- float targetKaolin, targetAsh, targetCopper, targetIron, targetCobalt
- int rewardSilver
- int rewardReputation

**ShapeRecipeData**（目标器型参数集，供 OrderData 引用）
- ShapeType shapeType
- float mouth, neck, shoulder, belly, foot（0~1，Slider归一化值）

**GlazeRecipeData**（目标釉料配方集，供 OrderData 引用）
- string glazeName
- float kaolin, ash, copper, iron, cobalt（0~1）

验收：
- 三个 ScriptableObject 可在 Inspector 中创建并配置
- OrderData 可正确引用 ShapeRecipeData 和 GlazeRecipeData
- 无硬编码字符串比较

依赖：A1

---

# 模块B：OrderSystem

## Task B1：创建测试订单数据
- 青釉碗
- 白釉碗
- 祭红碗

验收：订单数据存在

依赖：A2

## Task B2：订单读取
验收：能获得当前订单

依赖：B1

## Task B3：订单显示
显示：名称、目标器型、奖励

验收：当前订单正确显示

依赖：B2

---

# 模块C：ShapeSystem

## Task C1：创建5个Shape Slider
- Mouth
- Neck
- Shoulder
- Belly
- Foot

验收：Slider可拖动

依赖：A1

## Task C2：读取目标器型参数
验收：正确获得目标参数

依赖：B2

## Task C3：实现单参数误差计算
验收：可输出误差结果

依赖：C2

## Task C4：实现整体匹配度计算
输出：0~100%

验收：匹配度正常变化

依赖：C3

## Task C5：匹配度显示
验收：Slider变化时同步刷新

依赖：C4

---

# 模块D：GlazeSystem

## Task D1：创建5个Glaze Slider
- Kaolin
- Ash
- Copper
- Iron
- Cobalt

验收：Slider正常拖动

依赖：A1

## Task D2：读取目标釉料
验收：成功读取

依赖：B2

## Task D3：单材料误差计算
验收：误差正确

依赖：D2

## Task D4：整体配方匹配度
输出：Glaze Match %

验收：百分比正确变化

依赖：D3

## Task D5：配方结果显示
验收：玩家能看到结果

依赖：D4

---

# 模块E：FiringSystem

## Task E1：创建温度显示面板
验收：界面可显示数值

依赖：A1

## Task E2：实现温度增长
验收：温度正常上升

依赖：E1

## Task E2.1：风门控制
- 创建风门 Slider（0~1，模拟左右拖拽）
- 风门值影响升温速率：windRate = baseRate * (0.5 + windValue)

验收：
- Slider 拖动时温度上升速度可见变化
- windValue=0 时升温最慢，windValue=1 时升温最快

依赖：E2

## Task E2.2：投柴按钮
- 创建"投柴"Button
- 点击后 fuelStack += 1，触发温度脉冲（+fixedBoost）
- fuelStack 上限为 5，超出无效

验收：
- 点击按钮温度有脉冲式上涨
- 连续点击超过5次后不再增加

依赖：E2.1

## Task E2.3：开窗时机
- 烧制期间随机触发"开窗窗口"（间隔 10~20 秒）
- 窗口持续 3 秒，显示"开窗！"按钮
- 玩家在窗口内点击：windowBonus = true
- 错过：windowBonus = false
- windowBonus 传入 Fire Score 计算

验收：
- 窗口出现 3 秒后自动消失
- 点击与错过分别得到不同 windowBonus 值
- Fire Score 受 windowBonus 影响

依赖：E2.2

## Task E3：实现温度停止
验收：温度停止增长

依赖：E2.3

## Task E4：定义火候区间
- 欠烧
- 正常
- 过烧

验收：区间可识别

依赖：E2

## Task E5：火候评分计算
输出：Fire Score（含 windowBonus 影响）

验收：不同温度及窗口操作获得不同评分

依赖：E4、E2.3

## Task E6：火候结果显示
验收：状态实时更新

依赖：E5

---

# 模块F：ResultSystem

## Task F1：创建开窑按钮
验收：可点击

依赖：E6

## Task F2：汇总三项评分
- Shape Match
- Glaze Match
- Fire Score

验收：成功获得三项数据

依赖：C5、D5、E5

## Task F3：结果计算
- 次品
- 良品
- 佳品
- 精品
- 贡品

验收：正确生成等级

依赖：F2

## Task F4：奖励计算
- 银两
- 声望

验收：奖励正确

依赖：F3

## Task F5：结果显示
- 品级
- 银两
- 声望

验收：玩家可查看结果

依赖：F4

---

# 模块G：完整闭环验证

## Task G1：第一轮完整流程测试

接单 → Shape → Glaze → Firing（含风门/投柴/开窗）→ 开窑 → 结果

验收：
- [PASS] 全流程运行，Console 无报错
- [PASS] 结果面板正确显示品级、银两、声望
- [PASS] Shape Match / Glaze Match / Fire Score 均有非零输出

依赖：全部 A~F 任务

## Task G2：连续三订单测试

验收：
- [PASS] 青釉碗流程完整，结果可显示
- [PASS] 白釉碗流程完整，结果可显示
- [PASS] 祭红碗流程完整，结果可显示
- [PASS] 订单切换后数据正确刷新，无数据残留

依赖：G1

## Task G3：Phase3验收

验收：
- [PASS] 完整执行"接订单→器型→釉料→烧窑→开窑→结果"全链路
- [PASS] 三维烧窑决策（风门/投柴/开窗）均参与 Fire Score 计算
- [PASS] 不同操作策略产生不同品级结果（至少覆盖次品、良品、精品三级）
- [PASS] 连续运行 3 轮无崩溃、无数据污染

依赖：G2

---

# 推荐开发顺序

A1 → A2 → B1 → B2 → B3

C1 → C2 → C3 → C4 → C5

D1 → D2 → D3 → D4 → D5

E1 → E2 → E2.1 → E2.2 → E2.3 → E3 → E4 → E5 → E6

F1 → F2 → F3 → F4 → F5

G1 → G2 → G3

---

总计：30个独立Task
