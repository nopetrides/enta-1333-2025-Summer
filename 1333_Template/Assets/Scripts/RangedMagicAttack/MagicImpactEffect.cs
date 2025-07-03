using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Particle-only impact that spawns at target position,
/// then despawns itself after the particle system stops.
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class MagicImpactEffect : MonoBehaviour
{
    private ParticleSystem _ps;
    private Action<MagicImpactEffect> _onDespawn;

    private void Awake()
    {
        _ps = GetComponent<ParticleSystem>();
    }

    /// <summary>
    /// Activates particle at given position.
    /// </summary>
    public void Spawn(Vector3 position, Action<MagicImpactEffect> onDespawn)
    {
        transform.position = position;
        _onDespawn = onDespawn;
        gameObject.SetActive(true);
        _ps.Play();
        StartCoroutine(WaitAndDespawn());
    }

    private IEnumerator WaitAndDespawn()
    {
        // Wait for all particles to finish
        yield return new WaitForSeconds(_ps.main.duration + _ps.main.startLifetime.constantMax);
        _onDespawn?.Invoke(this);
    }
}
