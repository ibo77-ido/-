using UnityEngine;

[CreateAssetMenu(fileName = "ShapeRecipeData", menuName = "Phase3/Data/Shape Recipe Data")]
public class ShapeRecipeData : ScriptableObject
{
    public ShapeType shapeType;

    [Range(0f, 1f)] public float mouth;
    [Range(0f, 1f)] public float neck;
    [Range(0f, 1f)] public float shoulder;
    [Range(0f, 1f)] public float belly;
    [Range(0f, 1f)] public float foot;
}
