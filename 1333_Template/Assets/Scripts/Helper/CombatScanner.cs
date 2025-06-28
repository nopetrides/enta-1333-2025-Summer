using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Centralised scanner that spreads target-acquisition across frames.
/// </summary>
public class CombatScanner : MonoBehaviour
{
    [SerializeField] private int scansPerFrame = 20;

    private readonly List<UnitCombat> _units = new();
    private int _current;

    public static CombatScanner Instance { get; private set; }

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

            if (uc == null || !uc.enabled) continue;
            uc.ScanOnce();                
        }
    }

    public void Register(UnitCombat uc)
    {
        if (uc != null && !_units.Contains(uc)) _units.Add(uc);
    }

    public void Unregister(UnitCombat uc) => _units.Remove(uc);
}
