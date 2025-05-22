using UnityEngine;

namespace RTS_1333
{
	// GridSettings is a ScriptableObject for easy customization of grid dimensions and orientation.
	/// <summary>
	/// Represents the configuration settings for a grid.
	/// This class encapsulates properties to determine grid dimensions and behavior.
	/// </summary>
	[CreateAssetMenu(fileName = "GridSettings", menuName = "Game/GridSettings")]
	public class GridSettings : ScriptableObject
	{
		/// <summary>
		/// Represents the size of the grid along the X-axis.
		/// </summary>
		/// <remarks>
		/// This variable determines the number of grid cells along the X-axis in a grid-based system.
		/// The value is serialized and can be set in the Unity Inspector for easy configuration.
		/// It is used in grid-related operations, such as defining the grid's dimensions and calculating positions.
		/// </remarks>
		[SerializeField] private int _gridSizeX = 10;

		/// <summary>
		/// Represents the height of the grid, specifically the number of
		/// cells or nodes along the vertical axis of the grid.
		/// </summary>
		/// <remarks>
		/// This value is customizable through the Unity Inspector interface
		/// when modifying the associated GridSettings ScriptableObject.
		/// It is used to define the vertical dimensions of a grid used
		/// in gameplay or procedural generation systems.
		/// Adjustable alongside <c>_gridSizeX</c>, these parameters collectively
		/// determine the overall size and layout of the grid-based system.
		/// </remarks>
		/// <example>
		/// Use this parameter to define the number of rows in the grid
		/// for purposes such as gameplay logic, level design, or pathfinding
		/// systems in 2D or 3D space.
		/// </example>
		[SerializeField] private int _gridSizeY = 10;

		/// <summary>
		/// Represents the size of each individual node in the grid.
		/// </summary>
		/// <remarks>
		/// Determines the physical spacing between nodes in the grid structure.
		/// Modifying this value will impact the scale of the grid.
		/// Typically used to adjust the resolution of the grid for gameplay or design purposes.
		/// </remarks>
		[SerializeField] private float _nodeSize = 1f;

		/// <summary>
		/// Determines whether the grid is aligned to the XZ plane or the XY plane in the world space.
		/// </summary>
		/// <remarks>
		/// When set to <c>true</c>, the grid will be aligned to the XZ plane, which is common for
		/// 3D environments such as terrains or top-down game worlds.
		/// When set to <c>false</c>, the grid will be aligned to the XY plane, typically used
		/// for 2D games or UI-based grids.
		/// </remarks>
		/// <example>
		/// This variable can be configured in the Inspector to adjust the grid orientation
		/// without requiring code changes.
		/// </example>
		[SerializeField] private bool _useXZPlane = true;

		/// <summary>
		/// Represents the default terrain type used when no specific terrain type
		/// is specified for a grid.
		/// </summary>
		/// <remarks>
		/// The _defaultTerrainType variable is of type TerrainType, which is a ScriptableObject.
		/// This allows for customization of terrain properties such as name, color, walkability,
		/// and movement cost directly within the Unity Editor.
		/// </remarks>
		/// <example>
		/// Use this field in the GridSettings ScriptableObject to define the
		/// terrain that will be applied by default to all nodes in the grid
		/// unless overridden.
		/// </example>
		[SerializeField] private TerrainType _defaultTerrainType;

		/// <summary>
		/// An array storing different types of terrain that can exist within the grid system.
		/// </summary>
		/// <remarks>
		/// Each element in this array represents a specific terrain type defined by the TerrainType class.
		/// These terrain types include properties such as walkability, movement cost, and visual representation.
		/// </remarks>
		/// <seealso cref="TerrainType">
		/// The TerrainType ScriptableObject that defines the individual properties of each terrain.
		/// </seealso>
		[SerializeField] private TerrainType[] _terrainTypes;

		/// <summary>
		/// Represents the size of the grid along the X-axis in the grid system.
		/// </summary>
		/// <remarks>
		/// The value of this property determines how many grid cells are created
		/// along the horizontal axis of the grid (X-axis).
		/// </remarks>
		/// <value>
		/// An integer defining the number of cells along the X-axis of the grid.
		/// This value is set in the GridSettings ScriptableObject and is immutable
		/// at runtime for consistent grid behavior.
		/// </value>
		/// <example>
		/// This property is commonly accessed to determine dimensions of the grid
		/// when initializing or interacting with grid-related systems, such as
		/// placing objects or navigating the grid.
		/// </example>
		/// <seealso cref="GridSizeY"/>
		/// <seealso cref="GridManager.InitializeGrid()"/>
		public int GridSizeX => _gridSizeX;

