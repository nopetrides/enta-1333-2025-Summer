using UnityEngine;

/// <summary>
/// Handles building placement: preview, rotation, validation, and actual placement on grid.
/// All placement logic is managed here; BuildingBase does not handle placement.
/// </summary>
public class BuildingPlacementManager : MonoBehaviour
{
    [Header("Grid Reference")]
    [Tooltip("Reference to the GridManager used for placement validation.")]
    [SerializeField] private GridManager _gridManager;

    [Header("Ghost Preview Materials")]
    [Tooltip("Semi-transparent green material for valid placement")]
    [SerializeField] private Material _ghostValidMaterial;
    [Tooltip("Semi-transparent red material for invalid placement")]
    [SerializeField] private Material _ghostInvalidMaterial;

    private Camera _mainCamera;
    private BuildingDataSO _currentBuildingData;
    private BuildingBase _previewInstance;
    private int _currentYRotation = 0;

    private void Awake()
    {
        _mainCamera = Camera.main;
        if (_gridManager == null)
            Debug.LogError("BuildingPlacementManager: GridManager is not assigned.");
        if (_mainCamera == null)
            Debug.LogError("BuildingPlacementManager: MainCamera not found.");
    }

    private void Update()
    {
        if (_previewInstance == null)
            return;

        // Rotate preview on middle mouse click
        if (Input.GetMouseButtonDown(2))
        {
            _currentYRotation = (_currentYRotation + 90) % 360;
            Vector3 baseEuler = _previewInstance.GetBaseEulerAngles();
            _previewInstance.transform.rotation = Quaternion.Euler(
                baseEuler.x,
                baseEuler.y + _currentYRotation,
                baseEuler.z
            );
        }

        // Get world point under mouse
        if (!TryGetMouseWorldPosition(out Vector3 hitPoint))
            return;

        // Validate placement and get snapped position
        bool canPlace = CanPlaceAt(hitPoint, _currentBuildingData, _currentYRotation, out Vector3 snapPos);
        _previewInstance.transform.position = snapPos;

        // Update ghost material color
        Renderer previewRenderer = _previewInstance.GetComponentInChildren<Renderer>();
        previewRenderer.material = canPlace ? _ghostValidMaterial : _ghostInvalidMaterial;

        // Place building on left click
        if (canPlace && Input.GetMouseButtonDown(0))
            PlaceRealBuilding(snapPos);

        // Cancel preview on right click
        if (Input.GetMouseButtonDown(1))
            CancelPlacement();
    }

    /// <summary>
    /// Starts placement preview with the selected building data.
    /// </summary>
    public void StartPlacement(BuildingDataSO buildingData)
    {
        if (_previewInstance != null)
            Destroy(_previewInstance.gameObject);

        _currentBuildingData = buildingData;
        _currentYRotation = 0;

        _previewInstance = CreatePreviewInstance(buildingData);
        _previewInstance.StoreBaseRotation(_previewInstance.transform.rotation);
        Vector3 baseEuler = _previewInstance.GetBaseEulerAngles();
        _previewInstance.transform.rotation = Quaternion.Euler(baseEuler.x, baseEuler.y, baseEuler.z);

        Renderer previewRenderer = _previewInstance.GetComponentInChildren<Renderer>();
        previewRenderer.material = _ghostInvalidMaterial;
    }

    /// <summary>
    /// Finalizes placement of the building at the specified world position.
    /// </summary>
    private void PlaceRealBuilding(Vector3 worldPosition)
    {
        Vector3 baseEuler = _previewInstance.GetBaseEulerAngles();
        Quaternion finalRotation = Quaternion.Euler(
            baseEuler.x,
            baseEuler.y + _currentYRotation,
            baseEuler.z
        );

        GameObject go = Instantiate(
            _currentBuildingData.BuildingPrefab,
            worldPosition,
            finalRotation
        );
        var realInstance = go.GetComponent<BuildingBase>();
        realInstance.Initialize(_currentBuildingData, Team.Player);

        // Occupy grid nodes under the building footprint
        UpdateGridOccupancy(worldPosition, _currentBuildingData, _currentYRotation, false);

        Destroy(_previewInstance.gameObject);
        _previewInstance = null;
    }

