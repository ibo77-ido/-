using UnityEngine;

public sealed class BgmSceneAutoPlayer : MonoBehaviour
{
    [SerializeField] private BgmId bgmId = BgmId.WorkshopAmbient;
    [SerializeField, Min(0f)] private float fadeInSeconds = 1.5f;
    [SerializeField] private bool stopOnDestroy;
    [SerializeField, Min(0f)] private float fadeOutSeconds = 1f;

    private void Start()
    {
        BgmPlayer.Play(bgmId, fadeInSeconds);
    }

    private void OnDestroy()
    {
        if (stopOnDestroy)
        {
            BgmPlayer.Stop(fadeOutSeconds);
        }
    }
}
