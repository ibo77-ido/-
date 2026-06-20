using System;
using UnityEngine;

[CreateAssetMenu(fileName = "BgmBank", menuName = "Shared/Audio/BGM Bank")]
public sealed class BgmBank : ScriptableObject
{
    [SerializeField] private BgmEntry[] entries = Array.Empty<BgmEntry>();

    public bool TryGetClip(BgmId id, out AudioClip clip, out float volume)
    {
        for (int i = 0; i < entries.Length; i++)
        {
            if (entries[i].Id != id)
            {
                continue;
            }

            clip = entries[i].Clip;
            volume = Mathf.Clamp01(entries[i].Volume);
            return clip != null;
        }

        clip = null;
        volume = 1f;
        return false;
    }
}

[Serializable]
public struct BgmEntry
{
    public BgmId Id;
    public AudioClip Clip;
    [Range(0f, 1f)] public float Volume;
}
