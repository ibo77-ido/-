using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SfxBank", menuName = "Shared/Audio/SFX Bank")]
public sealed class SfxBank : ScriptableObject
{
    [SerializeField] private SfxEntry[] entries = Array.Empty<SfxEntry>();

    public bool TryGetClip(SfxId id, out AudioClip clip, out float volume)
    {
        int matchCount = 0;
        for (int i = 0; i < entries.Length; i++)
        {
            if (entries[i].Id == id && entries[i].Clip != null)
            {
                matchCount++;
            }
        }

        if (matchCount == 0)
        {
            clip = null;
            volume = 1f;
            return false;
        }

        int selected = UnityEngine.Random.Range(0, matchCount);
        for (int i = 0; i < entries.Length; i++)
        {
            if (entries[i].Id != id || entries[i].Clip == null)
            {
                continue;
            }

            if (selected > 0)
            {
                selected--;
                continue;
            }

            clip = entries[i].Clip;
            volume = Mathf.Clamp01(entries[i].Volume);
            return true;
        }

        clip = null;
        volume = 1f;
        return false;
    }
}

[Serializable]
public struct SfxEntry
{
    public SfxId Id;
    public AudioClip Clip;
    [Range(0f, 1f)] public float Volume;
}
