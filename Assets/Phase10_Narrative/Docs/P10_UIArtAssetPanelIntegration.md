# P10 UI Art Asset Panel Integration

## Goal

Scan the project art folder and wire available art assets into the Phase10 dialogue, dialogue log, and current order UI panels without editing Phase3 or Phase6 systems.

## Imported Art Resources

The selected art assets were copied into `Assets/Phase10_Narrative/Resources/P10Art` with ASCII filenames so Phase10 runtime UI can load them through `Resources.Load<Texture2D>`.

Resource groups:

- `P10Art/Dialog`: dialogue panel background, speaker nameplate, body texture, order/log/close/continue buttons.
- `P10Art/Log`: dialogue log background, item background, scrollbar track, scrollbar handle, divider.
- `P10Art/NPC`: XuLaoBo, ZhouZhangGui, ChenShuYuan, LuKe, and system avatars; NPC half-body art is also preserved for later panel polish.
- `P10Art/Props`: reward icon and narrative prop icons.

## Runtime Binding

`P10DialogueController` now applies Phase10 art resources when building its runtime UGUI surface:

- `DialoguePanel/Panel` uses `P10Art/Dialog/dialogue_bg`.
- `P10_DialogueSpeakerNameplate` uses `P10Art/Dialog/nameplate`.
- `P10_DialogueBodyTexture` uses `P10Art/Dialog/body_texture`.
- `P10_SpeakerPortraitImage` uses the current speaker's NPC avatar.
- Log/order/close buttons use the matching button art.
- `DialogueLogPanel` uses `P10Art/Log/log_bg`.
- Dialogue log entries use `P10Art/Log/log_item_bg`.
- Vertical scrollbars use `P10Art/Log/scrollbar_track` and `P10Art/Log/scrollbar_handle`.
- The current order panel includes a small prop strip using Phase10 prop and reward art.

The runtime surface caches generated sprites by resource path. If a texture is missing, the UI falls back to the previous plain-color panels instead of failing.

## Boundaries

- Runtime code changed only in Phase10 UI.
- No Phase3 or Phase6 gameplay systems are referenced.
- No scene or prefab write is required for the UI art binding.
- No order progression or narrative gate logic is changed.
- No stage, commit, or push.

## Validation

Editor validator:

- `Phase10_Narrative.P10ArtAssetUIPanelValidator.RunP10ArtAssetUIPanelValidation`

Coverage:

- Confirms all expected `Resources/P10Art` textures can load.
- Builds a runtime Phase10 dialogue surface.
- Confirms panel backgrounds, buttons, scrollbars, dialogue log entries, speaker portrait, and order prop icons receive sprites.

Recommended manual check:

- Enter Play Mode and trigger any Phase10 dialogue.
- Confirm the dialogue panel shows an NPC avatar, nameplate, background art, and textured body area.
- Open the dialogue log and current order panel to confirm background, scrollbar, button, and prop art are visible.
