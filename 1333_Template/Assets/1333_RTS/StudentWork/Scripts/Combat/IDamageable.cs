using UnityEngine;

public interface IDamageable // damageable interface for all gameobjects that can take damage
{
    void TakeDamage(int amount);
    int CurrentHealth { get; }
    int MaxHealth { get; }
    bool IsAlive { get; }
    Transform GetTransform(); 
}
