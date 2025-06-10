using UnityEngine;

/// <summary>
/// Abstract base class for all buildings, holding building data, handling team-based visuals,
/// grid-snapped placement, occupancy marking, and selection integration.
/// </summary>
[RequireComponent(typeof(Renderer))]
public abstract class BuildingBase : MonoBehaviour, ISelectable
{
    public Team team;
    public Material[] teamMaterials;
    public BuildingDataSO buildingData;

    private Renderer _renderer;
    protected GridManager _gridManager;
    protected bool _isPlaced;

    public int CurrentHealth { get; protected set; }
    public int MaxHealth { get; protected set; }
    public Vector2Int BuildingSize => new Vector2Int(buildingData.SizeX, buildingData.SizeZ);

    protected virtual void Awake()
    {
        _renderer = GetComponent<Renderer>();
        InitializeHealth();
    }

    /// <summary>
    /// Initializes the GridManager reference for placement logic.
    /// </summary>
    public void InitializePlacement(GridManager gridManager)
    {
        _gridManager = gridManager;
    }

    /// <summary>
    /// Checks if placement is valid at the given world position.
    /// Calculates the snapped position without committing to grid occupancy.
    /// </summary>
    /// <param name="worldPosition">Desired world-space position.</param>
    /// <param name="snapPosition">Output snapped center position of the footprint.</param>
    /// <returns>True if placement is valid; false otherwise.</returns>
    public bool CanPlaceAt(Vector3 worldPosition, out Vector3 snapPosition)
    {
        if (_gridManager == null || _isPlaced)
        {
            snapPosition = Vector3.zero;
            return false;
        }

        GridNode baseNode = _gridManager.getNodeFromWorldPosition(worldPosition);
        float nodeSize = _gridManager.GridSettings.NodeSize;
        int baseX = Mathf.RoundToInt(baseNode.worldPosition.x / nodeSize);
        int baseY = Mathf.RoundToInt(baseNode.worldPosition.z / nodeSize);

        // Always calculate snap position first
        float width = BuildingSize.x * nodeSize;
        float depth = BuildingSize.y * nodeSize;
        snapPosition = new Vector3(
            (baseX * nodeSize) + width * 0.5f - nodeSize * 0.5f,
            baseNode.worldPosition.y,
            (baseY * nodeSize) + depth * 0.5f - nodeSize * 0.5f
        );

        // Validate all footprint cells
        for (int dx = 0; dx < BuildingSize.x; dx++)
        {
            for (int dy = 0; dy < BuildingSize.y; dy++)
            {
                GridNode node = _gridManager.GetNode(baseX + dx, baseY + dy);
                if (node == null || !node.walkable)
                    return false;
            }
        }

        return true;
    }


    /// <summary>
    /// Attempts to place the building at the given world position. Uses CanPlaceAt validation.
    /// On success, snaps to grid, marks occupancy, and becomes permanent.
    /// </summary>
    /// <param name="worldPosition">Desired world-space position.</param>
    /// <returns>True if placement succeeded; false otherwise.</returns>
    public bool TryPlaceAt(Vector3 worldPosition, Team teamToAssign)
    {
        if (_isPlaced || _gridManager == null)
            return false;

        if (!CanPlaceAt(worldPosition, out Vector3 snapPos))
            return false;

        transform.position = snapPos;

        // Mark cells as non-walkable
        GridNode baseNode = _gridManager.getNodeFromWorldPosition(worldPosition);
        float nodeSize = _gridManager.GridSettings.NodeSize;
        int baseX = Mathf.RoundToInt(baseNode.worldPosition.x / nodeSize);
        int baseY = Mathf.RoundToInt(baseNode.worldPosition.z / nodeSize);

        for (int dx = 0; dx < BuildingSize.x; dx++)
        {
            for (int dy = 0; dy < BuildingSize.y; dy++)
            {
                _gridManager.SetWalkable(baseX + dx, baseY + dy, false);
            }
        }

        _isPlaced = true;

        // Assign team and apply material
        team = teamToAssign;
        ApplyTeamMaterial();

        return true;
    }

    protected void ApplyTeamMaterial()
    {
        int index = (int)team;
        if (teamMaterials == null || teamMaterials.Length == 0) return;
        if (index < 0 || index >= teamMaterials.Length) return;
        _renderer.material = teamMaterials[index];
    }

    protected void InitializeHealth()
    {
        MaxHealth = buildingData.Health;
        CurrentHealth = MaxHealth;
    }

    public abstract void OnSelected();
    public abstract void OnDeselected();
}
