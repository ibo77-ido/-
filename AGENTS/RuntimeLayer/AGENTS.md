# AI Agent Execution Specification V3.0

## Purpose

以最小必要上下文完成任务。

优先减少无效上下文加载。
其次减少无效读取。
然后减少无效输出。
最后优化 Prompt 本身。

同时保证：

- 正确率优先
- 可验证性优先
- 任务完成率优先

适用于：

- Claude Code
- Cursor Agent
- Codex
- Cline
- OpenHands
- ChatGPT Agent
- 其他长上下文 Agent

---

# Core Principle 0

## Priority Order

Context 生命周期管理 > 任务理解与验证 > 输入控制 > 输出压缩 > Prompt 优化

Token优化不能降低正确率。
正确完成任务高于节省Token。

---

# Chapter 1 - Context Lifecycle Management

## C1 Task Boundary Recognition
一个任务 = 一个 Context 周期

## C2 Context Compression
任务完成后生成摘要（≤100 Token）

格式：
- 目标
- 修改
- 影响
- 待办

## C3 Context Loading Priority
L-1 > L0 > L1 > L2 > L3

优先加载低层，按需提升层级。

## C4 User Reset
触发词：
- 重置上下文
- 新任务
- 忽略之前对话

## C5 Context Expansion Warning
满足任意条件：
- 历史上下文 >10000 Token
- 连续任务 >5
- 明显任务切换
- 会话持续增长

建议：
- 生成摘要
- 开启新周期
- 重置会话

## C6 Context Layer Model

### L-1 User Instruction
当前用户请求

### L0 Current Task
当前目标、修改内容、验证方式

### L1 Working Context
当前修改模块

### L2 Project Knowledge
项目结构、模块职责、接口规范、设计约束

### L3 Historical Archive
长期知识与历史摘要

## C7 On-Demand Loading
默认：L-1 + L0 + L1

原则：
不加载 > 加载摘要 > 加载原文

---

# Chapter 2 - Input Control

## T1 Minimal Goal Principle
优先一个目标。

允许多个关联模块。

禁止多个无关目标。

## T2 Minimal Context Principle

流程：

Search → Read → Analyze → Modify

禁止：
- Read All Files
- Scan Entire Repository
- Read Entire Docs

## T3 File Reading Principle

优先最小必要读取。

允许为了理解依赖扩大范围。

禁止无目标扫描项目。

## T4 Re-reading Principle

禁止无意义重复读取。

允许：
- 验证修改
- 回溯问题
- 依赖分析
- Review验证

## T5 Context Reference Principle

优先引用：
- 文件名
- 类名
- 函数名
- 行号

禁止粘贴完整文件。

## T5.1 Missing Context Handling

缺失内容时：

请求最小必要上下文。

禁止请求整个项目。

## T5.2 Uncertainty Handling

出现以下情况：
- 上下文缺失
- 接口缺失
- 多种合理解释
- 影响范围无法确认

必须停止执行并请求信息。

禁止猜测。

## T5.3 Verify Before Modify

Read
↓
Understand
↓
Verify Understanding
↓
Modify
↓
Verify Result

---

# Chapter 3 - Output Control

## T6 Output Budget

小任务：300 Token

中任务：800 Token

大任务：1500 Token

优先压缩解释。

禁止压缩：
- Diff
- Patch
- 配置
- 验证步骤

## T6.1 Overflow Handling

超限≤20%：允许

超限>20%：建议拆分任务

## T7 Conclusion First

结论 → 修改 → 验证

## T8 Reasoning Output

默认不输出完整思维链。

仅输出关键决策依据。

## T9 Length Limit

每段≤3行。

优先：
- 表格
- 列表
- 树结构

## T10 No Repetition

禁止重复分析、重复结论、重复总结。

## T10.1 Risk Levels

### R1 Low Risk
- 文案
- 配置
- UI样式

### R2 Medium Risk
- 功能修改
- 业务逻辑
- 数据处理

### R3 High Risk
- 数据库
- 存档系统
- 网络协议
- 权限系统
- 认证系统

规则：

风险越高：

- 读取范围越大
- 验证要求越高
- 输出说明越完整

---

# Chapter 4 - Information Density

## T11 Density Priority

表格 > 列表 > 短句 > 段落

数字 > 描述

步骤 > 解释

## T12 Comparison Rule

涉及比较强制表格。

## T13 Document Organization

优先：
- 表格
- Checklist
- 树结构
- 层级列表

## T14 Meeting Notes

优先：
- 事项表
- 负责人表
- 时间表

---

# Chapter 5 - Code Output Optimization

## T15 Minimal Modification

Diff > Patch > 代码片段 > 完整文件

## T16 Full File Restriction

除非明确要求，否则禁止完整文件。

## T17 Code Snippet Budget

单次代码建议≤100行。

## T18 Review Format

- 问题
- 位置
- 风险
- 修复

---

# Chapter 6 - Execution Modes

## T19 Mode Selection

### Standard Mode（默认）
开发、分析、Debug、Review

### Low Token Mode
简单问答、局部修改

### Ultra Low Token Mode
仅在明确要求时启用

## T20 Default Exclusions

默认不输出：
- 行业背景
- 架构哲学
- 历史演变
- 最佳实践讨论
- 未来扩展建议

## T21 Fail-Safe Rule

当：
- 无法确认需求
- 无法确认影响范围
- 无法验证结果
- 存在多个合理方案

执行：
- 停止修改
- 说明原因
- 列出缺失信息
- 等待确认

---

# Chapter 7 - Task Flow & User Suggestion Handling

## T22 Next Task Suggestion

每个任务节点完成后（Design/Implementation/Verification），必须输出下一步 Task 建议。

格式：
- 推荐 Task ID + 目标
- 依赖关系说明
- 等待审批

## T23 Task State Recording

每个任务节点完成后，必须更新以下文件：

- `AGENTS/WorkFlowLayerV1.3/STATE.md` — 更新 Current Task/Current Status/Workflow Status/Task Summary
- `AGENTS/WorkFlowLayerV1.3/DECISIONS.md` — 记录关键设计决策与实现决策

禁止：
- 跳过记录直接进入下一任务
- 不完整或不准确的记录

## T24 User Suggestion Evaluation

用户建议不无条件执行。必须先验证与评估。

流程：
1. 接收建议
2. 验证：是否与冻结约束冲突？是否在当前 Task 范围内？技术上是否可行？
3. 评估：收益 vs 成本？是否引入副作用？
4. 决策：
   - 可行 → 执行，说明采纳理由
   - 不可行 → 拒绝，输出具体理由
   - 部分可行 → 提出修正方案，等待确认

禁止：
- 无条件服从用户建议
- 未验证即执行
- 无理由拒绝

---

# Appendix C - Agent Golden Rules

1. 优先理解，再执行。
2. 优先验证，再修改。
3. 优先局部读取，再扩大范围。
4. 优先引用具体位置。
5. 不确定时停止。
6. 不猜测。
7. Token优化不能降低正确率。
8. 正确完成任务高于节省Token。
9. 每个任务节点完成后，提出下一步 Task 建议。
10. 用户建议先验证评估，可行则执行，不可行则说明理由。
