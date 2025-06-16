using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Barrack building that spawns a configured ArmyType at its spawn point when the N key is pressed.
/// </summary>
public class BuildingBarrack : BuildingBase
{
    [Header("Spawn Settings")]
    [Tooltip("Transform indicating where units should appear.")]
    [SerializeField] private Transform _spawnPoint = null;
    [Tooltip("Seconds between each individual unit spawn.")]
    [SerializeField] private float _spawnInterval = 1f;

    [Header("Formation Settings")]
    [Tooltip("Transform indicating the center of the formation.")]
    [SerializeField] private Transform _formationPoint = null;
    [Tooltip("Seconds to wait after last spawn before ordering formation.")]
    [SerializeField] private float _formationDelay = 0.5f;

    [Header("Renderers for Selection")]
    [Tooltip("Assign specific mesh renderers to tint on selection.")]
    [SerializeField] private Renderer[] _selectionRenderers;

    private ArmyManager _armyManager;
    private ResourceManager _resourceManager;
    private GridManager _gridManager;

    /// <summary>
    /// Injects the ArmyManager and ResourceManager. Called by BuildingPlacementManager after placement.
    /// </summary>
    public void Initialize(ArmyManager armyManager, ResourceManager resourceManager, GridManager gridManager)
    {
        _armyManager = armyManager;
        _resourceManager = resourceManager;
        _gridManager = gridManager; 
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            StartCoroutine(SpawnAndFormWave());
        }
    }

    /// <summary>
    /// Spawns a wave of units, collects them into a list, waits for all to appear,
    /// then orders them into a distributed formation around the formation point.
    /// </summary>
    private IEnumerator SpawnAndFormWave()
    {
        if (_armyManager == null || _spawnPoint == null || _formationPoint == null || _gridManager == null)
            yield break;

        // Determine how many units will spawn
        int totalUnits = _armyManager.GetCompositionCount(ArmyType.Archer);

        // Temporary list to track spawned units
        List<UnitBase> spawnedUnits = new List<UnitBase>();

        // Subscribe to spawn event
        void OnUnitSpawned(UnitBase unit)
        {
            spawnedUnits.Add(unit);
        }

        _armyManager.UnitSpawned += OnUnitSpawned;

        // Trigger the spawn coroutine (with callback)
        _armyManager.SpawnArmyByType(
            ArmyType.Archer,
            team,
            _spawnPoint.position,
            _spawnInterval
        );

        // Wait until all units are spawned
        float timeout = totalUnits * _spawnInterval + 1f;
        float elapsed = 0f;
        while (spawnedUnits.Count < totalUnits && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Unsubscribe from event
        _armyManager.UnitSpawned -= OnUnitSpawned;

        // Small delay before issuing move orders
        yield return new WaitForSeconds(_formationDelay);

        // Convert formation center to grid node
        GridNode center = _gridManager.getNodeFromWorldPosition(_formationPoint.position);

        // Find nearest free nodes for each spawned unit
        List<GridNode> formationNodes = _gridManager.FindNearestFreeNodes(center, spawnedUnits.Count);

        // Order each unit into its formation slot
        for (int i = 0; i < spawnedUnits.Count; i++)
        {
            UnitBase u = spawnedUnits[i];
            GridNode target = i < formationNodes.Count ? formationNodes[i] : center;

            u.SetReservedDestination(target);  // store the reservation target on the unit
            u.MoveTo(target);                  // run A*, target must still be free
        }
    }

    public override void OnSelected()
    {
        foreach (var r in _selectionRenderers)
            if (r != null)
                r.material.color = Color.gray;
    }

    public override void OnDeselected()
    {
        ApplyTeamMaterial();
    }
}
