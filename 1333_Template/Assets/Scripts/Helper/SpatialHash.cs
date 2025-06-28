using System.Collections.Generic;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>Light-weight fixed-grid spatial hash for quick neighbour queries.</summary>
public class SpatialHash
{
    private readonly float _cellSize;
    private readonly Dictionary<Vector2Int, List<UnitBase>> _buckets = new();

#if UNITY_EDITOR
    private const float _printInterval = 5f;   // seconds
    private double _lastPrintTime;
#endif

    public SpatialHash(float cellSize)
    {
        _cellSize = cellSize;

#if UNITY_EDITOR
        _lastPrintTime = EditorApplication.timeSinceStartup;
        EditorApplication.update += OnEditorUpdate;  // subscribe once
#endif
    }

#if UNITY_EDITOR
    /// <summary>
    /// Called by the Unity Editor every frame; prints bucket stats every _printInterval seconds.
    /// Removed automatically in a player build because UNITY_EDITOR is undefined.
    /// </summary>
    private void OnEditorUpdate()
    {
        double now = EditorApplication.timeSinceStartup;
        if (now - _lastPrintTime < _printInterval) return;
        _lastPrintTime = now;

        int bucketCount = _buckets.Count;
        int unitTotal = 0;

        foreach (var list in _buckets.Values)
            unitTotal += list.Count;

        float avg = bucketCount > 0 ? (float)unitTotal / bucketCount : 0f;
        Debug.Log($"[SpatialHash] Buckets: {bucketCount} | Units: {unitTotal} | Avg/Bucket: {avg:F2}");
    }
#endif

    private Vector2Int Hash(Vector3 p) =>
        new(Mathf.FloorToInt(p.x / _cellSize),
            Mathf.FloorToInt(p.z / _cellSize));

    /// <summary>
    /// Returns the grid hash (bucket key) for a world position without any look-up.
    /// This is lightweight and can be used to decide whether the unit changed cell.
    /// </summary>
    public Vector2Int GetHashFast(Vector3 worldPos)
    {
        return new Vector2Int(
            Mathf.FloorToInt(worldPos.x / _cellSize),
            Mathf.FloorToInt(worldPos.z / _cellSize));
    }

    public void Add(UnitBase u)
    {
        Vector2Int h = Hash(u.transform.position);
        if (!_buckets.TryGetValue(h, out var list))
        {
            list = new List<UnitBase>();
            _buckets[h] = list;
        }
        list.Add(u);
    }

    public void Remove(UnitBase u)
    {
        Vector2Int h = Hash(u.transform.position);
        if (_buckets.TryGetValue(h, out var list)) list.Remove(u);
    }

    /// <summary>Returns all units in ‘range’ (rough-box query, ~8× faster than naive).</summary>
    public IEnumerable<UnitBase> Query(Vector3 pos, float range)
    {
        int r = Mathf.CeilToInt(range / _cellSize);
        Vector2Int c = Hash(pos);

        for (int y = -r; y <= r; y++)
            for (int x = -r; x <= r; x++)
                if (_buckets.TryGetValue(new Vector2Int(c.x + x, c.y + y), out var list))
                    foreach (var u in list) yield return u;
    }
}
