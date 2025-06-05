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

    [Tooltip("ScriptableObject defining the mounted knight army composition.")]
    [SerializeField] private ArmyComposition _mountedKnightArmyS0 = null;

    [Tooltip("ScriptableObject defining the worker army composition.")]
    [SerializeField] private ArmyComposition _workerArmyS0 = null;

    [Tooltip("ScriptableObject defining the mounted high mage army composition.")]
    [SerializeField] private ArmyComposition _mountedHighMageArmyS0 = null;

    [Tooltip("ScriptableObject defining the archer army composition.")]
    [SerializeField] private ArmyComposition _archerArmySO = null;

    [Tooltip("ScriptableObject defining the crossbowman army composition.")]
    [SerializeField] private ArmyComposition _crossbowManArmySO = null;

    [Tooltip("ScriptableObject defining the commander army composition.")]
    [SerializeField] private ArmyComposition _commanderArmySO = null;

    [Tooltip("ScriptableObject defining the mage army composition.")]
    [SerializeField] private ArmyComposition _mageArmySO = null;

    [Tooltip("ScriptableObject defining the high mage army composition.")]
    [SerializeField] private ArmyComposition _highMageArmySO = null;

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
        HandleSpawnInput();
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
            GameObject prefab = entry.unitTypePrefab.unitPrefab;

            if (stats == null || prefab == null)
            {
                Debug.LogWarning($"ArmyManager: Null stats or prefab in ArmyComposition entry for team {team}.");
                continue;
            }

            for (int i = 0; i < entry.count; i++)
            {
                // 1) Choose a random spawn position on a walkable node
                GridNode? randomWalkableNode = _gridManager.GetRandomWalkableNode();
                if (!randomWalkableNode.HasValue)
                {
                    Debug.LogWarning("ArmyManager: No walkable nodes available to spawn units.");
                    return;
                }

                GridNode randomNode = randomWalkableNode.Value;
                Vector3 spawnPosition = randomNode.worldPosition;

                // 2) Instantiate the unit prefab at that position
                GameObject unitGO = Instantiate(prefab, spawnPosition, Quaternion.identity);

                // 3) Get the UnitBase component and register it
                UnitBase unitComponent = unitGO.GetComponent<UnitBase>();
                if (unitComponent != null)
                {
                    // Register the unit with UnitManager
                    _unitManager.RegisterUnit(unitComponent);

                    // 4) Initialize the unit with its type, gridManager, pathfinder, and team
                    unitComponent.Initialize(stats, _gridManager, _unitManager,_pathfinder, team);
                }
                else
                {
                    Debug.LogWarning($"ArmyManager: Spawned object {unitGO.name} does not have a UnitBase-derived component.");
                    Destroy(unitGO);
                }
            }
        }
    }

    /// <summary>
    /// Checks for key presses and calls SpawnArmy with the appropriate ArmyComposition and Team.
    /// </summary>
    private void HandleSpawnInput()
    {
        ArmyComposition composition = null;
        Team team = Team.Player;

        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            // Spawn All Enemy army
            composition = _enemyArmySO;
            team = Team.Enemy;
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            // Spawn All Player army
            composition = _playerArmySO;
            team = Team.Player;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            // Spawn Player Spearman army
            composition = _spearManArmySO;
            team = Team.Player;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            // Spawn Player Mounted Knight army
            composition = _mountedKnightArmyS0;
            team = Team.Player;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            // Spawn Player Worker army
            composition = _workerArmyS0;
            team = Team.Player;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            // Spawn Player Mounted High Mage army
            composition = _mountedHighMageArmyS0;
            team = Team.Player;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            // Spawn Player Archer army
            composition = _archerArmySO;
            team = Team.Player;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            // Spawn Player Crossbowman army
            composition = _crossbowManArmySO;
            team = Team.Player;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            // Spawn Player Commander army
            composition = _commanderArmySO;
            team = Team.Player;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            // Spawn Player Mage army
            composition = _mageArmySO;
            team = Team.Player;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            // Spawn Player High Mage army
            composition = _highMageArmySO;
            team = Team.Player;
        }

        if (composition != null)
        {
            SpawnArmy(composition, team);
        }
    }
}
