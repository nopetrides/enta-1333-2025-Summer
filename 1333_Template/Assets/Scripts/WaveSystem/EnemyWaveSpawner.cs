// EnemyWaveSpawner.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns predefined enemy waves (by ArmyType) through ArmyManager,
/// then orders every spawned unit to march toward the grid-center
/// (or the closest free node if occupied).
/// </summary>
public class EnemyWaveSpawner : MonoBehaviour
{
    // ---------- Inspector ----------
    [Header("References")]
    [SerializeField] private ArmyManager _armyManager = null;    // handles prefab + init
    [SerializeField] private GridManager _gridManager = null;    // path / node queries
    [Tooltip("World-space point used as the first search node for free spawn tiles.")]
    [SerializeField] private Transform _spawnOrigin = null;

    [Header("Wave Types")]
    [SerializeField] private ArmyType _wave1 = ArmyType.Wave1;
    [SerializeField] private ArmyType _wave2 = ArmyType.Wave2;
    [SerializeField] private ArmyType _wave3 = ArmyType.Wave3;
    [SerializeField] private ArmyType _wave4 = ArmyType.Wave4;
    [SerializeField] private ArmyType _wave5 = ArmyType.Wave5;

    [Header("Spawn Settings")]
    [Tooltip("Seconds between individual unit spawns.")]
    [SerializeField] private float _unitSpawnDelay = 0.25f;

    // ---------- Runtime ----------
    private readonly List<ArmyType> _waves = new();
    private int _currentWaveIdx = -1;

    /* ===================================================================== */
    /*  Life-cycle                                                           */
    /* ===================================================================== */

    private void Awake()
    {
        if (_armyManager == null || _gridManager == null || _spawnOrigin == null)
        {
            Debug.LogError("[EnemyWaveSpawner] Missing references – disabled.");
            enabled = false;
            return;
        }

        // Build internal wave list (ignore “None” entries)
        _waves.AddRange(new[] { _wave1, _wave2, _wave3, _wave4, _wave5 });
        _waves.RemoveAll(t => t == default);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            StartWaveByIndex(0);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            StartWaveByIndex(1);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            StartWaveByIndex(2);
        if (Input.GetKeyDown(KeyCode.Alpha4))
            StartWaveByIndex(3);
        if (Input.GetKeyDown(KeyCode.Alpha5))
            StartWaveByIndex(4);
    }

    /* ===================================================================== */
    /*  Public API                                                           */
    /* ===================================================================== */

    /// <summary>
    /// Starts the next configured wave (returns false if no waves remain).
    /// </summary>
    public bool StartNextWave()
    {
        _currentWaveIdx++;
        if (_currentWaveIdx >= _waves.Count)
            return false;

        StartCoroutine(SpawnWaveRoutine(_waves[_currentWaveIdx]));
        return true;
    }

    /// <summary>
    /// Optionally trigger a specific wave index (0-based).
    /// Returns false if the index is invalid or already spawned.
    /// </summary>
    public bool StartWaveByIndex(int index)
    {
        if (index < 0 || index >= _waves.Count || index <= _currentWaveIdx)
            return false;

        _currentWaveIdx = index;
        StartCoroutine(SpawnWaveRoutine(_waves[_currentWaveIdx]));
        return true;
    }

    /* ===================================================================== */
    /*  Internal Coroutines                                                  */
    /* ===================================================================== */

    /// <summary>
    /// Spawns one wave and commands every unit to march toward the grid center.
    /// </summary>
    private IEnumerator SpawnWaveRoutine(ArmyType waveType)
    {
        var spawned = new List<UnitBase>();

        // Spawn through ArmyManager and collect references
        yield return StartCoroutine(
            _armyManager.SpawnArmyAndCollect(
                waveType,
                Team.Enemy,
                _spawnOrigin.position,
                _unitSpawnDelay,
                spawned));                                               

        // ------------------------------------------------------------------
        // Compute center (player castle) and assign destinations
        // ------------------------------------------------------------------
        Vector2Int centerIdx = new(
            _gridManager.GridSettings.GridSizeX / 2,
            _gridManager.GridSettings.GridSizeY / 2);
        GridNode centerNode = _gridManager.GetNode(centerIdx.x, centerIdx.y);

        List<GridNode> targets = _gridManager.FindNearestFreeNodes(
            centerNode,
            spawned.Count);                                             

        for (int i = 0; i < spawned.Count; i++)
        {
            UnitBase u = spawned[i];
            GridNode dest = i < targets.Count ? targets[i] : centerNode;

            u.SetReservedDestination(dest);
            u.MoveTo(dest);                                             
        }

        Debug.Log($"[EnemyWaveSpawner] Wave #{_currentWaveIdx + 1} ({waveType}) – " +
                  $"{spawned.Count} units marching to center.");
    }
}
