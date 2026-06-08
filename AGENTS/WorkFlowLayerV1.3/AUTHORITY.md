# AUTHORITY.md

## Authority Hierarchy

Priority Order:

1. User Instruction
2. Project Blueprint
3. AUTHORITY.md
4. CORE.md
5. Workflow Files
6. AGENTS.md
7. Templates

Higher Priority Rules Override Lower Priority Rules.

---

## Conflict Resolution

When two rules conflict:

1. Choose higher authority rule.
2. Record decision in DECISIONS.md.
3. Continue execution.

---

## Workflow Lock

Workflow stages cannot be skipped unless explicitly authorized by a higher authority source.

Required sequence:

WF_00_INIT
→ WF_01_PLANNING
→ WF_02_EXECUTION
→ WF_03_VERIFICATION
→ WF_04_RECOVERY
→ WF_05_CLOSURE

---

## Assumption Policy

If information is missing:

- Make the smallest safe assumption.
- Record assumption.
- Continue execution.

Never block execution solely because information is incomplete.
