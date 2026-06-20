using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class BgmPlayer : MonoBehaviour
{
    private const string ResourceBankPath = "BgmBank";
    private static BgmPlayer instance;

    [SerializeField] private BgmBank bank;
    [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;

    private AudioSource source;
    private Coroutine fadeRoutine;
    private BgmId currentId = BgmId.None;

    public static void Play(BgmId id, float fadeSeconds = 1.5f)
    {
        GetOrCreate().PlayInternal(id, fadeSeconds);
    }

    public static void Stop(float fadeSeconds = 1f)
    {
        GetOrCreate().StopInternal(fadeSeconds);
    }

    private static BgmPlayer GetOrCreate()
    {
        if (instance != null)
        {
            return instance;
        }

        instance = FindObjectOfType<BgmPlayer>();
        if (instance != null)
        {
            instance.EnsureSource();
            return instance;
        }

        GameObject obj = new GameObject("BgmPlayer");
        instance = obj.AddComponent<BgmPlayer>();
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

    private void PlayInternal(BgmId id, float fadeSeconds)
    {
        if (id == BgmId.None)
        {
            StopInternal(fadeSeconds);
            return;
        }

        EnsureSource();
        EnsureBank();

        if (bank == null || !bank.TryGetClip(id, out AudioClip clip, out float volume))
        {
            return;
        }

        float targetVolume = volume * masterVolume;
        if (currentId == id && source.clip == clip && source.isPlaying)
        {
            FadeTo(targetVolume, fadeSeconds);
            return;
        }

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        fadeRoutine = StartCoroutine(SwitchClip(clip, id, targetVolume, Mathf.Max(0f, fadeSeconds)));
    }

    private void StopInternal(float fadeSeconds)
    {
        EnsureSource();
        currentId = BgmId.None;
        FadeTo(0f, fadeSeconds, true);
    }

    private IEnumerator SwitchClip(AudioClip clip, BgmId id, float targetVolume, float fadeSeconds)
    {
        if (source.isPlaying && source.volume > 0f && fadeSeconds > 0f)
        {
            yield return FadeRoutine(source.volume, 0f, fadeSeconds * 0.5f, false);
        }

        source.clip = clip;
        source.loop = true;
        source.volume = 0f;
        source.Play();
        currentId = id;

        yield return FadeRoutine(0f, targetVolume, fadeSeconds, false);
        fadeRoutine = null;
    }

    private void FadeTo(float targetVolume, float fadeSeconds, bool stopWhenSilent = false)
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        fadeRoutine = StartCoroutine(FadeRoutine(source.volume, targetVolume, Mathf.Max(0f, fadeSeconds), stopWhenSilent));
    }

    private IEnumerator FadeRoutine(float from, float to, float seconds, bool stopWhenSilent)
    {
        if (seconds <= 0f)
        {
            source.volume = to;
        }
        else
        {
            float elapsed = 0f;
            while (elapsed < seconds)
            {
                elapsed += Time.unscaledDeltaTime;
                source.volume = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / seconds));
                yield return null;
            }
        }

        source.volume = to;
        if (stopWhenSilent && Mathf.Approximately(to, 0f))
        {
            source.Stop();
            source.clip = null;
        }

        fadeRoutine = null;
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
        source.loop = true;
        source.spatialBlend = 0f;
    }

    private void EnsureBank()
    {
        if (bank == null)
        {
            bank = Resources.Load<BgmBank>(ResourceBankPath);
        }
    }
}
