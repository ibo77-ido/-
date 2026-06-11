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
3. Serialized References Changed
4. Scene Mutation Declaration
5. Acceptance Check
6. Risks
7. Next Recommended Task

---

### Rule A — Runtime Reference Audit

`Serialized References Changed` 必须明确列出：

- 新增了哪些 SerializeField
- 哪些 Inspector 需要重新拖引用
- 哪些 Button.onClick 变化
- 哪些 Prefab 引用变化

无变更时输出：

```
Serialized References Changed: NONE
```

有变更时输出：

```
Serialized References Changed:
- [NEW SerializeField] BridgeCanvas on BridgeManager
- [INSPECTOR REBIND] BridgeManager.phase3Panel → drag BridgeCanvas
- [onClick CHANGED] ExitButton.onClick → BridgeManager.OnExit
- [PREFAB REF] ResultPanel.prefab added BridgeResultAdapter
```

---

### Rule B — Scene Mutation Declaration

`Scene Mutation Declaration` 必须明确声明是否修改了 Scene 文件。

Unity 场景改动不可见、难 diff、最容易污染 Git，必须显式化。

无场景修改时输出：

```
Scene Mutation: NONE
```

有场景修改时输出：

```
Scene Mutation:
- Added _BridgeRoot
- Added BridgeCanvas under _BridgeRoot
- Moved Phase3UICanvas under _BridgeRoot
```


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
