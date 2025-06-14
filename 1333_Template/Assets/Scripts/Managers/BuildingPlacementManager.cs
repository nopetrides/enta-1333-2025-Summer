using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages building placement: preview with BuildingModel, rotation, validation,
/// and final instantiation of BuildingPrefab on the grid.
/// </summary>
public class BuildingPlacementManager : MonoBehaviour
{
    [Header("Grid Reference")]
    [Tooltip("Drag your GridManager here")]
    [SerializeField] private GridManager _gridManager;

    [Header("Ghost Preview Materials")]
    [Tooltip("Semi-transparent green material for valid placement")]
    [SerializeField] private Material _ghostValidMaterial;
    [Tooltip("Semi-transparent red material for invalid placement")]
    [SerializeField] private Material _ghostInvalidMaterial;

    private Camera _mainCamera;
    private BuildingDataSO _currentBuildingData;
    private GameObject _previewInstance;
    private Quaternion _previewBaseRotation;
    private int _currentYRotation = 0;

    /// <summary>
    /// Initializes references to main camera and grid manager.
    /// </summary>
    private void Awake()
    {
        _mainCamera = Camera.main;
        if (_gridManager == null)
            Debug.LogError("BuildingPlacementManager: GridManager is not assigned.");
        if (_mainCamera == null)
            Debug.LogError("BuildingPlacementManager: MainCamera not found.");
    }

    /// <summary>
    /// Handles input for preview rotation, placement validation, and actual placement.
    /// </summary>
    private void Update()
    {
        if (_previewInstance == null)
            return;

        // Rotate preview on middle mouse click
        if (Mouse.current.middleButton.wasPressedThisFrame)
        {
            _currentYRotation = (_currentYRotation + 90) % 360;
            ApplyRotation(_previewInstance.transform, _previewBaseRotation, _currentYRotation);
        }

        // Get world position under mouse cursor
        if (!TryGetMouseWorldPosition(out Vector3 hitPoint))
            return;

        // Determine if placement is valid and get snapped position
        bool canPlace = CanPlace(_currentBuildingData, hitPoint, _currentYRotation, out Vector3 snapPos);
        _previewInstance.transform.position = snapPos;

        // Update ghost preview material
        ApplyGhostMaterial(_previewInstance, canPlace ? _ghostValidMaterial : _ghostInvalidMaterial);

        // Confirm placement on left click
        if (canPlace && Mouse.current.leftButton.wasPressedThisFrame)
            PlaceRealBuilding(hitPoint);

        // Cancel placement on right click
        if (Mouse.current.rightButton.wasPressedThisFrame)
            CancelPlacement();
    }

    /// <summary>
    /// Begins placement by instantiating only the BuildingModel for a lightweight preview.
    /// </summary>
    /// <param name="buildingData">The BuildingDataSO containing prefab and model references.</param>
    public void StartPlacement(BuildingDataSO buildingData)
    {
        if (_previewInstance != null)
            Destroy(_previewInstance);

        _currentBuildingData = buildingData;
        _currentYRotation = 0;

        // Use BuildingModel (no scripts/colliders) for preview
        _previewInstance = Instantiate(buildingData.BuildingModel);
        _previewBaseRotation = _previewInstance.transform.rotation;
        ApplyRotation(_previewInstance.transform, _previewBaseRotation, _currentYRotation);

        // Start with invalid material until position is valid
        ApplyGhostMaterial(_previewInstance, _ghostInvalidMaterial);
    }

    /// <summary>
    /// Cancels the current building placement and destroys the preview instance.
    /// </summary>
    private void CancelPlacement()
    {
        Destroy(_previewInstance);
        _previewInstance = null;
    }

    /// <summary>
    /// Instantiates the real building prefab at the specified world position and continues placement mode.
    /// </summary>
    /// <param name="worldPosition">The raw world position under the cursor when placing.</param>
    private void PlaceRealBuilding(Vector3 worldPosition)
    {
        // Instantiate the actual prefab with all features
        var realGO = Instantiate(_currentBuildingData.BuildingPrefab);
        var realBase = realGO.GetComponent<BuildingBase>();
        realBase.buildingData = _currentBuildingData;
        realBase.team = Team.Player;
        realBase.ApplyTeamMaterial();

        // Copy rotation from preview
        realGO.transform.rotation = _previewInstance.transform.rotation;

        // Compute grid indices and footprint
        Vector2Int baseIndices = GetBaseIndices(worldPosition);
        Vector2Int footprint = GetRotatedSize(_currentBuildingData, _currentYRotation);

        // Initialize gate if needed
        if (realBase is BuildingGate gate)
            gate.InitializePlacement(baseIndices, footprint, _gridManager);

        // Snap into place and mark occupied
        Vector3 snapPos = CalculateSnapPosition(baseIndices, _currentBuildingData, _currentYRotation);
        realGO.transform.position = snapPos;
        MarkAreaOccupied(baseIndices, footprint, false);

        // Remove the old preview
        Destroy(_previewInstance);
        _previewInstance = null;

        // Continue placement mode by creating a new preview for the same building
        StartPlacement(_currentBuildingData);
    }

    /// <summary>
    /// Applies a Y-axis rotation on top of the base rotation.
    /// </summary>
    /// <param name="target">The transform to rotate.</param>
    /// <param name="baseRotation">The original rotation of the object.</param>
    /// <param name="yRotation">The additional yaw in degrees.</param>
    private void ApplyRotation(Transform target, Quaternion baseRotation, int yRotation)
    {
        Vector3 baseEuler = baseRotation.eulerAngles;
        target.rotation = Quaternion.Euler(baseEuler.x, baseEuler.y + yRotation, baseEuler.z);
    }

