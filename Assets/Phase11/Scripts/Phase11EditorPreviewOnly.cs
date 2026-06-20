using UnityEngine;

public class Phase11EditorPreviewOnly : MonoBehaviour
{
    [SerializeField] private bool hideInPlayMode = true;

    private void Awake()
    {
        if (hideInPlayMode && Application.isPlaying)
        {
            gameObject.SetActive(false);
        }
    }
}
