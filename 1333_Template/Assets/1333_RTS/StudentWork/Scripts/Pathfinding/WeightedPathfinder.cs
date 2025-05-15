using System.Collections.Generic;
using UnityEngine;

public class WeightedPathfinder
{
    private readonly System.Func<Vector2Int, List<Vector2Int>> getNeighbors;
    private readonly System.Func<Vector2Int, Vector2Int, float> getMovementCost;

    public WeightedPathfinder(
        System.Func<Vector2Int, List<Vector2Int>> getNeighbors,
        System.Func<Vector2Int, Vector2Int, float> getMovementCost)
    {
        this.getNeighbors = getNeighbors;
        this.getMovementCost = getMovementCost;
    }

    public (List<Vector2Int> path, Dictionary<Vector2Int, Vector2Int> cameFrom, HashSet<Vector2Int> visited) FindPath(Vector2Int start, Vector2Int end)
    {
        SortedList<float, Vector2Int> openSet = new SortedList<float, Vector2Int>();
        Dictionary<Vector2Int, float> costSoFar = new Dictionary<Vector2Int, float>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        openSet.Add(0, start);
        costSoFar[start] = 0;

        while (openSet.Count > 0)
        {
            var current = openSet.Values[0];
            openSet.RemoveAt(0);

            if (current == end)
            {
                return (ReconstructPath(cameFrom, start, end), cameFrom, visited);
            }

            visited.Add(current);

            foreach (Vector2Int neighbor in getNeighbors(current))
            {
                if (visited.Contains(neighbor)) continue;

                float newCost = costSoFar[current] + getMovementCost(current, neighbor);

                if (!costSoFar.ContainsKey(neighbor) || newCost < costSoFar[neighbor])
                {
                    costSoFar[neighbor] = newCost;
                    cameFrom[neighbor] = current;

                    float priority = newCost + (openSet.Count * 0.0001f);
                    openSet.Add(priority, neighbor);
                }
            }
        }

        return (new List<Vector2Int>(), cameFrom, visited);
    }

    public IEnumerable<Vector2Int> GetNeighbors(Vector2Int current)
    {
        return getNeighbors(current);
    }

    public float GetMovementCost(Vector2Int from, Vector2Int to)
    {
        return getMovementCost(from, to);
    }

    private List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int start, Vector2Int end)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int current = end;

        while (current != start)
        {
            path.Add(current);
            current = cameFrom[current];
        }
        path.Add(start);
        path.Reverse();
        return path;
    }
} 