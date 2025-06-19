using System.Collections;
using UnityEngine;

/// <summary>
/// Abstract base for concrete units (Archer, Commander, etc.).
/// Holds HP, team, state, animation, and selection logic.
/// Delegates pathfinding and movement to the attached UnitMovement component.
/// </summary>
public abstract class UnitBase : MonoBehaviour, ISelectable
{
    // ---------- Serialized references ----------
    [Header("References")]
    [Tooltip("ScriptableObject that defines stats and visuals for this unit.")]
    [SerializeField] protected UnitTypeSO _unitType = null;

    [Tooltip("Component that triggers Animator parameters when state changes.")]
    [SerializeField] protected UnitAnimHandler _animHandler = null;

    [Tooltip("Component responsible for pathfinding and translation.")]
    [SerializeField] private UnitMovement _movement = null;

    // ---------- Runtime data ----------
    protected float _currentHp;
    protected UnitState _state = UnitState.Idle;
    protected Team _team;

    protected UnitManager _unitManager;

    // Selection
    public bool IsSelected { get; private set; }

    // ---------- Properties ----------
    public virtual int Width => (_unitType != null) ? _unitType.Width : 1;
    public virtual int Height => (_unitType != null) ? _unitType.Height : 1;

    public UnitState CurrentState => _state;
    public Team UnitTeam => _team;
    public UnitTypeSO UnitType => _unitType;
    public float CurrentHp => _currentHp;

    // ---------- MonoBehaviour ----------
    protected virtual void Awake()
    {
        // Ensure movement component is present
        if (_movement == null && !TryGetComponent(out _movement))
            _movement = gameObject.AddComponent<UnitMovement>();
    }

    protected virtual void Update()
    {
        // Debug: kill unit with H key
        if (Input.GetKeyDown(KeyCode.H) && _state != UnitState.Dead)
            Die();
    }

    // ---------- Initialization ----------
    /// <summary>
    /// Must be called immediately after instantiation by ArmyManager (DI style).
    /// </summary>
    public virtual void Initialize(
        UnitTypeSO unitType,
        GridManager gridManager,
        UnitManager unitManager,
        AStarPathfinder pathfinder,
        Team team)
    {
        _unitType = unitType;
        _unitManager = unitManager;
        _team = team;

        _currentHp = unitType.MaxHp;
        _state = UnitState.Idle;

        // Inject references into Movement component
        _movement.Init(gridManager, pathfinder, unitType.MoveSpeed, 360f);

        _movement.OccupyCurrentNode();

        UnitCombat combat = GetComponent<UnitCombat>();
        if (combat != null)
            combat.Init(unitManager, unitType);

        // Apply team material
        Material teamMat = unitType.GetArmyMaterial(team);
        if (teamMat != null && TryGetComponent<UnitVisualController>(out var vc))
            vc.ApplyTeamMaterial(teamMat, unitType.IsMounted);
    }

    // ---------- Public API ----------
    public virtual void MoveTo(GridNode targetNode)
    {
        _movement.MoveTo(targetNode);
    }

    public void SetReservedDestination(GridNode node)
    {
        // Store and reserve via UnitMovement
        if (_movement != null)
            _movement.PlanAndReserveDestination(node);
    }

    public virtual void TakeDamage(float damageAmount)
    {
        if (_state == UnitState.Dead) return;

        _currentHp -= damageAmount;
        if (_currentHp <= 0f) Die();
    }

    public virtual void OnSelected()
    {
        IsSelected = true;
        if (TryGetComponent<UnitVisualController>(out var vc))
            vc.ShowSelectionIndicator();
    }

    public virtual void OnDeselected()
    {
        IsSelected = false;
        if (TryGetComponent<UnitVisualController>(out var vc))
            vc.HideSelectionIndicator();
    }

    // ---------- Internal helpers ----------
    public void InternalChangeState(UnitState newState)
    {
        _state = newState;
        _animHandler?.OnStateChanged(_state);
    }

    protected virtual void Die()
    {
        if (_state == UnitState.Dead) return;

        InternalChangeState(UnitState.Dead);

        if (_unitManager != null)
            _unitManager.UnregisterUnit(this);

        StartCoroutine(DestroyAfterDelay(5f));
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}
