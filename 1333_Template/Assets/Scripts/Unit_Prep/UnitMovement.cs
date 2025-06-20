using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles pathfinding, rotation, and translation for a UnitBase.
/// Supports a two-phase flow: (1) PlanAndReserveDestination, (2) MoveTo.
/// </summary>
[RequireComponent(typeof(UnitBase))]
public class UnitMovement : MonoBehaviour
{
    // Injected references
    private UnitBase _unit;
    private GridManager _grid;
    private AStarPathfinder _pathfinder;

    // Settings
    private float _moveSpeed = 5f;
    private float _rotationSpeed = 360f;

    // Path / reservation
    private List<Vector2Int> _path;
    private int _nextIdx;
    private GridNode _reservedDest;

    private bool _isPaused = false;
    private GridNode _currentNode;   // node currently occupied
    public GridManager Grid => _grid;   // expose for external query

    // Gizmo flag
    public static bool ShowPathGizmos = false;

    // ---------- Init ----------
    private void Awake() => _unit = GetComponent<UnitBase>();

    public void Init(GridManager grid, AStarPathfinder pathfinder,
                     float moveSpeed, float rotationSpeed)
    {
        _grid = grid;
        _pathfinder = pathfinder;
        _moveSpeed = moveSpeed;
        _rotationSpeed = rotationSpeed;
    }

    /// <summary>
    /// Marks the node under the unit as occupied (walkable = false & reserved).
    /// Call once right after the unit is spawned.
    /// </summary>
    public void OccupyCurrentNode()
    {
        if (_grid == null) return;

        _currentNode = _grid.getNodeFromWorldPosition(transform.position);
        if (_currentNode == null) return;

        _grid.ReserveNode(_currentNode);    // prevent others reserving the same
    }

    public void Pause() { _isPaused = true; }   // called by UnitCombat
    public void Resume() { _isPaused = false; }   // called by UnitCombat

    // ---------- Two-phase reservation ----------
    public void PlanAndReserveDestination(GridNode node)
    {
        if (node == null || _grid == null) return;

        // If already reserved by someone else, skip
        if (_grid.IsNodeReserved(node) && _reservedDest != node)
            return;

        // Release previous plan if any
        if (_reservedDest != null && _reservedDest != node)
            _grid.UnreserveNode(_reservedDest);

        // Reserve new node
        _grid.ReserveNode(node);
        _reservedDest = node;
    }

    // ---------- Movement start ----------
    /// <summary>
    /// Starts movement toward a target node.  
    /// If the destination is already reserved by THIS unit, the node is temporarily
    /// un-reserved during pathfinding so A* sees it as walkable, then re-reserved
    /// after the path is found.
    /// </summary>
    public void MoveTo(GridNode targetNode)
    {
        if (_grid == null || _pathfinder == null || targetNode == null) return;

        // --- 0) Reject if someone else has already reserved this node -------------
        if (_grid.IsNodeReserved(targetNode) && _reservedDest != targetNode)
            return;

        // --- 1) Temporarily open the destination while we search ------------------
        bool reservedByMe = (_reservedDest == targetNode);
        if (reservedByMe)
            _grid.UnreserveNode(targetNode); // make it walkable for A*

        // --- 2) Free our current tile so others can include it in their planning --
        GridNode start = _grid.getNodeFromWorldPosition(transform.position);
        int sx = Mathf.RoundToInt(start.worldPosition.x / _grid.GridSettings.NodeSize);
        int sy = Mathf.RoundToInt(start.worldPosition.z / _grid.GridSettings.NodeSize);
        _grid.SetWalkable(sx, sy, true);
        _grid.UnreserveNode(start);

        // --- 3) Run A* ------------------------------------------------------------
        _path = _pathfinder.FindPathWithNodes(start, targetNode, _unit.Width, _unit.Height);

        // --- 4) Fail-safe: restore reservation if pathfinding failed --------------
        if (_path == null || _path.Count == 0)
        {
            if (reservedByMe) _grid.ReserveNode(targetNode);
            return;
        }

        // --- 5) Path found → firmly reserve the destination -----------------------
        _grid.ReserveNode(targetNode);
        _reservedDest = targetNode;

        _nextIdx = 0;
        _unit.InternalChangeState(UnitState.Moving);
    }

    // ---------- Update loop ----------
    private void Update()
    {
        if (_unit.CurrentState == UnitState.Moving && !_isPaused)
            HandleMovement();
    }

    private void HandleMovement()
    {
        if (_path == null || _nextIdx >= _path.Count) return;

        Vector2Int c = _path[_nextIdx];
        GridNode node = _grid.GetNode(c.x, c.y);
        Vector3 dest = node.worldPosition + Vector3.up * 0.1f;

        // Rotate
        Vector3 dir = dest - transform.position; dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion look = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, look, _rotationSpeed * Time.deltaTime);
        }

        // Move
        transform.position = Vector3.MoveTowards(
            transform.position, dest, _moveSpeed * Time.deltaTime);

        // Waypoint reached?
        if (Vector3.Distance(transform.position, dest) < 0.05f)
            _nextIdx++;

        if (_nextIdx >= _path.Count)
            FinishMovement();
    }

    private void FinishMovement()
    {
        if (_reservedDest != null)
        {
            _grid.UnreserveNode(_reservedDest);

            Vector3 wp = _reservedDest.worldPosition;
            int tx = Mathf.RoundToInt(wp.x / _grid.GridSettings.NodeSize);
            int ty = Mathf.RoundToInt(wp.z / _grid.GridSettings.NodeSize);
            _grid.SetWalkable(tx, ty, false);
            _currentNode = _grid.GetNode(tx, ty); // remember it for later release
            _reservedDest = null;
        }

        _path = null;
        _unit.InternalChangeState(UnitState.Idle);
    }

    public void ReleaseOccupiedNode()
    {
        if (_grid == null || _currentNode == null) return;

        int x = Mathf.RoundToInt(_currentNode.worldPosition.x / _grid.GridSettings.NodeSize);
        int y = Mathf.RoundToInt(_currentNode.worldPosition.z / _grid.GridSettings.NodeSize);
        _grid.SetWalkable(x, y, true);      // open path-finding again
        _grid.UnreserveNode(_currentNode);  // just in case

        _currentNode = null;
    }

    private void OnDisable()
    {
        if (_reservedDest != null && _grid != null)
            _grid.UnreserveNode(_reservedDest);
    }

    // ---------- Gizmos ----------
    private void OnDrawGizmos()
    {
        if (!ShowPathGizmos || _grid == null) return;

        var path = _path;

        if (path == null || path.Count == 0) return;

        int last = path.Count - 1;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < last; i++)
        {
            Vector3 a = _grid.GetNode(path[i].x, path[i].y).worldPosition + Vector3.up * 0.1f;
            Vector3 b = _grid.GetNode(path[i + 1].x, path[i + 1].y).worldPosition + Vector3.up * 0.1f;
            Gizmos.DrawLine(a, b);
        }

        // Draw destination cube
        Gizmos.color = Color.green;
        Vector3 endPos = _grid.GetNode(path[last].x, path[last].y).worldPosition + Vector3.up * 0.1f;
        Gizmos.DrawCube(endPos, Vector3.one * (_grid.GridSettings.NodeSize * 0.8f));
    }
}
