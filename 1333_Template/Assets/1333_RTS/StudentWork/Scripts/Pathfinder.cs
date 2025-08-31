using System.Collections.Generic;
using NUnit;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class Pathfinder : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;

    public IEnumerable<object> VisitedNodes { get; internal set; }

    public List<GridNode> FindPath(Vector3 startPos, Vector3 endPos)  // main function to find a path between two positions
    {
        if (!gridManager.IsInitialized)
            gridManager.InitializeGrid();
        Vector2Int startIndex = WorldToGridIndex(startPos); //converts the 3D world positions into grid cell indices
        Vector2Int endIndex = WorldToGridIndex(endPos);

        
        if (!IsValidIndex(startIndex) || !IsValidIndex(endIndex))
            return null;

        Dictionary<Vector2Int, Vector2Int> cameFrom = new(); //keeps track of which cell  moved from to reach a cell
        Dictionary<Vector2Int, int> costSoFar = new();

        List<Vector2Int> openList = new List<Vector2Int> { startIndex };
        costSoFar[startIndex] = 0;

        while (openList.Count > 0)
        {
            Vector2Int current = openList[0];
            openList.RemoveAt(0);

            if (current == endIndex)
                break;

            foreach (Vector2Int neighbor in GetNeighbors(current))
            {
                GridNode neighborNode = gridManager.GetNode(neighbor.x, neighbor.y);
                if (!neighborNode.Walkable)
                    continue;

                int newCost = costSoFar[current] + neighborNode.Weight;

                if (!costSoFar.ContainsKey(neighbor) || newCost < costSoFar[neighbor])
                {
                    costSoFar[neighbor] = newCost;
                    cameFrom[neighbor] = current;
                    openList.Add(neighbor);
                }
            }
        }

        
        List<GridNode> path = new();   // reconstruct path
        Vector2Int pathCurrent = endIndex;
        if (!cameFrom.ContainsKey(endIndex))
            return null; 

        while (pathCurrent != startIndex)
        {
            path.Insert(0, gridManager.GetNode(pathCurrent.x, pathCurrent.y));
            pathCurrent = cameFrom[pathCurrent];
        }
        path.Insert(0, gridManager.GetNode(startIndex.x, startIndex.y));

        return path;
    }

  
    private Vector2Int WorldToGridIndex(Vector3 worldPos)     // helper function
    {
        var settings = gridManager.GridSettings;
        int x = Mathf.RoundToInt(worldPos.x / settings.NodeSize);
        int y = settings.UseXZPlane
            ? Mathf.RoundToInt(worldPos.z / settings.NodeSize)
            : Mathf.RoundToInt(worldPos.y / settings.NodeSize);
        return new Vector2Int(x, y);
    }

    private bool IsValidIndex(Vector2Int idx)  // returns true if a grid cell index is inside the grid bounds
    {
        var settings = gridManager.GridSettings;
        return idx.x >= 0 && idx.x < settings.GridSizeX && idx.y >= 0 && idx.y < settings.GridSizeY;
    }

    private List<Vector2Int> GetNeighbors(Vector2Int idx)
    {
        List<Vector2Int> neighbors = new();
        int[][] deltas = new int[][]
        {
        new int[] { 0, 1 },  
        new int[] { 1, 0 }, 
        new int[] { 0, -1 }, 
        new int[] { -1, 0 } 
        };
        foreach (var d in deltas)
        {
            Vector2Int n = new Vector2Int(idx.x + d[0], idx.y + d[1]);
            if (IsValidIndex(n))
                neighbors.Add(n);
        }
        return neighbors;
    }

}
