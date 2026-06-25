using System;
using UnityEngine;
using UnityEngine.Rendering;

public enum SoundType
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

public class SoundManager : MonoBehaviour
{
    [SerializeField] private SoundList[] soundList;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource mainMusicSource;

    private static SoundManager _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
    }

#if UNITY_EDITOR
    private void OnEnable()
    {
        string[] names = Enum.GetNames(typeof(SoundType));
        Array.Resize(ref soundList, names.Length);

        for (int i = 0; i < soundList.Length; i++)
        {
            soundList[i].Name = names[i];
        }

    }
#endif

    public static void PlaySound(SoundType sound, float volume = 1)
    {
        AudioClip[] clips = _instance.soundList[(int)sound].Sounds;
        AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];

        _instance.sfxSource.PlayOneShot(randomClip, volume);
    }

    public static void PlayUISound()
    {
        AudioClip[] clips = _instance.soundList[(int)SoundType.UI_ButtonPress].Sounds;
        AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];

        _instance.sfxSource.PlayOneShot(randomClip, 0.8f);
    }

    public static void PlayMusic(SoundType music, float volume = 1f, bool loop = true)
    {
        AudioClip clip = _instance.soundList[(int)music].Sounds[0];

        _instance.musicSource.clip = clip;
        _instance.musicSource.volume = volume;
        _instance.musicSource.loop = loop;
        _instance.musicSource.Play();
    }

    public static void PlayMainMusic()
    {
        AudioClip clip = _instance.soundList[(int)SoundType.Music_Main].Sounds[0];

        _instance.mainMusicSource.clip = clip;
        _instance.mainMusicSource.loop = true;
        _instance.mainMusicSource.volume = 1f;
        _instance.mainMusicSource.Play();
    }

    public static void StopAllSounds()
    {
        _instance.musicSource.Stop();
        _instance.sfxSource.Stop();
    }

    public static void StopMainMusic()
    {
        _instance.mainMusicSource.Stop();
    }
}

[Serializable]
public struct SoundList
{
    [HideInInspector] public string Name;

    [SerializeField] private AudioClip[] sounds;
    public AudioClip[] Sounds { get => sounds; }
}