    /// <summary>
    /// Applies a ghost material to all renderers under the instance.
    /// </summary>
    /// <param name="instance">The preview GameObject.</param>
    /// <param name="ghostMaterial">The material to apply.</param>
    private void ApplyGhostMaterial(GameObject instance, Material ghostMaterial)
    {
        foreach (var renderer in instance.GetComponentsInChildren<Renderer>())
        {
            renderer.material = ghostMaterial;
        }
    }

    /// <summary>
    /// Casts a ray from the mouse into the world to find the ground plane point.
    /// </summary>
    /// <param name="worldPosition">Out parameter for the hit point on the ground plane.</param>
    /// <returns>True if the ray hit the ground plane, otherwise false.</returns>
    private bool TryGetMouseWorldPosition(out Vector3 worldPosition)
    {
        Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane groundPlane = new(Vector3.up, Vector3.zero);
        if (groundPlane.Raycast(ray, out float enter))
        {
            worldPosition = ray.GetPoint(enter);
            return true;
        }
        worldPosition = Vector3.zero;
        return false;
    }

    /// <summary>
    /// Validates placement, computing snapped grid-aligned position.
    /// </summary>
    /// <param name="data">Building data including size.</param>
    /// <param name="worldPosition">Raw world coordinate under cursor.</param>
    /// <param name="rotation">Current Y rotation in degrees.</param>
    /// <param name="snapPosition">Out parameter for snapped position.</param>
    /// <returns>True if all grid cells in footprint are walkable.</returns>
    private bool CanPlace(BuildingDataSO data, Vector3 worldPosition, int rotation, out Vector3 snapPosition)
    {
        Vector2Int baseIndices = GetBaseIndices(worldPosition);
        snapPosition = CalculateSnapPosition(baseIndices, data, rotation);
        Vector2Int footprint = GetRotatedSize(data, rotation);
        return IsAreaWalkable(baseIndices, footprint);
    }

    /// <summary>
    /// Converts a world coordinate to grid indices.
    /// </summary>
    /// <param name="worldPosition">The world position to convert.</param>
    /// <returns>Grid indices as Vector2Int.</returns>
    private Vector2Int GetBaseIndices(Vector3 worldPosition)
    {
        GridNode node = _gridManager.getNodeFromWorldPosition(worldPosition);
        float nodeSize = _gridManager.GridSettings.NodeSize;
        int x = Mathf.RoundToInt(node.worldPosition.x / nodeSize);
        int y = Mathf.RoundToInt(node.worldPosition.z / nodeSize);
        return new Vector2Int(x, y);
    }

    /// <summary>
    /// Calculates the snapped world position for placing the building.
    /// </summary>
    /// <param name="indices">Base grid indices.</param>
    /// <param name="data">Building data including size.</param>
    /// <param name="rotation">Current Y rotation in degrees.</param>
    /// <returns>The world position snapped to grid center.</returns>
    private Vector3 CalculateSnapPosition(Vector2Int indices, BuildingDataSO data, int rotation)
    {
        float nodeSize = _gridManager.GridSettings.NodeSize;
        Vector2Int footprint = GetRotatedSize(data, rotation);
        float width = footprint.x * nodeSize;
        float depth = footprint.y * nodeSize;
        float y = _gridManager.getNodeFromWorldPosition(
            new Vector3(indices.x * nodeSize, 0, indices.y * nodeSize)
        ).worldPosition.y;

        return new Vector3(
            indices.x * nodeSize + (width - nodeSize) * 0.5f,
            y,
            indices.y * nodeSize + (depth - nodeSize) * 0.5f
        );
    }

    /// <summary>
    /// Determines the building footprint size based on Y rotation.
    /// </summary>
    /// <param name="data">Building data including original size.</param>
    /// <param name="rotation">Current Y rotation in degrees.</param>
    /// <returns>Rotated footprint as Vector2Int.</returns>
    private Vector2Int GetRotatedSize(BuildingDataSO data, int rotation) =>
        (rotation % 180 == 0)
            ? new Vector2Int(data.SizeX, data.SizeZ)
            : new Vector2Int(data.SizeZ, data.SizeX);

    /// <summary>
    /// Checks whether all nodes in the specified footprint are walkable.
    /// </summary>
    /// <param name="baseIndices">Starting grid indices.</param>
    /// <param name="footprint">Footprint size in grid cells.</param>
    /// <returns>True if all nodes are walkable.</returns>
    private bool IsAreaWalkable(Vector2Int baseIndices, Vector2Int footprint)
    {
        for (int dx = 0; dx < footprint.x; dx++)
            for (int dy = 0; dy < footprint.y; dy++)
            {
                var node = _gridManager.GetNode(baseIndices.x + dx, baseIndices.y + dy);
                if (node == null || !node.walkable)
                    return false;
            }
        return true;
    }

    /// <summary>
    /// Marks or unmarks grid nodes as walkable or non-walkable.
    /// </summary>
    /// <param name="baseIndices">Starting grid indices.</param>
    /// <param name="footprint">Footprint size in grid cells.</param>
    /// <param name="walkable">True to mark nodes as walkable, false otherwise.</param>
    private void MarkAreaOccupied(Vector2Int baseIndices, Vector2Int footprint, bool walkable)
    {
        for (int dx = 0; dx < footprint.x; dx++)
            for (int dy = 0; dy < footprint.y; dy++)
                _gridManager.SetWalkable(baseIndices.x + dx, baseIndices.y + dy, walkable);
    }
}