		/// <summary>
		/// Represents the vertical size (number of rows) of the grid in the game.
		/// </summary>
		/// <remarks>
		/// GridSizeY is one of the two primary dimensions for defining the grid in the game,
		/// alongside GridSizeX, which defines the horizontal size.
		/// It specifies how many grid cells there are vertically, or along the Y-axis,
		/// when the grid is created.
		/// This property is defined in the GridSettings ScriptableObject class,
		/// which enables easy customization of grid dimensions within the Unity Editor.
		/// The value of GridSizeY is backed by a private serialized field (_gridSizeY)
		/// to ensure it can be modified directly in the Editor while remaining encapsulated.
		/// GridSizeY is accessed as a read-only property, meaning its value cannot
		/// be modified at runtime through code but can be retrieved as needed.
		/// </remarks>
		/// <example>
		/// For instance, if GridSizeY is set to 10, the grid will have
		/// 10 rows of grid cells when initialized.
		/// </example>
		public int GridSizeY => _gridSizeY;

		/// <summary>
		/// Represents the size of an individual grid node.
		/// </summary>
		/// <remarks>
		/// This property determines the dimensions of each grid cell in the game environment.
		/// It directly impacts the spatial layout and positioning of objects within the grid.
		/// </remarks>
		/// <value>
		/// A float value that specifies the width and height of a single node in world units.
		/// </value>
		/// <example>
		/// The <c>NodeSize</c> property can be used to calculate world positions when constructing or manipulating the grid.
		/// </example>
		/// <seealso cref="GridSettings"/>
		/// <seealso cref="GridManager"/>
		public float NodeSize => _nodeSize;

		/// <summary>
		/// Determines the orientation of the grid in a 3D space by toggling between XZ and XY planes.
		/// </summary>
		/// <remarks>
		/// If set to true, the grid will align along the XZ plane, where the Y-axis is assumed to represent vertical height.
		/// If set to false, the grid will align along the XY plane, where the Z-axis is excluded or irrelevant.
		/// The choice of plane affects how nodes are positioned and calculated in a Unity scene.
		/// </remarks>
		/// <value>
		/// A boolean value indicating the grid orientation:
		/// - True: The grid is constructed along the XZ plane (default configuration for most game worlds).
		/// - False: The grid is constructed along the XY plane (useful for 2D or top-down games).
		/// </value>
		public bool UseXZPlane => _useXZPlane;

		/// <summary>
		/// Represents the default terrain type assigned to all cells in a grid when they are initialized.
		/// </summary>
		/// <remarks>
		/// The DefaultTerrainType property refers to a predefined TerrainType ScriptableObject, which defines
		/// key properties such as terrain name, movement cost, walkability, and gizmo visualization color.
		/// Any node on the grid inherits these default characteristics if no specific terrain type is assigned.
		/// </remarks>
		/// <value>
		/// A TerrainType object that serves as the grid's default setting for all nodes when initialized.
		/// This value is defined in the associated GridSettings ScriptableObject.
		/// </value>
		/// <example>
		/// When a grid is generated by the GridManager, each cell is initialized with the DefaultTerrainType
		/// specified in the GridSettings unless overridden by a custom terrain type.
		/// </example>
		/// <see cref="TerrainType"/> for details on the properties that compose a terrain type.
		/// <see cref="GridSettings"/> for further configuration of grid and terrain defaults.
		public TerrainType DefaultTerrainType => _defaultTerrainType;

		/// <summary>
		/// Represents an array of available terrain types in the grid system.
		/// </summary>
		/// <remarks>
		/// Provides a collection of <see cref="TerrainType"/> objects that define
		/// properties such as walkability, movement cost, and visual representation
		/// for each type of terrain in the game world.
		/// </remarks>
		/// <value>
		/// Returns an array of <see cref="TerrainType"/> objects. Each terrain type describes
		/// the characteristics of specific areas within the grid, allowing for customization
		/// of the game's environment and node behaviors.
		/// </value>
		/// <example>
		/// This property can be used to access terrain options for generating or modifying the game's grid,
		/// such as assigning random terrain types to grid nodes or applying terrain-specific logic for gameplay.
		/// </example>
		public TerrainType[] TerrainTypes => _terrainTypes;
	}
}