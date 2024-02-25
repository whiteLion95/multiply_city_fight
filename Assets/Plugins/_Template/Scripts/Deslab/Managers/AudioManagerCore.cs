using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManagerCore : Singleton<AudioManagerCore>
{
    [SerializeField] protected AudioSource audioSource;
    [SerializeField] protected List<AudioData> audioData;

    protected Dictionary<string, AudioClip> audioClips;

    protected override void Awake()
    {
        base.Awake();
        InitClips();
    }

    private void InitClips()
    {
        audioClips = new Dictionary<string, AudioClip>();

        foreach (AudioData data in audioData)
        {
            audioClips.Add(data.Name, data.Clip);
        }
    }

    public void PlayOneShot(string audioName)
    {
        AudioClip clip = audioClips[audioName];
        audioSource.PlayOneShot(clip);
    }

    public bool ToggleMute()
    {
        audioSource.mute = (audioSource.mute == true) ? audioSource.mute = false : audioSource.mute = true;
        return audioSource.mute;
    }
}

[System.Serializable]
public struct AudioData
{
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public AudioClip Clip { get; private set; }
}