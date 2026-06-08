# TASK EXECUTION TEMPLATE

当前执行：

Task {TASK_ID}

目标：

{TASK_OBJECTIVE}

流程：

Step 1
先输出 Design

Design 必须包含：

1. Files Created
2. Files Modified
3. Data Flow
4. Acceptance Strategy

禁止编写代码。

等待审批。

---

Design Approved 后：

执行实现。

要求：

* 严格限制在当前 Task 范围
* 不允许提前开发后续 Task
* 不允许修改无关系统
* 不允许增加计划外功能

完成后输出：

1. Files Created
2. Files Modified
3. Acceptance Check
4. Risks
5. Next Recommended Task


---

最后执行：

WF_03_VERIFICATION

输出：

PASS / FAIL

Evidence：

* 验收标准1
* 验收标准2
* 验收标准3

等待审批

---

通过后更新：

* STATE.md
* DECISIONS.md
