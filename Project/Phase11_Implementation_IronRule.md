# Phase11 Implementation Iron Rule

This rule is mandatory for all later Phase11 main UI work.

1. Only add new files, scripts, prefabs, config assets, and Inspector references.
2. Do not modify any existing code file, scene file, prefab file, asset file, or document file.
3. When Phase11 needs data from older systems, use new bridge scripts that read public APIs, UnityEvents, Inspector references, or runtime lookup.
4. Do not change Phase3, Phase6, Phase8, Phase9, or other existing modules for Phase11 integration.
5. If a goal appears to require editing an existing file, stop and redesign it as an additive bridge first.

Current approved scope:

1. `Assets/Game Main UI/Accept Order Button` and `Next Order Button` art assets remain visual only in the Phase11 main UI. Do not bridge gameplay logic to them.
2. The main panel uses an additive outside-click bridge to close. Do not add a close button.
3. Tooltip, bubble, and log item art assets are not instantiated and are not connected to display logic in this pass.
4. Silver and reputation display values come from `Phase11PlayerEconomyLedger`.
5. Reward accumulation is handled by the additive `Phase11ResultRewardBridge`, which reads Phase3 `ResultSystem.LastResult.goldReward` and `ResultSystem.LastResult.reputationReward`.
