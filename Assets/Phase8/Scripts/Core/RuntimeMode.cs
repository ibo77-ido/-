/// <summary>
/// Runtime mode of the bridge.
/// WorldMode and GameplayMode are mutually exclusive.
/// Only GameplayBridgeManager may switch the mode.
/// </summary>
public enum RuntimeMode
{
    WorldMode,
    GameplayMode
}
