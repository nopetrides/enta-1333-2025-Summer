using UnityEngine;
using FMODUnity;
using FMOD.Studio;

/// <summary>
/// Single SFX channel that owns one FMOD EventInstance.
/// Supports 3D (positional / attached) and 2D playback.
/// </summary>
public sealed class SfxPlayer
{
    // ---------- Constants ----------
    private const float POSITION_EPS = 0.1f;            // meters
    private const float POSITION_EPS_SQR = POSITION_EPS * POSITION_EPS;

    // ---------- Identity ----------
    internal int Id { get; set; }                       // set by pool

    // ---------- Runtime state ----------
    private EventInstance _instance;
    private Transform _follow;          // for 3D-attached
    private Vector3 _lastPos;           // last sent position
    private bool _isSpatial;            // 3D vs 2D switch
    private bool _isPlaying;
    private float _startTime;
    private SfxPriority _priority;

    // ---------- Public props ----------
    public bool IsPlaying => _isPlaying;
    public SfxPriority Priority => _priority;
    public float Age() => Time.time - _startTime;

    /* =================================================================== */
    /*  Playback                                                           */
    /* =================================================================== */

    /// <summary>Play 3D sound at world position (optionally attached).</summary>
    public void Play3D(EventReference reference,
                       Vector3 position,
                       SfxPriority priority,
                       Transform follow = null)
    {
        Stop(true);

        _instance = RuntimeManager.CreateInstance(reference);

        _follow = follow;
        _lastPos = follow != null ? follow.position : position;
        _isSpatial = true;
        _priority = priority;
        _startTime = Time.time;

        _instance.set3DAttributes(RuntimeUtils.To3DAttributes(_lastPos));
        _instance.start();
        _isPlaying = true;
    }

    /// <summary>Play 2D sound (no positional attributes).</summary>
    public void Play2D(EventReference reference,
                       SfxPriority priority)
    {
        Stop(true);

        _instance = RuntimeManager.CreateInstance(reference);
        _follow = null;
        _isSpatial = false;
        _priority = priority;
        _startTime = Time.time;

        _instance.start();
        _isPlaying = true;
    }

    /// <summary>
    /// Advance playback; recycle when finished and update 3D pos if needed.
    /// Returns true if finished.
    /// </summary>
    public bool Update()
    {
        if (!_isPlaying) return false;

        if (_isSpatial && _follow != null)
        {
            Vector3 cur = _follow.position;
            if ((cur - _lastPos).sqrMagnitude >= POSITION_EPS_SQR)
            {
                _instance.set3DAttributes(RuntimeUtils.To3DAttributes(cur));
                _lastPos = cur;
            }
        }

        _instance.getPlaybackState(out PLAYBACK_STATE state);
        if (state == PLAYBACK_STATE.STOPPED)
        {
            Stop(true);
            return true;
        }
        return false;
    }

    /// <summary>Stop playback.</summary>
    public void Stop(bool immediate = false)
    {
        if (!_instance.isValid()) return;

        _instance.stop(immediate ? STOP_MODE.IMMEDIATE : STOP_MODE.ALLOWFADEOUT);
        _instance.release();
        _instance.clearHandle();
        _isPlaying = false;
        _follow = null;
    }
}
