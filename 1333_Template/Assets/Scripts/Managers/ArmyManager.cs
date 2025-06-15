using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Identifiers for each army composition asset.
/// </summary>
public enum ArmyType
{
    PlayerArmy,
    EnemyArmy,
    Spearman,
    Archer,
    CrossbowMan,
    MountedKnight,
    Mage,
    HighMage,
    MountedHighMage,
    Commander,
    Worker
}

/// <summary>
/// Associates an ArmyType enum value with its ArmyCompositionSO asset.
/// </summary>
[System.Serializable]
public struct ArmyMapping
{
    public ArmyType type;
    public ArmyCompositionSO composition;
}

/// <summary>
/// Responsible for spawning units based on ArmyCompositionSO assets.
/// Supports both instant (debug) spawning and coroutine-driven, interval-based spawning by ArmyType.
/// </summary>
public class ArmyManager : MonoBehaviour
{
    [Header("Army Mappings")]
    [Tooltip("Map each ArmyType to its ArmyCompositionSO asset.")]
    [SerializeField] private List<ArmyMapping> _armyMappings = new List<ArmyMapping>();

    // Runtime lookup from ArmyType to SO
    private Dictionary<ArmyType, ArmyCompositionSO> _compositionLookup;

    // Injected dependencies
    private GridManager _gridManager;
    private UnitManager _unitManager;
    private AStarPathfinder _pathfinder;

    private void Awake()
    {
        _compositionLookup = new Dictionary<ArmyType, ArmyCompositionSO>();
        foreach (var mapping in _armyMappings)
        {
            if (!_compositionLookup.ContainsKey(mapping.type))
                _compositionLookup.Add(mapping.type, mapping.composition);
            else
                Debug.LogWarning($"ArmyManager: Duplicate mapping for {mapping.type}");
        }
        // DEBUG: what got loaded
        Debug.Log($"[ArmyManager] Loaded mappings: {string.Join(", ", _compositionLookup.Keys)}");
    }

    /// <summary>
    /// Initializes the ArmyManager with required dependencies.
    /// Must be called before any spawn methods.
    /// </summary>
    public void Initialize(GridManager gridManager, UnitManager unitManager)
    {
        _gridManager = gridManager;
        _unitManager = unitManager;

        if (_gridManager == null)
            Debug.LogError("ArmyManager: GridManager reference is null.");
        if (_unitManager == null)
            Debug.LogError("ArmyManager: UnitManager reference is null.");

        _pathfinder = new AStarPathfinder(_gridManager);
    }

    /// <summary>
    /// Instantly spawns all units of the given ArmyType at spawnPosition. Useful for debugging.
    /// </summary>
    public void SpawnArmyByTypeInstantly(ArmyType type, Team team, Vector3 spawnPosition)
    {
        if (!_compositionLookup.TryGetValue(type, out var composition) || composition == null)
        {
            Debug.LogError($"ArmyManager: No composition registered for {type}");
            return;
        }
        SpawnArmyAtPosition(composition, team, spawnPosition);
    }

    /// <summary>
    /// Spawns units of the given ArmyType one by one, waiting 'delay' seconds between each instantiation.
    /// </summary>
    public void SpawnArmyByType(ArmyType type, Team team, Vector3 spawnPosition, float delay)
    {
        if (!_compositionLookup.TryGetValue(type, out var composition) || composition == null)
        {
            Debug.LogError($"ArmyManager: No composition registered for {type}");
            return;
        }
        // DEBUG: spawning start
        Debug.Log($"[ArmyManager] SpawnArmyByType({type}) called. totalCount={GetCompositionCount(type)}, delay={delay}");
        StartCoroutine(SpawnArmyCoroutine(composition, team, spawnPosition, delay));
    }

    /// <summary>
    /// Synchronously instantiates all units defined in the composition at the given world-space position.
    /// </summary>
    public void SpawnArmyAtPosition(ArmyCompositionSO composition, Team team, Vector3 spawnPosition)
    {
        if (_gridManager == null || _unitManager == null)
        {
            Debug.LogError("ArmyManager: Must call Initialize() before spawning.");
            return;
        }

        foreach (var entry in composition.unitEntries)
        {
            var unitStats = entry.unitTypePrefab.unitType;
            var prefab = entry.unitTypePrefab.unitPrefab;

            if (unitStats == null || prefab == null)
            {
                Debug.LogWarning($"ArmyManager: Missing stats or prefab in '{composition.armyName}'.");
                continue;
            }

            for (int i = 0; i < entry.count; i++)
            {
                var unitGO = Instantiate(prefab, spawnPosition, Quaternion.identity);
                var unitComp = unitGO.GetComponent<UnitBase>();
                if (unitComp != null)
                {
                    _unitManager.RegisterUnit(unitComp);
                    unitComp.Initialize(unitStats, _gridManager, _unitManager, _pathfinder, team);
                }
                else
                {
                    Debug.LogWarning($"ArmyManager: '{unitGO.name}' missing UnitBase component.");
                    Destroy(unitGO);
                }
            }
        }
    }

    /// <summary>
    /// Coroutine that spawns each unit in the composition one at a time, waiting 'delay' seconds between spawns.
    /// </summary>
    private IEnumerator SpawnArmyCoroutine(
    ArmyCompositionSO composition,
    Team team,
    Vector3 spawnPosition,
    float delay)
    {
        Debug.Log($"[ArmyManager] Coroutine start for '{composition.armyName}'");
        foreach (var entry in composition.unitEntries)
        {
            for (int i = 0; i < entry.count; i++)
            {
                Debug.Log($"[ArmyManager] Instantiating {entry.unitTypePrefab.unitType.name} #{i + 1}/{entry.count}");
                var unitGO = Instantiate(entry.unitTypePrefab.unitPrefab, spawnPosition, Quaternion.identity);
                var unitComp = unitGO.GetComponent<UnitBase>();
                if (unitComp != null)
                {
                    _unitManager.RegisterUnit(unitComp);
                    unitComp.Initialize(entry.unitTypePrefab.unitType, _gridManager, _unitManager, _pathfinder, team);
                }
                else
                {
                    Debug.LogWarning($"ArmyManager: '{unitGO.name}' missing UnitBase component.");
                    Destroy(unitGO);
                }
                yield return new WaitForSeconds(delay);
            }
        }
        Debug.Log($"[ArmyManager] Coroutine end for '{composition.armyName}'");
    }

    /// <summary>
    /// Returns the total number of units defined in the given ArmyType composition.
    /// </summary>
    public int GetCompositionCount(ArmyType type)
    {
        if (!_compositionLookup.TryGetValue(type, out var composition) || composition == null)
            return 0;

        int total = 0;
        foreach (var entry in composition.unitEntries)
            total += entry.count;
        return total;
    }
}
