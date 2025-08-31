using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Transform target;  
    private int damage;
    public float speed = 10f;

   
 
    public float meshForwardOffset = 0f;  // projectile mesh does not head to target, this one is to face always the target by giving an offsets

    public void Launch(Transform target, int dmg)
    {
        AudioManager.Instance.PlayProjectileLaunch();
        this.target = target;
        this.damage = dmg;
        FaceTarget();
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

       
        Vector3 dir = (target.position - transform.position).normalized;   // move towards the target
        transform.position += dir * speed * Time.deltaTime;

        
        FaceTarget();  

       
        if (Vector3.Distance(transform.position, target.position) < 0.2f)   // on hit IDamageable, give damage
        {
            var dmgable = target.GetComponent<IDamageable>();
            if (dmgable != null)
                dmgable.TakeDamage(damage);

            Destroy(gameObject);
        }
    }

    private void FaceTarget()  // always face the target as it moves
    {
        if (target == null) return;
        Vector3 dir = (target.position - transform.position).normalized;
        if (dir != Vector3.zero)
        {
           
            transform.rotation = Quaternion.LookRotation(dir);

            
            if (meshForwardOffset != 0f)   // prefab mesh forward offset
                transform.rotation *= Quaternion.Euler(meshForwardOffset, 0f, 0f);
        }
    }
}
