// Import required namespaces
using System.Collections.Generic; // Provides classes for generic collection data structures like List.
using UnityEngine; // Provides access to Unity's core classes and functionality.

/// <summary>
/// The GridManager class is responsible for managing a grid system for the game.
/// It creates nodes, allows retrieval and modification of node properties, and manages terrain generation.
/// </summary>
public class GridManager : MonoBehaviour
{
	/// <summary>
	/// Stores the configuration settings for the grid system, such as its dimensions, node size,
	/// and orientation. This determines how the grid is initialized and functions in the game.
	/// </summary>
	[SerializeField]
	private GridSettings _gridSettings; // Holds grid parameters like dimensions, node size, and orientation.

	/// <summary>
	/// Provides configuration settings for the grid system used in the game.
	/// </summary>
	/// <remarks>
	/// Contains customizable parameters like grid dimensions, node size, orientation,
	/// default terrain type, and supported terrain types. This data drives the
	/// grid's structure and behavior.
	/// </remarks>
	public GridSettings GridSettings => _gridSettings;

	/// <summary>
	/// A 2D private array used to store and organize grid nodes, where each element represents a node
	/// at a specific position defined by its row and column indices.
	/// </summary>
	/// <remarks>
	/// Serves as the underlying data structure in the grid system created by the GridManager.
	/// Each node in the array encapsulates information such as position, walkability, weight, and terrain type.
	/// </remarks>
	private GridNode[,] _gridNodes;

	/// <summary>
	/// A list containing references to all GridNode objects in the grid.
	/// This list is primarily used for debugging purposes in the Unity editor.
	/// </summary>
	/// <remarks>
	/// The AllNodes list is populated during the grid initialization process and includes
	/// every GridNode from the entire grid. It allows for easier visualization
	/// and manipulation of grid nodes during development and debugging.
	/// </remarks>
	[Header("Debug for editor playmode only")] // Adds a header in the Unity inspector.
	[SerializeField]
	private List<GridNode> AllNodes = new(); // Contains all grid nodes in a single list for easier debugging.

	/// <summary>
	/// Indicates whether the grid system has been successfully initialized.
	/// </summary>
	/// <remarks>
	/// This property is set to true after the grid has been fully created and all nodes have been
	/// initialized. It provides a quick way to check if the grid is ready for use, ensuring that
	/// operations relying on the grid, such as spawning units, only execute when the grid is
	/// prepared.
	/// </remarks>
	public bool IsInitialized { get; private set; } // Indicates if the grid data is ready to use.

	/// <summary>
	/// Initializes the grid and populates it with GridNode objects using configured grid settings.
	/// </summary>
	/// <remarks>
	/// This method creates a two-dimensional array of GridNode objects based on the grid size
	/// defined in GridSettings. It ensures that each cell in the grid is instantiated
	/// and ready to use. Once the initialization is complete, the grid is marked as initialized.
	/// </remarks>
	/// <example>
	/// Proper implementation of this method requires valid `GridSettings` to be assigned.
	/// Each node in the grid will correspond to a location determined by the grid dimensions.
	/// </example>
	/// <exception cref="System.NullReferenceException">
	/// Thrown if required references, such as GridSettings, have not been set.
	/// Ensure that all dependencies are assigned via the Unity inspector or code before invoking.
	/// </exception>
	public void InitializeGrid()
	{
		// Create a 2D array for the grid using dimensions defined in GridSettings.
		_gridNodes = new GridNode[_gridSettings.GridSizeX, _gridSettings.GridSizeY];

		// Loop through each grid cell in X and Y directions.
		for (int x = 0; x < _gridSettings.GridSizeX; x++) // Iterate over X-axis.
		{
			for (int y = 0; y < _gridSettings.GridSizeY; y++) // Iterate over Y-axis.
			{
				// Calculate the world position of the current node based on grid orientation.
				Vector3 worldPos = _gridSettings.UseXZPlane 
					? new Vector3(x, 0, y) * _gridSettings.NodeSize // Position for XZ plane configuration.
					: new Vector3(x, y, 0) * _gridSettings.NodeSize; // Position for XY plane configuration.
				
				// Create a new grid node and set its properties.
				GridNode node = new GridNode
				{
					Name = $"Cell_{(x + _gridSettings.GridSizeX * x + y)}", // Create a unique name for the node.
					WorldPosition = worldPos, // Assign calculated world position.
					Walkable = _gridSettings.DefaultTerrainType.Walkable, // Default value: node is walkable.
					Weight = _gridSettings.DefaultTerrainType.MovementCost, // Default value: node has neutral weight.
					GizmoColor = _gridSettings.DefaultTerrainType.GizmoColor, // Default value: node has default color.
					TerrainType = _gridSettings.DefaultTerrainType // Default value: node has default terrain type.
				};

				// Store the node in the 2D grid array at the correct position.
				_gridNodes[x, y] = node;
			}
		}

		// Mark the grid as initialized upon successful completion.
		IsInitialized = true;
	}

	/// <summary>
	/// Updates the debug list in the Unity inspector to include all nodes in the grid.
	/// This is used for debugging and visualization during play mode.
	/// </summary>
	private void PopulateDebugList()
	{
		// Clear the existing list to avoid duplication of nodes.
		AllNodes.Clear();

		// Loop through each grid cell to populate the list.
		for (int x = 0; x < _gridSettings.GridSizeX; x++)
		{
			for (int y = 0; y < _gridSettings.GridSizeY; y++)
			{
				// Retrieve the node from the grid array and add it to the list.
				GridNode node = _gridNodes[x, y];
				AllNodes.Add(new GridNode
				{
					Name = $"Cell_{x}_{y}", // Name the node based on its position.
					WorldPosition = node.WorldPosition, // Copy world position.
					Walkable = node.Walkable, // Copy walkability status.
					Weight = node.Weight, // Copy weight value.
					GizmoColor = node.GizmoColor, // Copy Gizmo color.
					TerrainType = node.TerrainType // Copy terrain type.
				});
			}
		}
	}

