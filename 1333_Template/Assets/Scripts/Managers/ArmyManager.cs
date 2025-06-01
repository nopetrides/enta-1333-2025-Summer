using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles spawning of units for each army at runtime.
/// Reads from ArmyComposition ScriptableObjects for player and enemy armies,
/// instantiates the appropriate prefabs, and initializes each unit with its stats, GridManager, and AStarPathfinder.
/// </summary>
public class ArmyManager : MonoBehaviour
{
    [Header("Army Compositions")]
    [Tooltip("ScriptableObject defining the player army composition.")]
    [SerializeField] private ArmyComposition _playerArmySO = null;

    [Tooltip("ScriptableObject defining the enemy army composition.")]
    [SerializeField] private ArmyComposition _enemyArmySO = null;

    [Tooltip("ScriptableObject defining the spearman army composition.")]
    [SerializeField] private ArmyComposition _spearManArmyS0 = null;

    private GridManager _gridManager;
    private AStarPathfinder _pathfinder;

    public void Initialize(GridManager gridManager)
    {
        // Get GridManager from game manager
        _gridManager = gridManager;
        if (_gridManager == null)
        {
            Debug.LogError("ArmyManager: No GridManager found in the scene.");
        }
        // Create a new AStarPathfinder using the found GridManager
        _pathfinder = new AStarPathfinder(_gridManager);
    }

    private void Start()
    {
        // Spawn player army units
        /*if (_playerArmySO != null)
        {
            SpawnArmy(_playerArmySO, Team.Player);
        }*/

        // Spawn enemy army units
        /*if (_enemyArmySO != null)
        {
            SpawnArmy(_enemyArmySO, Team.Enemy);
        }*/
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            // Spawn player army units
            if (_playerArmySO != null)
            {
                SpawnArmy(_spearManArmyS0, Team.Player);
            }
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            // Spawn player army units
            if (_playerArmySO != null)
            {
                SpawnArmy(_spearManArmyS0, Team.Enemy);
            }
        }
    }

    /// <summary>
    /// Instantiates units for a specific army composition.
    /// Each entry in the composition contains a UnitTypePrefab and a count.
    /// </summary>
    /// <param name="composition">ArmyComposition SO containing unit entries.</param>
    /// <param name="team">Which team (Player or Enemy) these units belong to.</param>
    private void SpawnArmy(ArmyComposition composition, Team team)
    {
        foreach (UnitEntry entry in composition.unitEntries)
        {
            // Retrieve the UnitType stats and prefab reference
            UnitType stats = entry.unitTypePrefab.unitType;
            GameObject prefab = entry.unitTypePrefab.prefab;

            if (stats == null || prefab == null)
            {
                Debug.LogWarning($"ArmyManager: Null stats or prefab in ArmyComposition entry for team {team}.");
                continue;
            }

            for (int i = 0; i < entry.count; i++)
            {
                // 1) Choose a random spawn position on a walkable node
                GridNode? randomNodeNullable = _gridManager.GetRandomWalkableNode();
                if (!randomNodeNullable.HasValue)
                {
                    Debug.LogWarning("ArmyManager: No walkable nodes available to spawn units.");
                    return;
                }

                // Convert nullable to actual struct
                GridNode randomNode = randomNodeNullable.Value;
                Vector3 spawnPosition = randomNode.worldPosition + Vector3.up * 0.5f;

                // 2) Instantiate the unit prefab at that position
                GameObject unitGO = Instantiate(prefab, spawnPosition, Quaternion.identity);

                // 3) Initialize the UnitBase component on the spawned GameObject
                UnitBase unitComponent = unitGO.GetComponent<UnitBase>();
                if (unitComponent != null)
                {
                    unitComponent.Initialize(stats, _gridManager, _pathfinder, team);
                }
                else
                {
                    Debug.LogWarning($"ArmyManager: Spawned object {unitGO.name} does not have a UnitBase-derived component.");
                }
            }
        }
    }
}
