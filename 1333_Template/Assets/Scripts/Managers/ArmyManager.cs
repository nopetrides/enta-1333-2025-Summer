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
/// Supports interval-based spawning by ArmyType, and will distribute units
/// across nearby grid nodes to avoid overlap.
/// </summary>
public class ArmyManager : MonoBehaviour
{
    [Header("Army Mappings")]
    [Tooltip("Map each ArmyType to its ArmyCompositionSO asset.")]
    [SerializeField] private List<ArmyMapping> _armyMappings = new List<ArmyMapping>();

    // Runtime lookup from ArmyType to its composition asset
    private Dictionary<ArmyType, ArmyCompositionSO> _compositionLookup;

    // Injected dependencies
    private GridManager _gridManager;
    private UnitManager _unitManager;
    private AStarPathfinder _pathfinder;

    public event System.Action<UnitBase> UnitSpawned; // Unit spawn event

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
    }

    /// <summary>
    /// Initializes the ArmyManager with required managers.
    /// Must be called before any spawn methods.
    /// </summary>
    public void Initialize(GridManager gridManager, UnitManager unitManager)
    {
        _gridManager = gridManager;
        _unitManager = unitManager;
        _pathfinder = new AStarPathfinder(_gridManager);
    }

    /// <summary>
    /// Spawns units of the given ArmyType one by one, waiting 'delay' seconds between spawns,
    /// and positions each unit at the nearest free grid node around spawnPosition.
    /// </summary>
    /// <param name="type">Which ArmyType to spawn.</param>
    /// <param name="team">Team affiliation.</param>
    /// <param name="spawnPosition">Center world-space point for distribution.</param>
    /// <param name="delay">Seconds to wait between each unit spawn.</param>
    public void SpawnArmyByType(ArmyType type, Team team, Vector3 spawnPosition, float delay)
    {
        if (!_compositionLookup.TryGetValue(type, out var composition) || composition == null)
        {
            Debug.LogError($"ArmyManager: No composition registered for {type}");
            return;
        }
        StartCoroutine(SpawnArmyCoroutine(type, composition, team, spawnPosition, delay));
    }

    /// <summary>
    /// Coroutine that actually instantiates each unit, distributes them across free grid nodes,
    /// and registers/initializes them with the pathfinder.
    /// </summary>
    private IEnumerator SpawnArmyCoroutine(
        ArmyType type,
        ArmyCompositionSO composition,
        Team team,
        Vector3 spawnPosition,
        float delay)
    {
        // Determine how many units in total we need to spawn
        int totalCount = GetCompositionCount(type);

        // Find the grid node at the barrack’s spawn point
        GridNode centerNode = _gridManager.getNodeFromWorldPosition(spawnPosition);

        // Get up to totalCount nearest free nodes (walkable & not reserved)
        List<GridNode> spawnNodes = _gridManager.FindNearestFreeNodes(centerNode, totalCount);

        Debug.Log($"[ArmyManager] Distributing {totalCount} units around {centerNode.name}");

        int index = 0;
        // Loop through each entry in the composition
        foreach (var entry in composition.unitEntries)
        {
            for (int i = 0; i < entry.count; i++)
            {
                // Choose either the next free node or fallback to the center
                Vector3 pos = (index < spawnNodes.Count)
                    ? spawnNodes[index].worldPosition
                    : spawnPosition;

                // Instantiate the unit prefab at the chosen position
                GameObject unitGO = Instantiate(
                    entry.unitTypePrefab.unitPrefab,
                    pos,
                    Quaternion.identity);

                // Register and initialize the new unit
                UnitBase unitComp = unitGO.GetComponent<UnitBase>();
                UnitSpawned?.Invoke(unitComp);              // Invoke unit spawned event
                if (unitComp != null)
                {
                    _unitManager.RegisterUnit(unitComp);
                    unitComp.Initialize(
                        entry.unitTypePrefab.unitType,
                        _gridManager,
                        _unitManager,
                        _pathfinder,
                        team);
                }
                else
                {
                    Debug.LogWarning($"ArmyManager: '{unitGO.name}' missing UnitBase component.");
                    Destroy(unitGO);
                }

                index++;
                yield return new WaitForSeconds(delay);
            }
        }

        Debug.Log($"[ArmyManager] Finished spawning wave of {type}");
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

