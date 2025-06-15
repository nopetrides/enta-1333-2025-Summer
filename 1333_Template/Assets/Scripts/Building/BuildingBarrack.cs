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

    [Header("Renderers for Selection")]
    [Tooltip("Assign specific mesh renderers to tint on selection.")]
    [SerializeField] private Renderer[] _selectionRenderers;

    private ArmyManager _armyManager;
    private ResourceManager _resourceManager;

    /// <summary>
    /// Injects the ArmyManager and ResourceManager. Called by BuildingPlacementManager after placement.
    /// </summary>
    public void Initialize(ArmyManager armyManager, ResourceManager resourceManager)
    {
        _armyManager = armyManager;
        _resourceManager = resourceManager;
    }

    private void Update()
    {
        // When the player presses N, spawn a new wave
        if (Input.GetKeyDown(KeyCode.N))
        {
            SpawnWave();
        }
    }

    /// <summary>
    /// Calls ArmyManager to spawn this building’s army type with intervaled spawns.
    /// </summary>
    private void SpawnWave()
    {
        if (_armyManager == null || _spawnPoint == null)
            return;

        _armyManager.SpawnArmyByType(
            ArmyType.Archer,        // Hard coded need to change with UI
            team,
            _spawnPoint.position,
            _spawnInterval
        );
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
