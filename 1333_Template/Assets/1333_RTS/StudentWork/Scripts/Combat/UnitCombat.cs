using UnityEngine;
using System.Linq;

public class UnitCombat : MonoBehaviour
{
    [SerializeField] private float attackRange = 1.5f;   // tweakable combat system variables
    [SerializeField] private float attackCooldown = 1.0f;
    [SerializeField] private int attackDamage = 2;

    private float lastAttackTime = 0f;
    private IDamageable currentTarget;
    private UnitInstance unit;
    private Animator animator;

    void Awake()
    {
        unit = GetComponent<UnitInstance>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!unit.IsAlive)  // set attack anim false here
        {
            if (animator) animator.SetBool("isAttacking", false); 
            return;
        }

        if (currentTarget == null || !currentTarget.IsAlive)
        {
            currentTarget = FindNearestEnemy();

           
            if (animator) animator.SetBool("isAttacking", false);    // stop the  attack animation and return to idle if no valid target
       
        }

        if (currentTarget != null && currentTarget.IsAlive)
        {
            float dist = Vector3.Distance(transform.position, currentTarget.GetTransform().position);
            if (dist <= attackRange)
            {
                FaceTarget(currentTarget.GetTransform().position);

               
                if (Time.time - lastAttackTime >= attackCooldown)   // play attack animation and apply damage if cooldown done
                {
                    if (animator) animator.SetBool("isAttacking", true);
                    currentTarget.TakeDamage(attackDamage);
                    lastAttackTime = Time.time;
                    AudioManager.Instance.PlayUnitAttack();    // add attack sound 
                }
            }
            else
            {
               
                if (animator) animator.SetBool("isAttacking", false);   // if not in range stop attacking, play idle
            }
        }
        else
        {
         
            if (animator) animator.SetBool("isAttacking", false);     // no target at all, return to idle
        }
    }

    private void FaceTarget(Vector3 targetPos) // function for facing the target when in range
    {
        Vector3 direction = (targetPos - transform.position).normalized;
        direction.y = 0;
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion look = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, 10f * Time.deltaTime);
        }
    }

    IDamageable FindNearestEnemy()
    {
        var allTargets = FindObjectsOfType<MonoBehaviour>().OfType<IDamageable>();
        IDamageable best = null;
        float bestDist = float.MaxValue;
        foreach (var t in allTargets)
        {
            if (t == (IDamageable)unit) continue; // skip self
            if (!t.IsAlive) continue;
           
            // later I will add avoid friendly fire


            float dist = Vector3.Distance(transform.position, t.GetTransform().position);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = t;
            }
        }
        return best;
    }
}
