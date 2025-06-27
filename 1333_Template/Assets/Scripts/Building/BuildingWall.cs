// BuildingWall.cs
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A wall segment that can garrison one ranged unit on its top.
/// Works with the grid system: the garrisoned unit occupies the
/// single center cell and gains ×2 attack / vision range.
/// When the wall is destroyed, the unit is released to a nearby
/// walkable node.
/// </summary>
public class BuildingWall : BuildingBase
{
    [Header("Renderers for Selection")]
    [Tooltip("Assign specific mesh renderers to tint on selection")]
    [SerializeField] private Renderer[] _selectionRenderers;

    [Header("Wall Settings")]
    [Tooltip("World-space point where the garrisoned unit will spawn.")]
    [SerializeField] private Transform _spawnPoint = null;
    [SerializeField] private float _rangeBuff = 2f;
    [SerializeField] private Transform _defaultReturnPoint = null; 

    [Tooltip("Scan radius to look for friendly ranged units.")]
    [SerializeField] private float _scanRadius = 8f;

    private GridNode _prevNode;    // where the unit stood before garrison
    private Vector3 _prevPos;     // exact world position

    // runtime
    private UnitBase _garrisoned;                 // null = empty
    private GridNode _reservedCell;               // center node

    /* -------------------------------------------------------- */
    /*  Public API                                              */
    /* -------------------------------------------------------- */

    /// <summary>Returns true if a unit is already on the wall.</summary>
    public bool HasGarrison => _garrisoned != null;

    /// <summary>Returns a read-only list of ranged units in scan radius.</summary>
    public List<UnitBase> GetNearbyRangedUnits(UnitManager manager)
    {

        List<UnitBase> list = new List<UnitBase>();
        foreach (UnitBase u in manager.AllUnits)
        {
            if (u == null || u.Team != team) continue;
            if (u.GetComponent<UnitCombat>().IsGarrisoned) continue;
            if (u.UnitType.AttackType != AttackType.Ranged) continue;

            float d = Vector3.Distance(transform.position, u.transform.position);
            if (d <= _scanRadius) list.Add(u);
        }
        return list;
    }

    /// <summary>Garrisons the given unit on top of the wall.</summary>
    public void Garrison(UnitBase unit)
    {
        if (unit == null || HasGarrison) return;

        UnitMovement move = unit.GetComponent<UnitMovement>();
        _prevNode = _gridManager.getNodeFromWorldPosition(unit.transform.position);
        _prevPos = unit.transform.position;

        move.ReleaseOccupiedNode();

        /* 1) reserve the center cell */
        _reservedCell = _gridManager.getNodeFromWorldPosition(_spawnPoint.position);
        _gridManager.ReserveNode(_reservedCell);
        _gridManager.SetWalkable(
            Mathf.RoundToInt(_reservedCell.worldPosition.x / _gridManager.GridSettings.NodeSize),
            Mathf.RoundToInt(_reservedCell.worldPosition.z / _gridManager.GridSettings.NodeSize),
            false);

        /* 2) teleport the unit and disable movement */
        unit.transform.position = _spawnPoint.position;
        unit.GetComponent<UnitMovement>().Pause();        // freeze pathing
        UnitCombat combat = unit.GetComponent<UnitCombat>();
        combat?.SetRangeMultiplier(_rangeBuff);    // double range / vision
        combat?.SetAnchored(true);
        combat?.SetGarrisoned(true);

        _garrisoned = unit;
    }

    /* -------------------------------------------------------- */
    /*  Destruction override                                    */
    /* -------------------------------------------------------- */

    public override void DestroySelf()
    {
        ReleaseGarrison();

        base.DestroySelf();   // frees tiles and unregisters, then destroys GO
    }

    /* -------------------------------------------------------- */
    /*  Helpers                                                 */
    /* -------------------------------------------------------- */

    private void ReleaseGarrison()
    {
        Debug.Log("ReleaseGarrison!");
        if (_garrisoned == null) return;
        Debug.Log("Garrison is not null!");

        UnitMovement move = _garrisoned.GetComponent<UnitMovement>();
        move.Resume();

        /* 1) free the wall-top reservation BEFORE searching a destination */
        if (_reservedCell != null)
        {
            _gridManager.UnreserveNode(_reservedCell);
            _gridManager.SetWalkable(
                Mathf.RoundToInt(_reservedCell.worldPosition.x / _gridManager.GridSettings.NodeSize),
                Mathf.RoundToInt(_reservedCell.worldPosition.z / _gridManager.GridSettings.NodeSize),
                true);
            _reservedCell = null;
        }

        /* 2) choose return node */
        GridNode dst = null;

        // (a) original position if still free
        if (_prevNode != null && _prevNode.walkable && !_gridManager.IsNodeReserved(_prevNode))
            dst = _prevNode;

        // (b) explicit return point
        else if (_defaultReturnPoint != null)
            dst = _gridManager.getNodeFromWorldPosition(_defaultReturnPoint.position);

        // (c) nearest free node around wall
        if (dst == null || !dst.walkable)
        {
            GridNode here = _gridManager.getNodeFromWorldPosition(transform.position);
            List<GridNode> free = _gridManager.FindNearestFreeNodes(here, 1);
            if (free.Count > 0) dst = free[0];
        }

        /* 3) move or drop */
        if (dst != null)
        {
            // teleport to the destination node’s center
            _garrisoned.transform.position = dst.worldPosition;

            // ensure the grid knows this tile is now blocked by the unit
            move.ReleaseOccupiedNode();   // free any previous reservation
            move.PlanAndReserveDestination(dst);
            move.OccupyCurrentNode();     // sets walkable = false on dst

            _garrisoned.InternalChangeState(UnitState.Idle);  // reset anim state
        }
        else
        {
            // absolute fallback: drop slightly in front of the wall
            _garrisoned.transform.position = _prevPos + Vector3.forward * 0.2f;
            move.OccupyCurrentNode();
        }

        /* 4) reset buffs */
        UnitCombat combat = _garrisoned.GetComponent<UnitCombat>();
        combat?.SetRangeMultiplier(1f);
        combat?.SetAnchored(false);
        combat?.SetGarrisoned(false);

        _garrisoned = null;
    }

    /// <summary>Releases the garrisoned unit, if any.</summary>
    public void Ungarrison()
    {
        ReleaseGarrison();          // call the private helper you already have
    }

    public override void OnSelected()
    {
        ShowHpBar();
        // highlight the wall—replace with your own visuals if needed
        foreach (Renderer r in _selectionRenderers)
            if (r != null) r.material.color = Color.gray;
    }

    public override void OnDeselected()
    {
        HideHpBar();
        ApplyTeamMaterial();   // restore original team tint
    }
}
