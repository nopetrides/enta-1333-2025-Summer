// UnitBase.cs
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract base class for all units in the game. Implements ISelectable for selection.
/// Holds common stats, state machine, grid-based movement logic, and gizmo drawing for the path,
/// including colored squares at start and end points. Path gizmos only draw while unit is Moving.
/// </summary>
public abstract class UnitBase : MonoBehaviour, ISelectable
{
    [Header("References")]
    [Tooltip("The UnitType ScriptableObject that defines this unit's stats.")]
    [SerializeField] protected UnitType _unitType = null;

    // Current HP (initialized from _unitType.MaxHp)
    protected float _currentHp;

    // Movement speed (initialized from _unitType.MoveSpeed)
    protected float _moveSpeed;

    // Current path (list of grid coordinates) that this unit follows
    protected List<Vector2Int> _currentPath = null;

    // Index of the next node in _currentPath to move toward
    protected int _nextPathIndex = 0;

    // Unit's current state (Idle, Moving, etc.)
    protected UnitState _state = UnitState.Idle;

    // Which team this unit belongs to (Player or Enemy)
    protected Team _team;

    // Cached reference to GridManager (for world <-> grid conversions)
    protected GridManager _gridManager;

    // Cached reference to AStarPathfinder (for pathfinding)
    protected AStarPathfinder _pathfinder;

    /// <summary>
    /// Static toggle that controls whether unit paths are drawn as gizmos.
    /// Can be toggled in SelectionManager by pressing X.
    /// </summary>
    public static bool ShowPathGizmos = false;

    /// <summary>
    /// Width of this unit in grid cells (for multi-cell footprints). Defaults to 1 if _unitType is null.
    /// </summary>
    public virtual int Width => (_unitType != null) ? _unitType.Width : 1;

    /// <summary>
    /// Height of this unit in grid cells (for multi-cell footprints). Defaults to 1 if _unitType is null.
    /// </summary>
    public virtual int Height => (_unitType != null) ? _unitType.Height : 1;

    /// <summary>
    /// Current state of the unit (Idle, Moving, etc.).
    /// </summary>
    public UnitState CurrentState => _state;

    /// <summary>
    /// Which team this unit belongs to.
    /// </summary>
    public Team UnitTeam => _team;

    /// <summary>
    /// UnitType ScriptableObject containing this unit's stats.
    /// </summary>
    public UnitType UnitType => _unitType;

    /// <summary>
    /// Unity callback when this script instance is loaded. Initializes HP and move speed if unitType is set.
    /// </summary>
    protected virtual void Awake()
    {
        if (_unitType != null)
        {
            _currentHp = _unitType.MaxHp;
            _moveSpeed = _unitType.MoveSpeed;
        }
    }

    /// <summary>
    /// Initializes this unit with its stats, GridManager, AStarPathfinder, and Team.
    /// Must be called immediately after instantiating this GameObject.
    /// </summary>
    /// <param name="unitType">The ScriptableObject containing this unit's stats.</param>
    /// <param name="gridManager">Reference to the GridManager.</param>
    /// <param name="pathfinder">Reference to the AStarPathfinder instance.</param>
    /// <param name="team">Which Team (Player/Enemy) this unit belongs to.</param>
    public virtual void Initialize(UnitType unitType, GridManager gridManager, AStarPathfinder pathfinder, Team team)
    {
        _unitType = unitType;
        _currentHp = unitType.MaxHp;
        _moveSpeed = unitType.MoveSpeed;
        _gridManager = gridManager;
        _pathfinder = pathfinder;
        _team = team;

        // Apply the correct faction material based on team
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

    /// <summary>
    /// Unity callback every frame; handles movement if in the Moving state.
    /// </summary>
    protected virtual void Update()
    {
        if (_state == UnitState.Moving)
        {
            HandleMovement();
        }
    }

    /// <summary>
    /// Abstract method: move this unit to the specified target node (grid-based).
    /// Subclasses must implement the actual pathfinding call and change _state to Moving,
    /// as well as assign a new _currentPath and reset _nextPathIndex to zero.
    /// </summary>
    /// <param name="targetNode">GridNode that this unit should move toward.</param>
    public abstract void MoveTo(GridNode targetNode);

    /// <summary>
    /// Shared movement logic: moves along the _currentPath until destination is reached.
    /// </summary>
    protected virtual void HandleMovement()
    {
        if (_currentPath == null || _nextPathIndex >= _currentPath.Count)
        {
            return;
        }

        // Determine next target world position from the grid node
        Vector2Int nextCoords = _currentPath[_nextPathIndex];
        GridNode nextNode = _gridManager.GetNode(nextCoords.x, nextCoords.y);
        Vector3 nextWorldPos = nextNode.worldPosition + Vector3.up * 0.1f;

        // Move toward that position
        transform.position = Vector3.MoveTowards(
            transform.position,
            nextWorldPos,
            _moveSpeed * Time.deltaTime
        );

        // If nearly reached, advance to the next node
        if (Vector3.Distance(transform.position, nextWorldPos) < 0.01f)
        {
            _nextPathIndex++;
            if (_nextPathIndex >= _currentPath.Count)
            {
                // Arrived at the final destination
                _state = UnitState.Idle;
                OnArrivedAtDestination();
            }
        }
    }

    /// <summary>
    /// Called when the unit arrives at its final destination.
    /// Subclasses can override to perform arrival logic (e.g., attack or patrol).
    /// </summary>
    protected virtual void OnArrivedAtDestination()
    {
        // Default: remain idle
    }

    /// <summary>
    /// Draws the computed path as cyan lines in the Scene or Game view if ShowPathGizmos is enabled,
    /// but only while the unit is Moving. Also draws a small red square at the start node and
    /// a green square at the end node.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!ShowPathGizmos || _currentPath == null || _gridManager == null)
            return;

        // Only draw gizmos while the unit is still moving along its path.
        if (_state != UnitState.Moving)
            return;

        // Draw lines between consecutive nodes
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

        // Draw red square at the start node
        Vector2Int startCoords = _currentPath[0];
        GridNode startNode = _gridManager.GetNode(startCoords.x, startCoords.y);
        Vector3 startCenter = startNode.worldPosition + Vector3.up * 0.1f;
        Gizmos.color = Color.red;
        Gizmos.DrawCube(startCenter, Vector3.one * (_gridManager.GridSettings.NodeSize * 0.8f));

        // Draw green square at the end node
        Vector2Int endCoords = _currentPath[_currentPath.Count - 1];
        GridNode endNode = _gridManager.GetNode(endCoords.x, endCoords.y);
        Vector3 endCenter = endNode.worldPosition + Vector3.up * 0.1f;
        Gizmos.color = Color.green;
        Gizmos.DrawCube(endCenter, Vector3.one * (_gridManager.GridSettings.NodeSize * 0.8f));
    }
}

/// <summary>
/// States for a unit's state machine.
/// </summary>
public enum UnitState
{
    Idle,
    Moving,
    Attacking,
    Patrolling,
    Dead
}
