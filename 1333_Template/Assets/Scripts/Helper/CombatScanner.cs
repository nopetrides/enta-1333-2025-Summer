using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Centralized component that manages combat scanning for all units,
/// spreading out expensive target-acquisition checks across multiple frames.
/// Ensures only a limited number of units scan per frame for better performance.
/// </summary>
public class CombatScanner : MonoBehaviour
{
    // Number of unit scans to process each frame
    [SerializeField] private int scansPerFrame = 20;

    // List of all registered UnitCombat components in the scene
    private readonly List<UnitCombat> _units = new();
    // Index of the next unit to scan (cycles through the list)
    private int _current;

    // Singleton instance for easy global access
    public static CombatScanner Instance { get; private set; }

    /// <summary>
    /// Standard singleton pattern; keeps only one scanner in the scene.
    /// Destroys duplicates and persists this object across scenes.
    /// </summary>
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Called once per frame. Processes up to scansPerFrame units,
    /// spreading scanning load evenly over time.
    /// </summary>
    private void Update()
    {
        if (_units.Count == 0) return;

        int processed = 0;
        while (processed < scansPerFrame)
        {
            _current %= _units.Count;
            var uc = _units[_current];
            _current++;
            processed++;

            // Skip if component missing or disabled
            if (uc == null || !uc.enabled) continue;
            uc.ScanOnce();
        }
    }

    /// <summary>
    /// Registers a UnitCombat instance to be scanned every cycle.
    /// </summary>
    public void Register(UnitCombat uc)
    {
        if (uc != null && !_units.Contains(uc)) _units.Add(uc);
    }

    /// <summary>
    /// Unregisters a UnitCombat instance (e.g., on death or disable).
    /// </summary>
    public void Unregister(UnitCombat uc) => _units.Remove(uc);
}
