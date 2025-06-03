// UnitBase.cs
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract base class for all units in the game. Implements ISelectable for selection.
/// Holds common stats, state machine, grid-based movement logic (including smooth rotation),
/// and gizmo drawing for the path (colored squares at start/end). Path gizmos only draw while Moving.
/// </summary>
public abstract class UnitBase : MonoBehaviour, ISelectable
{
    [Header("References")]
    [Tooltip("The UnitType ScriptableObject that defines this unit's stats.")]
    [SerializeField] protected UnitType _unitType = null;

    [Tooltip("The helper class that handles animation for units.")]
    [SerializeField] protected UnitAnimHandler _AnimHandler;

    [Header("Movement Settings")]
    [Tooltip("How fast (in units/sec) the unit moves along the path.")]
    [SerializeField] protected float _moveSpeed = 5f;

    [Tooltip("How fast (in degrees/sec) the unit rotates to face its next waypoint.")]
    [SerializeField] protected float _rotationSpeed = 360f;

    // Current HP
    protected float _currentHp;

    // The path to follow (list of grid coordinates)
    protected List<Vector2Int> _currentPath = null;

    // Index of the next node in _currentPath
    protected int _nextPathIndex = 0;

    // Unit's current state (Idle, Moving, etc.)
    protected UnitState _state = UnitState.Idle;

    // Which team (Player or Enemy)
    protected Team _team;

    // Cached references
    protected GridManager _gridManager;
    protected AStarPathfinder _pathfinder;

    /// <summary>
    /// Static toggle that controls whether unit paths are drawn as gizmos.
    /// </summary>
    public static bool ShowPathGizmos = false;

    public virtual int Width => (_unitType != null) ? _unitType.Width : 1;
    public virtual int Height => (_unitType != null) ? _unitType.Height : 1;
    public UnitState CurrentState => _state;
    public Team UnitTeam => _team;
    public UnitType UnitType => _unitType;

    /// <summary>
    /// Initializes this unit with stats, GridManager, AStarPathfinder, and Team.
    /// Call immediately after instantiation.
    /// </summary>
    public virtual void Initialize(UnitType unitType, GridManager gridManager, AStarPathfinder pathfinder, Team team)
    {
        _unitType = unitType;
        _currentHp = unitType.MaxHp;
        _moveSpeed = unitType.MoveSpeed;
        _gridManager = gridManager;
        _pathfinder = pathfinder;
        _team = team;

        // Apply faction material
        Material mat = unitType.GetArmyMaterial(team);
        if (mat != null)
        {
            UnitHeadRef headRef = GetComponent<UnitHeadRef>();
            if (headRef != null && headRef.headRenderer != null)
            {
                headRef.headRenderer.material = mat;
            }
        }
    }

    protected virtual void Update()
    {
        if (_state == UnitState.Moving)
        {
            HandleMovement();
        }
    }

    /// <summary>
    /// Moves this unit to the specified target node.
    /// Uses AStarPathfinder.FindPathWithNodes to compute a List<Vector2Int> path.
    /// </summary>
    /// <param name="targetNode">The destination node on the grid.</param>
    public virtual void MoveTo(GridNode targetNode)
    {
        // Ensure GridManager and AStarPathfinder are available
        if (_gridManager == null || _pathfinder == null)
        {
            Debug.LogWarning($"SpearMan.MoveTo: Missing GridManager or AStarPathfinder on {name}.");
            return;
        }

        // 1) Determine the current grid node based on world position
        GridNode currentNode = _gridManager.getNodeFromWorldPosition(transform.position);
        if (currentNode.Equals(default(GridNode)))
        {
            Debug.LogWarning($"SpearMan.MoveTo: Could not identify current GridNode for {name}.");
            return;
        }

        // 2) Use AStarPathfinder.FindPathWithNodes to get a List<Vector2Int>
        List<Vector2Int> path = _pathfinder.FindPathWithNodes(
            currentNode,
            targetNode,
            Width,
            Height
        );

        // 3) If no path is found, log and return
        if (path == null || path.Count == 0)
        {
            Debug.Log($"SpearMan.MoveTo: No path found for {name} from {currentNode.name} to {targetNode.name}.");
            return;
        }

        // 4) Assign the new path and set state to Moving
        _currentPath = path;
        _nextPathIndex = 0;
        _state = UnitState.Moving;
        _AnimHandler.OnStateChanged(_state);
    }
    

