using System.Collections.Generic;
using UnityEngine;

public class AStarPathfinder : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;

    public Dictionary<Vector2Int, bool> VisitedNodes { get; private set; } = new(); //  //for visualization track which nodes were checked
    public Dictionary<Vector2Int, bool> FrontierNodes { get; private set; } = new();

    public List<GridNode> FindPath(Vector3 startPos, Vector3 endPos) // main a* pathfinding method
    {
        if (!gridManager.IsInitialized)
            gridManager.InitializeGrid();

        VisitedNodes.Clear();
        FrontierNodes.Clear();

        Vector2Int start = WorldToGridIndex(startPos);
        Vector2Int goal = WorldToGridIndex(endPos);

        if (!IsValidIndex(start) || !IsValidIndex(goal))
            return null;

        var cameFrom = new Dictionary<Vector2Int, Vector2Int>(); // path trace
        var gScore = new Dictionary<Vector2Int, int>(); // cost from start
        var fScore = new Dictionary<Vector2Int, int>(); // start cost +heuristic
        var openSet = new Dictionary<Vector2Int, int>();

        gScore[start] = 0;
        fScore[start] = Heuristic(start, goal);
        openSet[start] = fScore[start];
        FrontierNodes[start] = true;

        while (openSet.Count > 0) // main loop that continues evaluating nodes
        {
            Vector2Int current = GetNodeWithLowestFScore(openSet);

            if (current == goal)
                return ReconstructPath(cameFrom, current); // if lowest score construct and return

            openSet.Remove(current);
            FrontierNodes.Remove(current);
            VisitedNodes[current] = true;

            foreach (Vector2Int neighbor in GetNeighbors(current))
            {
                GridNode node = gridManager.GetNode(neighbor.x, neighbor.y);

               
                if ((!node.Walkable || node.Occupied || node.UnitPresent) && neighbor != goal)
                {
                    
                    continue;
                }

                int tentativeGScore = gScore[current] + node.Weight;
                if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = tentativeGScore + Heuristic(neighbor, goal);

                    if (!openSet.ContainsKey(neighbor) && !VisitedNodes.ContainsKey(neighbor))
                    {
                        openSet[neighbor] = fScore[neighbor];
                        FrontierNodes[neighbor] = true;
                    }
                }
            }
        }

        return null;
    }

    private Vector2Int GetNodeWithLowestFScore(Dictionary<Vector2Int, int> openSet)   // returning the node with the lowest fScore
    {
        Vector2Int best = default;
        int bestScore = int.MaxValue;
        foreach (var pair in openSet)
        {
            if (pair.Value < bestScore)
            {
                best = pair.Key;
                bestScore = pair.Value;
            }
        }
        return best;
    }

    private int Heuristic(Vector2Int a, Vector2Int b)       // manhattan distance for grid
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private List<GridNode> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current) // reconstruct path from current to came from 
    {
        List<GridNode> path = new();
        while (cameFrom.ContainsKey(current))
        {
            path.Insert(0, gridManager.GetNode(current.x, current.y));
            current = cameFrom[current];
        }
        path.Insert(0, gridManager.GetNode(current.x, current.y));
        return path;
    }

    private Vector2Int WorldToGridIndex(Vector3 worldPos)
    {
        var settings = gridManager.GridSettings;
        int x = Mathf.RoundToInt(worldPos.x / settings.NodeSize);
        int y = settings.UseXZPlane
            ? Mathf.RoundToInt(worldPos.z / settings.NodeSize)
            : Mathf.RoundToInt(worldPos.y / settings.NodeSize);
        return new Vector2Int(x, y);
    }

    private bool IsValidIndex(Vector2Int idx)
    {
        var settings = gridManager.GridSettings;
        return idx.x >= 0 && idx.x < settings.GridSizeX && idx.y >= 0 && idx.y < settings.GridSizeY;
    }

    private List<Vector2Int> GetNeighbors(Vector2Int idx) // directions neigboring
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
