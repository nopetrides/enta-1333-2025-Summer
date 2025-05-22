using System.Collections.Generic;
using UnityEngine;

namespace RTS_1333
{
	/// <summary>
	/// A condensed implementation of the A* pathfinding algorithm.
	/// Extends the abstract base class PathfindingAlgorithm.
	/// Facilitates pathfinding operations within a grid-based environment.
	/// </summary>
	public class AStarCondensedPathfinder : PathfindingAlgorithm
    {
		/// <summary>
		/// Calculates the shortest path between a start node and an end node using the A* algorithm.
		/// </summary>
		/// <param name="g">The grid manager containing the grid where the pathfinding happens.</param>
		/// <param name="s">The starting grid node for the pathfinding algorithm.</param>
		/// <param name="e">The target grid node where the pathfinding ends.</param>
		/// <param name="w">The maximum width limit for the valid traversal path.</param>
		/// <param name="h">The maximum height limit for the valid traversal path.</param>
		/// <returns>A list of GridNode objects representing the shortest path from start to end. Returns an empty list if no path is found.</returns>
		public List<GridNode> FindPath(GridManager g, GridNode s, GridNode e, int w, int h)
        {
            var o = new SortedSet<(int, GridNode)>(Comparer<(int, GridNode)>.Create((a, b) => a.Item1 == b.Item1 ? a.Item2.GetHashCode().CompareTo(b.Item2.GetHashCode()) : a.Item1.CompareTo(b.Item1)));
            var c = new Dictionary<GridNode, int>();
            var f = new Dictionary<GridNode, int>();
            var p = new Dictionary<GridNode, GridNode>();
            o.Add((0, s)); c[s] = 0; f[s] = H(s, e); p[s] = s;
            while (o.Count > 0)
            {
                var cur = o.Min.Item2; o.Remove(o.Min);
                if (cur.Equals(e)) break;
                foreach (var n in N(g, cur))
                {
                    if (!A(g, n, w, h)) continue;
                    var nc = c[cur] + n.Weight;
                    if (!c.ContainsKey(n) || nc < c[n])
                    {
                        c[n] = nc; f[n] = nc + H(n, e); p[n] = cur; o.Add((f[n], n));
                    }
                }
            }
            var path = new List<GridNode>();
            if (!p.ContainsKey(e)) return path;
            for (var x = e; !x.Equals(s); x = p[x]) path.Add(x);
            path.Add(s); path.Reverse();
            return path;
        }

		/// <summary>
		/// Finds and returns the optimal path from the start position to the end position based on the A* algorithm.
		/// </summary>
		/// <param name="g">The grid manager that provides access to the grid representation and configuration.</param>
		/// <param name="s">The start position in world space as a Vector3.</param>
		/// <param name="e">The end position in world space as a Vector3.</param>
		/// <param name="w">The movement weight or scaling factor along the width of the grid.</param>
		/// <param name="h">The movement weight or scaling factor along the height of the grid.</param>
		/// <returns>A list of <see cref="GridNode"/> objects representing the path from the start node to the end node.
		/// Returns an empty list if no valid path is found.</returns>
		public List<GridNode> FindPath(GridManager g, Vector3 s, Vector3 e, int w, int h)
        {
            var ns = g.GetNodeFromWorldPosition(s); var ne = g.GetNodeFromWorldPosition(e);
            return FindPath(g, ns, ne, w, h);
        }

		/// <summary>
		/// Finds a path between a start node and an end node on a grid.
		/// </summary>
		/// <param name="g">The GridManager instance that contains the grid structure and its nodes.</param>
		/// <param name="s">The starting GridNode for the path.</param>
		/// <param name="e">The ending GridNode for the path.</param>
		/// <return>
		/// A list of GridNode objects representing the calculated path from the start node to the end node.
		/// If no valid path is found, returns an empty list.
		/// </return>
		public List<GridNode> FindPath(GridManager g, GridNode s, GridNode e) => FindPath(g, s, e, 1, 1);

		/// <summary>
		/// Finds a path on a grid between a start point and an endpoint.
		/// </summary>
		/// <param name="g">The grid manager handling grid-related operations and structure.</param>
		/// <param name="s">The start position for the path, given in world coordinates.</param>
		/// <param name="e">The end position for the path, given in world coordinates.</param>
		/// <returns>A list of <see cref="GridNode"/> representing the calculated path from start to end, or null if no path is found.</returns>
		public List<GridNode> FindPath(GridManager g, Vector3 s, Vector3 e) => FindPath(g, s, e, 1, 1);