    /// <summary>
    /// Shared movement + rotation logic: each frame, rotate toward the next waypoint and move forward.
    /// </summary>
    protected virtual void HandleMovement()
    {
        if (_currentPath == null || _nextPathIndex >= _currentPath.Count)
        {
            return;
        }

        // 1) Determine the next target node & its world position
        Vector2Int nextCoords = _currentPath[_nextPathIndex];
        GridNode nextNode = _gridManager.GetNode(nextCoords.x, nextCoords.y);
        Vector3 nextWorldPos = nextNode.worldPosition + Vector3.up * 0.1f;

        // 2) Compute horizontal direction toward the next waypoint
        Vector3 direction = nextWorldPos - transform.position;
        direction.y = 0f; // zero out vertical component
        if (direction.sqrMagnitude > Mathf.Epsilon)
        {
            // 3) Compute target rotation
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            // 4) Smoothly rotate toward that direction
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                _rotationSpeed * Time.deltaTime
            );
        }

        // 5) Once facing roughly toward the waypoint, move forward
        //    We can either always MoveTowards or check angle difference:
        float angleDifference = Vector3.Angle(transform.forward, direction);
        // If you want to wait until almost facing the correct way, uncomment below:
        // if (angleDifference > 10f) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            nextWorldPos,
            _moveSpeed * Time.deltaTime
        );

        // 6) Check if we've reached that waypoint
        if (Vector3.Distance(transform.position, nextWorldPos) < 0.01f)
        {
            _nextPathIndex++;
            if (_nextPathIndex >= _currentPath.Count)
            {
                // Arrived at final destination
                _state = UnitState.Idle;
                OnArrivedAtDestination(_state);
            }
        }
    }



    /// <summary>
    /// Called when the unit reaches the final node. Notify animation handler.
    /// </summary>
    protected virtual void OnArrivedAtDestination(UnitState unitState)
    {
        if (_AnimHandler != null)
            _AnimHandler.OnStateChanged(unitState);
    }

    /// <summary>
    /// Draw path gizmos (cyan lines, red/green squares) while Moving.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!ShowPathGizmos || _currentPath == null || _gridManager == null)
            return;
        if (_state != UnitState.Moving)
            return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < _currentPath.Count - 1; i++)
        {
            Vector2Int aCoords = _currentPath[i];
            Vector2Int bCoords = _currentPath[i + 1];
            GridNode aNode = _gridManager.GetNode(aCoords.x, aCoords.y);
            GridNode bNode = _gridManager.GetNode(bCoords.x, bCoords.y);
            Vector3 aPos = aNode.worldPosition + Vector3.up * 0.1f;
            Vector3 bPos = bNode.worldPosition + Vector3.up * 0.1f;
            Gizmos.DrawLine(aPos, bPos);
        }

        // Red square at start node
        Vector2Int startCoords = _currentPath[0];
        GridNode startNode = _gridManager.GetNode(startCoords.x, startCoords.y);
        Vector3 startCenter = startNode.worldPosition + Vector3.up * 0.1f;
        Gizmos.color = Color.red;
        Gizmos.DrawCube(startCenter, Vector3.one * (_gridManager.GridSettings.NodeSize * 0.8f));

        // Green square at end node
        Vector2Int endCoords = _currentPath[_currentPath.Count - 1];
        GridNode endNode = _gridManager.GetNode(endCoords.x, endCoords.y);
        Vector3 endCenter = endNode.worldPosition + Vector3.up * 0.1f;
        Gizmos.color = Color.green;
        Gizmos.DrawCube(endCenter, Vector3.one * (_gridManager.GridSettings.NodeSize * 0.8f));
    }
}

// UnitState enum remains unchanged
public enum UnitState
{
    Idle,
    Moving,
    Attacking,
    Patrolling,
    Dead
}
