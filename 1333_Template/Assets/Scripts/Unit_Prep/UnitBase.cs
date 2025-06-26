using System.Collections;
using UnityEngine;

/// <summary>
/// Abstract base for concrete units (Archer, Commander, etc.).
/// Holds HP, team, state, animation, and selection logic.
/// Delegates pathfinding and movement to the attached UnitMovement component.
/// </summary>
public abstract class UnitBase : MonoBehaviour, ISelectable, IDamageable
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

    public static event System.Action<UnitBase> UnitDestroyed;
    private Collider _selectCollider;

    // Selection
    public bool IsSelected { get; private set; }

    // ---------- Properties ----------
    public virtual int Width => (_unitType != null) ? _unitType.Width : 1;
    public virtual int Height => (_unitType != null) ? _unitType.Height : 1;

    public UnitState CurrentState => _state;
    public Team UnitTeam => _team;
    public UnitTypeSO UnitType => _unitType;
    public float CurrentHp => _currentHp;

    // Damageable
    public Team Team => _team;
    public bool IsAlive => _state != UnitState.Dead;
    public Transform Tr => transform;

    // ---------- MonoBehaviour ----------
    protected virtual void Awake()
    {
        // Ensure movement component is present
        if (_movement == null && !TryGetComponent(out _movement))
            _movement = gameObject.AddComponent<UnitMovement>();

        _selectCollider = GetComponent<Collider>();
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

    public virtual void TakeDamage(int rawDamage)
    {
        if (_state == UnitState.Dead) return;

        int defense = (_unitType != null) ? _unitType.Defense : 0;

        // You can swap to percentage reduction if needed.
        int final = Mathf.Max(1, rawDamage - defense);   // never below 1

        _currentHp -= final;

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
        if (this == null) return;   // object already destroyed
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

        OnDeselected();
        DisableSelectable();
        UnitDestroyed?.Invoke(this);
        // free the tile this unit was occupying
        _movement?.ReleaseOccupiedNode();

        if (_unitManager != null)
            _unitManager.UnregisterUnit(this);

        StartCoroutine(DestroyAfterDelay(5f));
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        UnitDestroyed?.Invoke(this);
    }

    private void DisableSelectable()
    {
        if (_selectCollider != null)
            _selectCollider.enabled = false; 
    }
}
