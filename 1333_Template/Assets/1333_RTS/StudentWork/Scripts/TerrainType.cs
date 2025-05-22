using UnityEngine;

/// <summary>
/// Represents a specific type of terrain that can be used within the game environment.
/// </summary>
/// <remarks>
/// This class is a ScriptableObject, allowing for easy reuse and customization of terrain types in Unity's asset workflow.
/// </remarks>
[CreateAssetMenu(fileName = "TerrainType", menuName = "Game/Terrain Type")]
public class TerrainType : ScriptableObject
{
	/// <summary>
	/// Represents the name of a terrain type.
	/// </summary>
	/// <remarks>
	/// This variable stores a string that specifies the name assigned to a terrain type.
	/// It is serialized to allow customization via Unity's Inspector.
	/// </remarks>
	[SerializeField] private string _terrainName = "Default";

	/// <summary>
	/// Represents the color used to visually represent the terrain type in editor gizmos.
	/// </summary>
	/// <remarks>
	/// This variable is serialized to appear in the Unity Editor and can be customized.
	/// Helps differentiate terrain types visually during scene setup or debugging.
	/// </remarks>
	/// <value>
	/// A UnityEngine.Color object that defines the gizmo's color for this terrain type.
	/// Defaults to Color.green if not explicitly modified.
	/// </value>
	[SerializeField] private Color _gizmoColor = Color.green;

	/// <summary>
	/// Determines whether the terrain can be traversed by units.
	/// </summary>
	/// <remarks>
	/// This variable specifies if the terrain is passable or impassable.
	/// A value of true indicates units can move on the terrain;
	/// false means the terrain is unwalkable or obstructed.
	/// </remarks>
	[SerializeField] private bool _walkable = true;

	/// <summary>
	/// Represents the movement cost required to traverse the terrain.
	/// </summary>
	/// <remarks>
	/// Movement cost is an integer value that determines how resource-intensive
	/// it is to move across the terrain. Higher values imply greater difficulty.
	/// </remarks>
	[SerializeField] private int _movementCost = 1;

	/// Gets the name of the terrain type.
	/// This property provides access to the name of the terrain type, defined as a private string (_terrainName).
	/// Use this to identify the specific terrain type in the game, for example, "Mountain", "Forest", etc.
	/// The value is typically set via the Unity Inspector or by code during initialization.
	public string TerrainName => _terrainName;

	/// <summary>
	/// Represents the color used to visually render Gizmos for the terrain type in the Unity Editor.
	/// </summary>
	/// <remarks>
	/// This property is useful for debugging or visualizing specific terrain types in the Scene view.
	/// It represents a color assigned to a terrain type, used when Gizmos are drawn.
	/// </remarks>
	/// <value>
	/// A Color value indicating the default visualization color for the terrain type's Gizmos.
	/// </value>
	public Color GizmoColor => _gizmoColor;

	/// <summary>
	/// Determines if the terrain type is walkable.
	/// </summary>
	/// <remarks>
	/// A value of true indicates that entities can traverse this terrain type,
	/// while false prevents movement through this area.
	/// </remarks>
	/// <value>
	/// A boolean reflecting the terrain's walkability. Returns true if walkable; otherwise, false.
	/// </value>
	public bool Walkable => _walkable;

	/// Represents the movement cost associated with a specific terrain type.
	/// This property is used to indicate how difficult or resource-intensive it is
	/// for a character or unit to traverse a particular type of terrain.
	/// The value of the movement cost impacts pathfinding algorithms.
	/// Higher values represent terrains that require more effort to cross
	/// and thus are less preferred, while lower values allow easier traversal.
	/// Usage in game systems:
	/// - Pathfinding: Determines the most efficient route by considering movement cost.
	/// - AI decision-making: Influences how units decide movement through the game world.
	/// - Gameplay balancing: Variations in movement cost can define strategic areas.
	/// This property is read-only and is initialized to a default value within the scriptable object.
	public int MovementCost => _movementCost;
}