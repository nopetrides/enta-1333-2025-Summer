using System.Collections.Generic;
using UnityEngine;

namespace RTS_1333
{
    /// <summary>
    /// Readable, step-by-step Dijkstra pathfinder for teaching.
    /// </summary>
    public class DijkstraReadablePathfinder : PathfindingAlgorithm
    {
        /// <summary>
        /// Finds a path using Dijkstra's algorithm, considering unit size.
        /// </summary>
        public List<GridNode> FindPath(GridManager gridManager, GridNode start, GridNode end, int unitWidth, int unitHeight)
        {
            // Open set of nodes to explore.
            List<GridNode> openSet = new List<GridNode>();
            // Cost to reach each node.
            Dictionary<GridNode, int> costSoFar = new Dictionary<GridNode, int>();
            // Path reconstruction.
            Dictionary<GridNode, GridNode> cameFrom = new Dictionary<GridNode, GridNode>();

            // Add start node to open set.
            openSet.Add(start);
            // Cost to reach start is 0.
            costSoFar[start] = 0;
            // Start node has no parent.
            cameFrom[start] = start;

            // Main loop.
            while (openSet.Count > 0)
            {
                // Find node with lowest cost.
                GridNode current = openSet[0];
                foreach (var node in openSet)
                {
                    if (costSoFar[node] < costSoFar[current])
                        current = node;
                }

                // Stop if end is reached.
                if (current.Equals(end))
                    break;

                // Remove current from open set.
                openSet.Remove(current);

                // Check neighbors.
                foreach (GridNode neighbor in GetNeighbors(gridManager, current))
                {
                    // Skip if area is not walkable.
                    if (!IsAreaWalkable(gridManager, neighbor, unitWidth, unitHeight))
                        continue;

                    // Calculate new cost.
                    int newCost = costSoFar[current] + neighbor.Weight;

                    // Update if new path is cheaper or not visited.
                    if (!costSoFar.ContainsKey(neighbor) || newCost < costSoFar[neighbor])
                    {
                        costSoFar[neighbor] = newCost;
                        cameFrom[neighbor] = current;
                        if (!openSet.Contains(neighbor))
                            openSet.Add(neighbor);
                    }
                }
            }

            // Reconstruct path.
            List<GridNode> path = new List<GridNode>();
            GridNode pathNode = end;
            if (!cameFrom.ContainsKey(end))
                return path;
            while (!pathNode.Equals(start))
            {
                path.Add(pathNode);
                pathNode = cameFrom[pathNode];
            }
            path.Add(start);
            path.Reverse();
            return path;
        }

        /// <summary>
        /// Finds a path using Dijkstra's algorithm, position overload.
        /// </summary>
        public List<GridNode> FindPath(GridManager gridManager, Vector3 start, Vector3 end, int unitWidth, int unitHeight)
        {
            // Convert start position to closest grid node.
            GridNode startNode = gridManager.GetNodeFromWorldPosition(start);
            // Convert end position to closest grid node.
            GridNode endNode = gridManager.GetNodeFromWorldPosition(end);
            // Find path using node-based overload.
            return FindPath(gridManager, startNode, endNode, unitWidth, unitHeight);
        }

        /// <summary>
        /// Base class compatibility (defaults to 1x1 unit).
        /// </summary>
        public List<GridNode> FindPath(GridManager gridManager, GridNode start, GridNode end)
        {
            return FindPath(gridManager, start, end, 1, 1);
        }

        /// <summary>
        /// Base class compatibility (defaults to 1x1 unit).
        /// </summary>
        public List<GridNode> FindPath(GridManager gridManager, Vector3 start, Vector3 end)
        {
            return FindPath(gridManager, start, end, 1, 1);
        }

        // Throw if base class method is called directly.
        public override List<GridNode> FindPath(GridNode start, GridNode end)
        {
            throw new System.NotImplementedException("Use the overload that accepts GridManager.");
        }
        public override List<GridNode> FindPath(Vector3 start, Vector3 end)
        {
            throw new System.NotImplementedException("Use the overload that accepts GridManager.");
        }

        /// <summary>
        /// Gets four direct neighbors of a node.
        /// </summary>
        private List<GridNode> GetNeighbors(GridManager gridManager, GridNode node)
        {
            List<GridNode> neighbors = new List<GridNode>();
            int gridSizeX = gridManager.GridSettings.GridSizeX;
            int gridSizeY = gridManager.GridSettings.GridSizeY;
            float nodeSize = gridManager.GridSettings.NodeSize;
            int nodeX = Mathf.RoundToInt(node.WorldPosition.x / nodeSize);
            int nodeY = Mathf.RoundToInt(node.WorldPosition.z / nodeSize);
            if (nodeY + 1 < gridSizeY) neighbors.Add(gridManager.GetNode(nodeX, nodeY + 1));
            if (nodeY - 1 >= 0) neighbors.Add(gridManager.GetNode(nodeX, nodeY - 1));
            if (nodeX + 1 < gridSizeX) neighbors.Add(gridManager.GetNode(nodeX + 1, nodeY));
            if (nodeX - 1 >= 0) neighbors.Add(gridManager.GetNode(nodeX - 1, nodeY));
            return neighbors;
        }

        /// <summary>
        /// Checks if area for unit size is walkable.
        /// </summary>
        private bool IsAreaWalkable(GridManager grid, GridNode node, int width, int height)
        {
            float nodeSize = grid.GridSettings.NodeSize;
            int x = Mathf.RoundToInt(node.WorldPosition.x / nodeSize);
            int y = Mathf.RoundToInt(node.WorldPosition.z / nodeSize);
            for (int dx = 0; dx < width; dx++)
            {
                for (int dy = 0; dy < height; dy++)
                {
                    if (x + dx < 0 || x + dx >= grid.GridSettings.GridSizeX || y + dy < 0 || y + dy >= grid.GridSettings.GridSizeY)
                        return false;
                    if (!grid.GetNode(x + dx, y + dy).Walkable)
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Finds closest grid node to a position.
        /// </summary>
        private GridNode FindClosestNode(GridManager gridManager, Vector3 position)
        {
            int x = Mathf.RoundToInt(position.x / gridManager.GridSettings.NodeSize);
            int y = Mathf.RoundToInt(position.z / gridManager.GridSettings.NodeSize);
            return gridManager.GetNode(x, y);
        }
    }
} 