    /// <summary>
    /// Cancels the current placement preview.
    /// </summary>
    private void CancelPlacement()
    {
        Destroy(_previewInstance.gameObject);
        _previewInstance = null;
    }

    /// <summary>
    /// Creates and returns a building preview instance.
    /// </summary>
    private BuildingBase CreatePreviewInstance(BuildingDataSO data)
    {
        GameObject go = Instantiate(data.BuildingPrefab);
        var instance = go.GetComponent<BuildingBase>();
        instance.Initialize(data, Team.Player);
        return instance;
    }

    /// <summary>
    /// Validates placement at the world position and calculates snapped position.
    /// </summary>
    private bool CanPlaceAt(Vector3 worldPosition, BuildingDataSO data, int yRotation, out Vector3 snapPosition)
    {
        snapPosition = Vector3.zero;
        if (_gridManager == null)
            return false;

        Vector2Int size = (yRotation % 180 == 0)
            ? new Vector2Int(data.SizeX, data.SizeZ)
            : new Vector2Int(data.SizeZ, data.SizeX);

        Vector2Int baseIndices = GetBaseIndices(worldPosition, size);
        snapPosition = CalculateSnapPosition(baseIndices, size);

        for (int dx = 0; dx < size.x; dx++)
        {
            for (int dz = 0; dz < size.y; dz++)
            {
                GridNode node = _gridManager.GetNode(baseIndices.x + dx, baseIndices.y + dz);
                if (node == null || !node.walkable)
                    return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Marks or frees all grid nodes under this building footprint.
    /// </summary>
    private void UpdateGridOccupancy(Vector3 worldPosition, BuildingDataSO data, int yRotation, bool walkable)
    {
        int width = (yRotation % 180 == 0) ? data.SizeX : data.SizeZ;
        int depth = (yRotation % 180 == 0) ? data.SizeZ : data.SizeX;

        GridNode centerNode = _gridManager.getNodeFromWorldPosition(worldPosition);
        float nodeSize = _gridManager.GridSettings.NodeSize;
        int baseX = Mathf.RoundToInt(centerNode.worldPosition.x / nodeSize) - width / 2;
        int baseY = Mathf.RoundToInt(centerNode.worldPosition.z / nodeSize) - depth / 2;

        for (int dx = 0; dx < width; dx++)
        {
            for (int dz = 0; dz < depth; dz++)
            {
                _gridManager.SetWalkable(baseX + dx, baseY + dz, walkable);
            }
        }
    }

    /// <summary>
    /// Converts a world position to grid indices given the building footprint size.
    /// </summary>
    private Vector2Int GetBaseIndices(Vector3 worldPosition, Vector2Int size)
    {
        GridNode node = _gridManager.getNodeFromWorldPosition(worldPosition);
        float cellSize = _gridManager.GridSettings.NodeSize;
        int x = Mathf.RoundToInt(node.worldPosition.x / cellSize) - size.x / 2;
        int y = Mathf.RoundToInt(node.worldPosition.z / cellSize) - size.y / 2;
        return new Vector2Int(x, y);
    }

    /// <summary>
    /// Calculates the snapped world position aligned to the grid for the given indices and footprint size.
    /// </summary>
    private Vector3 CalculateSnapPosition(Vector2Int indices, Vector2Int size)
    {
        float cellSize = _gridManager.GridSettings.NodeSize;
        float width = size.x * cellSize;
        float depth = size.y * cellSize;

        return new Vector3(
            (indices.x * cellSize) + width / 2f - cellSize / 2f,
            _gridManager.getNodeFromWorldPosition(transform.position).worldPosition.y,
            (indices.y * cellSize) + depth / 2f - cellSize / 2f
        );
    }

    /// <summary>
    /// Converts mouse position to world space on a flat ground plane at Y = 0.
    /// </summary>
    private bool TryGetMouseWorldPosition(out Vector3 worldPosition)
    {
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float enter))
        {
            worldPosition = ray.GetPoint(enter);
            return true;
        }

        worldPosition = Vector3.zero;
        return false;
    }
}
