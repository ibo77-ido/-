using UnityEngine;
using System.Collections.Generic;

public class ScaleManager : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private AssetScaleConfigSO scaleConfig;

    private void Start()
    {
        ApplyAllScales();
    }

    public void ApplyAllScales()
    {
        if (scaleConfig == null)
        {
            Debug.Log("[ScaleManager] No scale config assigned");
            return;
        }

        ApplyCharacterScale();
        ApplyWorkstationScale();
        ApplyBuildingScale();

        Debug.Log("[ScaleManager] All scales applied");
    }

    public void ApplyCharacterScale()
    {
        if (scaleConfig == null) return;

        float s = scaleConfig.characterScale;
        var player = FindObjectOfType<PlayerCharacter>();
        if (player != null)
        {
            var artRoot = player.transform.Find("ArtRoot");
            if (artRoot != null)
            {
                artRoot.localScale = new Vector3(s, s, s);
            }
        }
    }

    public void ApplyWorkstationScale()
    {
        if (scaleConfig == null) return;

        float s = scaleConfig.workstationScale;
        var workstations = FindObjectsOfType<Workstation>();
        foreach (var ws in workstations)
        {
            var artRoot = ws.transform.Find("ArtRoot");
            if (artRoot != null)
            {
                artRoot.localScale = new Vector3(s, s, s);
            }
        }
    }

    public void ApplyBuildingScale()
    {
        if (scaleConfig == null) return;

        float s = scaleConfig.buildingScale;
        // Apply to StaticBlockerRoot ArtRoot equivalents if they exist
        // Currently buildings are in LogicRoot only, scale only their ArtRoot children
        var blockers = FindObjectsOfType<WorkstationVisualController>();
        foreach (var vc in blockers)
        {
            // Already handled in ApplyWorkstationScale
        }

        Debug.Log($"[ScaleManager] Building scale set to {s}");
    }

    public void ReApplyScales()
    {
        ApplyAllScales();
    }

    public AssetScaleConfigSO GetScaleConfig()
    {
        return scaleConfig;
    }
}