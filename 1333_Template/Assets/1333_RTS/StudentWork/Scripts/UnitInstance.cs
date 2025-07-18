using System;
using UnityEngine;


public class UnitInstance : UnitBase, IDamageable   // represents a spawned, active unit in the scene 
{

    [Header("Stats")]
    [SerializeField] private GameObject healthBarPrefab; // assign in inspector
    [SerializeField] private int maxHealth = 10;
    private int currentHealth;
    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public bool IsAlive => currentHealth > 0;
    private bool isAlive = true;

    public Transform GetTransform() => transform;
    private HealthBarUI healthBarInstance;




    [Header("Visuals & FX")]
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject skinRoot;
    [SerializeField] private LineRenderer pathLine;


    public Vector2Int currentGridPos;
    private GridManager gridManager;
    // [SerializeField] private ParticleSystem hurtFX;



    void Start()    // on start, begin with max health, and healthbar spawned
    {
        currentHealth = maxHealth;
        healthBarInstance = Instantiate(healthBarPrefab, transform).GetComponent<HealthBarUI>();
        healthBarInstance.Attach(this); 
        gridManager = FindObjectOfType<GridManager>();
    }

    public void TakeDamage(int amount) // damage function
    {
        currentHealth -= amount;

       
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
       
        if (!isAlive) return;
        isAlive = false;

    
        if (gridManager != null)      // removing unit occupancy from grid after death
        {
            Vector2Int idx = gridManager.WorldToGridIndex(transform.position);
            gridManager.SetUnitOccupancy(idx.x, idx.y, false, null);
        }

        // later I will add there  death animation etc

        Destroy(gameObject); 
    }



    public void Initialize(AStarPathfinder assignedPathfinder, Material teamMaterial) // assign material to each unit belonging to different armies
    {
        pathfinder = assignedPathfinder;
       

        foreach (Renderer renderer in skinRoot.GetComponentsInChildren<Renderer>())
        {

            Material[] mats = renderer.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = teamMaterial;
            }
            renderer.materials = mats;
        }
    }



    public void SetDestination(Vector3 position)   // set a movement target using a world position
    {
        movementTarget = position;
        path = pathfinder.FindPath(transform.position, position);
        pathIndex = 0;
        moving = path != null && path.Count > 1;
        DrawPathLine();

    }

   
    public void SetDestination(GridNode node)   // set a movement target using a grid node
    {
        SetDestination(node.WorldPosition);
    }

   
    public override void MoveTo(GridNode node)
    {
        SetDestination(node);
    }

    void Update()  
    {
          
        {
            if (moving && path != null && pathIndex < path.Count)    // occupancy update logic
            {
                Vector3 nextPoint = path[pathIndex].WorldPosition;
                float step = 5f * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, nextPoint, step);

                if (animator && !animator.GetBool("isWalking")) // set walking anim true here
                    animator.SetBool("isWalking", true);
               

                if (gridManager == null)
                    gridManager = FindObjectOfType<GridManager>(); 

                Vector2Int newGridPos = gridManager.WorldToGridIndex(transform.position);

                if (newGridPos != currentGridPos) 
                {
                   
                    gridManager.SetUnitOccupancy(currentGridPos.x, currentGridPos.y, false, this);   // unset old cell
                 
                    gridManager.SetUnitOccupancy(newGridPos.x, newGridPos.y, true, this);     // set new cell
                    currentGridPos = newGridPos;
                }
             

                if (Vector3.Distance(transform.position, nextPoint) < 0.05f)
                {
                    pathIndex++;
                    if (pathIndex >= path.Count)
                    {
                        moving = false;
                       

                        if (animator && animator.GetBool("isWalking"))  // set walking anim false here
                            animator.SetBool("isWalking", false);
                           
                    }
            }
        }

    }
            }
    private void DrawPathLine() // drawing a line to show path
    {
        if (pathLine == null || path == null || path.Count == 0)
        {
            if (pathLine != null) pathLine.positionCount = 0;
            return;
        }
        pathLine.positionCount = path.Count;
        for (int i = 0; i < path.Count; i++)
        {
            pathLine.SetPosition(i, path[i].WorldPosition + Vector3.up * 0.1f);
        }
        pathLine.startColor = Color.yellow;
        pathLine.endColor = Color.red;
    }
}