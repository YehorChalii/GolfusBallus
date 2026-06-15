using System;
using UnityEngine;

public enum AudioType
{
    Music_Main,
    Music_Wind,
    Music_GolfBallWins,
    Music_MudMonsterWins,

    SFX_GolfBallLaunch,
    SFX_GolfBallCharge,
    SFX_MudShot,
    SFX_MudSplash,

    UI_ButtonPress,
}

public class AudioManager : MonoBehaviour
{
    [Header("Sounds")]
    [SerializeField] private AudioList[] _audioList;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource _mainMusicSource;
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioSource _sfxSource;

    private void OnEnable()
    {
        AudioBus.OnSFXRequested += HandlePlaySFX;
        AudioBus.OnMusicRequested += HandlePlayMusic;
        AudioBus.OnStopMusicRequested += HandleStopMusic;
    }

    private void OnDisable()
    {
        AudioBus.OnSFXRequested -= HandlePlaySFX;
        AudioBus.OnMusicRequested -= HandlePlayMusic;
        AudioBus.OnStopMusicRequested -= HandleStopMusic;
    }

    private void Start()
    {
        HandlePlayMusic(AudioType.Music_Main, 0.4f, true);
    }

    private void HandlePlaySFX(AudioType sound, float volume)
    {
        AudioClip[] clips = GetClipsForType(sound);
        if (clips == null || clips.Length == 0) return;

        AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];
        _sfxSource.PlayOneShot(randomClip, volume);
    }

    private void HandlePlayMusic(AudioType music, float volume, bool loop)
    {
        AudioClip[] clips = GetClipsForType(music);
        if (clips == null || clips.Length == 0) return;

        AudioSource targetSource = (music == AudioType.Music_Main) ? _mainMusicSource : _musicSource;

        targetSource.clip = clips[0];
        targetSource.volume = volume;
        targetSource.loop = loop;
        targetSource.Play();
    }

    private void HandleStopMusic(bool stopAll)
    {
        _musicSource.Stop();
        if (stopAll) _mainMusicSource.Stop();
    }

    private AudioClip[] GetClipsForType(AudioType type)
    {
        int index = (int)type;
        if (index < 0 || index >= _audioList.Length) return null;
        return _audioList[index].Sounds;
    }
}

[Serializable]
public struct AudioList
{
    [SerializeField] private AudioClip[] sounds;

    public AudioClip[] Sounds => sounds;
}