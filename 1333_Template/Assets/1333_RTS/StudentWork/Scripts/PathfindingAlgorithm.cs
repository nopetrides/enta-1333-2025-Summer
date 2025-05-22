using System.Collections.Generic;
using UnityEngine;

namespace RTS_1333
{
    /// <summary>
    /// Abstract base class for all pathfinding algorithms.
    /// </summary>
    public abstract class PathfindingAlgorithm
    {
        /// <summary>
        /// Finds a path from the start node to the end node.
        /// </summary>
        /// <param name="start">The starting node.</param>
        /// <param name="end">The ending node.</param>
        /// <returns>A list of nodes representing the path from start to end.</returns>
        public abstract List<GridNode> FindPath(GridNode start, GridNode end);

        /// <summary>
        /// Finds a path from the start position to the end position.
        /// </summary>
        /// <param name="start">The starting world position.</param>
        /// <param name="end">The ending world position.</param>
        /// <returns>A list of nodes representing the path from start to end.</returns>
        public abstract List<GridNode> FindPath(Vector3 start, Vector3 end);
    }
} 