// SpearManAnimHandler.cs
using UnityEngine;

/// <summary>
/// Handles all animation transitions for a SpearMan unit. Currently only
/// walking (moving) and idle animations are implemented. Stubs for
/// attacking, dying, and other future states are provided for easy extension.
/// </summary>
[RequireComponent(typeof(Animator))]
public class UnitAnimHandler : MonoBehaviour
{
    private Animator _animator;

    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    //private static readonly int AttackTriggerHash = Animator.StringToHash("Attack");
    //private static readonly int IsDeadHash = Animator.StringToHash("IsDead");
    // Add additional Animator parameter hashes here, e.g.:
    // private static readonly int IsAttackingHash = Animator.StringToHash("IsAttacking");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        if (_animator == null)
        {
            Debug.LogError("SpearManAnimHandler: No Animator component found on " + name);
        }
    }

    /// <summary>
    /// Call this method whenever the SpearMan's state changes.
    /// Only Walking (Moving) vs. Idle is implemented for now. Future states
    /// such as Attacking, Dying, or Patrolling can be added in their cases.
    /// </summary>
    /// <param name="newState">The new UnitState of the SpearMan.</param>
    public void OnStateChanged(UnitState newState)
    {
        switch (newState)
        {
            case UnitState.Idle:
                // Stop walking animation
                _animator.SetBool(IsMovingHash, false);
                break;

            case UnitState.Moving:
                // Play walking animation
                _animator.SetBool(IsMovingHash, true);
                break;

            case UnitState.Attacking:
                // Stub: future attacking animation
                // _animator.SetTrigger(AttackTriggerHash);
                break;

            case UnitState.Patrolling:
                // Stub: might be same as moving or a unique animation
                // _animator.SetBool(IsMovingHash, true);
                break;

            case UnitState.Dead:
                // Stub: future death animation
                // _animator.SetBool(IsDeadHash, true);
                break;

            default:
                // Handle any other states if needed
                break;
        }
    }
}
