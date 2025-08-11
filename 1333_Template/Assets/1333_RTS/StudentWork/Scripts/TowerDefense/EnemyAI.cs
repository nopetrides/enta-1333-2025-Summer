using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("AI Settings")]
    [SerializeField] private float buildingDetectionRange = 10f;
    [SerializeField] private float attackDistance = 1.2f;
    [SerializeField] private float retargetInterval = 0.5f;

    private float lastRetargetTime = -100f;
    private GameObject lastTargetBuilding = null;

    private Transform kingTarget;
    private UnitInstance unitInstance;


    private Vector3 lastKingDestination = Vector3.zero;
    private bool kingInitialized = false;

    void Awake()
    {
        unitInstance = GetComponent<UnitInstance>();
     
    }

    public void SetTarget(Transform t)
    {
        kingTarget = t;
        kingInitialized = true;
        if (unitInstance != null && kingTarget != null)
        {
          
            unitInstance.SetDestination(kingTarget.position);
            lastKingDestination = kingTarget.position;
        }
    }

    void Update()
    {
        if (unitInstance == null || !unitInstance.IsAlive)
            return;


        if (Time.time - lastRetargetTime > retargetInterval)          // only update path every X seconds for performance
        {
            lastRetargetTime = Time.time;

            GameObject building = FindClosestDamagableBuilding();
            if (building != null)
            {
                float dist = Vector3.Distance(transform.position, building.transform.position);

             
                if (lastTargetBuilding != building)     // if this is a new building, pathfind to it
                {
                    lastTargetBuilding = building;
                    Vector3 targetPos = FindNearestWalkableToBuilding(building);
               
              
                    unitInstance.SetDestination(targetPos);
                }
                else
                {
                  
                }

            
                if (dist <= attackDistance)      // attack if close enough
                {
                   
                    var dmg = building.GetComponent<IDamageable>();
                    if (dmg != null)
                        dmg.TakeDamage(1);
                    // Destroy(gameObject); 
                }
                return; // skip king logic if focusing on building!
            }
            else
            {
                if (lastTargetBuilding != null)
                {
                  
                    lastTargetBuilding = null;
                }

               
                if (kingTarget != null && kingInitialized) // king chasing logic
                {
                    
                    float distToKingTarget = Vector3.Distance(unitInstance.GetDestination(), kingTarget.position);
                    if (distToKingTarget > 0.2f) // tweak threshold as needed
                    {
                       
                        unitInstance.SetDestination(kingTarget.position);
                        lastKingDestination = kingTarget.position;
                    }
                    else
                    {
                      
                    }
                }
            }
        }
    }

    GameObject FindClosestDamagableBuilding()
    {
        var allBuildings = GameObject.FindGameObjectsWithTag("Building");
        float bestDist = float.MaxValue;
        GameObject best = null;
        foreach (var b in allBuildings)
        {
            var dmg = b.GetComponent<IDamageable>();
            if (dmg == null || !dmg.IsAlive) continue;
            float dist = Vector3.Distance(transform.position, b.transform.position);
            if (dist < buildingDetectionRange && dist < bestDist)
            {
                bestDist = dist;
                best = b;
            }
        }
   
        return best;
    }

  
    
    
    Vector3 FindNearestWalkableToBuilding(GameObject building) // returns the nearest walkable, unoccupied grid node next to the building if none found, fallback to building center
    {
        GridManager gridManager = FindObjectOfType<GridManager>();
        if (gridManager == null) return building.transform.position;
        Vector2Int center = gridManager.WorldToGridIndex(building.transform.position);

        // Try larger area (5x5 around)
        float bestDist = float.MaxValue;
        Vector3 best = building.transform.position;

        for (int dx = -2; dx <= 2; dx++)
        {
            for (int dy = -2; dy <= 2; dy++)
            {
                int x = center.x + dx;
                int y = center.y + dy;
                if (gridManager.IsInBounds(x, y))
                {
                    var node = gridManager.GetNode(x, y);
                    if (node.Walkable && !node.Occupied)
                    {
                        float dist = Vector3.Distance(transform.position, node.WorldPosition);
                        if (dist < bestDist)
                        {
                            bestDist = dist;
                            best = node.WorldPosition;
                        }
                    }
                }
            }
        }
      

        return best;
    }
}
