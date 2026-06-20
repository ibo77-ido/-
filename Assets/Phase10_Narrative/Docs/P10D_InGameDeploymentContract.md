# P10D In-Game Deployment Contract

## Goal

P10D continues Phase10 as the in-game narrative deployment layer.

The goal is to turn the existing Phase10 narrative core, bridge preview work, and playable CH01 MVP foundation into a visible and testable in-game story path. P10D focuses on runtime mounting, NPC interaction entry, visible dialogue text, and a narrow ORDER_001 quest slice.

P10D does not close or rewrite Phase10 Core, Phase10B, or Phase10C. It classifies the existing playable and scene-facing work as deployment scope and continues from that state.

## Current Starting Point

The Gong branch already contains these accepted foundations:

```text
P10-00 through P10-06: independent narrative layer, PASS and pushed.
P10B-00 through P10B-06: bridge integration sequence, PASS or validation complete.
P10C-00 through P10C-06: CH01 playable story MVP, PASS.
P10-R00: plan reconciliation, PASS.
P10-R01: core blueprint compliance review, PASS WITH FOLLOW-UP.
```

Phase10C remains recorded as:

```text
IMPLEMENTED / AWAITING RECONCILIATION
```

P10D is the reconciliation continuation that gives the playable story layer a clear deployment contract.

## Deployment Boundary

Allowed by default for P10D tasks:

```text
Assets/Phase10_Narrative/**
```

Allowed only when a specific task explicitly approves it:

```text
Assets/Phase6/Scenes/Workshop_TestScene.unity
Assets/Scenes/**
```

Forbidden unless separately approved:

```text
Assets/Phase3/**
Assets/Phase6/**
Assets/Phase8/**
ProjectSettings/**
```

P10D must not make Phase3, Phase6, or Phase8 directly reference `P10NarrativeStateMachine`.

## Main Branch Impact

P10D work is performed on:

```text
Branch: Gong
Remote target: origin/Gong
```

It does not affect `main` unless a later human-approved merge, cherry-pick, or push to `main` is performed.

## Runtime Ownership

Phase10 owns:

```text
Narrative chapter state.
Narrative node progression.
Dialogue text selection.
NPC narrative entry points.
Narrative prop inspection events.
Narrative command requests.
```

Old gameplay systems own:

```text
Production orders.
Scoring.
Crafting data.
Workstation behavior.
Player movement.
Runtime mode and session policy.
```

Gameplay facts may enter Phase10 only as neutral facts or events.

Narrative requests may leave Phase10 only as neutral commands.

## Dialogue Text Requirement

P10D explicitly includes visible dialogue text.

The current `P10DialogueController` can publish dialogue events, but it does not yet render character names, dialogue lines, or choices. P10D should extend or supplement it so that the player can see narrative text during NPC or trigger-driven story playback.

The first implementation may use placeholder text or existing Phase10 dialogue node data. It must not require runtime file reads from `Project/StoryMain.md`.

Expected direction:

```text
P10NarrativeManager current node
        -> dialogue node lookup
        -> P10DialogueController
        -> visible UI text
        -> next / close / choice input
```

## NPC Interaction Requirement

P10D includes NPC narrative interaction entry.

The preferred first pass should reuse:

```text
P10NarrativeSceneTrigger
P10NarrativeSceneBindingHub
P10NarrativeManager
P10DialogueController
```

NPC interaction must remain story-facing. NPC placeholders must not own Phase3 order data, reward data, recipe data, scoring logic, or workstation behavior.

## ORDER_001 Quest Slice

P10D should implement or validate a narrow ORDER_001 flow before broadening to all CH01 nodes.

Minimum target:

```text
NPC or trigger starts ORDER_001 accept dialogue.
ORDER_001 completion fact can trigger pass dialogue.
ORDER_001 failure or retry path is documented or implemented in a bounded slice.
State remains Phase10-owned.
Gameplay data remains Phase3-owned.
```

## Recommended P10D Task Chain

```text
P10D-00 In-Game Deployment Contract
P10D-01 Runtime Mount Contract
P10D-02 Dialogue Text Surface
P10D-03 NPC Interaction Entry
P10D-04 ORDER_001 Quest Slice
P10D-05 In-Game Acceptance
P10D-06 Dialogue Log / Galgame Log
```

Each task requires human PASS before the next task begins.

## Dialogue Log / Galgame Log

P10D-06 adds a lightweight in-session dialogue history surface.

The first pass should:

```text
Add a Log button to the dialogue UI.
Store speaker, text, nodeId, and sequence for the current session.
Open a scrollable history panel for review.
Avoid disk writes, save integration, and Phase3 / Phase6 coupling.
```

Default scene mutation for this task remains:

```text
Scene Mutation: NONE
```

## Serialized Reference Rule

Every P10D implementation task must report:

```text
Serialized References Changed
Scene Mutation Declaration
```

If no serialized references changed:

```text
Serialized References Changed: NONE
```

If no scene file changed:

```text
Scene Mutation: NONE
```

Scene mutation is not allowed unless the task explicitly approves it.

## Existing Dirty File Warning

The current Gong workspace has pre-existing dirty files outside Phase10, including Phase3 and Phase6 files. P10D tasks must not stage, commit, or push those files unless a later task explicitly approves them.

Known local noise must remain unstaged:

```text
obj/
Library/
Temp/
P10_04_PlayMode.log
e10 narrative workspace contracts...
AGENTS/RuntimeLayer/MEMORY_Phase10.md
```

## Acceptance

P10D-00 passes when:

```text
This document exists under Assets/Phase10_Narrative/Docs/.
The P10D deployment boundary is explicit.
The dialogue text requirement is explicit.
The NPC interaction requirement is explicit.
The ORDER_001 quest slice target is explicit.
The main branch impact rule is explicit.
No runtime code is modified by P10D-00.
No scene file is modified by P10D-00.
No prefab, ScriptableObject, or ProjectSettings file is modified by P10D-00.
No git add, commit, or push is performed by P10D-00.
```
