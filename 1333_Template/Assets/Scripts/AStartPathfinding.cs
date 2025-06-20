using System.Collections.Generic;
using UnityEngine;

public class AStartPathfinding : PathfindingClass
{
    public List<GridNode> FindPath(GridManager gridManager, GridNode start, GridNode end, int unitWidth, int unitHeight)
    {
        // Open set: nodes to be evaluated
        List<GridNode> openSet = new List<GridNode>();

        // Cost from start to a node
        Dictionary<GridNode, int> gCost = new Dictionary<GridNode, int>();

        // Heuristic cost from a node to the end
        Dictionary<GridNode, int> hCost = new Dictionary<GridNode, int>();

        // Total cost (gCost + hCost)
        Dictionary<GridNode, int> fCost = new Dictionary<GridNode, int>();

        // For tracking the path
        Dictionary<GridNode, GridNode> cameFrom = new Dictionary<GridNode, GridNode>();

        // Initialize start node
        openSet.Add(start);
        gCost[start] = 0;
        hCost[start] = Heuristic(start, end);
        fCost[start] = gCost[start] + hCost[start];
        cameFrom[start] = null;

        // Main A* loop
        while (openSet.Count > 0)
        {
            // Get node in openSet with the lowest fCost
            GridNode current = openSet[0];
            foreach (var node in openSet)
            {
                if (fCost.ContainsKey(node) && fCost[node] < fCost[current])
                    current = node;
            }

            // If end node is reached, reconstruct and return the path
            if (current == end)
                return ReconstructPath(cameFrom, end);

            // Remove current from openSet
            openSet.Remove(current);

            // Evaluate neighbors
            foreach (GridNode neighbor in GetNeighbors(gridManager, current))
            {
                // Skip non-walkable neighbors
                if (!neighbor.walkable) continue;

                // Calculate tentative gCost
                int tentativeG = gCost[current] + neighbor.MovementCost;

                // If this path to neighbor is better or neighbor is not evaluated yet
                if (!gCost.ContainsKey(neighbor) || tentativeG < gCost[neighbor])
                {
                    // Update costs and path
                    gCost[neighbor] = tentativeG;
                    hCost[neighbor] = Heuristic(neighbor, end);
                    fCost[neighbor] = gCost[neighbor] + hCost[neighbor];
                    cameFrom[neighbor] = current;

                    // Add to open set if not already there
                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        // No path found
        return new List<GridNode>();
    }

    // path from end node to start node
    private List<GridNode> ReconstructPath(Dictionary<GridNode, GridNode> cameFrom, GridNode endNode)
    {
        List<GridNode> path = new();
        GridNode current = endNode;

        // Trace back from end to start using the cameFrom dictionary
        while (current != null)
        {
            path.Add(current);
            current = cameFrom.ContainsKey(current) ? cameFrom[current] : null;
        }

        path.Reverse(); // Reverse to get path from start to end
        return path;
    }

    // Retrieves neighbors of a node from the grid manager
    private List<GridNode> GetNeighbors(GridManager gridManager, GridNode node)
    {
        return gridManager.GetNeighborNodes(node);
    }

    // Estimates cost from node a to node b (Manhattan distance on XZ plane)
    private int Heuristic(GridNode a, GridNode b)
    {
        float dx = Mathf.Abs(a.WorldPosition.x - b.WorldPosition.x);
        float dz = Mathf.Abs(a.WorldPosition.z - b.WorldPosition.z); // Assuming movement on XZ plane
        return Mathf.RoundToInt(dx + dz);

    }

}
