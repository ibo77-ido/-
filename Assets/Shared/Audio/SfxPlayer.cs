using UnityEngine;

[DisallowMultipleComponent]
public sealed class SfxPlayer : MonoBehaviour
{
    private const string ResourceBankPath = "SfxBank";
    private static SfxPlayer instance;

    [SerializeField] private SfxBank bank;
    [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;

    private AudioSource source;

    public static void Play(SfxId id)
    {
        if (id == SfxId.None)
        {
            return;
        }

        GetOrCreate().PlayInternal(id);
    }

    private static SfxPlayer GetOrCreate()
    {
        if (instance != null)
        {
            return instance;
        }

        instance = FindObjectOfType<SfxPlayer>();
        if (instance != null)
        {
            instance.EnsureSource();
            return instance;
        }

        GameObject obj = new GameObject("SfxPlayer");
        instance = obj.AddComponent<SfxPlayer>();
        DontDestroyOnLoad(obj);
        instance.EnsureSource();
        return instance;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureSource();
    }

    private void PlayInternal(SfxId id)
    {
        EnsureSource();
        EnsureBank();

        if (bank == null || !bank.TryGetClip(id, out AudioClip clip, out float volume))
        {
            return;
        }

        source.PlayOneShot(clip, volume * masterVolume);
    }

    private void EnsureSource()
    {
        if (source == null)
        {
            source = GetComponent<AudioSource>();
        }

        if (source == null)
        {
            source = gameObject.AddComponent<AudioSource>();
        }

        source.playOnAwake = false;
        source.spatialBlend = 0f;
    }

    private void EnsureBank()
    {
        if (bank == null)
        {
            bank = Resources.Load<SfxBank>(ResourceBankPath);
        }
    }
}
