using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnweightedPathVisualizer
{
    private readonly UnweightedPathfinder pathfinder;
    private readonly GridManager gridManager;
    private readonly System.Func<IEnumerator, Coroutine> startCoroutine;

    public UnweightedPathVisualizer(
        UnweightedPathfinder pathfinder,
        GridManager gridManager,
        System.Func<IEnumerator, Coroutine> startCoroutine)
    {
        this.pathfinder = pathfinder;
        this.gridManager = gridManager;
        this.startCoroutine = startCoroutine;
    }

    public IEnumerator VisualizePath(
        Vector2Int start,
        Vector2Int end,
        HashSet<Vector2Int> visitedNodes,
        List<Vector2Int> currentPath,
        System.Action<List<Vector3>> onPathFound,
        Dictionary<Vector2Int, int> nodeDistances
    )
    {
        yield return startCoroutine(pathfinder.FindPath(start, end, (path, cameFrom, visited, order, current, neighbors, distances) =>
        {
            // Update visualization state
            visitedNodes.Clear();
            visitedNodes.UnionWith(visited);
            
            currentPath.Clear();
            currentPath.AddRange(path);

            // Update distances
            nodeDistances.Clear();
            foreach (var kvp in distances)
            {
                nodeDistances[kvp.Key] = kvp.Value;
            }

            // If we have a complete path, convert it to world positions
            if (path.Count > 0 && path[0] == start && path[path.Count - 1] == end)
            {
                onPathFound(ConvertPathToWorldPositions(path));
            }
        }));
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