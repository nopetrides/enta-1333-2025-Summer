using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles spawning of units for each army at runtime.
/// Reads from ArmyComposition ScriptableObjects for various army compositions,
/// instantiates the appropriate prefabs, registers each unit with UnitManager,
/// and initializes each unit with its stats, GridManager, and AStarPathfinder.
/// </summary>
public class ArmyManager : MonoBehaviour
{
    [Header("Army Compositions")]
    [Tooltip("ScriptableObject defining the player army composition.")]
    [SerializeField] private ArmyCompositionSO _playerArmySO = null;
    [Tooltip("ScriptableObject defining the enemy army composition.")]
    [SerializeField] private ArmyCompositionSO _enemyArmySO = null;
    [Tooltip("ScriptableObject defining the spearman army composition.")]
    [SerializeField] private ArmyCompositionSO _spearManArmySO = null;
    [Tooltip("ScriptableObject defining the mounted knight army composition.")]
    [SerializeField] private ArmyCompositionSO _mountedKnightArmySO = null;
    [Tooltip("ScriptableObject defining the worker army composition.")]
    [SerializeField] private ArmyCompositionSO _workerArmySO = null;
    [Tooltip("ScriptableObject defining the mounted high mage army composition.")]
    [SerializeField] private ArmyCompositionSO _mountedHighMageArmySO = null;
    [Tooltip("ScriptableObject defining the archer army composition.")]
    [SerializeField] private ArmyCompositionSO _archerArmySO = null;
    [Tooltip("ScriptableObject defining the crossbowman army composition.")]
    [SerializeField] private ArmyCompositionSO _crossbowManArmySO = null;
    [Tooltip("ScriptableObject defining the commander army composition.")]
    [SerializeField] private ArmyCompositionSO _commanderArmySO = null;
    [Tooltip("ScriptableObject defining the mage army composition.")]
    [SerializeField] private ArmyCompositionSO _mageArmySO = null;
    [Tooltip("ScriptableObject defining the high mage army composition.")]
    [SerializeField] private ArmyCompositionSO _highMageArmySO = null;

    // References injected at startup
    private GridManager _gridManager;
    private UnitManager _unitManager;
    private AStarPathfinder _pathfinder;

    /// <summary>
    /// Initializes the ArmyManager with required dependencies.
    /// Must be called by GameManager.Awake() before any spawning.
    /// </summary>
    /// <param name="gridManager">Reference to the GridManager in the scene.</param>
    /// <param name="unitManager">Reference to the UnitManager in the scene.</param>
    public void Initialize(GridManager gridManager, UnitManager unitManager)
    {
        _gridManager = gridManager;
        _unitManager = unitManager;

        if (_gridManager == null)
            Debug.LogError("ArmyManager: GridManager reference is null.");
        if (_unitManager == null)
            Debug.LogError("ArmyManager: UnitManager reference is null.");

        // Create a single AStarPathfinder instance using the provided GridManager
        _pathfinder = new AStarPathfinder(_gridManager);
    }

    /// <summary>
    /// Called every frame to handle debug key input for spawning armies.
    /// </summary>
    private void Update()
    {
        HandleSpawnInput();
    }

    /// <summary>
    /// Spawns units according to the given army composition for the specified team.
    /// Each UnitEntry in the composition defines a UnitTypePrefab and a count.
    /// </summary>
    /// <param name="composition">ArmyCompositionSO asset defining unit entries.</param>
    /// <param name="team">Team affiliation (Player or Enemy) for the spawned units.</param>
    public void SpawnArmy(ArmyCompositionSO composition, Team team)
    {
        if (_gridManager == null || _unitManager == null)
        {
            Debug.LogError("ArmyManager: Must call Initialize() before spawning.");
            return;
        }

        foreach (UnitEntry entry in composition.unitEntries)
        {
            UnitTypeSO unitStats = entry.unitTypePrefab.unitType;
            GameObject prefab = entry.unitTypePrefab.unitPrefab;

            if (unitStats == null || prefab == null)
            {
                Debug.LogWarning($"ArmyManager: Missing stats or prefab in entry for army '{composition.armyName}'.");
                continue;
            }

            for (int i = 0; i < entry.count; i++)
            {
                // Choose a random walkable node for spawn
                GridNode node = _gridManager.GetRandomWalkableNode();
                if (node == null)
                {
                    Debug.LogWarning("ArmyManager: No walkable nodes available to spawn units.");
                    return;
                }

                Vector3 spawnPos = node.worldPosition;
                GameObject unitGO = Instantiate(prefab, spawnPos, Quaternion.identity);

                UnitBase unitComp = unitGO.GetComponent<UnitBase>();
                if (unitComp != null)
                {
                    _unitManager.RegisterUnit(unitComp);
                    unitComp.Initialize(unitStats, _gridManager, _unitManager, _pathfinder, team);
                }
                else
                {
                    Debug.LogWarning($"ArmyManager: Spawned object '{unitGO.name}' lacks a UnitBase-derived component.");
                    Destroy(unitGO);
                }
            }
        }
    }

    /// <summary>
    /// Checks for debug key presses and calls SpawnArmy with the matching composition.
    /// BackQuote (`) spawns the enemy army; numbers 1–0 spawn various player armies.
    /// </summary>
    private void HandleSpawnInput()
    {
        ArmyCompositionSO composition = null;
        Team team = Team.Player;

        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            composition = _enemyArmySO;
            team = Team.Enemy;
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            composition = _playerArmySO;
            team = Team.Player;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            composition = _spearManArmySO;
            team = Team.Player;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            composition = _mountedKnightArmySO;
            team = Team.Player;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            composition = _workerArmySO;
            team = Team.Player;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            composition = _mountedHighMageArmySO;
            team = Team.Player;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            composition = _archerArmySO;
            team = Team.Player;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            composition = _crossbowManArmySO;
            team = Team.Player;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            composition = _commanderArmySO;
            team = Team.Player;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            composition = _mageArmySO;
            team = Team.Player;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            composition = _highMageArmySO;
            team = Team.Player;
        }

        if (composition != null)
            SpawnArmy(composition, team);
    }
}
