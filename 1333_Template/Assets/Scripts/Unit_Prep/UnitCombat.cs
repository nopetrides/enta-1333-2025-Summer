using System.Collections;
using UnityEngine;

/// <summary>
/// Handles enemy search, range checks, and attack execution.
/// </summary>
[RequireComponent(typeof(UnitBase))]
public class UnitCombat : MonoBehaviour
{
    [SerializeField] private float _scanInterval = 0.2f;

    private UnitBase _core;
    private UnitMovement _movement;
    private UnitManager _unitManager;
    private float _attackRange;
    private int _attackDamage;  
    private float _cooldown;
    private float _cooldownTimer;
    private UnitBase _currentTarget;
    private bool _isInitialized = false;   // flag to start scanning only after Init()


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

    private void AcquireOrUpdateTarget()
    {
        if (_unitManager == null) return;  // safety guard

        // 1) Validate current target
        if (_currentTarget != null)
        {
            float dist = Vector3.Distance(transform.position,
                                          _currentTarget.transform.position);
            if (_currentTarget.CurrentState == UnitState.Dead || dist > _attackRange)
                LoseTarget();
        }

        // 2) Search a new target
        if (_currentTarget == null)
        {
            _currentTarget = _unitManager.FindNearestEnemy(_core, _attackRange);
            if (_currentTarget != null)
                OnAttackStarted();
        }

        // 3) Attack attempt
        if (_currentTarget != null && _cooldownTimer <= 0f)
            PerformAttack();
        else
            _cooldownTimer -= Time.deltaTime;
    }

    private void PerformAttack()
    {
        _cooldownTimer = _cooldown;

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
        _movement?.Pause();
        _core.InternalChangeState(UnitState.Attacking);
    }

    private void LoseTarget()
    {
        _currentTarget = null;
        _movement?.Resume();
        _core.InternalChangeState(UnitState.Moving);
    }
}
