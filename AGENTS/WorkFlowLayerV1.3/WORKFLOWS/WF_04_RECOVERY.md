# WF_04_RECOVERY

## Purpose
Handle FAIL, BLOCKED tasks or requirement changes.

## Input
Current Task, Failure Info

## Output
Failure Report / Change Impact Report

## Root Cause Types
Specification Error, Implementation Error, External Dependency, Resource Limitation

## Procedure
1. Stop execution
2. Identify root cause and affected items
3. Propose corrective actions
4. Wait for human approval

## Forbidden
Silent retry, Acceptance skip, Scope rewrite, FAIL->PASS

Failure Type:

- Build Failure
- Test Failure
- Validation Failure
- Dependency Failure

Recovery Strategy:

- Retry
- Rollback
- Replan
- Escalate