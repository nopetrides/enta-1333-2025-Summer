using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BruteForcePathVisualizer
{
    private readonly BruteForcePathfinder pathfinder;
    private readonly GridManager gridManager;
    private readonly System.Func<IEnumerator> waitForNextStep;
    private readonly System.Func<IEnumerator, Coroutine> startCoroutine;

    public BruteForcePathVisualizer(
        BruteForcePathfinder pathfinder,
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
        List<Vector2Int> bestPath = null;
        float bestCost = float.MaxValue;
        List<Vector2Int> path = new List<Vector2Int>();

        // Stack to replace recursion
        Stack<(Vector2Int node, float cost, int neighborIndex)> stack = new Stack<(Vector2Int, float, int)>();
        stack.Push((start, 0, 0));
        visitedNodes.Add(start);
        path.Add(start);
        currentPath.Add(start);

        while (stack.Count > 0)
        {
            var (current, currentCost, neighborIndex) = stack.Pop();

            // If we've found a better path to this node before, skip
            if (currentCost >= bestCost)
            {
                // Remove from current path if we're backtracking
                if (path.Count > 0 && path[path.Count - 1] == current)
                {
                    path.RemoveAt(path.Count - 1);
                    currentPath.RemoveAt(currentPath.Count - 1);
                }
                continue;
            }

            // If we've reached the end, check if this is the best path
            if (current == end)
            {
                if (currentCost < bestCost)
                {
                    bestCost = currentCost;
                    bestPath = new List<Vector2Int>(path);
                }
                // Remove from current path if we're backtracking
                if (path.Count > 0 && path[path.Count - 1] == current)
                {
                    path.RemoveAt(path.Count - 1);
                    currentPath.RemoveAt(currentPath.Count - 1);
                }
                continue;
            }

            // Get neighbors for this node
            List<Vector2Int> neighbors = GetNeighbors(current);

            // If we've processed all neighbors, backtrack
            if (neighborIndex >= neighbors.Count)
            {
                // Remove from current path if we're backtracking
                if (path.Count > 0 && path[path.Count - 1] == current)
                {
                    path.RemoveAt(path.Count - 1);
                    currentPath.RemoveAt(currentPath.Count - 1);
                }
                visitedNodes.Remove(current);
                continue;
            }

            // Process next neighbor
            Vector2Int neighbor = neighbors[neighborIndex];
            
            // Push current state back with incremented neighbor index
            stack.Push((current, currentCost, neighborIndex + 1));

            if (!visitedNodes.Contains(neighbor))
            {
                visitedNodes.Add(neighbor);
                path.Add(neighbor);
                currentPath.Add(neighbor);

                float newCost = currentCost + CalculatePathLength(path);
                stack.Push((neighbor, newCost, 0));
            }

            yield return waitForNextStep();
        }

        if (bestPath != null)
        {
            onPathFound(ConvertPathToWorldPositions(bestPath));
        }
        else
        {
            Debug.LogWarning("No valid path found");
            onPathFound(new List<Vector3>());
        }
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

    private float CalculatePathLength(List<Vector2Int> path)
    {
        float length = 0;
        for (int i = 0; i < path.Count - 1; i++)
        {
            length += Vector2Int.Distance(path[i], path[i + 1]);
        }
        return length;
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