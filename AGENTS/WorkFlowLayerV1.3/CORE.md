# CORE.md

# Loading Order

Agent must load files in the following order:

1. AUTHORITY.md
2. STATE.md
3. CORE.md
4. Current Workflow File
5. Relevant Templates
6. AGENTS.md

Do not load unnecessary workflow files.


Higher level overrides lower level.

## Purpose
Contains all fundamental rules loaded permanently by any workflow: 
Mode, Authority, Hierarchy, StateMachine, Dependency, Gate.

## Project Modes
Prototype / Product / Research / Maintenance

## Authority System
Priority: Blueprint > Current Task > Task Queue > Runtime Rules > History

Conflict Resolution:
Higher-priority overrides lower-priority.

Reporting required for any conflicts.

## Project Hierarchy
Goal -> Milestone -> Feature -> Task

## Gate System
Stage Gate: Task may not proceed unless PASS + Human approval.

## State Machine
States: DRAFT, READY, ACTIVE, PASS, FAIL, BLOCKED, ARCHIVED
Transitions only allowed according to workflow.

## Dependency Rule
Task blocked if dependency not PASS.

## Workflow Selection
Agent must load exactly one Workflow at a time based on current action:
- Create new tasks -> WF_01_PLANNING.md
- Execute READY task -> WF_02_EXECUTION.md
- Verify implementation -> WF_03_VERIFICATION.md
- Handle FAIL/BLOCKED -> WF_04_RECOVERY.md
- Complete Milestone/Goal -> WF_05_CLOSURE.md
# CORE.md

## Purpose
Contains all fundamental rules loaded permanently by any workflow: 
Mode, Authority, Hierarchy, StateMachine, Dependency, Gate.

## Project Modes
Prototype / Product / Research / Maintenance

## Authority System
Priority: Blueprint > Current Task > Task Queue > Runtime Rules > History

Conflict Resolution:
Higher-priority overrides lower-priority.

Reporting required for any conflicts.

## Project Hierarchy
Goal -> Milestone -> Feature -> Task

## Gate System
Stage Gate: Task may not proceed unless PASS + Human approval.

## State Machine
States: DRAFT, READY, ACTIVE, PASS, FAIL, BLOCKED, ARCHIVED
Transitions only allowed according to workflow.

## Dependency Rule
Task blocked if dependency not PASS.

## Workflow Selection
Agent must load exactly one Workflow at a time based on current action:
- Create new tasks -> WF_01_PLANNING.md
- Execute READY task -> WF_02_EXECUTION.md
- Verify implementation -> WF_03_VERIFICATION.md
- Handle FAIL/BLOCKED -> WF_04_RECOVERY.md
- Complete Milestone/Goal -> WF_05_CLOSURE.md

# Workflow Gate

WF_00 must pass
before WF_01 starts.

WF_01 must pass
before WF_02 starts.

WF_02 must pass
before WF_03 starts.

WF_03 must pass
before WF_04 starts.

WF_04 must pass
before WF_05 starts.
