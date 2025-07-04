using UnityEngine;

/// <summary>
/// Anything that can be damaged by units (unit or building).
/// </summary>
public interface IDamageable
{
    Team Team { get; }
    bool IsAlive { get; }
    Transform Tr { get; }        // cached transform
    void TakeDamage(int amount);
}
