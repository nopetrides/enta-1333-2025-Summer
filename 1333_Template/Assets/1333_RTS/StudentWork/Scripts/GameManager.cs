using UnityEngine;

namespace RTS_1333
{
	/// <summary>
	/// The GameManager class serves as a central hub for coordinating game logic and gameplay systems.
	/// It is responsible for handling various aspects such as game state transitions, tracking
	/// progress, and interfacing between different systems within the game.
	/// </summary>
	/// <remarks>
	/// This class derives from MonoBehaviour, allowing it to take advantage of Unity's component-based
	/// system and lifecycle methods (e.g., Awake, Start, Update).
	/// It can be attached to a GameObject within a Unity scene as a component to manage the game logic effectively.
	/// </remarks>
	public class GameManager : MonoBehaviour
	{
		/// <summary>
		/// Represents an instance of the GridManager class, used to handle the grid-based
		/// functionalities within the game. This includes tasks such as initializing the grid,
		/// randomizing terrain, accessing grid properties, and managing grid-based operations.
		/// The grid system serves as a key part for gameplay elements like pathfinding
		/// and marker placement.
		/// </summary>
		[SerializeField] private GridManager _gridManager;

		/// <summary>
		/// The _unitManager variable is an instance of the UnitManager class.
		/// It is responsible for managing the game's units, including their spawning, placement, and interactions within the game.
		/// This reference enables the GameManager to coordinate unit-related functionality through UnitManager methods.
		/// </summary>
		/// <remarks>
		/// Declared as private and serialized, this variable can be set in the Unity Editor to reference a UnitManager component.
		/// This ensures seamless communication between the GameManager and UnitManager during gameplay scenarios.
		/// </remarks>
		[SerializeField] private UnitManager _unitManager;

		/// <summary>
		/// The pathfinder variable is an instance of the Pathfinder class.
		/// It is responsible for determining optimal paths between points in the game environment.
		/// This variable facilitates interaction with different pathfinding algorithms that can be customized or selected
		/// based on the game's requirements, such as A* or Dijkstra. It plays a crucial role in navigation and movement systems,
		/// particularly for AI agents or player-controlled entities requiring directional guidance across the game grid.
		/// </summary>
		[Header("Required References")]
		[SerializeField] private Pathfinder _pathfinder;

		/// <summary>
		/// The _startMarker variable represents the starting point in the game used for pathfinding.
		/// It is a reference to a Transform component, which defines the position, rotation, and scale
		/// of the starting marker in the Unity scene. The position of this marker is dynamically updated
		/// during gameplay, such as when generating random start positions on the grid.
		/// </summary>
		[SerializeField] private Transform _startMarker;

		/// <summary>
		/// The _endMarker variable is a serialized Transform object representing
		/// the target destination or endpoint within the game.
		/// It is often used in conjunction with pathfinding systems to determine
		/// where an entity, such as a character or object, should move toward.
		/// </summary>
		/// <remarks>
		/// - This field is private but serialized to allow configuration in the Unity Inspector.
		/// - The position of _endMarker is randomized within the grid dimensions defined by GridSettings.
		/// - It must be uniquely positioned, avoiding overlap with the start marker (_startMarker).
		/// - Proper setup ensures it functions correctly, contributing to gameplay mechanics
		/// like navigation and path visualization.
		/// </remarks>
		[SerializeField] private Transform _endMarker;

		/// <summary>
		/// The pathLine variable is an instance of the LineRenderer class.
		/// It is used to visually represent a path in the game, typically for
		/// purposes such as showing the route a character or object will follow
		/// or highlighting specific paths on the grid. The LineRenderer allows
		/// for dynamic and customizable line rendering, making it a versatile tool
		/// for path visualization in Unity.
		/// </summary>
		[SerializeField] private LineRenderer _pathLine;

		/// <summary>
		/// The markerHeight variable represents the vertical offset (height) in world units
		/// at which the start and end markers are positioned on the game grid.
		/// It is primarily used to ensure that these markers are visually distinguishable
		/// and appear at the intended height above the grid's surface.
		/// </summary>
		/// <remarks>
		/// This variable is typically set in the Unity Inspector due to its [SerializeField] attribute,
		/// allowing for easy customization without modifying the code.
		/// Changes to this value affect both the start and end markers, as they share the same height setting.
		/// </remarks>
		[Header("Randomization Settings")]
		[SerializeField] private float _markerHeight = 0.5f;


