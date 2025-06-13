using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages building placement: preview, rotation, placement validation,
/// and actual building instantiation on the grid.
/// Handles ghost preview material switching and communicates with the grid system for occupancy.
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
    private BuildingBase _previewInstance;
    private Quaternion _previewBaseRotation;
    private int _currentYRotation = 0;

    /// <summary>
    /// Called when the script instance is loaded. Sets up main camera and grid manager references.
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
    /// Called every frame. Updates preview, rotation, placement, and cancellation logic.
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

        // Update ghost preview materials (supporting gates)
        ApplyGhostMaterial(_previewInstance, canPlace ? _ghostValidMaterial : _ghostInvalidMaterial);

        // Confirm placement on left click
        if (canPlace && Mouse.current.leftButton.wasPressedThisFrame)
            PlaceRealBuilding(hitPoint);

        // Cancel placement on right click
        if (Mouse.current.rightButton.wasPressedThisFrame)
            CancelPlacement();
    }

    /// <summary>
    /// Starts the building placement process for a selected building.
    /// </summary>
    /// <param name="buildingData">The building data to be placed.</param>
    public void StartPlacement(BuildingDataSO buildingData)
    {
        if (_previewInstance != null)
            Destroy(_previewInstance.gameObject);

        _currentBuildingData = buildingData;
        _currentYRotation = 0;

        var previewGO = Instantiate(buildingData.BuildingPrefab);
        _previewInstance = previewGO.GetComponent<BuildingBase>();
        _previewInstance.buildingData = buildingData;

        _previewBaseRotation = previewGO.transform.rotation;
        ApplyRotation(previewGO.transform, _previewBaseRotation, _currentYRotation);

        // Set initial ghost material
        ApplyGhostMaterial(_previewInstance, _ghostInvalidMaterial);
    }

    /// <summary>
    /// Cancels the current building placement and destroys the preview instance.
    /// </summary>
    private void CancelPlacement()
    {
        Destroy(_previewInstance.gameObject);
        _previewInstance = null;
    }

    /// <summary>
    /// Instantiates the real building at the specified world position and finalizes placement.
    /// </summary>
    /// <param name="worldPosition">The desired world position for placement.</param>
    private void PlaceRealBuilding(Vector3 worldPosition)
    {
        // Instantiate prefab
        var realGO = Instantiate(_currentBuildingData.BuildingPrefab);

        // Generic base setup
        var realBase = realGO.GetComponent<BuildingBase>();
        realBase.buildingData = _currentBuildingData;
        realBase.team = Team.Player;
        realBase.ApplyTeamMaterial();

        // Compute placement data
        Vector2Int baseIndices = GetBaseIndices(worldPosition);
        Vector2Int footprint = GetRotatedSize(_currentBuildingData, _currentYRotation);

        // If gate, initialize gate placement
        if (realBase is BuildingGate gate)
            gate.InitializePlacement(baseIndices, footprint, _gridManager);

        // Snap position and occupy grid area
        Vector3 snapPos = CalculateSnapPosition(baseIndices, _currentBuildingData, _currentYRotation);
        realGO.transform.position = snapPos;
        MarkAreaOccupied(baseIndices, footprint, false);

        Destroy(_previewInstance.gameObject);
        _previewInstance = null;
    }

    /// <summary>
    /// Applies rotation to a transform based on base rotation and current Y rotation.
    /// </summary>
    /// <param name="target">The transform to rotate.</param>
    /// <param name="baseRotation">The base rotation quaternion.</param>
    /// <param name="yRotation">The additional rotation in degrees around the Y axis.</param>
    private void ApplyRotation(Transform target, Quaternion baseRotation, int yRotation)
    {
        Vector3 baseEuler = baseRotation.eulerAngles;
        target.rotation = Quaternion.Euler(baseEuler.x, baseEuler.y + yRotation, baseEuler.z);
    }

    /// <summary>
    /// Applies the ghost material to all mesh renderers on the preview instance.
    /// Supports both standard and skinned mesh renderers for gates.
    /// </summary>
    /// <param name="instance">The preview building instance.</param>
    /// <param name="ghostMaterial">The material to apply.</param>
    private void ApplyGhostMaterial(BuildingBase instance, Material ghostMaterial)
    {
        var meshRenderer = instance.GetComponentInChildren<Renderer>();
        if (meshRenderer != null)
            meshRenderer.material = ghostMaterial;

        if (instance is BuildingGate gate)
        {
            foreach (var smr in gate.GetComponentsInChildren<SkinnedMeshRenderer>())
                smr.material = ghostMaterial;
        }
    }

    /// <summary>
    /// Gets the world position under the mouse cursor projected onto the ground plane.
    /// </summary>
    /// <param name="worldPosition">The resulting world position.</param>
    /// <returns>True if a position was found; otherwise false.</returns>
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
    /// Checks if the building can be placed at the given position and calculates the snapped position.
    /// </summary>
    private bool CanPlace(BuildingDataSO data, Vector3 worldPosition, int rotation, out Vector3 snapPosition)
    {
        Vector2Int baseIndices = GetBaseIndices(worldPosition);
        snapPosition = CalculateSnapPosition(baseIndices, data, rotation);
        Vector2Int footprint = GetRotatedSize(data, rotation);
        return IsAreaWalkable(baseIndices, footprint);
    }

    /// <summary>
    /// Converts world position to grid indices.
    /// </summary>
    private Vector2Int GetBaseIndices(Vector3 worldPosition)
    {
        GridNode node = _gridManager.getNodeFromWorldPosition(worldPosition);
        float nodeSize = _gridManager.GridSettings.NodeSize;
        int x = Mathf.RoundToInt(node.worldPosition.x / nodeSize);
        int y = Mathf.RoundToInt(node.worldPosition.z / nodeSize);
        return new Vector2Int(x, y);
    }

    /// <summary>
    /// Calculates the snapped position of the building for perfect grid alignment.
    /// </summary>
    private Vector3 CalculateSnapPosition(Vector2Int indices, BuildingDataSO data, int rotation)
    {
        float nodeSize = _gridManager.GridSettings.NodeSize;
        Vector2Int footprint = GetRotatedSize(data, rotation);
        float width = footprint.x * nodeSize;
        float depth = footprint.y * nodeSize;
        float y = _gridManager.getNodeFromWorldPosition(new Vector3(indices.x * nodeSize, 0, indices.y * nodeSize)).worldPosition.y;
        return new Vector3(
            indices.x * nodeSize + width * 0.5f - nodeSize * 0.5f,
            y,
            indices.y * nodeSize + depth * 0.5f - nodeSize * 0.5f
        );
    }

    /// <summary>
    /// Determines the rotated size (footprint) of the building based on its rotation.
    /// </summary>
    private Vector2Int GetRotatedSize(BuildingDataSO data, int rotation) =>
        (rotation % 180 == 0)
            ? new Vector2Int(data.SizeX, data.SizeZ)
            : new Vector2Int(data.SizeZ, data.SizeX);

    /// <summary>
    /// Checks if all grid nodes in the footprint are walkable.
    /// </summary>
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
    /// Marks or unmarks grid nodes in the footprint as walkable.
    /// </summary>
    private void MarkAreaOccupied(Vector2Int baseIndices, Vector2Int footprint, bool walkable)
    {
        for (int dx = 0; dx < footprint.x; dx++)
            for (int dy = 0; dy < footprint.y; dy++)
                _gridManager.SetWalkable(baseIndices.x + dx, baseIndices.y + dy, walkable);
    }
}
