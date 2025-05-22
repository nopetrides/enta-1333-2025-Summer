using System.Collections.Generic;
using UnityEngine;

namespace RTS_1333
{
    /// <summary>
    /// Extremely readable, step-by-step A* pathfinder for teaching.
    /// </summary>
    public class AStarReadablePathfinder : PathfindingAlgorithm
    {
        /// <summary>
        /// Finds a path using the A* algorithm (readable version), considering unit size.
        /// </summary>
        public List<GridNode> FindPath(GridManager gridManager, GridNode start, GridNode end, int unitWidth, int unitHeight)
        {
            // Step 1: Prepare data structures.
            List<GridNode> openSet = new List<GridNode>(); // Nodes to explore
            Dictionary<GridNode, int> costSoFar = new Dictionary<GridNode, int>(); // Cost to reach each node
            Dictionary<GridNode, int> estimatedTotalCost = new Dictionary<GridNode, int>(); // f(n) = g(n) + h(n)
            Dictionary<GridNode, GridNode> cameFrom = new Dictionary<GridNode, GridNode>(); // Path reconstruction

            // Step 2: Initialize the algorithm.
            openSet.Add(start); // Start with the starting node.
            costSoFar[start] = 0; // The cost to reach the start node is 0.
            estimatedTotalCost[start] = Heuristic(start, end); // f(start) = h(start)
            cameFrom[start] = start; // The start node has no parent.

            // Step 3: Main loop - keep searching while there are nodes to explore.
            while (openSet.Count > 0)
            {
                // Find the node in openSet with the lowest estimated total cost (f-score).
                GridNode current = openSet[0];
                foreach (var node in openSet)
                {
                    if (estimatedTotalCost[node] < estimatedTotalCost[current])
                        current = node;
                }

                // If we've reached the end node, stop searching.
                if (current.Equals(end))
                    break;

                // Remove the current node from the open set.
                openSet.Remove(current);

                // Step 4: Explore neighbors (up, down, left, right).
                foreach (GridNode neighbor in GetNeighbors(gridManager, current))
                {
                    // Check if the area for this unit is walkable.
                    if (!IsAreaWalkable(gridManager, neighbor, unitWidth, unitHeight))
                        continue;

                    // The new cost to reach this neighbor.
                    int newCost = costSoFar[current] + neighbor.Weight;

                    // If we've never been here, or found a cheaper path, update.
                    if (!costSoFar.ContainsKey(neighbor) || newCost < costSoFar[neighbor])
                    {
                        costSoFar[neighbor] = newCost;
                        estimatedTotalCost[neighbor] = newCost + Heuristic(neighbor, end);
                        cameFrom[neighbor] = current;

                        // If not already in openSet, add it.
                        if (!openSet.Contains(neighbor))
                            openSet.Add(neighbor);
                    }
                }
            }

            // Step 5: Reconstruct the path from end to start.
            List<GridNode> path = new List<GridNode>();
            GridNode pathNode = end;

            // If we never reached the end, return an empty path.
            if (!cameFrom.ContainsKey(end))
                return path;

            // Walk backwards from the end node to the start node.
            while (!pathNode.Equals(start))
            {
                path.Add(pathNode);
                pathNode = cameFrom[pathNode];
            }
            path.Add(start); // Add the start node.

            // Reverse the path so it goes from start to end.
            path.Reverse();

            return path;
        }

        /// <summary>
        /// Finds a path using the A* algorithm (readable version, position overload).
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
        /// Override for base class compatibility (defaults to 1x1 unit). Requires gridManager.
        /// </summary>
        public List<GridNode> FindPath(GridManager gridManager, GridNode start, GridNode end)
        {
            return FindPath(gridManager, start, end, 1, 1);
        }

        /// <summary>
        /// Override for base class compatibility (defaults to 1x1 unit). Requires gridManager.
        /// </summary>
        public List<GridNode> FindPath(GridManager gridManager, Vector3 start, Vector3 end)
        {
            return FindPath(gridManager, start, end, 1, 1);
        }

        // The following two methods are required by the base class, but will throw if called directly.
        public override List<GridNode> FindPath(GridNode start, GridNode end)
        {
            throw new System.NotImplementedException("Use the overload that accepts GridManager.");
        }
        public override List<GridNode> FindPath(Vector3 start, Vector3 end)
        {
            throw new System.NotImplementedException("Use the overload that accepts GridManager.");
        }

        /// <summary>
        /// Gets the four direct neighbors (up, down, left, right) of a node.
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
        /// Checks if the area for a unit of given size is walkable at the neighbor's position.
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
        /// Finds the closest grid node to a given world position.
        /// </summary>
        private GridNode FindClosestNode(GridManager gridManager, Vector3 position)
        {
            int x = Mathf.RoundToInt(position.x / gridManager.GridSettings.NodeSize);
            int y = Mathf.RoundToInt(position.z / gridManager.GridSettings.NodeSize);
            return gridManager.GetNode(x, y);
        }

        /// <summary>
        /// Heuristic function for A* (Manhattan distance).
        /// </summary>
        private int Heuristic(GridNode a, GridNode b)
        {
            float dx = Mathf.Abs(a.WorldPosition.x - b.WorldPosition.x);
            float dz = Mathf.Abs(a.WorldPosition.z - b.WorldPosition.z);
            return Mathf.RoundToInt(dx + dz);
        }
    }
} 