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
        if (!unit.IsAlive)
        {
            if (animator) animator.SetBool("isAttacking", false);
            return;
        }

        if (currentTarget == null || !currentTarget.IsAlive)
        {
            currentTarget = FindNearestEnemy();
            if (animator) animator.SetBool("isAttacking", false);
        }

        if (currentTarget != null && currentTarget.IsAlive)
        {
            float dist = Vector3.Distance(transform.position, currentTarget.GetTransform().position);
            if (dist <= attackRange)
            {
                FaceTarget(currentTarget.GetTransform().position);

                if (Time.time - lastAttackTime >= attackCooldown)
                {
                    if (animator) animator.SetBool("isAttacking", true);
                    currentTarget.TakeDamage(attackDamage);
                    lastAttackTime = Time.time;
                    AudioManager.Instance?.PlayUnitAttack();
                }
            }
            else
            {
                if (animator) animator.SetBool("isAttacking", false);
            }
        }
        else
        {
            if (animator) animator.SetBool("isAttacking", false);
        }
    }

    private void FaceTarget(Vector3 targetPos)
    {
        Vector3 direction = (targetPos - transform.position).normalized;
        direction.y = 0;
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion look = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, 10f * Time.deltaTime);
        }
    }

   public IDamageable FindNearestEnemy()
    {
        var allTargets = FindObjectsOfType<MonoBehaviour>().OfType<IDamageable>();
        IDamageable best = null;
        float bestDist = float.MaxValue;

        foreach (var t in allTargets)
        {
            if (t == (IDamageable)unit) continue; // skip self
            if (!t.IsAlive) continue;

           
            var otherUnit = t as UnitInstance;
            if (otherUnit != null && otherUnit.teamId == unit.teamId) continue;

          
            var building = t as BuildingInstance;
            if (building != null && building.TeamId == unit.teamId) continue;

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
