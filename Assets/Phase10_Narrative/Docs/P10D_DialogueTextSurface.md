# P10D Dialogue Text Surface

## Goal

Make Phase10 narrative dialogue visible in game without modifying old gameplay systems or scene files.

P10D-02 adds a runtime dialogue surface to `P10DialogueController`. The surface reads the current narrative node from `P10NarrativeManager` and displays a speaker name plus dialogue text.

## Implementation

Modified file:

```text
Assets/Phase10_Narrative/Scripts/UI/P10DialogueController.cs
```

The controller now:

- finds `P10NarrativeManager` at runtime
- creates a runtime dialogue surface when a manager exists and no controller is already present
- polls the current narrative node
- resolves visible speaker and dialogue text
- displays the dialogue through a runtime uGUI `Canvas`
- provides `Next` and `Close` buttons to dismiss the current line
- creates a runtime `EventSystem` if one is not already present, so the buttons can receive input

## Data Source

Preferred data source:

```text
P10DialogueNodeSO.DialogueText
P10CharacterDataSO.DisplayName
```

Fallback data source:

```text
Known CH01 node ids mapped by the internal P10DialogueCatalog helper.
```

The fallback keeps the visible dialogue surface usable even when no Inspector references are bound yet.

## Boundary

P10D-02 does not modify:

```text
Assets/Phase3/**
Assets/Phase6/**
Assets/Phase8/**
Assets/Scenes/**
ProjectSettings/**
```

P10D-02 does not require runtime reads from:

```text
Project/StoryMain.md
```

## Runtime Flow

```text
P10NarrativeManager current node
        -> P10DialogueController
        -> P10DialogueCatalog
        -> runtime uGUI dialogue surface
```

## Serialized References Changed

`P10DialogueController` currently exposes this optional serialized field:

```text
showDialogueWindow
```

Existing scenes and prefabs do not require rebinding for the fallback runtime surface.

## Scene Mutation

```text
NONE
```

No scene file is modified by P10D-02.

## Acceptance

P10D-02 passes when:

- `P10DialogueController` can display visible dialogue text for the current narrative node
- speaker names are visible
- `Next` and `Close` can dismiss the current dialogue line
- no old gameplay system is modified
- no scene file is modified
- `Phase10_Narrative.csproj` builds with zero errors
