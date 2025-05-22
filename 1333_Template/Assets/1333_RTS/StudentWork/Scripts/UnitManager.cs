using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Responsible for managing game units, including their placement and interactions.
/// This class integrates unit-related functions with the grid system defined in GridManager.
/// </summary>
public class UnitManager : MonoBehaviour
{
	/// <summary>
	/// The gridManager variable provides an interface to manage and interact with the game's GridManager instance.
	/// This variable links the UnitManager to the GridManager, enabling it to access grid-related functionality
	/// required for tasks such as spawning units or modifying grid properties.
	/// </summary>
	/// <remarks>
	/// The gridManager variable is private and serialized, allowing it to be assigned
	/// from the Unity Inspector while still ensuring encapsulation within the UnitManager class.
	/// It must reference a properly initialized GridManager instance for operations to work as expected.
	/// </remarks>
	[SerializeField] private GridManager _gridManager;
	
	// todo, manage all armies. Player is army id 0
	private Dictionary<int, ArmyManager> _armyManager;
	// Player is army id 0
	public ArmyManager PlayerArmy => _armyManager?[0];

	/// <summary>
	/// Spawns a dummy unit at a random position on the grid managed by the GridManager.
	/// Verifies the grid's initialization state before executing the operation.
	/// Logs the coordinates and world position of the spawn location for tracking purposes.
	/// Currently, the method does not instantiate a physical unit object but sets the groundwork for future functionality.
	/// </summary>
	public void SpawnDummyUnit(Transform parent)
    {
        if (!_gridManager.IsInitialized)
        {
            Debug.LogError("Grid not initialized!");
            return;
        }

        int randomX = Random.Range(0, _gridManager.GridSettings.GridSizeX);
        int randomY = Random.Range(0, _gridManager.GridSettings.GridSizeY);

        GridNode spawnNode = _gridManager.GetNode(randomX, randomY);
        Debug.Log($"Dummy unit spawned at ({randomX}, {randomY}) - World Position: {spawnNode.WorldPosition}");

        // Instantiate dummy unit prefab here in future
    }
}
