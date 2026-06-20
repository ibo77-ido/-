# P10 Naming Contract

## Goal

This document defines naming rules for the Phase10 narrative layer.

Phase10 files and runtime objects must be easy to identify, easy to audit, and clearly separated from Phase3 gameplay data, Phase6 world layout, and Phase8 bridge/runtime code.

## Prefix Rules

Use these prefixes consistently:

```text
Phase10_Narrative  Folder and assembly identity
P10_               Shared Phase10 assets, scripts, data, props, and commands
P10_CH01_          Chapter 1 scene objects, nodes, prefabs, and content
P10_PROP_          Pure narrative props
```

Do not create Phase10 assets with generic names such as `NarrativeManager`, `DialogueRoot`, `OldLedger`, or `ChapterOneScene`. Use the P10 prefix.

## Folder Root

All Phase10 work for the current task sequence must live under:

```text
Assets/Phase10_Narrative/
```

Forbidden target locations for this Phase10 task sequence:

```text
Assets/Phase3/**
Assets/Phase6/**
Assets/Phase8/**
Assets/Scenes/**
ProjectSettings/**
```

## Scene Naming

Chapter 1 overlay scene:

```text
P10_CH01_NarrativeOverlay.unity
```

The scene belongs in:

```text
Assets/Phase10_Narrative/Scenes/
```

## Root Object Naming

Required root objects for the Chapter 1 overlay scene:

```text
P10_CH01_NarrativeRoot
P10_CH01_NarrativeManager
P10_CH01_DialogueUIRoot
P10_CH01_TriggerRoot
P10_CH01_NPCRoot
P10_CH01_PropRoot
P10_CH01_AnchorRoot
P10_CH01_EventBusRoot
```

## NPC Placeholder Naming

Chapter 1 placeholder NPC objects:

```text
P10_CH01_NPC_001_XuLaoBo_Placeholder
P10_CH01_NPC_002_ZhouZhangGui_Placeholder
P10_CH01_NPC_003_ChenShuYuan_Placeholder
P10_CH01_NPC_004_LuKe_Placeholder
```

## Narrative Prop Naming

Narrative props are story-only objects and must use `P10_PROP_` IDs:

```text
P10_PROP_001_FatherLedger
P10_PROP_002_OldKilnTool
P10_PROP_003_BrokenBowl
P10_PROP_004_AncientOrder
P10_PROP_005_FamilyLetter
```

## Narrative Node Naming

Chapter 1 narrative nodes must use stable string IDs:

```text
P10_CH01_NODE_PROLOGUE_01
P10_CH01_NODE_TUTORIAL_01
P10_CH01_NODE_ORDER_001_ACCEPT
P10_CH01_NODE_ORDER_001_PASS
P10_CH01_NODE_ORDER_001_FAIL
P10_CH01_NODE_ORDER_003_ACCEPT
P10_CH01_NODE_ORDER_003_PASS
P10_CH01_NODE_ORDER_003_FAIL
P10_CH01_NODE_ORDER_004_ACCEPT
P10_CH01_NODE_ORDER_004_PASS_NORMAL
P10_CH01_NODE_ORDER_004_FAIL
P10_CH01_NODE_ORDER_004_CLIMAX
P10_CH01_NODE_CHAPTER_ENDING
```

## Acceptance

- All new Phase10 files are under `Assets/Phase10_Narrative/**`.
- All Phase10 runtime objects use `P10_` or `P10_CH01_`.
- Chapter 1 node IDs are represented as node IDs, not enum states.
- Narrative props use `P10_PROP_*`.
- No generic names are introduced when a P10-prefixed name exists.
