// BuildingBarrack.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Barrack building that spawns a configured ArmyType at its spawn point when the N key is pressed.
/// After spawning, units form around a Banner if present; otherwise around the spawn point.
/// When the banner moves, any non-selected spawned units will re-form around the banner.
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
    private readonly List<UnitBase> _spawnedUnits = new List<UnitBase>();

    /// <summary>
    /// Injects the ArmyManager, ResourceManager, and GridManager.
    /// Called by BuildingPlacementManager after placement.
    /// Also subscribes to banner movement events.
    /// </summary>
    public void Initialize(ArmyManager armyManager, ResourceManager resourceManager, GridManager gridManager)
    {
        _armyManager = armyManager;
        _resourceManager = resourceManager;
        _gridManager = gridManager;
        Banner.BannerMoved += OnBannerMoved;
    }

    /// <summary>
    /// Public entry point for UI to spawn a single wave of units.
    /// </summary>
    public void SpawnUnit()
    {
        StartCoroutine(SpawnAndFormWave());
    }

    private IEnumerator SpawnAndFormWave()
    {
        if (_armyManager == null || _spawnPoint == null || _gridManager == null)
            yield break;

        // 1) Spawn units into a list
        List<UnitBase> spawned = new List<UnitBase>();
        yield return StartCoroutine(
            _armyManager.SpawnArmyAndCollect(
                ArmyType.Archer,
                team,
                _spawnPoint.position,
                _spawnInterval,
                spawned));

        // 2) Wait before initial formation
        yield return new WaitForSeconds(_formationDelay);

        // 3) Cache spawned units and issue formation
        _spawnedUnits.Clear();
        _spawnedUnits.AddRange(spawned);
        IssueFormationOrders();
    }

    /// <summary>
    /// Computes formation nodes around the banner (or spawn point),
    /// then issues MoveTo for each spawned unit that is not currently selected.
    /// </summary>
    private void IssueFormationOrders()
    {
        // Always use banner if it exists
        Banner banner = Object.FindAnyObjectByType<Banner>();
        Vector3 centerPos = (banner != null) ? banner.transform.position : _spawnPoint.position;

        GridNode centerNode = _gridManager.getNodeFromWorldPosition(centerPos);
        List<GridNode> nodes = _gridManager.FindNearestFreeNodes(centerNode, _spawnedUnits.Count);

        for (int i = 0; i < _spawnedUnits.Count; i++)
        {
            UnitBase unit = _spawnedUnits[i];
            if (unit.IsSelected)
                continue; // skip units currently selected by player

            GridNode target = (i < nodes.Count) ? nodes[i] : centerNode;
            unit.SetReservedDestination(target);
            unit.MoveTo(target);
        }
    }

    private void OnBannerMoved(Vector3 newPosition)
    {
        IssueFormationOrders();
    }

    private void OnDestroy()
    {
        Banner.BannerMoved -= OnBannerMoved;
    }

    /// <inheritdoc/>
    public override void OnSelected()
    {
        foreach (Renderer r in _selectionRenderers)
            if (r != null)
                r.material.color = Color.gray;
    }

    /// <inheritdoc/>
    public override void OnDeselected()
    {
        ApplyTeamMaterial();
    }
}
