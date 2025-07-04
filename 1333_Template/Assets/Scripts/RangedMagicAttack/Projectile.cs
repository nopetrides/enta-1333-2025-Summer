using UnityEngine;
using System;

/// <summary>
/// Abstract projectile that moves toward an IDamageable target
/// and returns itself to the pool on hit or invalid target.
/// </summary>
[RequireComponent(typeof(Collider))]
public abstract class Projectile : MonoBehaviour
{
    [SerializeField] private float _speed = 10f;
    [SerializeField] private float _hitThreshold = 0.25f;

    private int _damage;
    private IDamageable _target;        // Original interface ref
    private MonoBehaviour _targetMb;    // Cached MonoBehaviour for Unity-null check
    private Action<Projectile> _onDespawn;

    /* ===================================================================== */
    /*  Public API                                                           */
    /* ===================================================================== */

    /// <summary>
    /// Initializes and activates the projectile.
    /// </summary>
    public void Launch(Vector3 startPos,
                       IDamageable target,
                       int damage,
                       Action<Projectile> onDespawn)
    {
        transform.position = startPos;
        _target = target;
        _targetMb = target as MonoBehaviour;   // cache once
        _damage = damage;
        _onDespawn = onDespawn;
        gameObject.SetActive(true);
    }

    /* ===================================================================== */
    /*  MonoBehaviour                                                        */
    /* ===================================================================== */

    private void Update()
    {
        if (!IsTargetValid())
        {
            Despawn();
            return;
        }

        Vector3 targetPos = _targetMb.transform.position;

        Vector3 dir = (targetPos - transform.position).normalized;
        if (dir.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

        transform.position = Vector3.MoveTowards(transform.position,
                                                 targetPos,
                                                 _speed * Time.deltaTime);

        if ((transform.position - targetPos).sqrMagnitude <= _hitThreshold * _hitThreshold)
        {
            _target.TakeDamage(_damage);
            Despawn();
        }
    }

    /* ===================================================================== */
    /*  Helpers                                                              */
    /* ===================================================================== */

    /// <summary>
    /// Unity friendly target validity check (destroyed, null, dead).
    /// </summary>
    private bool IsTargetValid()
    {
        // Interface reference null?
        if (_target == null) return false;

        // Underlying Unity object destroyed?
        if (_targetMb == null) return false;

        // Custom alive flag
        return _target.IsAlive;
    }

    /// <summary>
    /// Returns the projectile to its pool of origin.
    /// </summary>
    private void Despawn()
    {
        _target = null;
        _targetMb = null;
        _onDespawn?.Invoke(this);
    }
}
