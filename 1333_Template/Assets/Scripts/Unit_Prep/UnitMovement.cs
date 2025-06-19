using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles grid-based pathfinding, rotation, and translation for a UnitBase.
/// Dependencies are injected from UnitBase.Initialize().
/// </summary>
[RequireComponent(typeof(UnitBase))]
public class UnitMovement : MonoBehaviour
{
    // ----- Injected at runtime -----
    private UnitBase _unit;
    private GridManager _gridManager;
    private AStarPathfinder _pathfinder;

    // ----- Movement settings -----
    private float _moveSpeed = 5f;
    private float _rotationSpeed = 360f;

    // ----- Path data -----
    private List<Vector2Int> _path;
    private int _nextIndex;
    private GridNode _reservedDest;

    // ----- Gizmo toggle -----
    public static bool ShowPathGizmos = false;

    // ---------- Initialization ----------
    private void Awake() => _unit = GetComponent<UnitBase>();

    /// <summary>
    /// Injects managers and speed settings from UnitBase.Initialize().
    /// </summary>
    public void Init(GridManager grid, AStarPathfinder pathfinder,
                     float moveSpeed, float rotationSpeed)
    {
        _gridManager = grid;
        _pathfinder = pathfinder;
        _moveSpeed = moveSpeed;
        _rotationSpeed = rotationSpeed;
    }

    // ---------- MonoBehaviour ----------
    private void Update()
    {
        if (_unit.CurrentState == UnitState.Moving)
            HandleMovement();
    }

    // ---------- Public entry ----------
    public void MoveTo(GridNode targetNode)
    {
        if (_gridManager == null || _pathfinder == null) return;
        if (_gridManager.IsNodeReserved(targetNode)) return;

        // Release current cell
        GridNode startNode = _gridManager.getNodeFromWorldPosition(transform.position);
        int sx = Mathf.RoundToInt(startNode.worldPosition.x / _gridManager.GridSettings.NodeSize);
        int sy = Mathf.RoundToInt(startNode.worldPosition.z / _gridManager.GridSettings.NodeSize);
        _gridManager.SetWalkable(sx, sy, true);
        _gridManager.UnreserveNode(startNode);

        // Pathfinding
        _path = _pathfinder.FindPathWithNodes(
            startNode, targetNode, _unit.Width, _unit.Height);

        if (_path == null || _path.Count == 0) return;

        // Reserve destination
        _gridManager.ReserveNode(targetNode);
        _reservedDest = targetNode;
        _nextIndex = 0;

        _unit.InternalChangeState(UnitState.Moving);
    }

    // ---------- Movement loop ----------
    private void HandleMovement()
    {
        if (_path == null || _nextIndex >= _path.Count) return;

        // Get node under next grid coordinate
        Vector2Int coords = _path[_nextIndex];
        GridNode node = _gridManager.GetNode(coords.x, coords.y);
        Vector3 targetPos = node.worldPosition + Vector3.up * 0.1f;

        // Rotate toward waypoint
        Vector3 dir = targetPos - transform.position; dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion look = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, look,
                _rotationSpeed * Time.deltaTime);
        }

        // Translate
        transform.position = Vector3.MoveTowards(
            transform.position, targetPos,
            _moveSpeed * Time.deltaTime);

        // Waypoint reached?
        if (Vector3.Distance(transform.position, targetPos) < 0.05f)
            _nextIndex++;

        // Path finished
        if (_nextIndex >= _path.Count)
            FinishMovement();
    }

    private void FinishMovement()
    {
        if (_reservedDest != null)
        {
            _gridManager.UnreserveNode(_reservedDest);

            Vector3 wp = _reservedDest.worldPosition;
            int tx = Mathf.RoundToInt(wp.x / _gridManager.GridSettings.NodeSize);
            int ty = Mathf.RoundToInt(wp.z / _gridManager.GridSettings.NodeSize);
            _gridManager.SetWalkable(tx, ty, false);

            _reservedDest = null;
        }

        _path = null;
        _unit.InternalChangeState(UnitState.Idle);
    }

    private void OnDisable()
    {
        if (_reservedDest != null && _gridManager != null)
            _gridManager.UnreserveNode(_reservedDest);
    }

    // ---------- Gizmos ----------
    private void OnDrawGizmos()
    {
        if (!ShowPathGizmos || _path == null || _gridManager == null) return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < _path.Count - 1; i++)
        {
            Vector3 a = _gridManager.GetNode(_path[i].x, _path[i].y).worldPosition + Vector3.up * 0.1f;
            Vector3 b = _gridManager.GetNode(_path[i + 1].x, _path[i + 1].y).worldPosition + Vector3.up * 0.1f;
            Gizmos.DrawLine(a, b);
        }

        // Start cube
        Gizmos.color = Color.cyan;
        Vector3 startPos = _gridManager.GetNode(_path[0].x, _path[0].y).worldPosition + Vector3.up * 0.1f;
        Gizmos.DrawCube(startPos, Vector3.one * (_gridManager.GridSettings.NodeSize * 0.8f));

        // End cube
        Gizmos.color = Color.green;
        Vector3 endPos = _gridManager.GetNode(_path[^1].x, _path[^1].y).worldPosition + Vector3.up * 0.1f;
        Gizmos.DrawCube(endPos, Vector3.one * (_gridManager.GridSettings.NodeSize * 0.8f));
    }
}
