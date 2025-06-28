using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Optimized A* Pathfinder for grid-based movement, using a custom min-heap for performance.
/// Designed for Unity integration. Avoids per-call allocations by reusing collections.
/// </summary>
public class AStarPathfinder
{
    // Reference to the grid manager, used for node lookups and grid settings
    private GridManager gridManager;

    // ---------------------------------------------------------------------
    //  Reusable containers for performance (allocated once per instance)
    // ---------------------------------------------------------------------
    // Min-heap (priority queue) of open nodes, sorted by f-cost
    private readonly MinHeap<GridNode> _open = new MinHeap<GridNode>();
    // Closed set: nodes already evaluated
    private HashSet<GridNode> _closed = new HashSet<GridNode>();
    // Maps node to lowest known g-cost (distance from start)
    private readonly Dictionary<GridNode, int> _gCost = new Dictionary<GridNode, int>();
    // Maps node to its parent node in the best path so far
    private readonly Dictionary<GridNode, GridNode> _cameFrom = new Dictionary<GridNode, GridNode>();

    /// <summary>
    /// Constructor. Stores a reference to the grid manager for future node queries.
    /// </summary>
    /// <param name="gridManager">GridManager for node and settings queries</param>
    public AStarPathfinder(GridManager gridManager)
    {
        this.gridManager = gridManager;
    }

    /*/// <summary>
    /// Public entry: Finds a path from startCoords to endCoords (grid indices).
    /// Used in Pathfinding Manager(Unused currently)
    /// Converts coords to GridNode, then calls the node-based method.
    /// </summary>
    /// <param name="startCoords">Start cell as (x, y)</param>
    /// <param name="endCoords">End cell as (x, y)</param>
    /// <param name="unitWidth">Optional: unit width in grid cells</param>
    /// <param name="unitHeight">Optional: unit height in grid cells</param>
    /// <returns>Path as list of grid indices, or empty if no path found</returns>
    public List<Vector2Int> FindPath(Vector2Int startCoords, Vector2Int endCoords, int unitWidth = 1, int unitHeight = 1)
    {
        // Get grid nodes for start/end coordinates
        GridNode start = gridManager.GetNode(startCoords.x, startCoords.y);
        GridNode end = gridManager.GetNode(endCoords.x, endCoords.y);

        // Call main search using node references
        return FindPathWithNodes(start, end, unitWidth, unitHeight);
    }*/

    /// <summary>
    /// Core A* pathfinding algorithm using reusable containers.
    /// Supports multi-tile (width/height) units.
    /// GC-free between calls.
    /// </summary>
    /// <param name="start">Start node</param>
    /// <param name="end">End node</param>
    /// <param name="unitWidth">Optional: unit width</param>
    /// <param name="unitHeight">Optional: unit height</param>
    /// <returns>Path as list of grid indices, or empty if no path found</returns>
    public List<Vector2Int> FindPathWithNodes(
        GridNode start,
        GridNode end,
        int unitWidth = 1,
        int unitHeight = 1)
    {
        // ----- Reset containers for new search -----
        _open.Clear();
        _gCost.Clear();
        _cameFrom.Clear();
        _closed.Clear();

        // ----- Initialize start node -----
        _open.Enqueue(start, 0f);   // Start node, f-cost = 0
        _gCost[start] = 0;
        _cameFrom[start] = start;   // Root sentinel (self-referencing)

        // ----- Main A* search loop -----
        while (_open.Count > 0)
        {
            GridNode current = _open.Dequeue();

            // Skip if node already closed (visited)
            if (!_closed.Add(current))
                continue;

            // If reached the end node, stop search
            if (current == end) break;

            // Check each walkable neighbor
            foreach (GridNode neighbor in GetNeighbors(current))
            {
                if (!neighbor.walkable) continue; // Only process walkable nodes

                // Optional: use IsAreaWalkable if supporting large (multi-tile) units
                // if (!IsAreaWalkable(neighbor, unitWidth, unitHeight)) continue;

                int newCost = _gCost[current] + neighbor.weight;

                // First time visiting this node
                if (!_gCost.ContainsKey(neighbor))
                {
                    _gCost[neighbor] = newCost;
                    float priority = newCost + Heuristic(neighbor, end);
                    _open.Enqueue(neighbor, priority);
                    _cameFrom[neighbor] = current;
                }
                // Found a cheaper path to a node already seen
                else if (newCost < _gCost[neighbor])
                {
                    _gCost[neighbor] = newCost;
                    float priority = newCost + Heuristic(neighbor, end);
                    _open.DecreaseKey(neighbor, priority);
                    _cameFrom[neighbor] = current;
                }
            }
        }

        // ----- Path reconstruction -----
        // If end was never reached, return empty path
        if (!_cameFrom.ContainsKey(end))
            return new List<Vector2Int>();

        List<Vector2Int> path = new List<Vector2Int>();
        float nodeSize = gridManager.GridSettings.NodeSize;
        GridNode node = end;

        // Trace path backwards from end to start
        while (node != start)
        {
            int gx = Mathf.RoundToInt(node.worldPosition.x / nodeSize);
            int gy = Mathf.RoundToInt(node.worldPosition.z / nodeSize);
            path.Add(new Vector2Int(gx, gy));
            node = _cameFrom[node];
        }
        path.Reverse();
        return path;
    }

