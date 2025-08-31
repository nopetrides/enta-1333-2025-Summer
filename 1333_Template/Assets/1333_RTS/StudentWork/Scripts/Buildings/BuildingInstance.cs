using UnityEngine;

public class BuildingInstance : BuildingBase, IDamageable
{
    [Header("Stats")]
    [SerializeField] private GameObject healthBarPrefab;
    [SerializeField] private int maxHealth = 30;

    private int currentHealth;
    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public bool IsAlive => currentHealth > 0;
    private bool isAlive = true;

    public Transform GetTransform() => transform;
    private HealthBarUI healthBarInstance;
    private GridManager gridManager;
    private Vector2Int occupiedOrigin;

    public BuildingType Config => buildingType;
    [SerializeField] private Material[] teamMaterials;

    public int TeamId { get; private set; } = 0; // set on placement

    void Start()
    {
        currentHealth = maxHealth;
        healthBarInstance = Instantiate(healthBarPrefab, transform).GetComponent<HealthBarUI>();
        healthBarInstance.Attach(this); // same as unit instance attach healthbar instances on buildings spawned
        gridManager = FindObjectOfType<GridManager>();
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        if (!isAlive) return;
        isAlive = false;

        if (gridManager != null)    // remove building occupancy from grid
        {
            Vector2Int idx = gridManager.WorldToGridIndex(transform.position);
            gridManager.SetBuildingOccupancy(occupiedOrigin.x, occupiedOrigin.y, Width, Height, false, buildingType);
        }

        Destroy(gameObject);
    }

    public void Initialize(BuildingType config, int teamId = 0) // set up this building with config and team id
    {
        buildingType = config;
        TeamId = teamId;
    }

    public override void PlaceOnNode(GridNode node)
    {
        // transform.position = node.WorldPosition;
    }

    public void SetOccupiedOrigin(Vector2Int origin)
    {
        occupiedOrigin = origin;
    }

    //public void SpawnUnit(GameObject unitPrefab, AStarPathfinder pathfinder)
    //{
    //    GridManager gridManager = FindObjectOfType<GridManager>();
    //    Vector2Int buildingGridPos = gridManager.WorldToGridIndex(transform.position);

    //    for (int dx = -1; dx <= Width; dx++)
    //    {
    //        for (int dy = -1; dy <= Height; dy++)
    //        {
    //            if (dx == -1 || dx == Width || dy == -1 || dy == Height)
    //            {
    //                int x = buildingGridPos.x + dx;
    //                int y = buildingGridPos.y + dy;
    //                if (gridManager.IsInBounds(x, y))
    //                {
    //                    var node = gridManager.GetNode(x, y);
    //                    if (node.Walkable && !node.Occupied)
    //                    {
    //                        Vector3 spawnPos = node.WorldPosition;
    //                        GameObject go = Instantiate(unitPrefab, spawnPos, Quaternion.identity);

    //                        var unitInstance = go.GetComponent<UnitInstance>();
    //                        if (unitInstance != null)
    //                        {
    //                            Material teamMat = null;
    //                            if (teamMaterials != null && TeamId < teamMaterials.Length)
    //                                teamMat = teamMaterials[TeamId];

    //                            unitInstance.Initialize(pathfinder, teamMat, TeamId); // propagate teamId!
    //                            return;
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //    }
    }

