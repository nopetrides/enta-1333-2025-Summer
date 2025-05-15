using System.Collections.Generic;
using UnityEngine;

public class Pathfinder : MonoBehaviour
{
    public enum PathfindingType
    {
        Unweighted,     // BFS - Fastest, ignores weights
        Weighted,       // Dijkstra's - Handles weights, optimal path
        BruteForce,     // Explores all paths - Demonstrates why we need better algorithms
        Naive          // "Draws" a line to target - Common beginner mistake
    }

    [Header("Required References")]
    [SerializeField] private GridManager gridManager;

    [Header("Pathfinding Settings")]
    [SerializeField] private PathfindingType pathfindingType = PathfindingType.Unweighted;

    private void Awake()
    {
        if (gridManager == null)
        {
            Debug.LogError("Pathfinder: GridManager reference is missing. Please assign it in the inspector.");
            enabled = false;
        }
    }

    public List<Vector3> FindPath(Vector3 startPos, Vector3 endPos)
    {
        if (!enabled) return new List<Vector3>();

        // Convert world positions to grid coordinates
        Vector2Int startCoord = WorldToGrid(startPos);
        Vector2Int endCoord = WorldToGrid(endPos);

        // Validate coordinates
        if (!IsValidCoordinate(startCoord) || !IsValidCoordinate(endCoord))
        {
            Debug.LogWarning("Invalid start or end coordinates for pathfinding");
            return new List<Vector3>();
        }

        return pathfindingType switch
        {
            PathfindingType.Unweighted => FindUnweightedPath(startCoord, endCoord),
            PathfindingType.Weighted => FindWeightedPath(startCoord, endCoord),
            PathfindingType.BruteForce => FindBruteForcePath(startCoord, endCoord),
            PathfindingType.Naive => FindNaivePath(startCoord, endCoord),
            _ => FindUnweightedPath(startCoord, endCoord)
        };
    }

