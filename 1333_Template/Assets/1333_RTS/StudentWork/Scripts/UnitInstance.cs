using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitInstance : UnitBase, ISelectable
{
    [Header("Movement")]
    private float moveSpeed;

    private AStarPathfinding _pathfinder; // Reference to the pathfinding system
    private List<GridNode> _currentPath = new(); // The current path this unit will follow
    private int _pathIndex = 0; // Index of the current node in the path
    private bool _isMoving = false; // Flag to check if the unit is moving

    private Renderer cachedRenderer; // Cached reference to the unit's renderer

    public bool IsMoving => _isMoving;

    // Initializes the unit with pathfinding and unit type (speed, material, etc.)
    public void Initialize(AStarPathfinding pathfinder, UnitType unitType)
    {
        _pathfinder = pathfinder;
        _unitType = unitType;
        moveSpeed = unitType.moveSpeed;
        cachedRenderer = GetComponentInChildren<Renderer>();

        if (cachedRenderer != null)
            cachedRenderer.material = unitType.teamMaterial; // Set team material
    }

    private void Update()
    {
        // Don't move if not active, or path is invalid
        if (!_isMoving || _currentPath == null || _currentPath.Count == 0 || _pathIndex >= _currentPath.Count)
            return;

        // Move toward the next point on the path
        Vector3 nextWaypoint = _currentPath[_pathIndex].WorldPosition;
        Vector3 direction = (nextWaypoint - transform.position).normalized;
        float step = moveSpeed * Time.deltaTime;

        transform.position = Vector3.MoveTowards(transform.position, nextWaypoint, step);

        // If close enough to the waypoint, move to the next one
        if (Vector3.Distance(transform.position, nextWaypoint) < 0.05f)
        {
            _pathIndex++;
            if (_pathIndex >= _currentPath.Count)
            {
                _isMoving = false; // Reached end of path
            }
        }
    }

    // Converts a world position to a grid path and follows it
    public void SetTarget(Vector3 worldPosition)
    {
        _currentPath = _pathfinder.FindPath(transform.position, worldPosition, Width, Height);
        _pathIndex = 0;
        _isMoving = _currentPath != null && _currentPath.Count > 1;
    }

    // Overload: use a grid node directly instead of world position
    public void SetTarget(GridNode node)
    {
        SetTarget(node.WorldPosition);
    }

    // Public method for external movement command
    public override void MoveTo(GridNode targetNode)
    {
        SetTarget(targetNode);
    }

    // Highlight the unit when selected
    public void OnSelect()
    {
        if (cachedRenderer != null)
            cachedRenderer.material.color = Color.cyan;
    }

    // Reset color to team color when deselected
    public void OnDeselect()
    {
        if (cachedRenderer != null && _unitType != null && _unitType.teamMaterial != null)
            cachedRenderer.material.color = _unitType.teamMaterial.color;
    }

    // Get the unit’s name label
    public string GetLabel()
    {
        return _unitType.name;
    }

    // Draw gizmo lines along the current path (visible in Scene view)
    private void OnDrawGizmos()
    {
        if (_currentPath == null || _currentPath.Count < 2) return;

        Gizmos.color = Color.yellow;

        for (int i = 0; i < _currentPath.Count - 1; i++)
        {
            Gizmos.DrawLine(
                _currentPath[i].WorldPosition + Vector3.up * 0.2f,
                _currentPath[i + 1].WorldPosition + Vector3.up * 0.2f
            );
        }
    }
}
