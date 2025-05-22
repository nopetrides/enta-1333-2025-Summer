using System.Collections.Generic;
using UnityEngine;

namespace RTS_1333
{
	/// <summary>
	/// Provides pathfinding implementation using Dijkstra's algorithm for a grid-based system.
	/// </summary>
	public class DijkstraCondensedPathfinder : PathfindingAlgorithm
    {
		/// <summary>
		/// Finds the shortest path between two nodes in a grid using Dijkstra's algorithm.
		/// </summary>
		/// <param name="g">The GridManager instance representing the grid structure.</param>
		/// <param name="s">The starting GridNode where the path begins.</param>
		/// <param name="e">The ending GridNode where the path ends.</param>
		/// <param name="w">The width of the considered traversal area in nodes.</param>
		/// <param name="h">The height of the considered traversal area in nodes.</param>
		/// <returns>A list of GridNode objects representing the path from the start node to the end node.
		/// Returns an empty list if no path exists.</returns>
		public List<GridNode> FindPath(GridManager g, GridNode s, GridNode e, int w, int h)
        {
            var o = new SortedSet<(int, GridNode)>(Comparer<(int, GridNode)>.Create((a, b) => a.Item1 == b.Item1 ? a.Item2.GetHashCode().CompareTo(b.Item2.GetHashCode()) : a.Item1.CompareTo(b.Item1)));
            var c = new Dictionary<GridNode, int>();
            var p = new Dictionary<GridNode, GridNode>();
            o.Add((0, s)); c[s] = 0; p[s] = s;
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
                        c[n] = nc; p[n] = cur; o.Add((nc, n));
                    }
                }
            }
            var path = new List<GridNode>();
            if (!p.ContainsKey(e)) return path;
            for (var x = e; !x.Equals(s); x = p[x]) path.Add(x);
            path.Add(s); path.Reverse();
            return path;
        }

		/// Finds the shortest path between a start and end position on a grid.
		/// This method can handle both `Vector3` positions and `GridNode` positions as input with overloads.
		/// Uses Dijkstra's algorithm for pathfinding logic.
		/// <param name="g">The grid manager instance that governs the grid and its nodes.</param>
		/// <param name="s">The starting position represented either as a `Vector3` or `GridNode`.</param>
		/// <param name="e">The ending position represented either as a `Vector3` or `GridNode`.</param>
		/// <param name="w">Grid cell width multiplier in grid coordinates, affects resolution of the path. Defaults to 1.</param>
		/// <param name="h">Grid cell height multiplier in grid coordinates, affects resolution of the path. Defaults to 1.</param>
		/// <return>A list of `GridNode` objects representing the shortest path from start to end.</return>
		public List<GridNode> FindPath(GridManager g, Vector3 s, Vector3 e, int w, int h)
        {
            var ns = g.GetNodeFromWorldPosition(s); var ne = g.GetNodeFromWorldPosition(e);
            return FindPath(g, ns, ne, w, h);
        }

		/// <summary>
		/// Finds the shortest path between a start node and an end node on a grid.
		/// </summary>
		/// <param name="g">The GridManager instance managing the grid structure and nodes.</param>
		/// <param name="s">The start node for the pathfinding computation.</param>
		/// <param name="e">The end node where the pathfinding terminates.</param>
		/// <param name="w">The width multiplier for refining grid resolution during pathfinding.</param>
		/// <param name="h">The height multiplier for refining grid resolution during pathfinding.</param>
		/// <returns>A list of GridNode objects forming the path from start to end, or an empty list if no path is found.</returns>
		public List<GridNode> FindPath(GridManager g, GridNode s, GridNode e) => FindPath(g, s, e, 1, 1);

		/// <summary>
		/// Finds the path between a start and an end point in a grid-based system using the Dijkstra algorithm.
		/// </summary>
		/// <param name="g">The grid manager responsible for handling the grid structure and grid nodes.</param>
		/// <param name="s">The start point on the grid, represented as a Vector3 world position.</param>
		/// <param name="e">The end point on the grid, represented as a Vector3 world position.</param>
		/// <param name="w">Width of the grid cell or segment.</param>
		/// <param name="h">Height of the grid cell or segment.</param>
		/// <returns>A list of grid nodes representing the shortest path between the start and end points.</returns>
		public List<GridNode> FindPath(GridManager g, Vector3 s, Vector3 e) => FindPath(g, s, e, 1, 1);

		/// <summary>
		/// Finds the shortest path between a start node and an end node using Dijkstra's algorithm.
		/// </summary>
		/// <param name="g">The grid manager responsible for managing the grid structure and nodes.</param>
		/// <param name="s">The starting grid node where the pathfinding begins.</param>
		/// <param name="e">The target grid node where the pathfinding ends.</param>
		/// <param name="w">Weight for horizontal nodes. Used to adjust movement cost.</param>
		/// <param name="h">Weight for vertical nodes. Used to adjust movement cost.</param>
		/// <returns>A list of grid nodes representing the shortest path from the start node to the end node. Returns an empty list if no path is found.</returns>
		public override List<GridNode> FindPath(GridNode s, GridNode e) { throw new System.NotImplementedException("Use the overload that accepts GridManager."); }

		/// Finds an optimal path between a start and end point using the Dijkstra algorithm.
		/// <param name="g">The GridManager instance that manages the grid's nodes and their states.</param>
		/// <param name="s">The starting GridNode from which the pathfinding begins.</param>
		/// <param name="e">The ending GridNode where the pathfinding terminates.</param>
		/// <param name="w">The width multiplier for node traversal. Defines how traversal cost scales horizontally.</param>
		/// <param name="h">The height multiplier for node traversal. Defines how traversal cost scales vertically.</param>
		/// <returns>Returns a List of GridNodes that represents the path from the start node to the end node.
		/// If no valid path exists, returns an empty list.</returns>
		public override List<GridNode> FindPath(Vector3 s, Vector3 e) { throw new System.NotImplementedException("Use the overload that accepts GridManager."); }

		/// <summary>
		/// Generates a list of neighboring nodes around a given node in the grid.
		/// </summary>
		/// <param name="g">The GridManager instance that contains grid configuration and node data.</param>
		/// <param name="n">The current node for which neighbors are being retrieved.</param>
		/// <returns>A list of neighboring GridNodes relative to the provided node.</returns>
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

		/// <summary>
		/// Checks if a node and its adjacent nodes (based on width and height) are within the grid boundaries
		/// and are walkable.
		/// </summary>
		/// <param name="g">The grid manager responsible for managing the grid and its nodes.</param>
		/// <param name="n">The grid node being evaluated.</param>
		/// <param name="w">The width of the area to check from the node in grid-space units.</param>
		/// <param name="h">The height of the area to check from the node in grid-space units.</param>
		/// <returns>True if the node and its relevant area are walkable and within bounds, otherwise false.</returns>
		bool A(GridManager g, GridNode n, int w, int h)
        {
            int x = Mathf.RoundToInt(n.WorldPosition.x / g.GridSettings.NodeSize), y = Mathf.RoundToInt(n.WorldPosition.z / g.GridSettings.NodeSize);
            for (int dx = 0; dx < w; dx++) for (int dy = 0; dy < h; dy++) if (x + dx < 0 || x + dx >= g.GridSettings.GridSizeX || y + dy < 0 || y + dy >= g.GridSettings.GridSizeY || !g.GetNode(x + dx, y + dy).Walkable) return false;
            return true;
        }

		/// <summary>
		/// Maps a position in world space to the corresponding node on the grid.
		/// </summary>
		/// <param name="g">The GridManager that manages the grid structure.</param>
		/// <param name="p">The world position to map to a grid node.</param>
		/// <returns>The GridNode at the specified position on the grid.</returns>
		GridNode F(GridManager g, Vector3 p)
        {
            int x = Mathf.RoundToInt(p.x / g.GridSettings.NodeSize), y = Mathf.RoundToInt(p.z / g.GridSettings.NodeSize);
            return g.GetNode(x, y);
        }
    }
}