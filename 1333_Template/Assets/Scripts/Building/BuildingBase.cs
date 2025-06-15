using UnityEngine;

/// <summary>
/// Abstract base class for all buildings: handles team visuals and health.
/// </summary>
[RequireComponent(typeof(Renderer))]
public abstract class BuildingBase : MonoBehaviour, ISelectable
{
    public Team team;                    // Team affiliation for this building
    public Material[] teamMaterials;     // Materials corresponding to each team
    public BuildingDataSO buildingData;  // Data object containing building properties

    protected Renderer _renderer;          // Cached Renderer component

    /// <summary>
    /// Current health of the building.
    /// </summary>
    public int CurrentHealth { get; private set; }

    /// <summary>
    /// Maximum health of the building.
    /// </summary>
    public int MaxHealth { get; private set; }

    /// <summary>
    /// Initialize renderer and health.
    /// </summary>
    protected virtual void Awake()
    {
        _renderer = GetComponent<Renderer>();
        InitializeHealth();
    }

    /// <summary>
    /// Sets up health based on buildingData.
    /// </summary>
    private void InitializeHealth()
    {
        MaxHealth = buildingData.Health;
        CurrentHealth = MaxHealth;
    }

    /// <summary>
    /// Applies the material corresponding to the building's team.
    /// Made virtual to allow subclasses (e.g., gates) to override.
    /// </summary>
    public virtual void ApplyTeamMaterial()
    {
        int index = (int)team;
        if (teamMaterials == null || index < 0 || index >= teamMaterials.Length)
            return;

        _renderer.material = teamMaterials[index];
    }

    public abstract void OnSelected();
    public abstract void OnDeselected();
}