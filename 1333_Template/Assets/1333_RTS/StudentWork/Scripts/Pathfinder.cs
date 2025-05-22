using System.Collections.Generic;
using UnityEngine;
using Vector3 = System.Numerics.Vector3;


public class Pathfinder : MonoBehaviour
{
	/// <summary>
	/// Possible pathfinding algorithms
	/// </summary>
	public enum PathFindingStyle
	{
		BreadthFirstSearch,
		DepthFirstSearch,
		AStar,
		Dijkstra,
		GreedyBestFirstSearch,
		IterativeDeepAStar,
		JumpPointSearch,
		RecursiveBestFirstSearch,
		RapidlyExploringRandomTree,
		UniformCostSearch
	}

	[SerializeField] private PathFindingStyle PathfindingToUse;

	public List<GridNode> FindPath(GridNode start, GridNode end)
	{
		new List<GridNode>();
		switch (PathfindingToUse)
		{
			//todo
		}

		return new List<GridNode>();
	}

	public List<GridNode> FindPath(Vector3 start, Vector3 end)
	{
		new List<GridNode>();
		switch (PathfindingToUse)
		{
			//todo
		}

		return new List<GridNode>();
	}
}
