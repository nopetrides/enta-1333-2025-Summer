using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeightedPathVisualizer
{
    private readonly WeightedPathfinder pathfinder;
    private readonly GridManager gridManager;
    private readonly System.Func<IEnumerator> waitForNextStep;
    private readonly System.Func<IEnumerator, Coroutine> startCoroutine;

    public WeightedPathVisualizer(
        WeightedPathfinder pathfinder,
        GridManager gridManager,
        System.Func<IEnumerator> waitForNextStep,
        System.Func<IEnumerator, Coroutine> startCoroutine)
    {
        this.pathfinder = pathfinder;
        this.gridManager = gridManager;
        this.waitForNextStep = waitForNextStep;
        this.startCoroutine = startCoroutine;
    }

    public IEnumerator VisualizePath(
        Vector2Int start,
        Vector2Int end,
        HashSet<Vector2Int> visitedNodes,
        List<Vector2Int> currentPath,
        System.Action<List<Vector3>> onPathFound)
    {
        SortedList<float, Vector2Int> openSet = new SortedList<float, Vector2Int>();
        Dictionary<Vector2Int, float> costSoFar = new Dictionary<Vector2Int, float>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();

        openSet.Add(0, start);
        costSoFar[start] = 0;
        visitedNodes.Add(start);
        currentPath.Add(start);

        while (openSet.Count > 0)
        {
            var current = openSet.Values[0];
            openSet.RemoveAt(0);
            currentPath[currentPath.Count - 1] = current;

            if (current == end)
            {
                yield return startCoroutine(VisualizePathReconstruction(cameFrom, start, end, onPathFound));
                yield break;
            }

            foreach (Vector2Int neighbor in GetNeighbors(current))
            {
                if (visitedNodes.Contains(neighbor)) continue;

                float newCost = costSoFar[current] + GetMovementCost(current, neighbor);

                if (!costSoFar.ContainsKey(neighbor) || newCost < costSoFar[neighbor])
                {
                    costSoFar[neighbor] = newCost;
                    cameFrom[neighbor] = current;
                    visitedNodes.Add(neighbor);
                    currentPath.Add(neighbor);

                    float priority = newCost + (openSet.Count * 0.0001f);
                    openSet.Add(priority, neighbor);
                }
            }

            yield return waitForNextStep();
        }

        Debug.LogWarning("No valid path found");
        onPathFound(new List<Vector3>());
    }

    private IEnumerator VisualizePathReconstruction(
        Dictionary<Vector2Int, Vector2Int> cameFrom,
        Vector2Int start,
        Vector2Int end,
        System.Action<List<Vector3>> onPathFound)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int current = end;
        path.Add(current);

        while (current != start)
        {
            current = cameFrom[current];
            path.Add(current);
            yield return waitForNextStep();
        }

        path.Reverse();
        onPathFound(ConvertPathToWorldPositions(path));
    }

    private List<Vector2Int> GetNeighbors(Vector2Int coord)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        int[] dx = { -1, 0, 1, 0, -1, -1, 1, 1 };
        int[] dy = { 0, 1, 0, -1, -1, 1, -1, 1 };

        // Check cardinal directions first
        for (int i = 0; i < 4; i++)
        {
            Vector2Int neighbor = new Vector2Int(coord.x + dx[i], coord.y + dy[i]);
            if (IsValidCoordinate(neighbor) && gridManager.IsWalkable(neighbor))
            {
                neighbors.Add(neighbor);
            }
        }

        // Check diagonal directions if allowed
        if (gridManager.GridSettings.AllowDiagonal)
        {
            for (int i = 4; i < 8; i++)
            {
                Vector2Int neighbor = new Vector2Int(coord.x + dx[i], coord.y + dy[i]);
                if (IsValidCoordinate(neighbor) && gridManager.IsWalkable(neighbor))
                {
                    // Check if we can cut corners
                    Vector2Int cardinal1 = new Vector2Int(coord.x + dx[i - 4], coord.y + dy[i - 4]);
                    Vector2Int cardinal2 = new Vector2Int(coord.x + dx[(i - 3) % 4], coord.y + dy[(i - 3) % 4]);
                    if (gridManager.IsWalkable(cardinal1) && gridManager.IsWalkable(cardinal2))
                    {
                        neighbors.Add(neighbor);
                    }
                }
            }
        }

        return neighbors;
    }

    private float GetMovementCost(Vector2Int from, Vector2Int to)
    {
        float baseCost = Vector2Int.Distance(from, to);
        float weight = gridManager.GetNodeWeight(to);
        return baseCost * weight;
    }

    private bool IsValidCoordinate(Vector2Int coord)
    {
        return coord.x >= 0 && coord.x < gridManager.GridSettings.GridSizeX &&
               coord.y >= 0 && coord.y < gridManager.GridSettings.GridSizeY;
    }

    private List<Vector3> ConvertPathToWorldPositions(List<Vector2Int> path)
    {
        List<Vector3> worldPath = new List<Vector3>();
        foreach (Vector2Int coord in path)
        {
            worldPath.Add(GridToWorld(coord));
        }
        return worldPath;
    }

    private Vector3 GridToWorld(Vector2Int gridPos)
    {
        float nodeSize = gridManager.GridSettings.NodeSize;
        return new Vector3(gridPos.x * nodeSize, 0, gridPos.y * nodeSize);
    }
} 