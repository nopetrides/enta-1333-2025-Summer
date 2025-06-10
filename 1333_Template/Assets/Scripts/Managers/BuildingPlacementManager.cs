using UnityEngine;

/// <summary>
/// Manages building placement: preview follows mouse and final building snaps to grid.
/// Supports rotation by middle mouse button and respects prefab's original rotation.
/// </summary>
public class BuildingPlacementManager : MonoBehaviour
{
    [Header("Grid Reference")]
    [Tooltip("Drag your GridManager here")]
    [SerializeField] private GridManager _gridManager; // Reference to the GridManager used for placement validation

    [Header("Ghost Preview Materials")]
    [Tooltip("Semi-transparent green material for valid placement")]
    [SerializeField] private Material _ghostValidMaterial; // Material to show when placement is valid
    [Tooltip("Semi-transparent red material for invalid placement")]
    [SerializeField] private Material _ghostInvalidMaterial; // Material to show when placement is invalid

    private Camera _mainCamera; // Main camera used to convert mouse position to world position
    private BuildingDataSO _currentBuildingData; // Currently selected building data
    private BuildingInstance _previewInstance; // Instance of the preview (ghost) building
    private int _currentYRotation = 0; // Current Y-axis rotation in degrees

    private void Awake()
    {
        // Cache main camera and validate references
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
            _previewInstance.SetRotation(_currentYRotation);
        }

        // Get the world position where the mouse points
        if (!TryGetMouseWorldPosition(out Vector3 hitPoint))
            return;

        // Check if placement is valid and get the snapped position
        bool canPlace = _previewInstance.CanPlaceAt(hitPoint, out Vector3 snapPos);
        _previewInstance.transform.position = snapPos; // Move preview to snapped position

        // Update ghost color based on validity
        var renderer = _previewInstance.GetComponentInChildren<Renderer>();
        renderer.material = canPlace ? _ghostValidMaterial : _ghostInvalidMaterial;

        // Confirm placement on left click
        if (canPlace && Input.GetMouseButtonDown(0))
            PlaceRealBuilding(hitPoint);

        // Cancel placement on right click
        if (Input.GetMouseButtonDown(1))
            CancelPlacement();
    }

    /// <summary>
    /// Starts placement preview with the selected building data.
    /// </summary>
    public void StartPlacement(BuildingDataSO buildingData)
    {
        // Destroy any existing preview instance
        if (_previewInstance != null)
            Destroy(_previewInstance.gameObject);

        _currentBuildingData = buildingData;
        _currentYRotation = 0;

        // Create new preview instance and initialize rotation
        _previewInstance = CreateInstance(buildingData);
        _previewInstance.SetRotation(_currentYRotation);

        // Set initial material to invalid
        var renderer = _previewInstance.GetComponentInChildren<Renderer>();
        renderer.material = _ghostInvalidMaterial;
    }

    /// <summary>
    /// Finalizes placement of the building at the specified world position.
    /// </summary>
    private void PlaceRealBuilding(Vector3 worldPosition)
    {
        // Instantiate a real building instance and apply rotation
        var realInstance = CreateInstance(_currentBuildingData);
        realInstance.SetRotation(_currentYRotation);
        realInstance.TryPlaceAt(worldPosition, Team.Player);

        // Remove the preview instance
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
    /// Instantiates and prepares a building instance from the prefab.
    /// </summary>
    private BuildingInstance CreateInstance(BuildingDataSO data)
    {
        GameObject go = Instantiate(data.BuildingPrefab);
        var instance = go.GetComponent<BuildingInstance>();
        instance.buildingData = data;
        instance.InitializePlacement(_gridManager);
        instance.SetBaseRotation(go.transform.rotation); // Store the prefab's original rotation
        return instance;
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