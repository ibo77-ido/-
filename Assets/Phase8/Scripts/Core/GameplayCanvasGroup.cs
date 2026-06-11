using UnityEngine;

/// <summary>
/// Aggregates all Phase3 gameplay panels under one container.
/// Does NOT control panel show/hide logic — GameManager.UpdatePanels() owns that.
/// Only provides root-level activation and panel references for Bridge validation.
/// </summary>
public class GameplayCanvasGroup : MonoBehaviour
{
    [Header("Gameplay Panels")]
    [SerializeField] private GameObject panelOrder;
    [SerializeField] private GameObject panelShape;
    [SerializeField] private GameObject panelGlaze;
    [SerializeField] private GameObject panelFiring;
    [SerializeField] private GameObject panelResult;

    public GameObject PanelOrder => panelOrder;
    public GameObject PanelShape => panelShape;
    public GameObject PanelGlaze => panelGlaze;
    public GameObject PanelFiring => panelFiring;
    public GameObject PanelResult => panelResult;

    /// <summary>
    /// Validates that all gameplay panel references are assigned.
    /// </summary>
    public bool ValidateReferences()
    {
        return panelOrder != null
            && panelShape != null
            && panelGlaze != null
            && panelFiring != null
            && panelResult != null;
    }
}
