using System.Collections.Generic;
using UnityEngine;

public class BruteForcePathfinder
{
    private readonly System.Func<Vector2Int, List<Vector2Int>> getNeighbors;
    private readonly int maxPathLength;

    public BruteForcePathfinder(
        System.Func<Vector2Int, List<Vector2Int>> getNeighbors,
        int maxPathLength = 100)
    {
        this.getNeighbors = getNeighbors;
        this.maxPathLength = maxPathLength;
    }

    public (List<Vector2Int> path, HashSet<Vector2Int> visited) FindPath(Vector2Int start, Vector2Int end)
    {
        List<Vector2Int> bestPath = null;
        float bestCost = float.MaxValue;
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        List<Vector2Int> currentPath = new List<Vector2Int>();

        // Stack to replace recursion
        Stack<(Vector2Int node, float cost, int neighborIndex)> stack = new Stack<(Vector2Int, float, int)>();
        stack.Push((start, 0, 0));
        visited.Add(start);
        currentPath.Add(start);

        while (stack.Count > 0)
        {
            var (current, currentCost, neighborIndex) = stack.Pop();

            // If we've found a better path to this node before, skip
            if (currentCost >= bestCost)
            {
                // Remove from current path if we're backtracking
                if (currentPath.Count > 0 && currentPath[currentPath.Count - 1] == current)
                {
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
                    bestPath = new List<Vector2Int>(currentPath);
                }
                // Remove from current path if we're backtracking
                if (currentPath.Count > 0 && currentPath[currentPath.Count - 1] == current)
                {
                    currentPath.RemoveAt(currentPath.Count - 1);
                }
                continue;
            }

            // Get neighbors for this node
            List<Vector2Int> neighbors = getNeighbors(current);

            // If we've processed all neighbors, backtrack
            if (neighborIndex >= neighbors.Count)
            {
                // Remove from current path if we're backtracking
                if (currentPath.Count > 0 && currentPath[currentPath.Count - 1] == current)
                {
                    currentPath.RemoveAt(currentPath.Count - 1);
                }
                visited.Remove(current);
                continue;
            }

            // Process next neighbor
            Vector2Int neighbor = neighbors[neighborIndex];
            
            // Push current state back with incremented neighbor index
            stack.Push((current, currentCost, neighborIndex + 1));

            if (!visited.Contains(neighbor))
            {
                visited.Add(neighbor);
                currentPath.Add(neighbor);

                float newCost = currentCost + CalculatePathLength(currentPath);
                stack.Push((neighbor, newCost, 0));
            }
        }

        return (bestPath ?? new List<Vector2Int>(), visited);
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
} 