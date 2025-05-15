using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnweightedPathfinder
{
    private readonly System.Func<Vector2Int, List<Vector2Int>> getNeighbors;
    private readonly System.Func<IEnumerator> waitForNextStep;

    public UnweightedPathfinder(
        System.Func<Vector2Int, List<Vector2Int>> getNeighbors,
        System.Func<IEnumerator> waitForNextStep)
    {
        this.getNeighbors = getNeighbors;
        this.waitForNextStep = waitForNextStep;
    }

    public IEnumerator FindPath(
        Vector2Int start,
        Vector2Int end,
        System.Action<List<Vector2Int>, Dictionary<Vector2Int, Vector2Int>, HashSet<Vector2Int>, List<Vector2Int>, Vector2Int?, List<Vector2Int>, Dictionary<Vector2Int, int>> onStep)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        List<Vector2Int> explorationOrder = new List<Vector2Int>();
        Vector2Int? currentNode = null;
        List<Vector2Int> currentNeighbors = new List<Vector2Int>();
        Dictionary<Vector2Int, int> distances = new Dictionary<Vector2Int, int>();

        // Initialize start node
        queue.Enqueue(start);
        visited.Add(start);
        explorationOrder.Add(start);
        distances[start] = 0;
        onStep(new List<Vector2Int>(), cameFrom, visited, explorationOrder, start, new List<Vector2Int>(), distances);
        yield return waitForNextStep();

        while (queue.Count > 0)
        {
            // Get next node to explore
            currentNode = queue.Dequeue();
            currentNeighbors = getNeighbors(currentNode.Value);
            onStep(new List<Vector2Int>(), cameFrom, visited, explorationOrder, currentNode, currentNeighbors, distances);
            yield return waitForNextStep();

            if (currentNode.Value == end)
            {
                // Reconstruct path
                List<Vector2Int> path = new List<Vector2Int>();
                Vector2Int current = end;
                path.Add(current);
                onStep(path, cameFrom, visited, explorationOrder, current, new List<Vector2Int>(), distances);
                yield return waitForNextStep();

                while (current != start)
                {
                    current = cameFrom[current];
                    path.Add(current);
                    onStep(path, cameFrom, visited, explorationOrder, current, new List<Vector2Int>(), distances);
                    yield return waitForNextStep();
                }

                path.Reverse();
                onStep(path, cameFrom, visited, explorationOrder, null, new List<Vector2Int>(), distances);
                yield break;
            }

            // Process each neighbor
            foreach (Vector2Int neighbor in currentNeighbors)
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    cameFrom[neighbor] = currentNode.Value;
                    queue.Enqueue(neighbor);
                    explorationOrder.Add(neighbor);
                    distances[neighbor] = distances[currentNode.Value] + 1;
                    onStep(new List<Vector2Int>(), cameFrom, visited, explorationOrder, currentNode, currentNeighbors, distances);
                    yield return waitForNextStep();
                }
            }
        }

        // No path found
        onStep(new List<Vector2Int>(), cameFrom, visited, explorationOrder, null, new List<Vector2Int>(), distances);
    }
} 