    /// <summary>
    /// Returns four orthogonal neighbors for the given node (up, down, left, right).
    /// </summary>
    private IEnumerable<GridNode> GetNeighbors(GridNode node)
    {
        float nodeSize = gridManager.GridSettings.NodeSize;
        int x = Mathf.RoundToInt(node.worldPosition.x / nodeSize);
        int y = Mathf.RoundToInt(node.worldPosition.z / nodeSize);

        int maxX = gridManager.GridSettings.GridSizeX;
        int maxY = gridManager.GridSettings.GridSizeY;

        if (y + 1 < maxY) yield return gridManager.GetNode(x, y + 1);
        if (y - 1 >= 0) yield return gridManager.GetNode(x, y - 1);
        if (x + 1 < maxX) yield return gridManager.GetNode(x + 1, y);
        if (x - 1 >= 0) yield return gridManager.GetNode(x - 1, y);
    }

    /*/// <summary>
    /// Checks if an area (multi-tile rectangle) is fully walkable.
    /// Use for large units(occupy multi grid cell); returns false if any node in the area is blocked or out of bounds.
    /// </summary>
    private bool IsAreaWalkable(GridManager gm, GridNode node, int width, int height)
    {
        float nodeSize = gm.GridSettings.NodeSize;
        int baseX = Mathf.RoundToInt(node.worldPosition.x / nodeSize);
        int baseY = Mathf.RoundToInt(node.worldPosition.z / nodeSize);

        for (int dx = 0; dx < width; dx++)
        {
            for (int dy = 0; dy < height; dy++)
            {
                int nx = baseX + dx;
                int ny = baseY + dy;
                if (nx < 0 || nx >= gm.GridSettings.GridSizeX ||
                    ny < 0 || ny >= gm.GridSettings.GridSizeY)
                    return false;

                GridNode checkNode = gm.GetNode(nx, ny);
                if (!checkNode.walkable)
                    return false;
            }
        }
        return true;
    }*/

    /// <summary>
    /// Heuristic function: Manhattan distance in world space (used for grid navigation).
    /// </summary>
    /// <returns>Estimated cost from node a to node b</returns>
    private int Heuristic(GridNode a, GridNode b)
    {
        float dx = Mathf.Abs(a.worldPosition.x - b.worldPosition.x);
        float dz = Mathf.Abs(a.worldPosition.z - b.worldPosition.z);
        return Mathf.RoundToInt(dx + dz);
    }
}
