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


    /// <summary>
    /// Injects the ArmyManager, ResourceManager, and GridManager. Called by BuildingPlacementManager after placement.
    /// </summary>
    public void Initialize(ArmyManager armyManager, ResourceManager resourceManager, GridManager gridManager)
    {
        _armyManager = armyManager;
        _resourceManager = resourceManager;
        _gridManager = gridManager;
    }

    /// <summary>
    /// Public entry point for UI to spawn a single wave unit.
    /// </summary>
    public void SpawnUnit()
    {
        StartCoroutine(SpawnAndFormWave());
    }

    private IEnumerator SpawnAndFormWave()
    {
        if (_armyManager == null || _spawnPoint == null || _gridManager == null)
            yield break;

        // 1) Prepare list and call the new API
        List<UnitBase> spawnedUnits = new List<UnitBase>();
        yield return StartCoroutine(
            _armyManager.SpawnArmyAndCollect(
                ArmyType.Archer,
                team,
                _spawnPoint.position,
                _spawnInterval,
                spawnedUnits));

        // 2) Wait a moment before issuing formation orders
        yield return new WaitForSeconds(_formationDelay);

        // 3) Decide formation center (Banner if present)
        Vector3 formationPos;
        Banner banner = Object.FindAnyObjectByType<Banner>();
        if (banner != null)
            formationPos = banner.transform.position;
        else
            formationPos = _spawnPoint.position;

        GridNode centerNode = _gridManager.getNodeFromWorldPosition(formationPos);
        List<GridNode> formationNodes =
            _gridManager.FindNearestFreeNodes(centerNode, spawnedUnits.Count);

        // 4) Issue movement to each unit
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
    }

    public override void OnDeselected()
    {
        ApplyTeamMaterial();
    }
}
