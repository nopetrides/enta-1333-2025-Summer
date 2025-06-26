using UnityEngine;

/// <summary>
/// Abstract base class for all buildings: handles team visuals, health,
/// damage interface, and grid footprint.
/// Implements IDamageable so units can attack it.
/// </summary>
[RequireComponent(typeof(Renderer))]
public abstract class BuildingBase : MonoBehaviour, ISelectable, IDamageable
{
    [Header("Team & Data")]
    public Team team;                       // team affiliation
    public Material[] teamMaterials;        // material per team index
    public BuildingDataSO buildingData;     // stats / footprint data

    // Cached refs
    protected Renderer _renderer;
    protected GridManager _gridManager;
    protected UnitManager _unitManager;

    // Footprint (= size on grid after rotation)
    protected Vector2Int _baseIdx;          // bottom-left grid index
    protected Vector2Int _footprint;        // width / height in tiles

    // Health
    public int CurrentHealth { get; private set; }
    public int MaxHealth { get; private set; }

    private HealthBarUI _hpBar;

    // IDamageable implementation
    public Team Team => team;
    public bool IsAlive => CurrentHealth > 0;
    public Transform Tr => transform;

    // ---------- MonoBehaviour --------------------------------
    protected virtual void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _hpBar = GetComponentInChildren<HealthBarUI>(true);
        InitializeHealth();
    }

    private void OnEnable()
    {
        // Auto-register to UnitManager list
        if (_unitManager != null)
            _unitManager.RegisterBuilding(this);
    }

    private void OnDisable()
    {
        // In case DestroySelf() wasn't called
        if (_unitManager != null)
            _unitManager.UnregisterBuilding(this);
    }

    // ---------- Health / Damage ------------------------------
    public void TakeDamage(int amount)
    {
        if (!IsAlive) return;

        CurrentHealth -= amount;
        _hpBar?.SetRatio(CurrentHealth / (float)MaxHealth);   // update health bar

        if (CurrentHealth <= 0)
            DestroySelf();                 // delegate to custom destroy
    }

    private void InitializeHealth()
    {
        MaxHealth = buildingData.Health;
        CurrentHealth = MaxHealth;
    }

    // ---------- Grid & Placement -----------------------------
    /// <summary>
    /// Called right after the building is spawned/placed.
    /// </summary>
    public void SetupPlacement(GridManager gm,
                               Vector2Int baseIdx,
                               Vector2Int footprint,
                               UnitManager um)
    {
        _gridManager = gm;
        _baseIdx = baseIdx;
        _footprint = footprint;
        _unitManager = um;

        // register once placed
        _unitManager.RegisterBuilding(this);
    }

    /// <summary>
    /// Returns the closest point on the building's footprint perimeter
    /// so units stop at the correct attack distance instead of the pivot.
    /// Uses renderer.bounds as fallback when grid info is missing.
    /// </summary>
    public Vector3 GetClosestEdgePoint(Vector3 from)
    {
        if (this == null || this.Equals(null)) return from;

        if (_gridManager != null && _gridManager.isInitialized)
        {
            float size = _gridManager.GridSettings.NodeSize;
            bool xz = _gridManager.GridSettings.UseXZPlane;

            // bottom-left world position of footprint
            Vector3 min = _gridManager.IdxToWorld(_baseIdx, false);

            // proper top-right outer edge (no extra padding)
            Vector3 max = _gridManager.IdxToWorld(_baseIdx + _footprint, false);

            // clamp
            if (xz)
            {
                float cx = Mathf.Clamp(from.x, min.x, max.x);
                float cz = Mathf.Clamp(from.z, min.z, max.z);
                return new Vector3(cx, transform.position.y, cz);
            }
            else
            {
                float cx = Mathf.Clamp(from.x, min.x, max.x);
                float cy = Mathf.Clamp(from.y, min.y, max.y);
                return new Vector3(cx, cy, transform.position.z);
            }
        }

        // fallback
        Bounds b = _renderer.bounds;
        Vector3 p = b.ClosestPoint(from);
        p.y = transform.position.y;
        return p;
    }




    // ---------- Visuals --------------------------------------
    public virtual void ApplyTeamMaterial()
    {
        int idx = (int)team;
        if (teamMaterials != null && idx >= 0 && idx < teamMaterials.Length)
            _renderer.material = teamMaterials[idx];
    }

    // ---------- Destruction ----------------------------------
    public virtual void DestroySelf()
    {
        // free tiles
        if (_gridManager != null)
        {
            for (int dx = 0; dx < _footprint.x; dx++)
                for (int dy = 0; dy < _footprint.y; dy++)
                    _gridManager.SetWalkable(_baseIdx.x + dx,
                                             _baseIdx.y + dy, true);
        }

        if (_unitManager != null)
            _unitManager.UnregisterBuilding(this);

        Destroy(gameObject);
    }

    // ---------- Selection (to be implemented by subclasses) --
    public abstract void OnSelected();
    public abstract void OnDeselected();
}
