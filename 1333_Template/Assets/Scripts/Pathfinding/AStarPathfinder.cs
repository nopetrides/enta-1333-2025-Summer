using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Optimized A* Pathfinder using a custom priority queue to ensure Unity compatibility.
/// Tracking collections (VisitedNodes, FrontierNodes) have been removed.
/// </summary>
public class AStarPathfinder
{
    private GridManager gridManager;

    // ---------------------------------------------------------------------
    //  Reusable containers (no per-call allocation)
    // ---------------------------------------------------------------------
    private readonly MinHeap<GridNode> _open = new MinHeap<GridNode>();
    private readonly Dictionary<GridNode, int> _gCost = new Dictionary<GridNode, int>();
    private readonly Dictionary<GridNode, GridNode> _cameFrom = new Dictionary<GridNode, GridNode>();

    /// <summary>
    /// Constructor: Stores a reference to the GridManager for node queries.
    /// </summary>
    /// <param name="gridManager">The GridManager instance used to access grid nodes and settings.</param>
    public AStarPathfinder(GridManager gridManager)
    {
        this.gridManager = gridManager;
    }

    /// <summary>
    /// Entry point: Finds a path using grid coordinates and returns it as a list of Vector2Int.
    /// </summary>
    /// <param name="startCoords">The starting grid coordinates (x, y).</param>
    /// <param name="endCoords">The target grid coordinates (x, y).</param>
    /// <param name="unitWidth">Width of the unit in grid cells (default is 1).</param>
    /// <param name="unitHeight">Height of the unit in grid cells (default is 1).</param>
    /// <returns>A list of Vector2Int that represents the path from start to end. Returns an empty list if no path exists.</returns>
    public List<Vector2Int> FindPath(Vector2Int startCoords, Vector2Int endCoords, int unitWidth = 1, int unitHeight = 1)
    {
        // Obtain the corresponding GridNode instances for the start and end coordinates.
        GridNode start = gridManager.GetNode(startCoords.x, startCoords.y);
        GridNode end = gridManager.GetNode(endCoords.x, endCoords.y);

        // Delegate to the method that works directly with GridNode objects.
        return FindPathWithNodes(start, end, unitWidth, unitHeight);
    }

    /// <summary>
    /// Core A* search. Uses the reusable containers declared at class scope,
    /// so no new allocations are made per call (GC-free).
    /// The optional width/height parameters let you reuse this for multi-tile units
    /// even if they are not used right now.
    /// </summary>
    public List<Vector2Int> FindPathWithNodes(
        GridNode start,
        GridNode end,
        int unitWidth = 1,
        int unitHeight = 1)
    {
        // ----- clear reusable containers -----------------------------------
        _open.Clear();
        _gCost.Clear();
        _cameFrom.Clear();

        // ----- initialise ---------------------------------------------------
        _open.Enqueue(start, 0f);   // F-cost = 0
        _gCost[start] = 0;
        _cameFrom[start] = start;   // root sentinel

        // ----- main loop ----------------------------------------------------
        while (_open.Count > 0)
        {
            GridNode current = _open.Dequeue();
            if (current == end) break;

            foreach (GridNode neighbor in GetNeighbors(current))
            {
                if (!neighbor.walkable) continue;                // wall
                                                                 // if (!IsAreaWalkable(neighbor, unitWidth, unitHeight)) continue;

                int newCost = _gCost[current] + neighbor.weight;
                if (!_gCost.ContainsKey(neighbor) || newCost < _gCost[neighbor])
                {
                    _gCost[neighbor] = newCost;
                    float priority = newCost + Heuristic(neighbor, end);
                    _open.Enqueue(neighbor, priority);
                    _cameFrom[neighbor] = current;
                }
            }
        }

        // ----- reconstruct path --------------------------------------------
        if (!_cameFrom.ContainsKey(end))
            return new List<Vector2Int>();   // no path found

        List<Vector2Int> path = new List<Vector2Int>();
        float nodeSize = gridManager.GridSettings.NodeSize;
        GridNode node = end;

        // back-track from end -> start
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
    /// Returns the four orthogonal neighbours of a node.
    /// Uses the GridManager instance stored in this pathfinder.
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

    /// <summary>
    /// Checks whether a rectangular area of the grid (based on unit width and height) is fully walkable.
    /// Prevents movement if any node in that area is not walkable or out of bounds.
    /// </summary>
    /// <param name="gm">The GridManager for node queries and settings.</param>
    /// <param name="node">The base node (bottom-left) to check area from.</param>
    /// <param name="width">The width of the unit in grid cells.</param>
    /// <param name="height">The height of the unit in grid cells.</param>
    /// <returns>True if all nodes in the area are walkable; otherwise, false.</returns>
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
    }

    /// <summary>
    /// Estimates the remaining cost (heuristic) from node a to node b using Manhattan distance in world-space.
    /// </summary>
    /// <param name="a">The GridNode representing the current location.</param>
    /// <param name="b">The GridNode representing the target location.</param>
    /// <returns>An integer heuristic cost estimate.</returns>
    private int Heuristic(GridNode a, GridNode b)
    {
        float dx = Mathf.Abs(a.worldPosition.x - b.worldPosition.x);
        float dz = Mathf.Abs(a.worldPosition.z - b.worldPosition.z);
        return Mathf.RoundToInt(dx + dz);
    }
}
