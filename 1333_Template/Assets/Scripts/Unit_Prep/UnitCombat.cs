using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles target acquisition, smart repositioning, and attack execution.
/// Works with IDamageable (units or buildings) and always moves closer to the target.
/// </summary>
[RequireComponent(typeof(UnitBase))]
public class UnitCombat : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float _scanInterval = 0.2f;      // seconds between scans
    [SerializeField] private float _repositionDelay = 0.05f;  // small wait before attack
    [SerializeField] private int _repositionTriesMax = 3;     // safety cap

    [Header("Smart Reposition")]
    [SerializeField] private int _repositionCandidates = 8;   // nodes sampled each step
    [SerializeField] private float _minDistanceGain = 0.25f;  // must get at least this closer

    [SerializeField] private bool _enableDebug = true;        // editor only

    // --- Cached refs & runtime data --------------------------
    private UnitBase _core;
    private UnitMovement _movement;
    private UnitManager _unitManager;

    private float _attackRange;
    private float _visionRange;
    // base values copied from UnitTypeSO, never modified
    private float _baseAttackRange;
    private float _baseVisionRange;

    // runtime multiplier, 1 = normal
    private float _rangeMul = 1f;
    private int _attackDamage;
    private float _cooldown;
    private float _cooldownTimer;

    private IDamageable _currentTarget;
    private bool _isInitialized = false;
    private bool _isRepositioning = false;

    // runtime flags
    private bool _anchored = false;   // true = cannot move / reposition
    private bool _isGarrisoned = false;

    /// <summary>True while the unit is stationed on a wall.</summary>
    public bool IsGarrisoned => _isGarrisoned;

    // ---------- debug helper ---------------------------------
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void Log(string msg)
    {
        if (_enableDebug) Debug.Log($"[UnitCombat] {name}: {msg}");
    }

    // ---------- Init -----------------------------------------
    public void Init(UnitManager um, UnitTypeSO type)
    {
        _unitManager = um;
        _baseAttackRange = type.AttackRange;   // store originals
        _baseVisionRange = type.VisionRange;
        _attackDamage = type.Damage;
        _cooldown = type.AttackCooldown;

        ApplyMultiplier();

        _isInitialized = true;
        StartCoroutine(ScanLoop());
    }

    private void Awake()
    {
        _core = GetComponent<UnitBase>();
        _movement = GetComponent<UnitMovement>();
    }

    private void Update()
    {
        if (_cooldownTimer > 0f)
            _cooldownTimer -= Time.deltaTime;
    }

    // ---------- Scan loop ------------------------------------
    private IEnumerator ScanLoop()
    {
        while (!_isInitialized) yield return null;

        while (true)
        {
            if (_core.CurrentState != UnitState.Dead)
                AcquireOrUpdateTarget();

            yield return new WaitForSeconds(_scanInterval);
        }
    }

    // ---------------------------------------------------------
    //  Targeting & attack
    // ---------------------------------------------------------
    private void AcquireOrUpdateTarget()
    {
        if (_unitManager == null) return;

        // 1) Validate current target
        if (_currentTarget != null)
        {
            float dist = Vector3.Distance(transform.position, TargetPos(_currentTarget));
            if (!_currentTarget.IsAlive || dist > _visionRange)
                LoseTarget();
        }

        // 2) Acquire new target if none
        if (_currentTarget == null)
        {
            _currentTarget = _unitManager.FindNearestEnemyUnit(_core, _visionRange) ??
                             _unitManager.FindNearestEnemyBuilding(_core, _visionRange);

            if (_currentTarget != null)
            {
                Log($"Target acquired → {_currentTarget.Tr.name}");
                OnAttackStarted();
            }
        }

        // 3) While hitting a building, always prefer a nearer enemy unit
        if (_currentTarget is BuildingBase)
        {
            var nearerUnit = _unitManager.FindNearestEnemyUnit(_core, _visionRange);
            if (nearerUnit != null) _currentTarget = nearerUnit;
        }

        // 4) Attack or reposition
        if (_currentTarget != null && !_isRepositioning && _cooldownTimer <= 0f)
        {
            float dist = Vector3.Distance(transform.position, TargetPos(_currentTarget));
            if (dist <= _attackRange)
            {
                PerformAttack();
            }
            else
            {
                if (!_anchored)
                    OnAttackStarted(); // move closer
            }
        }
    }

    /// <summary>
    /// Deals damage and triggers animation.
    /// </summary>
    private void PerformAttack()
    {
        _cooldownTimer = _cooldown;
        Log($"Attack → {_currentTarget.Tr.name} for {_attackDamage}");

        // Face target
        Vector3 dir = TargetPos(_currentTarget) - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

        _currentTarget.TakeDamage(_attackDamage);
        _core.InternalChangeState(UnitState.Attacking);
    }

    private void OnAttackStarted()
    {
        if (!_isRepositioning)
            StartCoroutine(RepositionThenAttack());
    }

    // ---------------------------------------------------------
    //  Smart reposition coroutine
    // ---------------------------------------------------------
    private IEnumerator RepositionThenAttack()
    {
        _isRepositioning = true;
        Log("Reposition start");

        int tries = 0;
        GridManager gm = _movement.Grid;

        while (tries < _repositionTriesMax && IsRepositionContextValid())
        {
            tries++;

            GridNode from = gm.getNodeFromWorldPosition(transform.position);

            // 1) sample neighbour nodes
            List<GridNode> nodes = gm.FindNearestFreeNodes(from, _repositionCandidates);
            nodes.Remove(from); // current node

            Vector3 tgtPos = TargetPos(_currentTarget);
            float distNow = Vector3.Distance(transform.position, tgtPos);

            // 2) keep only nodes that actually bring the unit closer
            nodes.RemoveAll(n =>
            {
                float d = Vector3.Distance(n.worldPosition, tgtPos);
                return (distNow - d) < _minDistanceGain;
            });

            // 3) no candidate → direct chase
            if (nodes.Count == 0)
            {
                Log("No node but still far → direct chase");
                GridNode tgtNode = gm.getNodeFromWorldPosition(tgtPos);
                _movement.PlanAndReserveDestination(tgtNode);
                _movement.MoveTo(tgtNode);
                break;
            }

            // 4) pick the closest node to the target
            nodes.Sort((a, b) =>
                Vector3.Distance(a.worldPosition, tgtPos)
                .CompareTo(Vector3.Distance(b.worldPosition, tgtPos)));

            GridNode pick = nodes[0];
            _movement.PlanAndReserveDestination(pick);
            _movement.MoveTo(pick);
            Log($"Moving to better spot {pick.worldPosition}");

            // 5) wait until movement done
            while (_core.CurrentState == UnitState.Moving && IsRepositionContextValid())
                yield return null;

            if (!IsRepositionContextValid()) break;
            yield return new WaitForSeconds(_repositionDelay);

            // 6) if now inside range → stop
            if (Vector3.Distance(transform.position, tgtPos) <= _attackRange)
                break;
        }

        _isRepositioning = false;

        if (!IsRepositionContextValid())
            LoseTarget();
    }

    // ---------------------------------------------------------
    //  Helpers
    // ---------------------------------------------------------
    /// <summary>
    /// Returns the point we should head toward: unit pivot or nearest building edge.
    /// </summary>
    private Vector3 TargetPos(IDamageable target)
    {
        // Buildings have variable footprint; ask them for closest edge
        if (target is BuildingBase b)
            return b.GetClosestEdgePoint(transform.position);

        return target.Tr.position;
    }

    private bool IsRepositionContextValid()
    {
        if (_currentTarget == null || !_currentTarget.IsAlive)
            return false;
        return _movement != null && _movement.Grid != null;
    }

    /// Locks or unlocks movement/reposition logic (used by walls).
    public void SetAnchored(bool value)
    {
        _anchored = value;
        if (value) _core.InternalChangeState(UnitState.Idle);
    }
    /// Called by BuildingWall to toggle the flag (used by walls).
    public void SetGarrisoned(bool value)
    {
        _isGarrisoned = value;
    }

    /// <summary>
    /// Sets a new range multiplier (e.g. 2 for walls) and recalculates the
    /// effective attack / vision ranges used during combat.
    /// </summary>
    public void SetRangeMultiplier(float m)
    {
        _rangeMul = Mathf.Max(0.1f, m);
        ApplyMultiplier();
    }

    /// Recalculates the effective ranges from base values and multiplier.
    private void ApplyMultiplier()
    {
        _attackRange = _baseAttackRange * _rangeMul;
        _visionRange = _baseVisionRange * _rangeMul;
    }

    /// <summary>
    /// Clears the current target and makes sure the unit
    /// ends up centered on a walkable, blocked-for-others node.
    /// </summary>
    private void LoseTarget()
    {
        _currentTarget = null;

        if (_anchored)
        {
            _movement?.OccupyCurrentNode();   // mark the tile we sit on
            _core.InternalChangeState(UnitState.Idle);
            return;
        }

        // If the combat just ended between two cells, nudge the unit
        // onto the nearest free node so path-finding stays consistent.
        if (_movement != null && _movement.Grid != null)
        {
            GridManager grid = _movement.Grid;
            GridNode here = grid.getNodeFromWorldPosition(transform.position);

            bool cellAlreadyBlocked = !here.walkable || grid.IsNodeReserved(here);
            float toCenter = Vector3.Distance(transform.position,
                                                       here.worldPosition);

            // When the unit is off-center or the cell is not yet blocked,
            // find the closest free node and walk there.
            if (!cellAlreadyBlocked ||
                toCenter > grid.GridSettings.NodeSize * 0.25f)
            {
                List<GridNode> free = grid.FindNearestFreeNodes(here, 1);
                if (free.Count > 0)
                {
                    GridNode dst = free[0];
                    _movement.PlanAndReserveDestination(dst);
                    _movement.MoveTo(dst);
                    // FinishMovement() will set Idle after the move completes.
                    return;
                }
            }

            // Otherwise simply claim the cell we are standing on.
            _movement.OccupyCurrentNode();
        }

        _movement?.Resume();
        _core.InternalChangeState(UnitState.Idle);
    }
}
