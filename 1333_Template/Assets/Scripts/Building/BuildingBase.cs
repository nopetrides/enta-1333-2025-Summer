using UnityEngine;

/// <summary>
/// Abstract base class for all buildings.
/// Manages team, health, material, and selection logic.
/// Does not handle placement or grid logic.
/// </summary>
[RequireComponent(typeof(Renderer))]
public abstract class BuildingBase : MonoBehaviour, ISelectable
{
    public Team team { get; private set; }
    public Material[] teamMaterials;
    public BuildingDataSO buildingData;

    private Renderer _renderer;
    private Vector3 _baseEulerAngles;

    public int CurrentHealth { get; private set; }
    public int MaxHealth { get; private set; }

    protected virtual void Awake()
    {
        _renderer = GetComponent<Renderer>();
    }

    /// <summary>
    /// Initializes building state after placement.
    /// </summary>
    /// <param name="data">Building data ScriptableObject.</param>
    /// <param name="team">Team to assign.</param>
    public virtual void Initialize(BuildingDataSO data, Team team)
    {
        buildingData = data;
        this.team = team;
        MaxHealth = data.Health;
        CurrentHealth = MaxHealth;
        ApplyTeamMaterial();
    }

    /// <summary>
    /// Changes the team and updates visuals.
    /// </summary>
    /// <param name="team">Team to assign.</param>
    public void SetTeam(Team team)
    {
        this.team = team;
        ApplyTeamMaterial();
    }

    /// <summary>
    /// Applies the team material to the building renderer.
    /// </summary>
    private void ApplyTeamMaterial()
    {
        int index = (int)team;
        if (teamMaterials != null && index >= 0 && index < teamMaterials.Length)
            _renderer.material = teamMaterials[index];
    }

    public void StoreBaseRotation(Quaternion baseRotation)
    {
        _baseEulerAngles = baseRotation.eulerAngles;
    }

    public Vector3 GetBaseEulerAngles()
    {
        return _baseEulerAngles;
    }

    /// <summary>
    /// Receives damage, updates health, destroys on zero.
    /// </summary>
    /// <param name="amount">Damage amount.</param>
    public void TakeDamage(int amount)
    {
        CurrentHealth -= amount;
        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            Destroy(gameObject);
        }
    }

    public abstract void OnSelected();
    public abstract void OnDeselected();
}
