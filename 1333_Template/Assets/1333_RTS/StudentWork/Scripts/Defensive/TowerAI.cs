using UnityEngine;
using System.Linq;

public class TowerAI : MonoBehaviour
{
    [SerializeField] private float attackRange = 6f;       // tower attack variables, referancing launch point and its prefab
    [SerializeField] private float attackCooldown = 1.0f;
    [SerializeField] private int attackDamage = 4;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;

    private float lastAttackTime = 0f;
    private IDamageable selfDamageable;

    void Awake()
    {
        selfDamageable = GetComponent<IDamageable>();  // cache reference to own IDamageable
    }

    void Update()
    {
        var target = FindNearestEnemy(); // function for launching the projectile enemies in range 
        if (target != null)
        {
            float dist = Vector3.Distance(transform.position, target.GetTransform().position);
            if (dist <= attackRange)
            {
                if (Time.time - lastAttackTime >= attackCooldown)
                {
                    if (projectilePrefab != null)
                    {
                        Vector3 spawnPos = firePoint ? firePoint.position : transform.position;
                        GameObject projObj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
                        Projectile proj = projObj.GetComponent<Projectile>();
                        if (proj != null)
                            proj.Launch(target.GetTransform(), attackDamage);
                    }
                    else
                    {
                        target.TakeDamage(attackDamage);
                    }
                    lastAttackTime = Time.time;
                }
            }
        }
    }

    IDamageable FindNearestEnemy()  // checker for enemies in attack range
    {
        var allTargets = FindObjectsOfType<MonoBehaviour>().OfType<IDamageable>();
        IDamageable best = null;
        float bestDist = float.MaxValue;
        foreach (var t in allTargets)
        {
            if (t == selfDamageable) continue; // avoid self
            if (!t.IsAlive) continue;

          
            GameObject obj = t.GetTransform().gameObject;    // only attack objects tagged as "enemy"
            if (!obj.CompareTag("Enemy")) continue;

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
