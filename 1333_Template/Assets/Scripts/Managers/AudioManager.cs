using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System.Collections.Generic;

/// <summary>
/// AudioManager is a Singleton that manages all FMOD audio in the game.
/// It handles music, ambience, sound effects, and event instances.
/// Area-related enums and triggers have been removed for simplicity.
/// </summary>
public class AudioManager : Singleton<AudioManager>
{
    // Active FMOD event instances for cleanup
    private readonly List<EventInstance> _eventInstances = new List<EventInstance>();
    // StudioEventEmitters for cleanup
    private readonly List<StudioEventEmitter> _eventEmitters = new List<StudioEventEmitter>();
    // Optional 3D attachment
    private readonly Dictionary<EventInstance, Transform> _eventInstancePositions = new Dictionary<EventInstance, Transform>();

    private EventInstance _musicInstance;
    private EventInstance _ambienceInstance;

    [Header("Volume Settings")]
    [Range(0, 1)] public float masterVolume = 1f;
    [Range(0, 1)] public float musicVolume = 1f;
    [Range(0, 1)] public float ambienceVolume = 1f;
    [Range(0, 1)] public float sfxVolume = 1f;

    private Bus _masterBus;
    private Bus _musicBus;
    private Bus _ambienceBus;
    private Bus _sfxBus;

    /// <summary>
    /// Initialize FMOD buses and internal collections.
    /// </summary>
    private void Awake()
    {
        _masterBus = RuntimeManager.GetBus("bus:/");
        _musicBus = RuntimeManager.GetBus("bus:/Music");
        _ambienceBus = RuntimeManager.GetBus("bus:/Ambience");
        _sfxBus = RuntimeManager.GetBus("bus:/SFX");
    }

    /// <summary>
    /// Update volumes and 3D positions each frame.
    /// </summary>
    private void Update()
    {
        _masterBus.setVolume(masterVolume);
        _musicBus.setVolume(musicVolume);
        _ambienceBus.setVolume(ambienceVolume);
        _sfxBus.setVolume(sfxVolume);

        if (_eventInstancePositions.Count > 0)
        {
            foreach (var kvp in _eventInstancePositions)
            {
                var inst = kvp.Key;
                var trans = kvp.Value;
                if (inst.isValid() && trans != null)
                    inst.set3DAttributes(RuntimeUtils.To3DAttributes(trans.position));
            }
        }
    }

    /// <summary>
    /// Play a one-shot sound effect at the given world position.
    /// </summary>
    public void PlayOneShot(EventReference sound, Vector3 position)
    {
        RuntimeManager.PlayOneShot(sound, position);
    }

    /// <summary>
    /// Create an FMOD event instance, optionally attached to a Transform for 3D.
    /// </summary>
    public EventInstance CreateEventInstance(EventReference reference, Transform attachTo = null)
    {
        var inst = RuntimeManager.CreateInstance(reference);
        _eventInstances.Add(inst);

        if (attachTo != null)
        {
            _eventInstancePositions[inst] = attachTo;
            inst.set3DAttributes(RuntimeUtils.To3DAttributes(attachTo.position));
        }

        return inst;
    }

    /// <summary>
    /// Initialize and start background music.
    /// </summary>
    public void PlayMusic(EventReference musicRef)
    {
        StopMusic(true);
        _musicInstance = CreateEventInstance(musicRef);
        _musicInstance.start();
    }

    /// <summary>
    /// Stop current music playback.
    /// </summary>
    public void StopMusic(bool immediate = false)
    {
        if (_musicInstance.isValid())
        {
            _musicInstance.stop(immediate ? STOP_MODE.IMMEDIATE : STOP_MODE.ALLOWFADEOUT);
            _musicInstance.release();
            _musicInstance.clearHandle();
        }
    }

    /// <summary>
    /// Set a parameter on the music event instance.
    /// </summary>
    public void SetMusicParameter(string name, float value)
    {
        if (_musicInstance.isValid())
            _musicInstance.setParameterByName(name, value);
    }

    /// <summary>
    /// Initialize and start ambience.
    /// </summary>
    public void PlayAmbience(EventReference ambienceRef)
    {
        StopAmbience(true);
        _ambienceInstance = CreateEventInstance(ambienceRef);
        _ambienceInstance.start();
    }

    /// <summary>
    /// Stop current ambience playback.
    /// </summary>
    public void StopAmbience(bool immediate = false)
    {
        if (_ambienceInstance.isValid())
        {
            _ambienceInstance.stop(immediate ? STOP_MODE.IMMEDIATE : STOP_MODE.ALLOWFADEOUT);
            _ambienceInstance.release();
            _ambienceInstance.clearHandle();
        }
    }

    /// <summary>
    /// Initialize or attach a StudioEventEmitter on a GameObject.
    /// </summary>
    public StudioEventEmitter InitializeEventEmitter(EventReference reference, GameObject go)
    {
        var emitter = go.GetComponent<StudioEventEmitter>() ?? go.AddComponent<StudioEventEmitter>();
        emitter.EventReference = reference;
        _eventEmitters.Add(emitter);
        return emitter;
    }

    /// <summary>
    /// Cleanup all FMOD instances and emitters.
    /// </summary>
    private void CleanUp()
    {
        foreach (var inst in _eventInstances)
        {
            if (inst.isValid())
            {
                inst.stop(STOP_MODE.IMMEDIATE);
                inst.release();
            }
        }
        _eventInstances.Clear();
        _eventInstancePositions.Clear();

        foreach (var emitter in _eventEmitters)
            emitter?.Stop();
        _eventEmitters.Clear();

        StopMusic(true);
        StopAmbience(true);
    }

    private void OnDestroy()
    {
        CleanUp();
    }
}
