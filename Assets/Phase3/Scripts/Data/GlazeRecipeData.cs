using UnityEngine;

[CreateAssetMenu(fileName = "GlazeRecipeData", menuName = "Phase3/Data/Glaze Recipe Data")]
public class GlazeRecipeData : ScriptableObject
{
    public string glazeName;

    [Range(0f, 1f)] public float kaolin;
    [Range(0f, 1f)] public float ash;
    [Range(0f, 1f)] public float copper;
    [Range(0f, 1f)] public float iron;
    [Range(0f, 1f)] public float cobalt;
}