		/// <summary>
		/// Called when the GameManager script instance is being loaded.
		/// Ensures that all necessary components and references are properly initialized before the game starts.
		/// </summary>
		/// <remarks>
		/// This method checks for required references, such as the GridManager and other components.
		/// If any references are missing, it disables the GameManager to prevent further execution
		/// and outputs an error message to the console.
		/// Successfully validates references and triggers initialization of the grid and pathfinding logic if all conditions are met.
		/// </remarks>
		private void Awake()
		{
			if (!ValidateReferences())
			{
				Debug.LogError(
					"GridTest: Missing required references. Please assign all required components in the inspector.");
				enabled = false;
				return;
			}

			_gridManager.InitializeGrid();
			_unitManager.SpawnDummyUnit(_startMarker);
			_unitManager.SpawnDummyUnit(_endMarker);
			RandomizeAndPathFind();
		}

		/// <summary>
		/// Validates that all required references for the GameManager script are properly assigned in the Unity Inspector.
		/// This method ensures that essential components, such as the GridManager, Pathfinder, and required Transforms,
		/// are available before proceeding with any game logic dependent on them. If any reference is missing,
		/// the game initialization process will halt, preventing runtime errors related to null references.
		/// </summary>
		/// <returns>
		/// Returns a boolean value indicating whether all required references have been assigned.
		/// - Returns true if all dependencies are properly assigned.
		/// - Returns false if any required reference is missing.
		/// </returns>
		private bool ValidateReferences()
		{
			if (!_gridManager || !_pathfinder || !_startMarker ||
				!_endMarker || !_pathLine)
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Called once per frame during the runtime of the game.
		/// This method is used to monitor user input or perform operations that need to be checked frequently.
		/// </summary>
		/// <remarks>
		/// In this implementation, it checks if the Space key is pressed and triggers the process
		/// to randomize terrain data and perform a pathfinding operation accordingly.
		/// </remarks>
		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Space))
			{
				RandomizeAndPathFind();
			}
		}

		/// <summary>
		/// Randomizes the terrain and marker positions on the grid, and computes a new path between the start and end markers.
		/// This method is used to simulate dynamic changes in the grid environment and update pathfinding accordingly.
		/// </summary>
		/// <remarks>
		/// - First, this method calls <c>RandomizeAll</c>, which randomizes the terrain and positions of start and end markers.
		/// - Then, pathfinding is initiated to determine the updated path based on the new marker positions.
		/// - Intended to be executed both during initialization and reacting to user input.
		/// </remarks>
		private void RandomizeAndPathFind()
		{
			RandomizeAll();
			// Find and visualize path
			var path = _pathfinder.FindPath(_startMarker.position, _endMarker.position);
			string msg = $"Path found: {path.Count} steps. Start at {_startMarker.position}, end at {_endMarker.position}.\nStart at";
			foreach (var p in path)
			{
				msg += $"\n> {p.WorldPosition}";
			}

			msg += $" > end at {_endMarker.position}.";
			Debug.Log(msg);
		}

		/// <summary>
		/// Randomizes all game elements controlled by the GameManager.
		/// This includes invoking terrain randomization within the grid and modifying markers.
		/// </summary>
		/// <remarks>
		/// This method utilizes the GridManager's terrain randomization functionality, ensuring each node
		/// within the grid has a randomized terrain type. Additionally, it randomizes the positions or
		/// properties of markers used within the game.
		/// Ensuring diversity in the game world layout on every run, this method is typically called
		/// before initiating pathfinding or game-specific logic.
		/// </remarks>
		private void RandomizeAll()
		{
			_gridManager.RandomizeTerrain();
			RandomizeMarkers();
		}

		/// <summary>
		/// Randomizes the positions of the start and end markers within the grid boundaries.
		/// The method ensures that the start and end markers are placed at distinct random grid positions.
		/// </summary>
		/// <remarks>
		/// This method relies on the grid settings provided by the GridManager class
		/// to determine the grid dimensions (GridSizeX, GridSizeY) and the size of each node (NodeSize).
		/// It adjusts the vertical position of the markers based on the configured marker height.
		/// </remarks>
		private void RandomizeMarkers()
		{
			int gridSizeX = _gridManager.GridSettings.GridSizeX;
			int gridSizeY = _gridManager.GridSettings.GridSizeY;
			float nodeSize = _gridManager.GridSettings.NodeSize;

			// Randomize start marker
			int startX = Random.Range(0, gridSizeX);
			int startY = Random.Range(0, gridSizeY);
			_startMarker.position = new Vector3(startX * nodeSize, _markerHeight, startY * nodeSize);

			// Randomize end marker (ensuring it's different from start)
			int endX, endY;
			do
			{
				endX = Random.Range(0, gridSizeX);
				endY = Random.Range(0, gridSizeY);
			} while (endX == startX && endY == startY);

			_endMarker.position = new Vector3(endX * nodeSize, _markerHeight, endY * nodeSize);
		}
	}
}