		/// Finds a path between two points on a grid using A* algorithm.
		/// <param name="g">The GridManager object containing the grid information.</param>
		/// <param name="s">The starting GridNode from which the path begins.</param>
		/// <param name="e">The target GridNode where the path should end.</param>
		/// <param name="w">The width parameter for pathfinding grid cells, typically signifies the scale or resolution adjustment.</param>
		/// <param name="h">The height parameter for pathfinding grid cells, typically signifies the scale or resolution adjustment.</param>
		/// <returns>A list of GridNode objects representing the computed path from the start to the end.</returns>
		public override List<GridNode> FindPath(GridNode s, GridNode e) { throw new System.NotImplementedException("Use the overload that accepts GridManager."); }

		/// <summary>
		/// Finds a path between a start and end node in a grid-based layout.
		/// Utilizes A* algorithm for efficient pathfinding.
		/// </summary>
		/// <param name="g">The grid manager handling the grid system.</param>
		/// <param name="s">The starting node of the path.</param>
		/// <param name="e">The destination node of the path.</param>
		/// <param name="w">The width scaling factor for grid adjustments.</param>
		/// <param name="h">The height scaling factor for grid adjustments.</param>
		/// <returns>Returns a list of GridNode objects representing the path from start to end.</returns>
		public override List<GridNode> FindPath(Vector3 s, Vector3 e) { throw new System.NotImplementedException("Use the overload that accepts GridManager."); }

		/// Retrieves a list of neighboring grid nodes for the given node.
		/// <param name="g">The GridManager managing the grid and its nodes.</param>
		/// <param name="n">The current node for which neighbors are being retrieved.</param>
		/// <returns>A list of neighboring GridNode objects surrounding the specified node.</returns>
		List<GridNode> N(GridManager g, GridNode n)
        {
            var l = new List<GridNode>();
            int gx = Mathf.RoundToInt(n.WorldPosition.x / g.GridSettings.NodeSize), gy = Mathf.RoundToInt(n.WorldPosition.z / g.GridSettings.NodeSize);
            if (gy + 1 < g.GridSettings.GridSizeY) l.Add(g.GetNode(gx, gy + 1));
            if (gy - 1 >= 0) l.Add(g.GetNode(gx, gy - 1));
            if (gx + 1 < g.GridSettings.GridSizeX) l.Add(g.GetNode(gx + 1, gy));
            if (gx - 1 >= 0) l.Add(g.GetNode(gx - 1, gy));
            return l;
        }

		/// Finds the shortest path between two nodes in a grid using the A* algorithm.
		/// This method has an implementation accepting a GridManager, start node,
		/// end node, and grid dimensions to define movement constraints.
		/// <param name="g">
		/// The GridManager instance providing access to the grid structure and settings.
		/// </param>
		/// <param name="s">
		/// The starting node for the pathfinding operation. Represents the path origin.
		/// </param>
		/// <param name="e">
		/// The target node for the pathfinding operation. Represents the destination.
		/// </param>
		/// <param name="w">
		/// The width of the agent in nodes, used to handle object size constraints during traversal.
		/// </param>
		/// <param name="h">
		/// The height of the agent in nodes, used to handle object size constraints during traversal.
		/// </param>
		/// <returns>
		/// A list of GridNode instances representing the shortest calculated path from the start to
		/// the end node. Returns an empty list if no valid path is found.
		/// </returns>
		bool A(GridManager g, GridNode n, int w, int h)
        {
            int x = Mathf.RoundToInt(n.WorldPosition.x / g.GridSettings.NodeSize), y = Mathf.RoundToInt(n.WorldPosition.z / g.GridSettings.NodeSize);
            for (int dx = 0; dx < w; dx++) for (int dy = 0; dy < h; dy++) if (x + dx < 0 || x + dx >= g.GridSettings.GridSizeX || y + dy < 0 || y + dy >= g.GridSettings.GridSizeY || !g.GetNode(x + dx, y + dy).Walkable) return false;
            return true;
        }

		/// Converts a given position in world space to its corresponding grid node.
		/// <param name="g">The GridManager instance that provides access to the grid and its nodes.</param>
		/// <param name="p">The Vector3 world position to be converted to a grid node.</param>
		/// <returns>Returns the GridNode at the world position mapped to the grid.</returns>
		GridNode F(GridManager g, Vector3 p)
        {
            int x = Mathf.RoundToInt(p.x / g.GridSettings.NodeSize), y = Mathf.RoundToInt(p.z / g.GridSettings.NodeSize);
            return g.GetNode(x, y);
        }

		/// <summary>
		/// Calculates a heuristic value (H) for two grid nodes to estimate the cost of the shortest path.
		/// Uses Manhattan distance as the heuristic.
		/// </summary>
		/// <param name="a">The starting grid node.</param>
		/// <param name="b">The target grid node.</param>
		/// <returns>The Manhattan distance between the two nodes rounded to the nearest integer.</returns>
		int H(GridNode a, GridNode b)
        {
            float dx = Mathf.Abs(a.WorldPosition.x - b.WorldPosition.x), dz = Mathf.Abs(a.WorldPosition.z - b.WorldPosition.z);
            return Mathf.RoundToInt(dx + dz);
        }
    }
} 