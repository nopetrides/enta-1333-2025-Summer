// BuildingBarrack.cs
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Barrack building that spawns a configured ArmyType at its spawn point when the N key is pressed.
/// After spawning, units form around a Banner if present; otherwise around the spawn point.
/// </summary>
public class BuildingBarrack : BuildingBase
{
    [Header("Spawn Settings")]
    [Tooltip("Transform indicating where units should appear.")]
    [SerializeField] private Transform _spawnPoint = null;
    [Tooltip("Seconds between each individual unit spawn.")]
    [SerializeField] private float _spawnInterval = 1f;

    [Header("Formation Settings")]
    [Tooltip("Seconds to wait after last spawn before ordering formation.")]
    [SerializeField] private float _formationDelay = 0.5f;

    [Header("Renderers for Selection")]
    [Tooltip("Assign specific mesh renderers to tint on selection.")]
    [SerializeField] private Renderer[] _selectionRenderers;

    private ArmyManager _armyManager;
    private ResourceManager _resourceManager;
    private GridManager _gridManager;


    // Injected selection UI
    private BarrackSelectedUI _selectionUI;

    /// <summary>
    /// Injects the ArmyManager, ResourceManager, and GridManager. Called by BuildingPlacementManager after placement.
    /// </summary>
    public void Initialize(ArmyManager armyManager, ResourceManager resourceManager, GridManager gridManager, BarrackSelectedUI ui)
    {
        _armyManager = armyManager;
        _resourceManager = resourceManager;
        _gridManager = gridManager;

        _selectionUI = ui;
        _selectionUI.Hide();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            StartCoroutine(SpawnAndFormWave());
        }
    }

    /// <summary>
    /// Public entry point for UI to spawn a single wave unit.
    /// </summary>
    public void SpawnUnit()
    {
        StartCoroutine(SpawnAndFormWave());
    }

    /// <summary>
    /// Spawns a wave of units, waits for them to appear, then issues move orders
    /// around a Banner if present, otherwise around the spawn point.
    /// </summary>
    private IEnumerator SpawnAndFormWave()
    {
        if (_armyManager == null || _spawnPoint == null || _gridManager == null)
            yield break;

        // Determine how many units will spawn
        int totalUnits = _armyManager.GetCompositionCount(ArmyType.Archer);

        // Track spawned units
        List<UnitBase> spawnedUnits = new List<UnitBase>();

        // Subscribe to spawn event
        void OnUnitSpawned(UnitBase unit)
        {
            spawnedUnits.Add(unit);
        }
        _armyManager.UnitSpawned += OnUnitSpawned;

        // Trigger spawn
        _armyManager.SpawnArmyByType(
            ArmyType.Archer,
            team,
            _spawnPoint.position,
            _spawnInterval
        );

        // Wait for all units or timeout
        float timeout = totalUnits * _spawnInterval + 1f;
        float elapsed = 0f;
        while (spawnedUnits.Count < totalUnits && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        _armyManager.UnitSpawned -= OnUnitSpawned;

        // Delay before formation
        yield return new WaitForSeconds(_formationDelay);

        // Determine formation center: Banner if exists, else spawn point
        Vector3 formationPos;
        Banner banner = Object.FindAnyObjectByType<Banner>(); 
        if (banner != null)
            formationPos = banner.transform.position;
        else
            formationPos = _spawnPoint.position;

        // Convert to grid node
        GridNode centerNode = _gridManager.getNodeFromWorldPosition(formationPos);

        // Find free nodes around center
        List<GridNode> formationNodes = _gridManager.FindNearestFreeNodes(centerNode, spawnedUnits.Count);

        // Issue move orders
        for (int i = 0; i < spawnedUnits.Count; i++)
        {
            UnitBase u = spawnedUnits[i];
            GridNode target = (i < formationNodes.Count) ? formationNodes[i] : centerNode;
            u.SetReservedDestination(target);
            u.MoveTo(target);
        }
    }

    public override void OnSelected()
    {
        foreach (var r in _selectionRenderers)
            if (r != null)
                r.material.color = Color.gray;

        // pass this instance to UI
        _selectionUI.GetBarrackInstance(this);
        // show UI
        _selectionUI?.Show();

    }

    public override void OnDeselected()
    {
        ApplyTeamMaterial();

        // clear Barrack instance in UI
        _selectionUI.ClearBarrackInstance();
        // hide UI
        _selectionUI?.Hide();
    }
}
