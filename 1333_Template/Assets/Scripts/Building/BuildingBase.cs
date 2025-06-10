using UnityEngine;

/// <summary>
/// Abstract base class for all buildings: placement, occupancy, team visuals, and health.
/// Handles rotated grid alignment without mutating the building data.
/// </summary>
[RequireComponent(typeof(Renderer))]
public abstract class BuildingBase : MonoBehaviour, ISelectable
{
    public Team team;                    // Team affiliation for this building
    public Material[] teamMaterials;     // Materials corresponding to each team
    public BuildingDataSO buildingData;  // ScriptableObject containing building properties

    private Renderer _renderer;         // Cached Renderer component
    protected GridManager _gridManager;  // Reference to the GridManager

    private bool _isPlaced;             // Flag indicating if building is placed
    private int _yRotation;             // Current Y-axis rotation in degrees
    private Quaternion _baseRotation;   // Base rotation from the prefab

    public int CurrentHealth { get; private set; }  // Current health of the building
    public int MaxHealth { get; private set; }      // Maximum health of the building

    protected virtual void Awake()
    {
        _renderer = GetComponent<Renderer>();
        InitializeHealth(); // Setup health from buildingData
    }

    /// <summary>
    /// Stores the GridManager reference for placement logic.
    /// </summary>
    public void InitializePlacement(GridManager gridManager)
    {
        _gridManager = gridManager;
    }

    /// <summary>
    /// Stores the prefab's original rotation for correct combined rotation application.
    /// </summary>
    public void SetBaseRotation(Quaternion baseRotation)
    {
        _baseRotation = baseRotation;
    }

    /// <summary>
    /// Sets the Y-axis rotation and applies it combined with the base rotation.
    /// </summary>
    public void SetRotation(int yRotation)
    {
        _yRotation = yRotation % 360;
        // Preserve original X/Z, add to Y
        Vector3 baseEuler = _baseRotation.eulerAngles;
        transform.rotation = Quaternion.Euler(baseEuler.x, baseEuler.y + _yRotation, baseEuler.z);
    }

    /// <summary>
    /// Determines if the building can be placed at the world position and returns a snapped center position.
    /// </summary>
    public bool CanPlaceAt(Vector3 worldPosition, out Vector3 snapPosition)
    {
        snapPosition = Vector3.zero;
        if (_gridManager == null || _isPlaced)
            return false;

        Vector2Int baseIndices = GetBaseIndices(worldPosition);
        snapPosition = CalculateSnapPosition(baseIndices);
        return IsAreaWalkable(baseIndices);
    }

    /// <summary>
    /// Attempts to place the building: snaps position, marks occupancy, sets team and material.
    /// </summary>
    public bool TryPlaceAt(Vector3 worldPosition, Team teamToAssign)
    {
        if (_isPlaced || _gridManager == null)
            return false;

        if (!CanPlaceAt(worldPosition, out Vector3 snapPos))
            return false;

        transform.position = snapPos;
        MarkAreaOccupied(worldPosition, false);

        _isPlaced = true;
        team = teamToAssign;
        ApplyTeamMaterial();

        return true;
    }

    /// <summary>
    /// Returns the grid footprint size adjusted for current rotation.
    /// </summary>
    protected Vector2Int GetRotatedSize()
    {
        return (_yRotation % 180 == 0)
            ? new Vector2Int(buildingData.SizeX, buildingData.SizeZ)
            : new Vector2Int(buildingData.SizeZ, buildingData.SizeX);
    }

    private Vector2Int GetBaseIndices(Vector3 worldPosition)
    {
        GridNode node = _gridManager.getNodeFromWorldPosition(worldPosition);
        float size = _gridManager.GridSettings.NodeSize;
        int x = Mathf.RoundToInt(node.worldPosition.x / size);
        int y = Mathf.RoundToInt(node.worldPosition.z / size);
        return new Vector2Int(x, y);
    }

    private Vector3 CalculateSnapPosition(Vector2Int indices)
    {
        float size = _gridManager.GridSettings.NodeSize;
        Vector2Int footprint = GetRotatedSize();

        float width = footprint.x * size;
        float depth = footprint.y * size;

        return new Vector3(
            (indices.x * size) + width * 0.5f - size * 0.5f,
            _gridManager.getNodeFromWorldPosition(transform.position).worldPosition.y,
            (indices.y * size) + depth * 0.5f - size * 0.5f
        );
    }

    private bool IsAreaWalkable(Vector2Int baseIndices)
    {
        Vector2Int footprint = GetRotatedSize();
        for (int dx = 0; dx < footprint.x; dx++)
        {
            for (int dy = 0; dy < footprint.y; dy++)
            {
                GridNode node = _gridManager.GetNode(baseIndices.x + dx, baseIndices.y + dy);
                if (node == null || !node.walkable)
                    return false;
            }
        }
        return true;
    }

    private void MarkAreaOccupied(Vector3 worldPosition, bool walkable)
    {
        Vector2Int baseIndices = GetBaseIndices(worldPosition);
        Vector2Int footprint = GetRotatedSize();

        for (int dx = 0; dx < footprint.x; dx++)
        {
            for (int dy = 0; dy < footprint.y; dy++)
            {
                _gridManager.SetWalkable(baseIndices.x + dx, baseIndices.y + dy, walkable);
            }
        }
    }

    private void ApplyTeamMaterial()
    {
        int index = (int)team;
        if (teamMaterials == null || index < 0 || index >= teamMaterials.Length)
            return;

        _renderer.material = teamMaterials[index];
    }

    private void InitializeHealth()
    {
        MaxHealth = buildingData.Health;
        CurrentHealth = MaxHealth;
    }

    public abstract void OnSelected();
    public abstract void OnDeselected();
}
