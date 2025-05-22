using System.Collections.Generic;
using UnityEngine;

namespace RTS_1333
{
	/// <summary>
	/// Central pathfinding manager that delegates to selected algorithm.
	/// </summary>
	public class Pathfinder : MonoBehaviour
	{
		/// <summary>
		/// Enum for available pathfinding algorithms.
		/// </summary>
		public enum PathFindingStyle
		{
			/// <summary>
			/// Dijkstra's is used when all edge costs must be considered and no heuristic is available.
			/// </summary>
			Dijkstra,
			/// <summary>
			/// A* is the industry standard for real-time games (including RTS), due to its efficiency and ability to use heuristics for faster solutions.
			/// </summary>
			AStar,
			// The following are not used in this project, but are included for educational purposes.
			#region Unused
			/// <summary>
			/// BFS is only optimal for unweighted graphs (all costs equal), which is less realistic for RTS games with terrain.
			/// </summary>
			BreadthFirstSearch,
			/// <summary>
			/// DFS is not optimal and rarely used for pathfinding in games.
			/// </summary>
			DepthFirstSearch,
			/// <summary>
			/// Greedy Best-First Search: Fast but not guaranteed to find the shortest path; good for teaching heuristics but less practical for guaranteed results.
			/// </summary>
			GreedyBestFirstSearch,
			/// <summary>
			/// Iterative Deepening A*: More advanced, used for memory-constrained environments (e.g., robotics, not typical RTS).
			/// </summary>
			IterativeDeepAStar,
			/// <summary>
			/// Jump Point Search: An optimization for A* on uniform-cost grids (which most game grids are not, unless they are designed as such); more complex, best introduced after students master A*.
			/// </summary>
			JumpPointSearch,
			/// <summary>
			/// Recursive Best-First Search: Used for memory efficiency, but harder to understand and less common in grid-based games.
			/// </summary>
			RecursiveBestFirstSearch,
			/// <summary>
			/// Rapidly-Exploring Random Tree (RRT): Used for continuous spaces (robotics, motion planning), not grid-based RTS.
			/// </summary>
			RapidlyExploringRandomTree,
			/// <summary>
			/// Uniform Cost Search: Equivalent to Dijkstra's for grid-based graphs.
			/// </summary>
			UniformCostSearch
			#endregion
		}

		/// <summary>
		/// Enum for algorithm variant (readable or condensed).
		/// </summary>
		public enum AlgorithmVariant
		{
			Readable,
			Condensed
		}

		[Header("Algorithm Selection")]
		[SerializeField] private PathFindingStyle pathfindingToUse = PathFindingStyle.Dijkstra;
		[SerializeField] private AlgorithmVariant algorithmVariant = AlgorithmVariant.Readable;

		[Header("Grid Reference")]
		[SerializeField] private GridManager gridManager;

		[Header("Visualization")]
		[SerializeField] private bool drawLastPathGizmos = true; // Toggle for drawing path gizmos.
		[SerializeField] private Color pathGizmoColor = Color.cyan; // Color for path gizmos.
		
		private readonly DijkstraReadablePathfinder _dijkstraReadable = new();
		private readonly DijkstraCondensedPathfinder _dijkstraCondensed = new();
		private readonly AStarReadablePathfinder _aStarReadable = new();
		private readonly AStarCondensedPathfinder _aStarCondensed = new();
		
		// Stores the last calculated path for visualization.
		private List<GridNode> _lastPath = new();

		/// <summary>
		/// Finds a path between two nodes, considering unit size.
		/// Stores the path for visualization if enabled.
		/// </summary>
		public List<GridNode> FindPath(GridNode start, GridNode end, int unitWidth = 1, int unitHeight = 1)
		{
			// Find the path using the selected algorithm.
			List<GridNode> path;
			switch (pathfindingToUse)
			{
				case PathFindingStyle.Dijkstra:
					if (algorithmVariant == AlgorithmVariant.Readable)
						path = _dijkstraReadable.FindPath(gridManager, start, end, unitWidth, unitHeight);
					else
						path = _dijkstraCondensed.FindPath(gridManager, start, end, unitWidth, unitHeight);
					break;
				case PathFindingStyle.AStar:
					if (algorithmVariant == AlgorithmVariant.Readable)
						path = _aStarReadable.FindPath(gridManager, start, end, unitWidth, unitHeight);
					else
						path = _aStarCondensed.FindPath(gridManager, start, end, unitWidth, unitHeight);
					break;
				default:
					path = new List<GridNode>();
					break;
			}
			// Store the path for gizmo drawing if enabled.
			if (drawLastPathGizmos)
				_lastPath = path;
			return path;
		}

		/// <summary>
		/// Finds a path between two world positions, considering unit size.
		/// Stores the path for visualization if enabled.
		/// </summary>
		public List<GridNode> FindPath(Vector3 start, Vector3 end, int unitWidth = 1, int unitHeight = 1)
		{
			// Convert start position to closest grid node.
			GridNode startNode = gridManager.GetNodeFromWorldPosition(start);
			// Convert end position to closest grid node.
			GridNode endNode = gridManager.GetNodeFromWorldPosition(end);
			// Use node-based overload for pathfinding.
			return FindPath(startNode, endNode, unitWidth, unitHeight);
		}

		/// <summary>
		/// Draws the last calculated path using Gizmos for visualization.
		/// </summary>
		private void OnDrawGizmos()
		{
			// Only draw if enabled and path exists.
			if (!drawLastPathGizmos || _lastPath == null || _lastPath.Count < 2)
				return;
			// Set gizmo color.
			Gizmos.color = pathGizmoColor;
			// Draw lines between each node in the path.
			for (int i = 0; i < _lastPath.Count - 1; i++)
			{
				Gizmos.DrawLine(_lastPath[i].WorldPosition, _lastPath[i + 1].WorldPosition);
			}
		}
	}
}