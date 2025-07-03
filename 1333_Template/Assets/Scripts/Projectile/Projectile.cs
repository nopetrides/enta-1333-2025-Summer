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
    private IDamageable _target;
    private Action<Projectile> _onDespawn;

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
        _damage = damage;
        _onDespawn = onDespawn;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (_target == null)
        {
            Despawn();
            return;
        }

        Vector3 targetPos = ((MonoBehaviour)_target).transform.position;
        transform.position = Vector3.MoveTowards(transform.position,
                                                 targetPos,
                                                 _speed * Time.deltaTime);

        if (Vector3.SqrMagnitude(transform.position - targetPos) <=
            _hitThreshold * _hitThreshold)
        {
            _target.TakeDamage(_damage);
            Despawn();
        }
    }

    /// <summary>
    /// Returns the projectile to its pool of origin.
    /// </summary>
    private void Despawn()
    {
        _target = null;
        _onDespawn?.Invoke(this);
    }
}
