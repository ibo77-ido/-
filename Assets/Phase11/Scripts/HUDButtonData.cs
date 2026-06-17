using UnityEngine;

[System.Serializable]
public class HUDButtonData
{
    public AreaType areaType;
    public string label;
    public Sprite icon;
    public string tooltip;
    public bool autoInteract = true;
}
