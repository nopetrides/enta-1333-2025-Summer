// UnitAnimHandler.cs
using UnityEngine;

/// <summary>
/// Handles animation transitions for a unit. 
/// Current states include Idle, Moving, and Dead. Additional states (Attacking, Patrolling) can be added.
/// </summary>
[RequireComponent(typeof(Animator))]
public class UnitAnimHandler : MonoBehaviour
{
    private Animator _animator;

    // Animator parameter hashes
    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    private static readonly int DeadTriggerHash = Animator.StringToHash("DeadTrigger");
    private static readonly int AttackTriggerHash = Animator.StringToHash("AttackTrigger");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        if (_animator == null)
        {
            Debug.LogError("UnitAnimHandler: No Animator component found on " + name);
        }
    }

    /// <summary>
    /// Called when the unit's state changes. Switches the Animator parameters accordingly.
    /// </summary>
    /// <param name="newState">The new UnitState to transition into.</param>
    public void OnStateChanged(UnitState newState)
    {
        switch (newState)
        {
            case UnitState.Idle:
                // Stop moving animation
                _animator.SetBool(IsMovingHash, false);
                break;

            case UnitState.Moving:
                // Play moving animation
                _animator.SetBool(IsMovingHash, true);
                break;

            case UnitState.Dead:
                // Play death animation once by setting DeadTrigger
                _animator.SetTrigger(DeadTriggerHash);
                break;

            case UnitState.Attacking:
                // Stop locomotion blend tree, then fire one-shot attack trigger
                _animator.SetBool(IsMovingHash, false);
                _animator.SetTrigger(AttackTriggerHash);
                break;

            default:
                break;
        }
    }
}
