using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Light-weight fixed-grid spatial hash for quick neighbour queries.
/// </summary>
public class SpatialHash
{
    private readonly float _cellSize;
    private readonly Dictionary<Vector2Int, List<UnitBase>> _buckets = new();

#if UNITY_EDITOR
    private const float _printInterval = 5f; // seconds
    private double _lastPrintTime;
#endif

    // ------------------------------------------------------------------
    // ctor
    // ------------------------------------------------------------------
    public SpatialHash(float cellSize)
    {
        _cellSize = cellSize;

#if UNITY_EDITOR
        // If play mode is already running (domain reload off) start logging now
        if (EditorApplication.isPlaying)
        {
            _lastPrintTime = EditorApplication.timeSinceStartup;
            EditorApplication.update += OnEditorUpdate;
        }

        // Subscribe to play-mode changes to add/remove the update callback
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
#endif
    }

    // ------------------------------------------------------------------
    // Editor-only helpers (logging & clean subscribe / unsubscribe)
    // ------------------------------------------------------------------
#if UNITY_EDITOR
    private void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            _lastPrintTime = EditorApplication.timeSinceStartup;
            EditorApplication.update += OnEditorUpdate;
        }
        else if (state == PlayModeStateChange.ExitingPlayMode)
        {
            EditorApplication.update -= OnEditorUpdate;
        }
    }

    /// <summary>
    /// Called every Editor frame; logs bucket stats only during play.
    /// </summary>
    private void OnEditorUpdate()
    {
        if (!EditorApplication.isPlaying) return; // safety

        double now = EditorApplication.timeSinceStartup;
        if (now - _lastPrintTime < _printInterval) return;
        _lastPrintTime = now;

        int bucketCount = _buckets.Count;
        int unitTotal = 0;
        foreach (var list in _buckets.Values) unitTotal += list.Count;

        float avg = bucketCount > 0 ? (float)unitTotal / bucketCount : 0f;
        Debug.Log($"[SpatialHash] Buckets: {bucketCount} | Units: {unitTotal} | Avg/Bucket: {avg:F2}");
    }
#endif

    // ------------------------------------------------------------------
    // Public API
    // ------------------------------------------------------------------

    /// <summary>
    /// Returns the bucket key for a world position. Cheap hash, no lookup.
    /// </summary>
    public Vector2Int GetHashFast(Vector3 worldPos) =>
        new Vector2Int(
            Mathf.FloorToInt(worldPos.x / _cellSize),
            Mathf.FloorToInt(worldPos.z / _cellSize));

    public void Add(UnitBase u)
    {
        Vector2Int h = GetHashFast(u.transform.position);
        if (!_buckets.TryGetValue(h, out var list))
        {
            list = new List<UnitBase>();
            _buckets[h] = list;
        }
        list.Add(u);
    }

    public void Remove(UnitBase u)
    {
        Vector2Int h = GetHashFast(u.transform.position);
        if (_buckets.TryGetValue(h, out var list))
        {
            list.Remove(u);
            if (list.Count == 0) _buckets.Remove(h);  
        }
    }

    public void Remove(UnitBase u, Vector2Int bucketKey)
    {
        if (_buckets.TryGetValue(bucketKey, out var list))
        {
            list.Remove(u);
            if (list.Count == 0) _buckets.Remove(bucketKey); 
        }
    }


    /// <summary>
    /// Rough box query: returns every unit roughly inside range.
    /// </summary>
    public IEnumerable<UnitBase> Query(Vector3 pos, float range)
    {
        int r = Mathf.CeilToInt(range / _cellSize);
        Vector2Int c = GetHashFast(pos);

        for (int y = -r; y <= r; y++)
            for (int x = -r; x <= r; x++)
                if (_buckets.TryGetValue(new Vector2Int(c.x + x, c.y + y), out var list))
                    foreach (var u in list) yield return u;
    }
}
