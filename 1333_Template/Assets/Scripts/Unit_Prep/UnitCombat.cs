using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles enemy search, range checks, and attack execution.
/// </summary>
[RequireComponent(typeof(UnitBase))]
public class UnitCombat : MonoBehaviour
{
    [SerializeField] private float _scanInterval = 0.2f;
    [SerializeField] private int _repositionCandidates = 8;   // how many nearby nodes to sample
    [SerializeField] private float _repositionDelay = 0.05f;
    [SerializeField] private int _repositionTriesMax = 3;   // NEW: max retries


    [SerializeField] private bool _enableDebug = true; //Debug

    private bool _isRepositioning = false;

    private UnitBase _core;
    private UnitMovement _movement;
    private UnitManager _unitManager;
    private float _attackRange;
    private int _attackDamage;  
    private float _cooldown;
    private float _cooldownTimer;
    private UnitBase _currentTarget;
    private bool _isInitialized = false;   // flag to start scanning only after Init()

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void Log(string msg)
    {
        if (_enableDebug) Debug.Log($"[UnitCombat] {name}: {msg}");
    }


    public void Init(UnitManager um, UnitTypeSO type)
    {
        _unitManager = um;
        _attackRange = type.AttackRange;
        _attackDamage = type.Damage;
        _cooldown = type.AttackCooldown;

        _isInitialized = true;
        StartCoroutine(ScanLoop());        // start scanning after fields are set
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

    private IEnumerator ScanLoop()
    {
        while (!_isInitialized)
            yield return null;

        while (true)
        {
            if (_core.CurrentState != UnitState.Dead)
                AcquireOrUpdateTarget();
            yield return new WaitForSeconds(_scanInterval);
        }
    }

    /// <summary>
    /// Picks a random nearby free node, moves there, then enables attacking.
    /// </summary>
    private IEnumerator RepositionThenAttack()
    {
        _isRepositioning = true;
        Log("Reposition start");

        int tries = 0;
        bool reached = false;

        while (tries < _repositionTriesMax && !reached)
        {
            if (!IsRepositionContextValid())
                break;

            if (_currentTarget == null || _currentTarget.CurrentState == UnitState.Dead)
            {
                Log("Target vanished during reposition");
                   break;                          // Stop loop and go LoseTarget()
            }
            if (_movement == null || _movement.Grid == null)
                yield break;
            
            tries++;

            // 1) Collect nearby free nodes *within attack range*
            GridManager gm = _movement.Grid;
            GridNode start = gm.getNodeFromWorldPosition(transform.position);

            List<GridNode> candidates = gm.FindNearestFreeNodes(start, _repositionCandidates);
            if (candidates.Count > 0) candidates.Remove(start);

            // filter by distance to current target
            Vector3 tgtPos = _currentTarget.transform.position;
            candidates.RemoveAll(n => Vector3.Distance(n.worldPosition, tgtPos) > _attackRange);

            if (candidates.Count == 0)
            {
                Log("No in-range candidate — break.");
                break;                       
            }

            GridNode pick = candidates[Random.Range(0, candidates.Count)];
            Log($"Try {tries}: move to {pick.worldPosition}");
            _movement.PlanAndReserveDestination(pick);
            _movement.MoveTo(pick);

            // Wait until movement finished
            while (_core.CurrentState == UnitState.Moving)
            {
                if (!IsRepositionContextValid())
                    break;
                yield return null;
            }

            if (!IsRepositionContextValid())
                break;

            yield return new WaitForSeconds(_repositionDelay);

            if (_currentTarget != null)
            {
                float dist = Vector3.Distance(transform.position, _currentTarget.transform.position);
                reached = dist <= _attackRange;
            }
            else
            {
                break;       
            }
        }

        _isRepositioning = false;

        if (!IsRepositionContextValid())
        {
            Log("Context invalid → LoseTarget");
            LoseTarget();           // target lost or self disabled
            yield break;
        }
    }


    private void AcquireOrUpdateTarget()
    {
        if (_unitManager == null) return;  // safety guard

        // 1) Validate current target
        if (_currentTarget != null && (_currentTarget.CurrentState == UnitState.Dead ||
                               Vector3.Distance(transform.position, _currentTarget.transform.position) > _attackRange))
        {
            LoseTarget();
        }

        // 2) Search a new target
        if (_currentTarget == null)
        {
            _currentTarget = _unitManager.FindNearestEnemy(_core, _attackRange);
            if (_currentTarget != null)
            {
                Log($"Target acquired → {_currentTarget.name}");
                OnAttackStarted();
            }
        }

        // 3) Attack attempt
        if (_currentTarget != null && !_isRepositioning && _cooldownTimer <= 0f)
            {
            float dist = Vector3.Distance(transform.position, _currentTarget.transform.position);
               if (dist <= _attackRange)
                PerformAttack();
               else
                OnAttackStarted();      
            }
    }

    private void PerformAttack()
    {
        Debug.Log(""); // game object name: is just perform attacked
        _cooldownTimer = _cooldown;
        Log($"Attack → {_currentTarget.name} for {_attackDamage} dmg (CD {_cooldown}s)");

        // Face target
        Vector3 dir = _currentTarget.transform.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

        // Apply damage
        _currentTarget.TakeDamage(_attackDamage); 

        // Animation trigger
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
        // target still exists and alive?
        if (_currentTarget == null || _currentTarget.CurrentState == UnitState.Dead)
            return false;

        // movement / grid still available?
        if (_movement == null || _movement.Grid == null)
            return false;

        return true;
    }
}
