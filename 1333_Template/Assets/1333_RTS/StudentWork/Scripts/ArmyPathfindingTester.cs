using System.Collections.Generic;
using UnityEngine;


public class ArmyPathfindingTester : MonoBehaviour   // test scene script to set up multiple armies, spawn units
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private AStarPathfinder pathfinder;
    [SerializeField] private List<ArmyComposition> armyCompositions = new();
    [SerializeField] private UnitManager unitManager;
    [SerializeField] private List<Material> armyMaterials;


    [Header("Patrol & AI Settings")]
    [SerializeField] private int patrolRadius = 8;
    [SerializeField] private float detectionDistance = 4f;

   
    private readonly List<ArmyManager> armies = new();    // all armies in the scene.

    
    private enum UnitState { Patrol, Follow }
    private readonly Dictionary<UnitInstance, UnitState> unitStates = new();
    private readonly Dictionary<UnitInstance, Vector3[]> patrolPoints = new();
    private readonly Dictionary<UnitInstance, int> patrolTargetIndex = new();
    private readonly Dictionary<UnitInstance, UnitInstance> followTargets = new();
    private readonly Dictionary<UnitInstance, Vector3> lastKnownEnemyPos = new();




   
    public void Initialize()
    {
        armies.Clear();
        for (int i = 0; i < armyCompositions.Count; i++)
        {
            var army = new ArmyManager { ArmyId = i + 1, GridManager = gridManager };
            SpawnArmy(army, armyCompositions[i]);
            armies.Add(army);
        }
    }

   
    public void SpawnArmy(ArmyManager army, ArmyComposition composition)    // spawns units from an ArmyComposition into the scene at valid random locations.
    {
        foreach (var entry in composition.units)
        {
            for (int i = 0; i < entry.count; i++)
            {
                Vector3 spawnPos = FindRandomWalkableSpot(entry.unitTypePrefab.unitType.Width, entry.unitTypePrefab.unitType.Height);
                if (spawnPos == Vector3.zero)
                    continue;

                GameObject go = Instantiate(entry.unitTypePrefab.prefab, spawnPos, Quaternion.identity);
                UnitInstance unit = go.GetComponent<UnitInstance>();
                Material armyMat = armyMaterials[army.ArmyId % armyMaterials.Count];
                unit.Initialize(pathfinder, armyMat);
                army.Units.Add(unit);

                Vector2Int spawnGrid = gridManager.WorldToGridIndex(spawnPos);
                gridManager.SetUnitOccupancy(spawnGrid.x, spawnGrid.y, true, unit); // set unit ocupancy


                unitStates[unit] = UnitState.Patrol;
                patrolPoints[unit] = new Vector3[2] {
                    GetRandomPatrolPoint(spawnPos, unit.Width, unit.Height),
                    GetRandomPatrolPoint(spawnPos, unit.Width, unit.Height)
                };
                patrolTargetIndex[unit] = 0;
            }
        }
    }

    public void SpawnUnitInArmy(int armyIndex, UnitTypePrefab unitPrefab) // spawning new units in specific armies 
    {
        if (armyIndex < 0 || armyIndex >= armies.Count) return;

        ArmyManager army = armies[armyIndex];
        Vector3 spawnPos = FindRandomWalkableSpot(unitPrefab.unitType.Width, unitPrefab.unitType.Height);
        if (spawnPos == Vector3.zero) return;

        Material armyMat = armyMaterials[army.ArmyId % armyMaterials.Count];
        GameObject go = Instantiate(unitPrefab.prefab, spawnPos, Quaternion.identity);
        UnitInstance unit = go.GetComponent<UnitInstance>();
        unit.Initialize(pathfinder, armyMat);
        army.Units.Add(unit);
        Vector2Int spawnGrid = gridManager.WorldToGridIndex(spawnPos);
        gridManager.SetUnitOccupancy(spawnGrid.x, spawnGrid.y, true, unit);


    }

    private Vector3 FindRandomWalkableSpot(int width, int height)     // find a random spot big enough for a unit of this size.
    {
        for (int attempt = 0; attempt < 100; attempt++)
        {
            int x = Random.Range(0, gridManager.GridSettings.GridSizeX - width + 1);
            int y = Random.Range(0, gridManager.GridSettings.GridSizeY - height + 1);
            if (IsRegionWalkable(x, y, width, height))
                return gridManager.GetNode(x, y).WorldPosition;
        }
        Debug.LogWarning("No valid spawn location found.");
        return Vector3.zero;
    }

    
    private bool IsRegionWalkable(int x, int y, int width, int height)   // bool check if a rectangular region is all walkable.
    {
        for (int dx = 0; dx < width; dx++)
            for (int dy = 0; dy < height; dy++)
                if (!gridManager.GetNode(x + dx, y + dy).Walkable)
                    return false;
        return true;
    }

    
    private Vector3 GetRandomPatrolPoint(Vector3 origin, int width, int height)   // gets a patrol point within radius of a position.
    {
        var node = gridManager.GetNodeFromWorldPosition(origin);
        float size = gridManager.GridSettings.NodeSize;
        int nodeX = Mathf.RoundToInt(node.WorldPosition.x / size);
        int nodeY = Mathf.RoundToInt(node.WorldPosition.z / size);
        int x = Mathf.Clamp(Random.Range(nodeX - patrolRadius, nodeX + patrolRadius), 0, gridManager.GridSettings.GridSizeX - 1);
        int y = Mathf.Clamp(Random.Range(nodeY - patrolRadius, nodeY + patrolRadius), 0, gridManager.GridSettings.GridSizeY - 1);
        return gridManager.GetNode(x, y).WorldPosition;
    }



}
