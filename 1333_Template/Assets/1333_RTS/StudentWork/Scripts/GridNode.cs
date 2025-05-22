using UnityEngine;

namespace RTS_1333
{
	// Represents each node on our grid. Brutally efficient with struct usage and flags.
	/// <summary>
	/// Represents a single node within a grid system.
	/// Each node contains information about its position, walkability, weight, and a unique name.
	/// </summary>
	[System.Serializable]
	public struct GridNode
	{
		/// <summary>
		/// Represents the type of terrain associated with a grid node.
		/// </summary>
		/// <remarks>
		/// This variable defines the terrain type for a specific grid node, determining unique properties
		/// such as visual appearance or gameplay interactions tied to the terrain.
		/// The `TerrainType` class is a ScriptableObject, making it ideal for centralized, reusable configurations
		/// across multiple grid nodes or levels in the game.
		/// </remarks>
		public TerrainType TerrainType;

		/// <summary>
		/// Represents the name or identifier of a grid node.
		/// </summary>
		/// <remarks>
		/// This variable is used to assign or retrieve a unique name for each grid node.
		/// The naming convention typically associates the node's coordinates or unique index
		/// within the grid to ensure easy identification.
		/// </remarks>
		/// <example>
		/// Example usage includes naming grid cells based on their coordinates (e.g., "Cell_0_0")
		/// or assigning an identifier for debugging or visualization purposes.
		/// </example>
		public string Name;

		/// <summary>
		/// Represents the world position of a grid node in a 3D space.
		/// </summary>
		/// <remarks>
		/// This variable stores the exact position of the node in world coordinates.
		/// It allows for spatial representation within the game world, where each
		/// node maps to a specific point in 3D space. The position is typically
		/// derived from the grid's configuration and scaling.
		/// </remarks>
		/// <example>
		/// Useful for visually debugging grid nodes, spawning units at specific locations,
		/// and determining positions for gameplay interactions like pathfinding.
		/// </example>
		public Vector3 WorldPosition;

		/// <summary>
		/// Indicates whether a specific GridNode is walkable.
		/// </summary>
		/// <remarks>
		/// This variable holds the boolean value representing the accessibility of a GridNode.
		/// If set to true, the node is walkable, indicating that movement or pathfinding can occur through this node.
		/// If set to false, the node is non-walkable and treated as an obstacle or restricted area during pathfinding computations.
		/// </remarks>
		public bool Walkable;

		/// <summary>
		/// Represents the weight of a grid node.
		/// </summary>
		/// <remarks>
		/// This value is used to determine the cost associated with traversing the node.
		/// A higher weight generally indicates a higher traversal cost. This can be
		/// influenced by terrain type, obstacles, or other gameplay mechanics.
		/// </remarks>
		/// <example>
		/// While using pathfinding algorithms, nodes with higher weight will typically
		/// be less preferable to traverse compared to nodes with lower weight, depending
		/// on the implementation.
		/// </example>
		public int Weight;

		/// <summary>
		/// Specifies the color used to visually represent this grid node in the scene view.
		/// </summary>
		/// <remarks>
		/// This variable determines how the grid node appears when debugging or visualizing
		/// the grid system in the Unity editor. The color can be set to indicate different states,
		/// such as walkable, non-walkable, or highlighted for specific purposes.
		/// </remarks>
		/// <example>
		/// The GizmoColor might be used to visually differentiate accessible nodes (e.g., green)
		/// from blocked nodes (e.g., red) in a grid-based pathfinding system or game level design.
		/// </example>
		public Color GizmoColor;

		// Future-proof: Add faction-based walkability or additional node metadata here
	}
}