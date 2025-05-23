// ===== AStarPathfinder.cs =====
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Advanced A* pathfinder over a GridManager’s nodes, considering terrain weight costs.
/// </summary>
public class AStarPathfinder
{
    private GridManager gridManager;

    public AStarPathfinder(GridManager gridManager)
    {
        this.gridManager = gridManager;
    }

    /// <summary>
    /// Find a path from start to goal using A*. Returns a list of grid coordinates.
    /// </summary>
    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal)
    {
        Debug.Log("A* pathfinding FindPath()");
        // Sets for processing
        List<Vector2Int> _openSet = new List<Vector2Int>();  // Nodes to be evaluated
        HashSet<Vector2Int> _closedSet = new HashSet<Vector2Int>();  // Nodes already evaluated

        // Dictionaries to store cost scores
        Dictionary<Vector2Int, float> _gScore = new Dictionary<Vector2Int, float>(); // Cost from start
        Dictionary<Vector2Int, float> _fScore = new Dictionary<Vector2Int, float>(); // gScore + heuristic
        Dictionary<Vector2Int, Vector2Int> _cameFrom = new Dictionary<Vector2Int, Vector2Int>(); // For path tracing

        // Initialize with the start node
        _openSet.Add(start);
        _gScore[start] = 0f;
        _fScore[start] = GetHScore(start, goal);

        // Directions: up, down, left, right
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };

        int maxX = gridManager.GridSettings.GridSizeX;
        int maxY = gridManager.GridSettings.GridSizeY;

        // Continue while there are nodes to evaluate
        while (_openSet.Count > 0)
        {
            // Find the node in openSet with the lowest fScore
            Vector2Int current = GetLowestFScore(_openSet, _fScore);

            // If we've reached the goal, build and return the path
            if (current == goal)
                return ReconstructPath(_cameFrom, current);

            // Move current from openSet to closedSet
            _openSet.Remove(current);
            _closedSet.Add(current);

            // Check all neighbor nodes
            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighbor = current + dir;

                // Skip if out of grid bounds
                if (neighbor.x < 0 || neighbor.x >= maxX || neighbor.y < 0 || neighbor.y >= maxY)
                    continue;

                // Skip if already evaluated
                if (_closedSet.Contains(neighbor))
                    continue;

                GridNode node = gridManager.GetNode(neighbor.x, neighbor.y);

                // Skip if not walkable
                if (!node.walkable)
                    continue;

                // Calculate new cost to neighbor
                float tentativeG = _gScore[current] + node.weight;

                // If neighbor not in openSet, add it
                if (!_openSet.Contains(neighbor))
                {
                    _openSet.Add(neighbor);
                }
                // If this new path is not better, skip
                else if (tentativeG >= _gScore.GetValueOrDefault(neighbor, float.MaxValue))
                {
                    continue;
                }

                // This path is the best so far: record it
                _cameFrom[neighbor] = current;
                _gScore[neighbor] = tentativeG;
                _fScore[neighbor] = tentativeG + GetHScore(neighbor, goal);
            }
        }

        // No path found: return empty list
        return new List<Vector2Int>();
    }

    /// <summary>
    /// Returns the node with the lowest fScore from openSet.
    /// </summary>
    private Vector2Int GetLowestFScore(List<Vector2Int> openSet, Dictionary<Vector2Int, float> fScore)
    {
        Vector2Int bestNode = openSet[0];
        float bestScore = fScore.GetValueOrDefault(bestNode, float.MaxValue);

        foreach (Vector2Int node in openSet)
        {
            float score = fScore.GetValueOrDefault(node, float.MaxValue);
            if (score < bestScore)
            {
                bestScore = score;
                bestNode = node;
            }
        }
        return bestNode;
    }

    /// <summary>
    /// Reconstruct the path by walking backwards from goal to start.
    /// </summary>
    private List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        List<Vector2Int> path = new List<Vector2Int> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Add(current);
        }
        path.Reverse();
        return path;
    }

    /// <summary>
    /// Simple Manhattan distance heuristic for grid.
    /// </summary>
    private float GetHScore(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
}
