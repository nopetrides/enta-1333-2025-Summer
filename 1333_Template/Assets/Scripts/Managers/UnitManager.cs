// UnitManager.cs
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages all units in the scene. Provides a central registry so other managers
/// (e.g. SelectionManager) can query all active units efficiently. No longer a singleton;
/// must be passed via dependency injection in GameManager.
/// </summary>
public class UnitManager : MonoBehaviour
{
    // Internal list of all registered units.
    private readonly List<UnitBase> _allUnits = new List<UnitBase>();

    /// <summary>
    /// Read-only view of all currently registered units.
    /// </summary>
    public IReadOnlyList<UnitBase> AllUnits => _allUnits;

    /// <summary>
    /// Call this when new unit spawns in game scene.
    /// </summary>
    /// <param name="unit">The unit instance to register.</param>
    public void RegisterUnit(UnitBase unit)
    {
        if (unit != null && !_allUnits.Contains(unit))
        {
            _allUnits.Add(unit);
        }
    }

    /// <summary>
    /// Call this when unit is dead and destroyed.
    /// </summary>
    /// <param name="unit">The unit instance to unregister.</param>
    public void UnregisterUnit(UnitBase unit)
    {
        _allUnits.Remove(unit);
    }
}