    #region Unweighted Pathfinding (BFS)
    private List<Vector3> FindUnweightedPath(Vector2Int start, Vector2Int end)
    {
        // Standard BFS implementation
        // Uses a simple queue and only considers walkability
        // Guarantees shortest path in terms of number of steps
        // Time Complexity: O(V + E) where V is vertices and E is edges
        // Space Complexity: O(V) for the queue and visited set

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            if (current == end)
            {
                return ReconstructPath(cameFrom, start, end);
            }

            foreach (Vector2Int neighbor in GetNeighbors(current))
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    cameFrom[neighbor] = current;
                    queue.Enqueue(neighbor);
                }
            }
        }

        Debug.LogWarning("No valid path found");
        return new List<Vector3>();
    }
    #endregion

    #region Weighted Pathfinding (Dijkstra's)
    private List<Vector3> FindWeightedPath(Vector2Int start, Vector2Int end)
    {
        // Dijkstra's algorithm implementation
        // Uses a priority queue to consider movement costs
        // Guarantees shortest path in terms of total movement cost
        // Time Complexity: O((V + E)logV) with binary heap
        // Space Complexity: O(V) for the priority queue and visited set

        // Priority queue implementation using a sorted list
        // Key: total cost to reach node, Value: node coordinates
        SortedList<float, Vector2Int> openSet = new SortedList<float, Vector2Int>();
        Dictionary<Vector2Int, float> costSoFar = new Dictionary<Vector2Int, float>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();

        // Initialize start node
        openSet.Add(0, start);
        costSoFar[start] = 0;

        while (openSet.Count > 0)
        {
            // Get node with lowest cost
            var current = openSet.Values[0];
            openSet.RemoveAt(0);

            if (current == end)
            {
                return ReconstructPath(cameFrom, start, end);
            }

            closedSet.Add(current);

            foreach (Vector2Int neighbor in GetNeighbors(current))
            {
                if (closedSet.Contains(neighbor)) continue;

                float newCost = costSoFar[current] + GetMovementCost(current, neighbor);

                if (!costSoFar.ContainsKey(neighbor) || newCost < costSoFar[neighbor])
                {
                    costSoFar[neighbor] = newCost;
                    cameFrom[neighbor] = current;

                    // Add to open set with total cost as priority
                    // Add small offset to handle duplicate costs
                    float priority = newCost + (openSet.Count * 0.0001f);
                    openSet.Add(priority, neighbor);
                }
            }
        }

        Debug.LogWarning("No valid path found");
        return new List<Vector3>();
    }

    private float GetMovementCost(Vector2Int from, Vector2Int to)
    {
        // Get the movement cost of the destination node
        // This is where terrain weights come into play
        return gridManager.GetNode(to.x, to.y).Weight;
    }
    #endregion

    #region Brute Force Pathfinding
    private List<Vector3> FindBruteForcePath(Vector2Int start, Vector2Int end)
    {
        // Brute force implementation that explores ALL possible paths
        // This is intentionally inefficient to demonstrate why we use proper algorithms
        // Time Complexity: O(b^d) where b is branching factor (4 for grid) and d is depth
        // Space Complexity: O(b^d) for the recursion stack and path storage
        // This is exponentially worse than both BFS and Dijkstra's!

        List<Vector2Int> bestPath = null;
        float bestCost = float.MaxValue;
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        List<Vector2Int> currentPath = new List<Vector2Int>();

        void ExplorePath(Vector2Int current, float currentCost)
        {
            // If we've found a better path to this node before, stop exploring
            if (currentCost >= bestCost) return;

            // If we've reached the end, check if this is the best path
            if (current == end)
            {
                if (currentCost < bestCost)
                {
                    bestCost = currentCost;
                    bestPath = new List<Vector2Int>(currentPath);
                }
                return;
            }

            // Try all possible neighbors
            foreach (Vector2Int neighbor in GetNeighbors(current))
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    currentPath.Add(neighbor);
                    
                    // Recursively explore this path
                    float newCost = currentCost + GetMovementCost(current, neighbor);
                    ExplorePath(neighbor, newCost);
                    
                    // Backtrack
                    currentPath.RemoveAt(currentPath.Count - 1);
                    visited.Remove(neighbor);
                }
            }
        }

        // Start the exploration
        visited.Add(start);
        currentPath.Add(start);
        ExplorePath(start, 0);

        if (bestPath != null)
        {
            return ConvertPathToWorldPositions(bestPath);
        }

        Debug.LogWarning("No valid path found");
        return new List<Vector3>();
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
    #endregion

    #region Naive Pathfinding
    private List<Vector3> FindNaivePath(Vector2Int start, Vector2Int end)
    {
        // A naive implementation that "draws" a line to the target
        // This is a common beginner mistake - trying to go straight to the target
        // without considering obstacles or weights
        // Time Complexity: O(n) where n is the number of nodes in the path
        // Space Complexity: O(n) for the path list
        // This will often fail to find valid paths and ignore obstacles!

        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int current = start;
        path.Add(current);

        // Keep moving towards the target until we reach it or get stuck
        while (current != end)
        {
            // Calculate direction to target
            Vector2Int direction = new Vector2Int(
                Mathf.Clamp(end.x - current.x, -1, 1),
                Mathf.Clamp(end.y - current.y, -1, 1)
            );

            // Try to move in the direction of the target
            Vector2Int next = current + direction;

            // If we can't move in the preferred direction, try moving in just x or just y
            if (!IsValidCoordinate(next) || !gridManager.GetNode(next.x, next.y).Walkable)
            {
                // Try moving in x direction only
                next = current + new Vector2Int(direction.x, 0);
                if (!IsValidCoordinate(next) || !gridManager.GetNode(next.x, next.y).Walkable)
                {
                    // Try moving in y direction only
                    next = current + new Vector2Int(0, direction.y);
                    if (!IsValidCoordinate(next) || !gridManager.GetNode(next.x, next.y).Walkable)
                    {
                        // If we can't move at all, we're stuck
                        Debug.LogWarning("Naive pathfinding got stuck!");
                        return new List<Vector3>();
                    }
                }
            }

            current = next;
            path.Add(current);

            // Safety check to prevent infinite loops
            if (path.Count > gridManager.GridSettings.GridSizeX * gridManager.GridSettings.GridSizeY)
            {
                Debug.LogWarning("Naive pathfinding exceeded maximum path length!");
                return new List<Vector3>();
            }
        }

        return ConvertPathToWorldPositions(path);
    }
    #endregion

    private List<Vector3> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int start, Vector2Int end)
    {
        List<Vector3> path = new List<Vector3>();
        Vector2Int current = end;

        while (current != start)
        {
            path.Add(GridToWorld(current));
            current = cameFrom[current];
        }
        path.Add(GridToWorld(start));
        path.Reverse();
        return path;
    }

    private Vector2Int WorldToGrid(Vector3 worldPos)
    {
        float nodeSize = gridManager.GridSettings.NodeSize;
        int x = Mathf.RoundToInt(worldPos.x / nodeSize);
        int y = Mathf.RoundToInt(worldPos.z / nodeSize);
        return new Vector2Int(x, y);
    }

    private Vector3 GridToWorld(Vector2Int gridPos)
    {
        float nodeSize = gridManager.GridSettings.NodeSize;
        return new Vector3(gridPos.x * nodeSize, 0, gridPos.y * nodeSize);
    }

    private bool IsValidCoordinate(Vector2Int coord)
    {
        return coord.x >= 0 && coord.x < gridManager.GridSettings.GridSizeX &&
               coord.y >= 0 && coord.y < gridManager.GridSettings.GridSizeY;
    }

    private List<Vector2Int> GetNeighbors(Vector2Int coord)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1),  // North
            new Vector2Int(1, 0),  // East
            new Vector2Int(0, -1), // South
            new Vector2Int(-1, 0)  // West
        };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int neighbor = coord + dir;
            if (IsValidCoordinate(neighbor) && gridManager.GetNode(neighbor.x, neighbor.y).Walkable)
            {
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }
} 