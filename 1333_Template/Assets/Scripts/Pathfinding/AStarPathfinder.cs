using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Optimized A* Pathfinder using a custom priority queue to ensure Unity compatibility.
/// Tracking collections (VisitedNodes, FrontierNodes) have been removed.
/// </summary>
public class AStarPathfinder
{
    private GridManager gridManager;

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
    /// Runs the A* algorithm between two GridNodes, then converts the resulting node path
    /// into a List<Vector2Int> of grid coordinates.
    /// </summary>
    /// <param name="start">The starting GridNode.</param>
    /// <param name="end">The target GridNode.</param>
    /// <param name="unitWidth">Width of the unit in grid cells.</param>
    /// <param name="unitHeight">Height of the unit in grid cells.</param>
    /// <returns>A list of Vector2Int coordinates representing the path, or an empty list if no path is found.</returns>
    public List<Vector2Int> FindPathWithNodes(GridNode start, GridNode end, int unitWidth, int unitHeight)
    {
        // Initialize the open set (priority queue) and dictionaries to track costs and path.
        SimplePriorityQueue<GridNode> openSet = new SimplePriorityQueue<GridNode>();
        Dictionary<GridNode, int> costSoFar = new Dictionary<GridNode, int>();
        Dictionary<GridNode, GridNode> cameFrom = new Dictionary<GridNode, GridNode>();

        // Start with the start node: zero cost, priority zero.
        openSet.Enqueue(start, 0);
        costSoFar[start] = 0;
        cameFrom[start] = start;

        // Loop until there are no more nodes to explore.
        while (openSet.Count > 0)
        {
            // Dequeue the node with the lowest priority (estimated total cost).
            GridNode current = openSet.Dequeue();

            // If we have reached the end node, exit the loop.
            if (current.Equals(end))
                break;

            // Explore each neighbor of the current node.
            foreach (GridNode neighbor in GetNeighbors(gridManager, current))
            {
                // Skip neighbor if the area is not fully walkable for the given unit size.
                if (!IsAreaWalkable(gridManager, neighbor, unitWidth, unitHeight))
                    continue;

                // Calculate new cost to reach this neighbor.
                int newCost = costSoFar[current] + neighbor.weight;

                // If this neighbor is not in costSoFar or we found a cheaper path to it, update.
                if (!costSoFar.ContainsKey(neighbor) || newCost < costSoFar[neighbor])
                {
                    costSoFar[neighbor] = newCost;
                    int priority = newCost + Heuristic(neighbor, end);
                    openSet.Enqueue(neighbor, priority);
                    cameFrom[neighbor] = current;
                }
            }
        }

        // If the end node was never reached, return an empty path.
        if (!cameFrom.ContainsKey(end))
            return new List<Vector2Int>();

        // Reconstruct the path by walking backwards from end to start.
        List<GridNode> nodePath = new List<GridNode>();
        GridNode pathNode = end;
        while (!pathNode.Equals(start))
        {
            nodePath.Add(pathNode);
            pathNode = cameFrom[pathNode];
        }
        nodePath.Add(start);
        nodePath.Reverse(); // Reverse to get start-to-end order.

        // Convert nodePath (GridNode instances) into Vector2Int coordinates.
        List<Vector2Int> path = new List<Vector2Int>();
        float nodeSize = gridManager.GridSettings.NodeSize;
        foreach (GridNode node in nodePath)
        {
            int x = Mathf.RoundToInt(node.worldPosition.x / nodeSize);
            int y = Mathf.RoundToInt(node.worldPosition.z / nodeSize);
            path.Add(new Vector2Int(x, y));
        }

        return path;
    }

    /// <summary>
    /// Returns the four direct neighbors (up, down, left, right) of a given node.
    /// Does not include diagonal neighbors.
    /// </summary>
    /// <param name="gm">The GridManager for bounds and node retrieval.</param>
    /// <param name="node">The current GridNode to find neighbors for.</param>
    /// <returns>An enumerable of neighboring GridNode instances.</returns>
    private IEnumerable<GridNode> GetNeighbors(GridManager gm, GridNode node)
    {
        // Convert node's world position into grid indices.
        int x = Mathf.RoundToInt(node.worldPosition.x / gm.GridSettings.NodeSize);
        int y = Mathf.RoundToInt(node.worldPosition.z / gm.GridSettings.NodeSize);

        // Check each direction and yield the neighbor if it is within grid bounds.
        if (y + 1 < gm.GridSettings.GridSizeY) yield return gm.GetNode(x, y + 1);
        if (y - 1 >= 0) yield return gm.GetNode(x, y - 1);
        if (x + 1 < gm.GridSettings.GridSizeX) yield return gm.GetNode(x + 1, y);
        if (x - 1 >= 0) yield return gm.GetNode(x - 1, y);
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
                if (!checkNode.walkable || gm.IsNodeReserved(checkNode))
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
