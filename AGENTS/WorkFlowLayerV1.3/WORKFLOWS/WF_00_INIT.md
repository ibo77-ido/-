# WF_00_INIT

## Purpose
Initialize project context and determine next workflow.

## Input
CORE.md, PROJECT/BLUEPRINT.md, PROJECT/TASK_QUEUE.md, PROJECT/STATE.json

## Output
Workflow selection decision

## Procedure
- Load CORE and STATE
- Validate Blueprint hierarchy and task states
- Determine next workflow according to rules
