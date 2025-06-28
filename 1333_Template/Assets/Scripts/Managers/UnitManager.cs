using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages all units and buildings in the scene. Other managers
/// (for example SelectionManager) can query these lists. This class
/// is no longer a singleton; inject it through GameManager.
/// </summary>
public class UnitManager : MonoBehaviour
{
    // Internal list of all registered buildings.
    private readonly List<BuildingBase> _allBuildings = new();

    // Internal list of all registered units.
    private readonly List<UnitBase> _allUnits = new List<UnitBase>();

    [SerializeField] private LayerMask _visionBlockMask; // Obstacle layer

    /// <summary>Read-only view of all currently registered units.</summary>
    public IReadOnlyList<UnitBase> AllUnits => _allUnits;

    /// <summary>Read-only view of all currently registered buildings.</summary>
    public IReadOnlyList<IDamageable> AllBuildings => _allBuildings;

    [SerializeField] private float _spatialCellSize = 1.5f;  
    private SpatialHash _spatial;
    public SpatialHash Spatial => _spatial;

    // ---------------------------------------------------------------------
    // DEBUG SETTINGS
    // ---------------------------------------------------------------------

    [Header("Debug")]
    [Tooltip("When true, the registry is printed every frame in Update.")]
    [SerializeField] private bool _logRegistryEveryFrame = false;

    [Tooltip("Seconds between logs. Set to 0 to log every frame.")]
    [SerializeField] private float _logInterval = 0.5f; // keep console usable

    private float _logTimer = 0f;

    // ---------------------------------------------------------------------
    // MonoBehaviour
    // ---------------------------------------------------------------------
    private void Awake()
    {
        _spatial = new SpatialHash(_spatialCellSize); // SpatialHash generate
    }

    private void Update()
    {
        if (!_logRegistryEveryFrame) return;

        // Throttle to _logInterval seconds.
        _logTimer += Time.deltaTime;
        if (_logTimer < _logInterval) return;
        _logTimer = 0f;

        PrintRegistryDebug();
    }

    // ---------------------------------------------------------------------
    // Public API
    // ---------------------------------------------------------------------

    /// <summary>
    /// Writes the current contents of _allUnits and _allBuildings to the console.
    /// </summary>
    public void PrintRegistryDebug()
    {
        const string header = "UnitManager Registry Dump";
        System.Text.StringBuilder sb = new System.Text.StringBuilder(header).AppendLine();

        // Units
        sb.AppendLine($"Units ({_allUnits.Count})");
        foreach (UnitBase u in _allUnits)
        {
            if (u == null) continue;
            sb.AppendLine($" - {u.name} | Team: {u.UnitTeam} | State: {u.CurrentState}");
        }

        // Buildings
        sb.AppendLine($"\nBuildings ({_allBuildings.Count})");
        foreach (BuildingBase b in _allBuildings)
        {
            if (b == null) continue;
            sb.AppendLine($" - {b.Tr.name} | Team: {b.Team} | HP: {b.CurrentHealth}");
        }

        Debug.Log(sb.ToString(), this); // ping UnitManager in console
    }

    /// <summary>Call this when a new unit spawns in the scene.</summary>
    public void RegisterUnit(UnitBase unit)
    {
        if (unit != null && !_allUnits.Contains(unit))
        {
            _allUnits.Add(unit);
        }
    }

    /// <summary>Call this when a unit dies and is destroyed.</summary>
    public void UnregisterUnit(UnitBase unit)
    {
        _allUnits.Remove(unit);
    }

    /*public UnitBase FindNearestEnemy(UnitBase seeker, float range)
    {
        float bestDist = float.MaxValue;
        UnitBase best = null;

        Vector3 seekerPos = seeker.transform.position;

        foreach (UnitBase u in _allUnits)
        {
            if (u.UnitTeam == seeker.UnitTeam || u.CurrentState == UnitState.Dead)
                continue;

            float dist = Vector3.Distance(seekerPos, u.transform.position);
            if (dist > range || dist >= bestDist) continue;
            if (!HasLineOfSight(seekerPos, u.transform.position)) continue;

            bestDist = dist;
            best = u;
        }
        return best;
    }*/

    /// <summary>
    /// Returns true when no obstacle collider exists between the two points.
    /// A thin ray is cast at eye height (0.5 meters).
    /// </summary>
    private bool HasLineOfSight(Vector3 a, Vector3 b)
    {
        const float eyeHeight = 0.5f;
        Vector3 from = a + Vector3.up * eyeHeight;
        Vector3 to = b + Vector3.up * eyeHeight;

        // Physics.Linecast returns true when something is in the way.
        return !Physics.Linecast(from, to, _visionBlockMask);
    }

    public void RegisterBuilding(BuildingBase b)
    {
        if (b != null && !_allBuildings.Contains(b))
            _allBuildings.Add(b);
    }

    public void UnregisterBuilding(BuildingBase b) => _allBuildings.Remove(b);

    /// Returns a position representing the closest attackable point on the target.
    private Vector3 GetDamageablePos(IDamageable dmg, Vector3 from)
    {
        return (dmg is BuildingBase bb)
            ? bb.GetClosestEdgePoint(from)     // Building: Outside point
            : dmg.Tr.position;                 // Unit: Pivot
    }

    /// Looks for the nearest enemy in <paramref name="list"/> within <paramref name="range"/>.
    /// A garrisoned unit (on a wall) ignores LOS blocking so it can shoot over its own wall.
    private IDamageable FindNearest(
        UnitBase seeker,
        float range,
        List<IDamageable> list)
    {
        Team seekerTeam = seeker.UnitTeam;
        Vector3 seekerPos = seeker.transform.position;
        bool skipLOS = seeker.GetComponent<UnitCombat>()?.IsGarrisoned ?? false;

        float bestDist = float.MaxValue;
        IDamageable pick = null;

        foreach (var t in list)
        {
            if (t == null || !t.IsAlive || t.Team == seekerTeam) continue;

            Vector3 tgtPos = GetDamageablePos(t, seekerPos);
            float d = Vector3.Distance(seekerPos, tgtPos);
            if (d > range || d >= bestDist) continue;

            // Only units are blocked by obstacles and only when not garrisoned.
            if (t is UnitBase && !skipLOS)
            {
                if (!HasLineOfSight(seekerPos, tgtPos)) continue;
            }

            bestDist = d;
            pick = t;
        }
        return pick;
    }

    /// <summary>
    /// Returns nearest hostile unit inside 'range' using SpatialHash query.
    /// </summary>
    public UnitBase FindNearestEnemyUnit(UnitBase seeker, float range)
    {
        if (_spatial == null) return null;

        UnitBase closest = null;
        float bestSqr = range * range;
        Vector3 seekerPos = seeker.transform.position;

        foreach (UnitBase target in _spatial.Query(seekerPos, range))
        {
            // Same team / dead / self skip
            if (target == null || target == seeker || target.Team == seeker.Team || !target.IsAlive)
                continue;

            float sqr = (target.transform.position - seekerPos).sqrMagnitude;
            if (sqr < bestSqr)
            {
                // Optional: line-of-sight check only for final candidates
                if (!Physics.Linecast(seekerPos + Vector3.up * 0.5f,
                                      target.transform.position + Vector3.up * 0.5f,
                                      _visionBlockMask))
                {
                    bestSqr = sqr;
                    closest = target;
                }
            }
        }

        return closest;
    }

    public IDamageable FindNearestEnemyBuilding(UnitBase seeker, float range) =>
        FindNearest(seeker, range, _allBuildings.ConvertAll<IDamageable>(b => b));
}
