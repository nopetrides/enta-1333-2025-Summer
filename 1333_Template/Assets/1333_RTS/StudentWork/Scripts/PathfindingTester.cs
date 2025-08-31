using System.Collections.Generic;
using UnityEngine;

public class PathfindingTester : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private Pathfinder pathfinder;
    [SerializeField] private Transform startMarker;
    [SerializeField] private Transform endMarker;
    [SerializeField] private LineRenderer pathLineRenderer; 

    [Header("Terrain Randomization")]
    [SerializeField] private List<TerrainType> availableTerrains;

    [Header("Path Debug")]
    [SerializeField] private Color pathColor = Color.yellow;
    private List<GridNode> lastPath;

    void Start()                // on  game start randomize the terrain grid and run pathfinding
    {
        RandomizeTerrain();
        RunPathfinder();
    }

    public void RandomizeTerrain()  // main function getting a random terrain from list and updates if walkable
    {
        if (!gridManager.IsInitialized)
            gridManager.InitializeGrid();

        for (int x = 0; x < gridManager.GridSettings.GridSizeX; x++)
        {
            for (int y = 0; y < gridManager.GridSettings.GridSizeY; y++)
            {
                TerrainType randTerrain = availableTerrains[Random.Range(0, availableTerrains.Count)];
                GridNode node = gridManager.GetNode(x, y);
                node.Walkable = randTerrain.IsWalkable;
                node.Weight = randTerrain.MovementCost;
                gridManager.SetNode(x, y, node); 
            }
        }
    }

    public void RunPathfinder()  // using to find a path by pathfinder cs
    {
        lastPath = pathfinder.FindPath(startMarker.position, endMarker.position);
        DrawPathWithLineRenderer();
    }

    private void DrawPathWithLineRenderer()
    {
        if (pathLineRenderer == null) return;
        if (lastPath == null || lastPath.Count == 0)
        {
            pathLineRenderer.positionCount = 0;
            return;
        }

        pathLineRenderer.positionCount = lastPath.Count;
        for (int i = 0; i < lastPath.Count; i++)
        {
            pathLineRenderer.SetPosition(i, lastPath[i].WorldPosition + Vector3.up * 0.5f); 
        }
        pathLineRenderer.startColor = pathColor;
        pathLineRenderer.endColor = pathColor;
    }

    private void OnDrawGizmos() 
    {
        if (lastPath == null) return;
        Gizmos.color = pathColor;
        for (int i = 0; i < lastPath.Count - 1; i++)
        {
            Gizmos.DrawLine(lastPath[i].WorldPosition, lastPath[i + 1].WorldPosition);
        }
    }

    void Update()                 // pressing space to randomize terrain and rerun pathfinder
    {
     
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RandomizeTerrain();
            RunPathfinder();
        }
    }
}
