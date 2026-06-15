using System;

public static class AudioBus
{
    public static event Action<AudioType, float> OnSFXRequested;
    public static event Action<AudioType, float, bool> OnMusicRequested;
    public static event Action<bool> OnStopMusicRequested;

    public static void PlaySFX(AudioType sound, float volume = 1f)
    {
        OnSFXRequested?.Invoke(sound, volume);
    }

    public static void PlayMusic(AudioType music, float volume = 1f, bool loop = true)
    {
        OnMusicRequested?.Invoke(music, volume, loop);
    }

    public static void StopMusic(bool stopAll = false)
    {
        OnStopMusicRequested?.Invoke(stopAll);
    }
}