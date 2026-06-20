# P10D-14 Chapter 1 Flow Verification Harness Implementation

## Summary

P10D-14 implements the P10D-13 preferred approach as an Editor-only validator:

```text
Phase10_Narrative.P10D14Chapter1FlowHarnessValidator.RunP10D14Chapter1FlowHarnessValidation
```

The validator lives under `Assets/Phase10_Narrative/Scripts/Editor` and is intended for Unity editor or batchmode execution. It does not add runtime UI, does not connect to a main scene, and does not change formal task progression rules.

## Implemented Harness

The validator creates only temporary in-memory objects:

```text
P10NarrativeManager
P10NarrativeSceneBindingHub
P10DialogueController
P10NarrativeNpcInteraction
temporary Chinese probe P10DialogueNodeSO
temporary Chinese probe P10CharacterDataSO
```

All temporary objects are destroyed when validation exits. No scene or prefab asset is saved.

## Covered Flow

The normal Chapter 1 path verifies:

```text
Xu Lao Bo NPC interaction starts Prologue.
Dialogue Next enters Tutorial.
Dialogue Next enters ORDER_001_ACCEPT.
ORDER_001_ACCEPT cannot skip to pass through Dialogue Next.
OrderCompleted(ORDER_001) enters ORDER_001_PASS.
Snapshot save/load restores state, node, played nodes, and Dialogue Log.
Restored flow continues to ORDER_003_ACCEPT.
ORDER_003_ACCEPT cannot skip to pass through Dialogue Next.
OrderCompleted(ORDER_003) enters ORDER_003_PASS.
Dialogue Next enters ORDER_004_ACCEPT.
Approved Order004PassNormal trigger enters ORDER_004_PASS_NORMAL.
Approved ChapterEnding trigger reaches Completed on P10_CH01_NODE_CHAPTER_ENDING.
```

Dialogue Log validation covers:

```text
New node display appends one entry.
Repeated current-node display does not append a duplicate entry.
Entries contain sequence, nodeId, speakerName, and dialogueText.
Log panel opens and closes from the runtime dialogue surface.
Snapshot version 2 preserves Dialogue Log entries.
Snapshot restore uses restored entries as duplicate baseline.
```

Chinese text validation covers:

```text
Temporary Chinese node text: 中文显示测试：窑火重燃
Temporary Chinese speaker: 中文测试员
Runtime UI Text receives the exact Chinese strings.
Runtime UI font is non-null.
Dialogue Log stores the exact Chinese strings.
Snapshot save/load preserves the exact Chinese strings.
```

## Isolation

The harness is editor-only by path and by dependency on `UnityEditor`.

It does not:

```text
Modify P10NarrativeManager progression rules.
Modify P10DialogueController runtime behavior.
Modify P10NarrativeBridgePort or bridge adapters.
Reference Phase3, Phase6, Phase8, main scenes, or Yu branch assets.
Open or save scenes.
Add formal runtime debug UI.
Add serialized scene or prefab references.
Stage, commit, or push.
```

## Yu Branch Bridge Safety

The validator checks that the expected Phase10 bridge surface still exists:

```text
P10NarrativeGameplayEventAdapter supports OrderCompleted.
P10NarrativeGameplayEventAdapter supports ScoreThresholdReached.
P10NarrativeBridgePort exposes SubmitGameplayFact, ReceiveNarrativeCommand, and RegisterAnchorPosition.
```

The harness simulates missing full-game facts only through existing Phase10-owned runtime events and approved trigger pairs. Future Yu branch integration can replace harness fact injection with real bridge facts without a Phase10 runtime contract change.

## Execution

Recommended Unity batchmode entry point:

```text
-executeMethod Phase10_Narrative.P10D14Chapter1FlowHarnessValidator.RunP10D14Chapter1FlowHarnessValidation
```

Expected pass log:

```text
P10D-14 Chapter 1 Flow Harness validation passed.
```

## Result

```text
Editor-only: YES
Runtime code implementation: YES, editor-only validator code
Formal runtime progression rule changes: NO
Formal runtime UI changes: NO
Scene mutation: NONE
Serialized references changed: NONE
Yu bridge impact: NO
```
