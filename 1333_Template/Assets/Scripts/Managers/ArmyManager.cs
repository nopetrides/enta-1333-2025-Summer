// ArmyManager.cs
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles spawning of units for each army at runtime.
/// Reads from ArmyComposition ScriptableObjects for player and enemy armies,
/// instantiates the appropriate prefabs, registers each unit with UnitManager,
/// and initializes each unit with its stats, GridManager, and AStarPathfinder.
/// </summary>
public class ArmyManager : MonoBehaviour
{
    [Header("Army Compositions")]
    [Tooltip("ScriptableObject defining the player army composition.")]
    [SerializeField] private ArmyComposition _playerArmySO = null;

    [Tooltip("ScriptableObject defining the enemy army composition.")]
    [SerializeField] private ArmyComposition _enemyArmySO = null;

    [Tooltip("ScriptableObject defining the spearman army composition.")]
    [SerializeField] private ArmyComposition _spearManArmySO = null;

    private GridManager _gridManager;
    private UnitManager _unitManager;
    private AStarPathfinder _pathfinder;

    /// <summary>
    /// Must be called by GameManager.Awake() to provide references to GridManager and UnitManager.
    /// </summary>
    /// <param name="gridManager">Reference to the GridManager in the scene.</param>
    /// <param name="unitManager">Reference to the UnitManager in the scene.</param>
    public void Initialize(GridManager gridManager, UnitManager unitManager)
    {
        _gridManager = gridManager;
        _unitManager = unitManager;

        if (_gridManager == null)
        {
            Debug.LogError("ArmyManager: GridManager reference is null.");
        }

        if (_unitManager == null)
        {
            Debug.LogError("ArmyManager: UnitManager reference is null.");
        }

        // Create a single AStarPathfinder instance using the provided GridManager
        _pathfinder = new AStarPathfinder(_gridManager);
    }

    private void Update()
    {
        // Press N to spawn Player Spearman army
        if (Input.GetKeyDown(KeyCode.N))
        {
            if (_spearManArmySO != null)
            {
                SpawnArmy(_spearManArmySO, Team.Player);
            }
        }

        // Press M to spawn Enemy Spearman army
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (_spearManArmySO != null)
            {
                SpawnArmy(_spearManArmySO, Team.Enemy);
            }
        }
    }

    /// <summary>
    /// Instantiates units for a specific army composition.
    /// Each entry in the composition contains a UnitTypePrefab and a count.
    /// After instantiating, each UnitBase is registered with UnitManager and initialized.
    /// </summary>
    /// <param name="composition">ArmyComposition SO containing unit entries.</param>
    /// <param name="team">Which team (Player or Enemy) these units belong to.</param>
    private void SpawnArmy(ArmyComposition composition, Team team)
    {
        if (_gridManager == null || _unitManager == null)
        {
            Debug.LogError("ArmyManager: Must call Initialize(gridManager, unitManager) before spawning.");
            return;
        }

        foreach (UnitEntry entry in composition.unitEntries)
        {
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

                GridNode randomNode = randomNodeNullable.Value;
                Vector3 spawnPosition = randomNode.worldPosition + Vector3.up * 0.5f;

                // 2) Instantiate the unit prefab at that position
                GameObject unitGO = Instantiate(prefab, spawnPosition, Quaternion.identity);

                // 3) Get the UnitBase component and register it
                UnitBase unitComponent = unitGO.GetComponent<UnitBase>();
                if (unitComponent != null)
                {
                    // Register the unit with UnitManager
                    _unitManager.RegisterUnit(unitComponent);

                    // 4) Initialize the unit with its type, gridManager, pathfinder, and team
                    unitComponent.Initialize(stats, _gridManager, _pathfinder, team);
                }
                else
                {
                    Debug.LogWarning($"ArmyManager: Spawned object {unitGO.name} does not have a UnitBase-derived component.");
                    Destroy(unitGO);
                }
            }
        }
    }
}
