# P10C Playable Story MVP Scope

## Goal

P10C continues Phase10 work toward a Chapter 1 playable story MVP.

The goal is to make the existing CH01 narrative layer playable in a narrow, testable path while keeping Phase10 as the story layer. P10C is a continuation inside Phase10, not a new Phase.

## Current State

Phase10 independent narrative layer is complete.

Phase10B Bridge Integration work remains closed and PASS.

```text
P10-00 through P10-06: PASS
P10B-00 through P10B-06: PASS
Phase10B Final Closure: PASS
```

P10C must not rewrite Phase10B history, broaden completed bridge tasks, or reopen accepted Phase10B work unless a later task explicitly asks for a recovery change.

## P10C Task Chain

```text
P10C-00 Scope Freeze
P10C-01 Story Flow Mapping
P10C-02 Map Placeholder Placement
P10C-03 Narrative Interaction Binding
P10C-04 Gameplay Story Loop
P10C-05 Save/State Safety
P10C-06 MVP Unity Acceptance
```

Each task requires human PASS before the next task begins.

## Allowed Future Scope

Future P10C tasks may modify Phase10 files when explicitly required by the task:

```text
Assets/Phase10_Narrative/**
```

Future P10C tasks may add or adjust placeholder narrative data, validation scripts, or Phase10-owned runtime components only when the task explicitly asks for implementation.

Future P10C tasks may request read-only validation against old systems when required, but old systems must not directly reference the Phase10 state machine.

## Phase6 Map And Scene Rule

Later Phase6 map or scene edits are not allowed automatically.

Any edit under these paths requires explicit approval in that specific task:

```text
Assets/Phase6/**
Assets/Scenes/**
```

If a later task approves Phase6 map or scene placement, the task must state:

```text
exact file paths allowed
objects allowed to add or move
whether scene save is allowed
validation evidence required
rollback or recovery expectations
```

P10C-00 does not approve any Phase6 map, scene, prefab, or workstation modification.

## Placeholder Rules

P10C MVP may use placeholders only.

Allowed placeholder categories in later approved tasks:

```text
blockout NPCs
blockout narrative props
blockout trigger volumes
temporary anchor markers
temporary UI/debug affordances
```

Placeholder naming must continue Phase10 naming rules:

```text
Phase10_Narrative
P10_
P10_CH01_
P10_PROP_
```

Placeholder NPCs, props, and triggers must remain story-facing. They must not become production gameplay item data, scoring data, crafting recipes, rewards, kiln resources, or workstation rules.

## Forbidden Scope

P10C must not:

```text
connect final art
connect audio or sound systems
perform broad Phase3 refactors
perform broad Phase6 refactors
perform broad Phase8 refactors
make old systems directly reference P10NarrativeStateMachine
make old systems directly mutate Phase10 chapter state
modify ProjectSettings unless separately approved
stage or commit unrelated local noise
```

Forbidden local noise includes:

```text
obj/
Library/
Temp/
P10_04_PlayMode.log
e10 narrative workspace contracts...
```

## Runtime Boundary

Phase10 owns narrative state and node flow.

Old systems may provide gameplay facts only through an approved adapter or task-specific boundary. Old systems must not directly call `P10NarrativeStateMachine`.

Narrative commands must remain adapter-mediated. P10C must not use Phase10 code to directly execute broad old-system behavior unless a later task explicitly approves a bounded slice.

## P10C-00 Declarations

P10C-00 is scope freeze only.

```text
Serialized References Changed: NONE
Scene Mutation: NONE
Code Changes: NONE
Prefab Changes: NONE
ProjectSettings Changes: NONE
```

## Acceptance

P10C-00 passes when:

```text
This document exists under Assets/Phase10_Narrative/Docs/.
Phase10B is explicitly recorded as closed/PASS.
P10C is explicitly recorded as a continuation inside Phase10, not a new Phase.
Later Phase6 map or scene edits require explicit task approval.
Placeholder NPC, prop, trigger, and anchor rules are recorded.
Forbidden scope is recorded.
No code, scene, prefab, or ProjectSettings file is modified by P10C-00.
```
