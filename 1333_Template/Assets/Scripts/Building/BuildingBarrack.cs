// BuildingBarrack.cs
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Barrack building that periodically spawns units defined in an ArmyCompositionSO.
/// </summary>
public class BuildingBarrack : BuildingBase
{
    [Header("Spawn Settings")]
    [Tooltip("Army composition asset to spawn each interval.")]
    [SerializeField] private Transform SpawnPoint;
    [Tooltip("Time in seconds between each spawn wave.")]
    [SerializeField] private float _spawnInterval = 1f;

    [Header("Renderers for Selection")]
    [Tooltip("Assign specific mesh renderers to tint on selection")]
    [SerializeField] private Renderer[] _selectionRenderers;

    private ArmyManager _armyManager;
    private ResourceManager _resourceManager;
    private float _spawnTimer;

    /// <summary>
    /// Injects the ArmyManager. Called by BuildingPlacementManager on placement.
    /// </summary>
    public void Initialize(ArmyManager armyManager, ResourceManager resourceManager)
    {
        _armyManager = armyManager;
        _resourceManager = resourceManager;
    }


    public override void OnSelected()
    {
        // Change only specified mesh renderers to blue
        foreach (var r in _selectionRenderers)
        {
            if (r != null)
                r.material.color = Color.gray;
        }
    }

    public override void OnDeselected()
    {
        // Revert to original team material
        ApplyTeamMaterial();
    }
}
