using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Optimized A* Pathfinder using a custom priority queue to ensure Unity compatibility.
/// Tracking collections (VisitedNodes, FrontierNodes) have been removed.
/// </summary>
public class AStarPathfinder
{
    private GridManager gridManager;

    public AStarPathfinder(GridManager gridManager)
    {
        this.gridManager = gridManager;
    }

    /// <summary>
    /// Entry point: Finds a path using grid coordinates and returns it as a list of Vector2Int.
    /// </summary>
    public List<Vector2Int> FindPath(Vector2Int startCoords, Vector2Int endCoords, int unitWidth = 1, int unitHeight = 1)
    {
        GridNode start = gridManager.GetNode(startCoords.x, startCoords.y);
        GridNode end = gridManager.GetNode(endCoords.x, endCoords.y);

        return FindPathWithNodes(start, end, unitWidth, unitHeight);
    }

    /// <summary>
    /// Runs the A* algorithm between two GridNodes, then converts the resulting node path
    /// into a List<Vector2Int> of grid coordinates.
    /// </summary>
    public List<Vector2Int> FindPathWithNodes(GridNode start, GridNode end, int unitWidth, int unitHeight)
    {
        SimplePriorityQueue<GridNode> openSet = new SimplePriorityQueue<GridNode>();
        Dictionary<GridNode, int> costSoFar = new Dictionary<GridNode, int>();
        Dictionary<GridNode, GridNode> cameFrom = new Dictionary<GridNode, GridNode>();

        openSet.Enqueue(start, 0);
        costSoFar[start] = 0;
        cameFrom[start] = start;

        while (openSet.Count > 0)
        {
            GridNode current = openSet.Dequeue();

            if (current.Equals(end))
                break;

            foreach (GridNode neighbor in GetNeighbors(gridManager, current))
            {
                if (!IsAreaWalkable(gridManager, neighbor, unitWidth, unitHeight))
                    continue;

                int newCost = costSoFar[current] + neighbor.weight;
                if (!costSoFar.ContainsKey(neighbor) || newCost < costSoFar[neighbor])
                {
                    costSoFar[neighbor] = newCost;
                    int priority = newCost + Heuristic(neighbor, end);
                    openSet.Enqueue(neighbor, priority);
                    cameFrom[neighbor] = current;
                }
            }
        }

        if (!cameFrom.ContainsKey(end))
            return new List<Vector2Int>();

        List<GridNode> nodePath = new List<GridNode>();
        GridNode pathNode = end;
        while (!pathNode.Equals(start))
        {
            nodePath.Add(pathNode);
            pathNode = cameFrom[pathNode];
        }
        nodePath.Add(start);
        nodePath.Reverse();

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

    private IEnumerable<GridNode> GetNeighbors(GridManager gm, GridNode node)
    {
        int x = Mathf.RoundToInt(node.worldPosition.x / gm.GridSettings.NodeSize);
        int y = Mathf.RoundToInt(node.worldPosition.z / gm.GridSettings.NodeSize);

        if (y + 1 < gm.GridSettings.GridSizeY) yield return gm.GetNode(x, y + 1);
        if (y - 1 >= 0) yield return gm.GetNode(x, y - 1);
        if (x + 1 < gm.GridSettings.GridSizeX) yield return gm.GetNode(x + 1, y);
        if (x - 1 >= 0) yield return gm.GetNode(x - 1, y);
    }

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

                if (!gm.GetNode(nx, ny).walkable)
                    return false;
            }
        }

        return true;
    }

    private int Heuristic(GridNode a, GridNode b)
    {
        float dx = Mathf.Abs(a.worldPosition.x - b.worldPosition.x);
        float dz = Mathf.Abs(a.worldPosition.z - b.worldPosition.z);
        return Mathf.RoundToInt(dx + dz);
    }
}
