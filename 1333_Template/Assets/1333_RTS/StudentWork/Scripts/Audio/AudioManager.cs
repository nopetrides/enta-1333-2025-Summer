using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("General")]
    [SerializeField] private AudioMixer mixer; // referance to the mixer

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;        // for sfx
    [SerializeField] private AudioSource musicSource;      // for loopong music and ambient

    [Header("Audio Clips")]
    [SerializeField] private AudioClip buttonClick;   // audio clips
    [SerializeField] private AudioClip unitAttack;
    [SerializeField] private AudioClip unitHit;
    [SerializeField] private AudioClip buildingPlaced;
    [SerializeField] private AudioClip buildingSelected;
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip gameAmbience;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }


    public void PlaySFX(AudioClip clip)       // to play a one shot sound effect
    {
        if (clip) sfxSource.PlayOneShot(clip);
    }

  
    public void PlayButtonClick() => PlaySFX(buttonClick);    // for ui and gameplay events
    public void PlayBuildingPlaced() => PlaySFX(buildingPlaced);
    public void PlayUnitAttack() => PlaySFX(unitAttack);
    public void PlayUnitHit() => PlaySFX(unitHit);
    public void PlayBuildingSelected() => PlaySFX(buildingSelected);


    public void PlayMusic(AudioClip music, bool loop = true)       // music
    {
        if (musicSource.clip == music && musicSource.isPlaying) return;
        musicSource.clip = music;
        musicSource.loop = loop;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
        musicSource.clip = null;
    }

    public void PauseMusic() => musicSource.Pause();
    public void ResumeMusic() => musicSource.UnPause();


    public void SetMusicVolume(float volume) => mixer.SetFloat("MusicVol", Mathf.Log10(Mathf.Clamp01(volume)) * 20);      // mixer volume controls for ui settings
    public void SetSFXVolume(float volume) => mixer.SetFloat("SFXVol", Mathf.Log10(Mathf.Clamp01(volume)) * 20);
}
