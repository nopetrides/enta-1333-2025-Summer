using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    // Prefab used to spawn units in the scene
    [SerializeField] private GameObject unitPrefab;

    // Reference to the A* pathfinding system to assign to each unit
    [SerializeField] private AStarPathfinding pathfinder;

    // The unit's configuration, including move speed and material
    [SerializeField] private UnitType unitType;

    // List of all units spawned in the game
    private List<UnitInstance> allUnits = new();

    // List of units currently selected by the player
    public List<UnitInstance> SelectedUnits { get; private set; } = new();

    // Spawns a demo batch of units at random walkable positions on the grid
    public void SpawnDemoUnits(GridNode[,] grid, GridSettings settings)
    {
        int spawned = 0;
        int targetCount = 5;

        while (spawned < targetCount)
        {
            int x = Random.Range(0, settings.GridSizeX);
            int y = Random.Range(0, settings.GridSizeY);
            GridNode node = grid[x, y];

            // Skip non-walkable tiles
            if (!node.walkable) continue;

            // Adjust spawn position slightly above the tile
            Vector3 spawnPos = node.WorldPosition + Vector3.up * 0.5f;
            SpawnUnit(spawnPos);
            spawned++;
        }
    }

    // Instantiates a unit and initializes it
    public void SpawnUnit(Vector3 position)
    {
        var obj = Instantiate(unitPrefab, position, Quaternion.identity);
        var unit = obj.GetComponent<UnitInstance>();
        unit.Initialize(pathfinder, unitType);
        allUnits.Add(unit);
    }

    // Handles selecting a unit (single or multi-select depending on shift key)
    public void SelectUnit(UnitInstance unit, bool additive)
    {
        if (!additive)
        {
            DeselectAll(); // Deselect others if not holding shift
        }

        if (!SelectedUnits.Contains(unit))
        {
            SelectedUnits.Add(unit);
            unit.OnSelect();
        }
    }

    // Deselects all currently selected units
    public void DeselectAll()
    {
        foreach (var unit in SelectedUnits)
        {
            unit.OnDeselect();
        }
        SelectedUnits.Clear();
    }

    // Issues a move command to all selected units
    public void CommandSelectedUnitsToMove(GridNode targetNode)
    {
        foreach (var unit in SelectedUnits)
        {
            unit.MoveTo(targetNode);
        }
    }
}
