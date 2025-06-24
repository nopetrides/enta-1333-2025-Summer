using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles target acquisition, smart repositioning, and attack execution.
/// Guarantees the chosen node is always closer to the target, preventing
/// units from drifting away and losing their enemy.
/// </summary>
[RequireComponent(typeof(UnitBase))]
public class UnitCombat : MonoBehaviour
{
    [SerializeField] private float _scanInterval = 0.2f;          // How often to look for targets
    [SerializeField] private int _repositionCandidates = 8;       // Nodes sampled around the unit
    [SerializeField] private float _repositionDelay = 0.05f;      // Wait after movement before attack
    [SerializeField] private int _repositionTriesMax = 3;         // Safety retry cap

    [Header("Smart-reposition tuning")]
    [Tooltip("Picked node must be at least this much closer to the target than the current position.")]
    [SerializeField] private float _minDistanceGain = 0.25f;      // Guarantees real progress

    [SerializeField] private bool _enableDebug = true;            // Editor only

    private bool _isRepositioning = false;

    // --- Cached refs & runtime data ---
    private UnitBase _core;
    private UnitMovement _movement;
    private UnitManager _unitManager;
    private float _attackRange;
    private int _attackDamage;
    private float _cooldown;
    private float _cooldownTimer;
    private UnitBase _currentTarget;
    private float _visionRange;
    private bool _isInitialized = false;

    // ---------- Debug helper ----------
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void Log(string msg)
    {
        if (_enableDebug) Debug.Log($"[UnitCombat] {name}: {msg}");
    }

    // ---------- Init ----------
    public void Init(UnitManager um, UnitTypeSO type)
    {
        _unitManager = um;
        _attackRange = type.AttackRange;
        _visionRange = type.VisionRange;
        _attackDamage = type.Damage;
        _cooldown = type.AttackCooldown;

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

    // ---------- Scan loop ----------
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

    // ---------- Smart reposition ----------
    /// <summary>
    /// Moves to a free node that is demonstrably closer to the target.
    /// Aborts immediately when inside attack range.
    /// </summary>
    private IEnumerator RepositionThenAttack()
    {
        _isRepositioning = true;
        Log("Reposition start");

        int tries = 0;

        while (tries < _repositionTriesMax)
        {
            if (!IsRepositionContextValid())
                break;

            tries++;

            GridManager gm = _movement.Grid;
            GridNode from = gm.getNodeFromWorldPosition(transform.position);

            // 1) Sample neighbour nodes
            List<GridNode> nodes = gm.FindNearestFreeNodes(from, _repositionCandidates);
            if (nodes.Count > 0) nodes.Remove(from);

            Vector3 tgtPos = _currentTarget.transform.position;
            float distNow = Vector3.Distance(transform.position, tgtPos);

            // 2) Keep only nodes that are closer to the target by at least 
            nodes.RemoveAll(n =>
            {
                float d = Vector3.Distance(n.worldPosition, tgtPos);
                return (distNow - d) < _minDistanceGain;
            });

            if (nodes.Count == 0)
            {
                Log("No node but still far → direct chase");
                GridNode tgtNode = gm.getNodeFromWorldPosition(tgtPos);
                _movement.MoveTo(tgtNode);
                break;  // Nothing would improve the situation
            }

            // 3) Pick the closest node to the target
            nodes.Sort((a, b) =>
                Vector3.Distance(a.worldPosition, tgtPos)
                .CompareTo(Vector3.Distance(b.worldPosition, tgtPos)));

            GridNode pick = nodes[0];
            _movement.PlanAndReserveDestination(pick);
            _movement.MoveTo(pick);

            Log($"Moving to better spot {pick.worldPosition}");

            // 4) Wait until movement is done
            while (_core.CurrentState == UnitState.Moving)
            {
                if (!IsRepositionContextValid()) break;
                yield return null;
            }

            if (!IsRepositionContextValid()) break;
            yield return new WaitForSeconds(_repositionDelay);

            // 5) Finished moving and now inside range? → break loop
            if (Vector3.Distance(transform.position, tgtPos) <= _attackRange)
                break;
        }

        _isRepositioning = false;

        if (!IsRepositionContextValid())
            LoseTarget();   // Target died or self disabled
    }

    // ---------- Targeting & attack ----------
    private void AcquireOrUpdateTarget()
    {
        if (_unitManager == null) return;

        // Validate current target
        if (_currentTarget != null)
        {
            float dist = Vector3.Distance(transform.position, _currentTarget.transform.position);
            
                 // Lose only when target is dead OR completely outside vision range
            if (_currentTarget.CurrentState == UnitState.Dead || dist > _visionRange)
            {
                LoseTarget();
            }
        }

        // Search new target
        if (_currentTarget == null)
        {
            _currentTarget = _unitManager.FindNearestEnemy(_core, _visionRange);
            if (_currentTarget != null)
            {
                Log($"Target acquired → {_currentTarget.name}");
                OnAttackStarted();
            }
        }

        // Attack
        if (_currentTarget != null && !_isRepositioning && _cooldownTimer <= 0f)
        {
            float dist = Vector3.Distance(transform.position, _currentTarget.transform.position);
            if (dist <= _attackRange)
                PerformAttack();
            else
                OnAttackStarted();   // Still out of range → reposition again
        }
    }

    /// <summary>Deals damage and triggers animation.</summary>
    private void PerformAttack()
    {
        _cooldownTimer = _cooldown;
        Log($"Attack → {_currentTarget.name} for {_attackDamage}");

        // Face target
        Vector3 dir = _currentTarget.transform.position - transform.position;
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

    private void LoseTarget()
    {
        Log("Lost target");
        _currentTarget = null;
        _movement?.Resume();
        _core.InternalChangeState(UnitState.Idle);
    }

    private bool IsRepositionContextValid()
    {
        if (_currentTarget == null || _currentTarget.CurrentState == UnitState.Dead)
            return false;
        if (_movement == null || _movement.Grid == null)
            return false;
        return true;
    }
}
