// UnitBase.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract base class for all units in the game. Implements ISelectable for selection.
/// Contains common stats, state machine, grid-based movement logic (including smooth rotation),
/// and gizmo drawing for the path (colored lines and cubes). Path gizmos only draw when Moving.
/// Includes a debug feature: pressing H will immediately kill this unit.
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

    // Current health points for this unit
    protected float _currentHp;

    // The current path to follow as a list of grid coordinates
    protected List<Vector2Int> _currentPath = null;

    // Index of the next node in _currentPath to move toward
    protected int _nextPathIndex = 0;

    // The current state of this unit (Idle, Moving, Attacking, Patrolling, or Dead)
    protected UnitState _state = UnitState.Idle;

    // The team this unit belongs to (Player or Enemy)
    protected Team _team;

    // References for pathfinding and unit management
    protected GridManager _gridManager;
    protected UnitManager _unitManager;
    protected AStarPathfinder _pathfinder;

    /// <summary>
    /// Static toggle that controls whether unit paths are drawn as gizmos.
    /// </summary>
    public static bool ShowPathGizmos = false;

    /// <summary>
    /// The width of the unit in grid cells, obtained from the UnitType.
    /// </summary>
    public virtual int Width => (_unitType != null) ? _unitType.Width : 1;

    /// <summary>
    /// The height of the unit in grid cells, obtained from the UnitType.
    /// </summary>
    public virtual int Height => (_unitType != null) ? _unitType.Height : 1;

    /// <summary>
    /// Exposes the current state of the unit.
    /// </summary>
    public UnitState CurrentState => _state;

    /// <summary>
    /// Exposes which team this unit belongs to.
    /// </summary>
    public Team UnitTeam => _team;

    /// <summary>
    /// Exposes the UnitType ScriptableObject for this unit.
    /// </summary>
    public UnitType UnitType => _unitType;

    /// <summary>
    /// Initializes this unit with its type, GridManager, UnitManager, AStarPathfinder, and team.
    /// Must be called immediately after instantiation.
    /// </summary>
    /// <param name="unitType">ScriptableObject containing stats for this unit.</param>
    /// <param name="gridManager">Reference to the GridManager in the scene.</param>
    /// <param name="unitManager">Reference to the UnitManager in the scene.</param>
    /// <param name="pathfinder">Shared AStarPathfinder instance.</param>
    /// <param name="team">The team (Player or Enemy) this unit belongs to.</param>
    public virtual void Initialize(
        UnitType unitType,
        GridManager gridManager,
        UnitManager unitManager,
        AStarPathfinder pathfinder,
        Team team)
    {
        _unitType = unitType;
        _currentHp = unitType.MaxHp;
        _moveSpeed = unitType.MoveSpeed;
        _gridManager = gridManager;
        _unitManager = unitManager;
        _pathfinder = pathfinder;
        _team = team;

        // Apply the correct material based on team color
        Material teamMaterial = unitType.GetArmyMaterial(team);
        if (teamMaterial != null)
        {
            UnitHeadRef headRef = GetComponent<UnitHeadRef>();
            if (headRef != null && headRef.headRenderer != null)
            {
                headRef.headRenderer.material = teamMaterial;
            }
        }
    }

    protected virtual void Update()
    {
        // Debug: If H is pressed, immediately kill this unit once
        if (Input.GetKeyDown(KeyCode.H) && _state != UnitState.Dead)
        {
            Die();
            return;
        }

        // If the unit is dead, do not process movement or other states
        if (_state == UnitState.Dead)
            return;

        // If currently moving, handle movement logic each frame
        if (_state == UnitState.Moving)
        {
            HandleMovement();
        }
    }

    /// <summary>
    /// Apply damage to this unit. If health drops to zero or below, trigger death.
    /// </summary>
    /// <param name="damageAmount">Amount of damage to apply.</param>
    public virtual void TakeDamage(float damageAmount)
    {
        if (_state == UnitState.Dead)
            return;

        _currentHp -= damageAmount;
        if (_currentHp <= 0f)
        {
            Die();
        }
    }

    /// <summary>
    /// Handle unit death: change state to Dead, notify animation handler, unregister from UnitManager,
    /// and destroy the GameObject after a delay to allow the death animation to play.
    /// </summary>
    protected virtual void Die()
    {
        if (_state == UnitState.Dead)
            return;

        _state = UnitState.Dead;

        // Notify the animation handler of the state change via Trigger
        if (_AnimHandler != null)
        {
            _AnimHandler.OnStateChanged(_state);
        }

        // Unregister this unit from the UnitManager
        if (_unitManager != null)
        {
            _unitManager.UnregisterUnit(this);
        }

        // Start coroutine to destroy this GameObject after a delay
        StartCoroutine(DestroyAfterDelay(5f));
    }

    /// <summary>
    /// Coroutine to destroy the GameObject after a specified delay.
    /// </summary>
    /// <param name="delay">Time in seconds to wait before destroying the object.</param>
    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    /// <summary>
    /// Move this unit toward the specified target node. Calculates a path using AStarPathfinder.
    /// Sets the unit state to Moving if a valid path is found.
    /// </summary>
    /// <param name="targetNode">Destination node on the grid.</param>
    public virtual void MoveTo(GridNode targetNode)
    {
        // Ensure that gridManager and pathfinder have been assigned
        if (_gridManager == null || _pathfinder == null)
        {
            Debug.LogWarning($"{name}.MoveTo: Missing GridManager or AStarPathfinder.");
            return;
        }

        // Determine which grid node the unit is currently over
        GridNode currentNode = _gridManager.getNodeFromWorldPosition(transform.position);
        if (currentNode.Equals(default(GridNode)))
        {
            Debug.LogWarning($"{name}.MoveTo: Could not identify current GridNode.");
            return;
        }

        // Compute a path from currentNode to targetNode using the unit's dimensions
        List<Vector2Int> path = _pathfinder.FindPathWithNodes(
            currentNode,
            targetNode,
            Width,
            Height);

        // If no path is found, log a message and exit
        if (path == null || path.Count == 0)
        {
            Debug.Log($"{name}.MoveTo: No path found from {currentNode.name} to {targetNode.name}.");
            return;
        }

        // Assign the path, reset the next index, and change state to Moving
        _currentPath = path;
        _nextPathIndex = 0;
        _state = UnitState.Moving;

        // Notify animation handler of new state
        if (_AnimHandler != null)
        {
            _AnimHandler.OnStateChanged(_state);
        }
    }

    /// <summary>
    /// Handles unit movement and rotation each frame while the unit is in the Moving state.
    /// Rotates toward the next waypoint, moves forward, and updates path index when a waypoint is reached.
    /// </summary>
    protected virtual void HandleMovement()
    {
        // If there is no path or we have already reached the final index, do nothing
        if (_currentPath == null || _nextPathIndex >= _currentPath.Count)
            return;

        // Get the next grid coordinates and corresponding world position
        Vector2Int nextCoords = _currentPath[_nextPathIndex];
        GridNode nextNode = _gridManager.GetNode(nextCoords.x, nextCoords.y);
        Vector3 nextWorldPos = nextNode.worldPosition + Vector3.up * 0.1f;

        // Calculate horizontal direction vector toward the next waypoint
        Vector3 direction = nextWorldPos - transform.position;
        direction.y = 0f; // Remove any vertical component

        if (direction.sqrMagnitude > Mathf.Epsilon)
        {
            // Compute target rotation to face the waypoint
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

            // Smoothly rotate toward the desired direction
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                _rotationSpeed * Time.deltaTime);
        }

        // Move forward along the calculated direction
        transform.position = Vector3.MoveTowards(
            transform.position,
            nextWorldPos,
            _moveSpeed * Time.deltaTime);

        // Check if the unit has reached (or is very close to) the waypoint
        if (Vector3.Distance(transform.position, nextWorldPos) < 0.01f)
        {
            _nextPathIndex++;
            // If we have reached the last waypoint, switch to Idle state
            if (_nextPathIndex >= _currentPath.Count)
            {
                _state = UnitState.Idle;
                OnArrivedAtDestination(_state);
            }
        }
    }

    /// <summary>
    /// Called when the unit reaches its final destination. Notifies the animation handler.
    /// </summary>
    /// <param name="unitState">The state that was just entered (usually Idle).</param>
    protected virtual void OnArrivedAtDestination(UnitState unitState)
    {
        if (_AnimHandler != null)
        {
            _AnimHandler.OnStateChanged(unitState);
        }
    }

    /// <summary>
    /// Draws path gizmos in the Scene view when ShowPathGizmos is true and the unit is Moving.
    /// Draws cyan lines between waypoints, a red cube at the start, and a green cube at the end.
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

        // Draw a red cube at the start node
        Vector2Int startCoords = _currentPath[0];
        GridNode startNode = _gridManager.GetNode(startCoords.x, startCoords.y);
        Vector3 startCenter = startNode.worldPosition + Vector3.up * 0.1f;
        Gizmos.color = Color.red;
        Gizmos.DrawCube(startCenter, Vector3.one * (_gridManager.GridSettings.NodeSize * 0.8f));

        // Draw a green cube at the end node
        Vector2Int endCoords = _currentPath[_currentPath.Count - 1];
        GridNode endNode = _gridManager.GetNode(endCoords.x, endCoords.y);
        Vector3 endCenter = endNode.worldPosition + Vector3.up * 0.1f;
        Gizmos.color = Color.green;
        Gizmos.DrawCube(endCenter, Vector3.one * (_gridManager.GridSettings.NodeSize * 0.8f));
    }
}

// UnitState enum
public enum UnitState
{
    Idle,
    Moving,
    Attacking,
    Patrolling,
    Dead
}
