using UnityEngine;
using FMODUnity;
using System.Collections.Generic;

/// <summary>
/// Manages a pool of SfxPlayers with voice limiting & priority pre-emption.
/// Provides 3D, 3D-attached, and 2D playback plus stop-by-handle.
/// </summary>
public class SfxPlayerPool : MonoBehaviour
{
    /* ============================== Handle ============================ */
    public readonly struct SfxHandle
    {
        public static readonly SfxHandle Invalid = new(-1);
        public int Id { get; }
        internal SfxHandle(int id) => Id = id;
        public bool IsValid() => Id >= 0;
    }

    /* ============================ Inspector =========================== */
    [Header("Pool Settings")]
    [SerializeField, Min(1)] private int _maxChannels = 32;

    /* ============================ Internals =========================== */
    private readonly List<SfxPlayer> _players = new();
    private readonly Queue<SfxPlayer> _idle = new();

    /* ============================ Awake =============================== */
    private void Awake()
    {
        for (int i = 0; i < _maxChannels; i++)
        {
            var p = new SfxPlayer { Id = i };
            _players.Add(p);
            _idle.Enqueue(p);
        }
    }

    /* ============================ Update ============================== */
    private void Update()
    {
        foreach (var p in _players)
            if (p.Update() && !_idle.Contains(p))
                _idle.Enqueue(p);
    }

    /* ====================== Public Playback API ======================= */

    public SfxHandle TryPlay3D(EventReference ev,
                               Vector3 pos,
                               SfxPriority pr = SfxPriority.Medium)
    {
        return InternalPlay(ev, pos, pr, null, spatial: true);
    }

    public SfxHandle TryPlayAttached(EventReference ev,
                                     Transform follow,
                                     SfxPriority pr = SfxPriority.Medium)
    {
        if (follow == null) return SfxHandle.Invalid;
        return InternalPlay(ev, follow.position, pr, follow, spatial: true);
    }

    public SfxHandle TryPlay2D(EventReference ev,
                               SfxPriority pr = SfxPriority.Medium)
    {
        return InternalPlay(ev, Vector3.zero, pr, null, spatial: false);
    }

    public void Stop(SfxHandle handle, bool immediate = false)
    {
        if (!handle.IsValid()) return;
        var p = _players[handle.Id];
        p.Stop(immediate);
        if (!_idle.Contains(p)) _idle.Enqueue(p);
    }

    /// Stop all voices immediately
    public void Shutdown()
    {
        foreach (var p in _players)
            p.Stop(true);
        _idle.Clear();
    }

    /* ======================= Internal Logic =========================== */

    private SfxHandle InternalPlay(EventReference ev,
                                   Vector3 pos,
                                   SfxPriority pr,
                                   Transform follow,
                                   bool spatial)
    {
        var player = AllocatePlayer(pr);
        if (player == null) return SfxHandle.Invalid;

        if (spatial)
            player.Play3D(ev, pos, pr, follow);
        else
            player.Play2D(ev, pr);

        return new SfxHandle(player.Id);
    }

    private SfxPlayer AllocatePlayer(SfxPriority requestPriority)
    {
        if (_idle.Count > 0) return _idle.Dequeue();

        // find worst active player
        SfxPlayer worst = null;
        foreach (var p in _players)
        {
            if (!p.IsPlaying) continue;
            if (worst == null ||
               p.Priority < worst.Priority ||
               (p.Priority == worst.Priority && p.Age() > worst.Age()))
                worst = p;
        }

        if (worst == null) return null; // should not happen

        if (requestPriority > worst.Priority ||
            (requestPriority == worst.Priority && worst.Age() > 0.2f))
        {
            worst.Stop(true);
            return worst;
        }

        return null; // drop
    }
}
