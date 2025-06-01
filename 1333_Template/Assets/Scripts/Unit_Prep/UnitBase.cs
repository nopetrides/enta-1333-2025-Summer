using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract base class for all units in the game.
/// Holds common stats, state machine, and grid-based movement logic.
/// Concrete unit classes (e.g. SpearMan, Archer) must inherit from this and implement MoveTo().
/// </summary>
public abstract class UnitBase : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The UnitType ScriptableObject that defines this unit's stats.")]
    [SerializeField] protected UnitType _unitType = null;

    // Current HP of this unit (initialized from _unitType.MaxHp)
    protected float _currentHp;

    // Movement speed (initialized from _unitType.MoveSpeed)
    protected float _moveSpeed;

    // The current path (list of grid coordinates) that this unit is following
    protected List<Vector2Int> _currentPath = null;

    // Index of the next node in _currentPath to move toward
    protected int _nextPathIndex = 0;

    // Unit's current state (Idle, Moving, etc.)
    protected UnitState _state = UnitState.Idle;

    // Which team this unit belongs to (Player or Enemy)
    protected Team _team;

    // Cached reference to GridManager (for world <-> grid coordinates conversion)
    protected GridManager _gridManager;

    // Cached reference to AStarPathfinder (for direct pathfinding calls)
    protected AStarPathfinder _pathfinder;

    /// <summary>
    /// Width of this unit in grid cells (for larger units).
    /// </summary>
    public virtual int Width => (_unitType != null) ? _unitType.Width : 1;

    /// <summary>
    /// Height of this unit in grid cells (for larger units).
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
    /// Unity callback called when this script instance is being loaded.
    /// Initialize HP, move speed, but actual setup occurs in Initialize().
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
    /// Initializes this unit with the given UnitType, GridManager, AStarPathfinder, and Team.
    /// Should be called right after Instantiate.
    /// </summary>
    /// <param name="unitType">The ScriptableObject containing this unit's stats.</param>
    /// <param name="gridManager">Reference to the GridManager in the scene.</param>
    /// <param name="pathfinder">Reference to the AStarPathfinder instance in the scene.</param>
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
    /// Called every frame to handle movement or other state behaviors.
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
    /// Concrete subclasses must implement the actual pathfinding call and state change.
    /// </summary>
    /// <param name="targetNode">GridNode to move toward</param>
    public abstract void MoveTo(GridNode targetNode);

    /// <summary>
    /// Common movement logic that every unit can share.
    /// Moves along the _currentPath until destination is reached.
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

        // If we have almost reached it, advance to the next node
        if (Vector3.Distance(transform.position, nextWorldPos) < 0.01f)
        {
            _nextPathIndex++;
            if (_nextPathIndex >= _currentPath.Count)
            {
                // Arrived at final destination
                _state = UnitState.Idle;
                OnArrivedAtDestination();
            }
        }
    }

    /// <summary>
    /// Called when the unit arrives at the final destination.
    /// Subclasses can override to perform actions on arrival.
    /// </summary>
    protected virtual void OnArrivedAtDestination()
    {
        // Default: do nothing (stay Idle)
    }
}

/// <summary>
/// Possible states for a unit's state machine.
/// </summary>
public enum UnitState
{
    Idle,
    Moving,
    Attacking,
    Patrolling,
    Dead
}
