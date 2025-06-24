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

    [SerializeField] private LayerMask _visionBlockMask; // Obstacle layer

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

    public UnitBase FindNearestEnemy(UnitBase seeker, float range)
    {
        float bestDist = float.MaxValue;
        UnitBase best = null;

        Vector3 seekerPos = seeker.transform.position;

        foreach (UnitBase u in _allUnits)
        {
            if (u.UnitTeam == seeker.UnitTeam || u.CurrentState == UnitState.Dead)
                continue;

            float dist = Vector3.Distance(seekerPos, u.transform.position);
            if (dist > range || dist >= bestDist)
                continue;

            if (!HasLineOfSight(seekerPos, u.transform.position))
                continue;

            bestDist = dist;
            best = u;
        }
        return best;
    }

    /// <summary>
    /// Returns true if there is no Obstacle collider between the two points.
    /// Casts a thin ray at ~0.5m height.
    /// </summary>
    private bool HasLineOfSight(Vector3 a, Vector3 b)
    {
        const float eyeHeight = 0.5f;          // adjust for your sprites
        Vector3 from = a + Vector3.up * eyeHeight;
        Vector3 to = b + Vector3.up * eyeHeight;

        // Hit returns true when *something* is in the way
        return !Physics.Linecast(from, to, _visionBlockMask);
    }
}
