using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Brute-force pathfinder using BFS with simple arrays.
/// </summary>
public class BruteForcePathfinder
{
    private GridManager gridManager;
    private int width, height;
    private bool[,] visited;
    private Vector2Int[,] previous;

    // Directions we can move: right, left, up, down
    private static readonly Vector2Int[] directions =
    {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1)
    };

    public BruteForcePathfinder(GridManager gridManager)
    {
        this.gridManager = gridManager;
    }

    /// <summary>
    /// Finds a path from start to goal by exploring neighbors layer by layer.
    /// Returns the list of coordinates from start to goal (or empty if none).
    /// </summary>
    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal)
    {
        Debug.Log("Brute-force pathfinding FindPath()");
        // Dimensions and helper arrays
        width = gridManager.GridSettings.GridSizeX;
        height = gridManager.GridSettings.GridSizeY;
        visited = new bool[width, height];
        previous = new Vector2Int[width, height];

        // Queue for BFS
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        // Start from the starting cell
        queue.Enqueue(start);
        visited[start.x, start.y] = true;
        previous[start.x, start.y] = start;

        // BFS loop
        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            // Stop if we reached the goal
            if (current == goal)
                break;

            // Check neighbors
            foreach (Vector2Int dir in directions)
            {
                Vector2Int next = current + dir;

                // Skip out of bounds
                if (next.x < 0 || next.x >= width || next.y < 0 || next.y >= height)
                    continue;

                // Skip if already visited
                if (visited[next.x, next.y])
                    continue;
                // Skip if the node is NOT walkable
                GridNode node = gridManager.GetNode(next.x, next.y);
                if (!node.walkable)
                    continue;

                // Mark visited
                visited[next.x, next.y] = true;
                // Remember how we got here
                previous[next.x, next.y] = current;
                // Add to queue for next exploration
                queue.Enqueue(next);
            }
        }

        // Reconstruct path from goal to start
        return BuildPath(start, goal);
    }

    // Builds the path by following 'previous' back from goal to start
    private List<Vector2Int> BuildPath(Vector2Int start, Vector2Int goal)
    {
        List<Vector2Int> path = new List<Vector2Int>();

        // If goal wasn't reached, there is no path
        if (!visited[goal.x, goal.y])
            return path;

        Vector2Int step = goal;
        while (step != start)
        {
            path.Add(step);
            step = previous[step.x, step.y];
        }
        
        path.Add(start);
        path.Reverse(); // make it start->goal order
        return path;
    }
}