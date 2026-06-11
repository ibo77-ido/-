using UnityEngine;

/// <summary>
/// Gameplay Runtime UI Host.
/// Owns the GameplayCanvasRoot — the parent of all Phase3 UI panels.
/// Controls root-level visibility (show/hide the entire gameplay UI layer).
/// Does NOT control individual panel switching — GameManager.UpdatePanels() owns that.
/// </summary>
public class GameplayRuntimeHost : MonoBehaviour
{
    [Header("Gameplay UI")]
    [SerializeField] private GameObject gameplayCanvasRoot;
    [SerializeField] private GameplayCanvasGroup canvasGroup;

    public GameplayCanvasGroup CanvasGroup => canvasGroup;
    public GameObject CanvasRoot => gameplayCanvasRoot;
    public bool IsVisible => gameplayCanvasRoot != null && gameplayCanvasRoot.activeSelf;

    /// <summary>
    /// Shows the entire gameplay UI root.
    /// Called by GameplayBridgeManager during Enter flow.
    /// </summary>
    public void ShowGameplayUI()
    {
        if (gameplayCanvasRoot != null)
        {
            gameplayCanvasRoot.SetActive(true);
            Debug.Log("[GameplayRuntimeHost] Gameplay UI shown");
        }
    }

    /// <summary>
    /// Hides the entire gameplay UI root.
    /// Called by GameplayBridgeManager during Exit flow.
    /// </summary>
    public void HideGameplayUI()
    {
        if (gameplayCanvasRoot != null)
        {
            gameplayCanvasRoot.SetActive(false);
            Debug.Log("[GameplayRuntimeHost] Gameplay UI hidden");
        }
    }

    /// <summary>
    /// Validates that all required references are set up.
    /// gameplayCanvasRoot and canvasGroup must be assigned.
    /// Panel references in canvasGroup can be bound at runtime (additive scene load),
    /// so ValidateSetup() is called BEFORE the first gameplay enter,
    /// AFTER Phase3 scene has been integrated.
    /// </summary>
    public bool ValidateSetup()
    {
        if (gameplayCanvasRoot == null)
        {
            Debug.LogError("[GameplayRuntimeHost] gameplayCanvasRoot is null");
            return false;
        }

        if (canvasGroup == null)
        {
            Debug.LogError("[GameplayRuntimeHost] canvasGroup is null");
            return false;
        }

        // Panel references may be bound at runtime after Phase3 scene loads.
        // Only validate if Phase3 has been integrated (panels should be assigned then).
        if (!canvasGroup.ValidateReferences())
        {
            Debug.LogWarning("[GameplayRuntimeHost] canvasGroup panel references not yet bound — Phase3 scene may not be loaded");
            return false;
        }

        return true;
    }
}