	/// <summary>
	/// Retrieves the GridNode object at the specified grid coordinates.
	/// </summary>
	/// <param name="x">The x-coordinate of the node to retrieve. Represents the column index in the grid.</param>
	/// <param name="y">The y-coordinate of the node to retrieve. Represents the row index in the grid.</param>
	/// <returns>The GridNode object located at the specified coordinates within the grid.</returns>
	/// <exception cref="IndexOutOfRangeException">Thrown if the specified coordinates are outside the bounds of the grid.</exception>
	public GridNode GetNode(int x, int y)
	{
		// Ensure the requested coordinates are within grid bounds.
		if (x < 0 || x >= _gridSettings.GridSizeX || y < 0 || y >= _gridSettings.GridSizeY)
			throw new System.IndexOutOfRangeException("Grid node indices out of range."); // Throw an error if out of bounds.

		// Return the requested node.
		return _gridNodes[x, y];
	}

	/// <summary>
	/// Updates the walkable state for a specific node within the grid.
	/// </summary>
	/// <param name="x">The x-coordinate of the grid node to modify.</param>
	/// <param name="y">The y-coordinate of the grid node to modify.</param>
	/// <param name="walkable">A boolean indicating whether the node should be walkable (true) or non-walkable (false).</param>
	public void SetWalkable(int x, int y, bool walkable)
	{
		// Update the walkability state of the node at the given position.
		_gridNodes[x, y].Walkable = walkable;
	}

	/// <summary>
	/// Randomizes the terrain type for all nodes within the grid.
	/// </summary>
	/// <remarks>
	/// This method iterates through each node in the grid and assigns it a random terrain type
	/// chosen from the list of TerrainTypes in the GridSettings.
	/// </remarks>
	public void RandomizeTerrain()
	{
		int gridSizeX = GridSettings.GridSizeX;
		int gridSizeY = GridSettings.GridSizeY;

		for (int x = 0; x < gridSizeX; x++)
		{
			for (int y = 0; y < gridSizeY; y++)
			{
				TerrainType randomTerrain = GridSettings.TerrainTypes[Random.Range(0, GridSettings.TerrainTypes.Length)];
				SetTerrainType(x, y, randomTerrain);
			}
		}
	}

	/// <summary>
	/// Updates the terrain type for a specific node in the grid, along with its associated properties.
	/// </summary>
	/// <param name="x">
	/// The x-coordinate of the grid node to update. Must be a valid coordinate within the grid.
	/// </param>
	/// <param name="y">
	/// The y-coordinate of the grid node to update. Must be a valid coordinate within the grid.
	/// </param>
	/// <param name="terrainType">
	/// The new TerrainType to assign to the grid node. This determines the node's walkability, movement cost, and visual representation.
	/// </param>
	private void SetTerrainType(int x, int y, TerrainType terrainType)
	{
		if (!IsValidCoordinate(x, y)) return;
        
		GridNode node = _gridNodes[x, y];
		node.TerrainType = terrainType;
		node.Walkable = terrainType.Walkable;
		node.Weight = terrainType.MovementCost;
		node.GizmoColor = terrainType.GizmoColor;
		_gridNodes[x, y] = node;
	}


	/// <summary>
	/// Validates if the given x and y coordinates are within the bounds of the grid.
	/// </summary>
	/// <param name="x">The x-coordinate to validate. Must be non-negative and less than GridSizeX.</param>
	/// <param name="y">The y-coordinate to validate. Must be non-negative and less than GridSizeY.</param>
	/// <returns>
	/// Returns true if the provided coordinates are within the valid range of the grid dimensions; otherwise, false.
	/// </returns>
	private bool IsValidCoordinate(int x, int y)
	{
		return x >= 0 && x < _gridSettings.GridSizeX && y >= 0 && y < _gridSettings.GridSizeY;
	}

	/// <summary>
	/// Draws visual representations of the grid nodes in the Unity Editor Scene view using Gizmos to aid in debugging and development.
	/// </summary>
	/// <remarks>
	/// Unity calls this method. It renders wireframe cubes representing the grid nodes based on their positions and walkability within the scene.
	/// It is intended to help developers visualize the grid system during development and make adjustments.
	/// </remarks>
	private void OnDrawGizmos()
	{
		// If grid is not initialized or gridSettings is missing, abort visualization.
		if (_gridNodes == null || _gridSettings == null) return;

		// Loop through each grid cell to draw its representation.
		for (int x = 0; x < _gridSettings.GridSizeX; x++)
		{
			for (int y = 0; y < _gridSettings.GridSizeY; y++)
			{
				// Retrieve the node for the current coordinates.
				GridNode node = _gridNodes[x, y];

				// Change Gizmos color based on walkability: green for walkable, red for non-walkable.
				Gizmos.color = node.GizmoColor;

				// Draw a wireframe cube at the node's world position.
				Gizmos.DrawWireCube(node.WorldPosition, Vector3.one * _gridSettings.NodeSize * 0.9f);
			}
		}

		// Calls the debug list to be created
		PopulateDebugList();
	}
}