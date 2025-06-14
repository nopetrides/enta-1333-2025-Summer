using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;

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
    /// Handles preview rotation, movement, and placement input.
    /// Prevents only placement and cancellation clicks on relevant UI elements,
    /// but always updates preview position and rotation.
    /// </summary>
    private void Update()
    {
        if (_previewInstance == null)
            return;

        // Rotate preview on middle mouse click (regardless of UI)
        if (Mouse.current.middleButton.wasPressedThisFrame)
        {
            _currentYRotation = (_currentYRotation + 90) % 360;
            ApplyRotation(_previewInstance.transform, _previewBaseRotation, _currentYRotation);
        }

        // Always update preview position
        if (TryGetMouseWorldPosition(out Vector3 hitPoint))
        {
            bool canPlace = CanPlace(_currentBuildingData, hitPoint, _currentYRotation, out Vector3 snapPos);
            _previewInstance.transform.position = snapPos;
            ApplyGhostMaterial(_previewInstance, canPlace ? _ghostValidMaterial : _ghostInvalidMaterial);

            // Place on left-click if valid and not over blocking UI
            if (canPlace && Mouse.current.leftButton.wasPressedThisFrame && !IsPointerOverUI())
            {
                PlaceRealBuilding(hitPoint);
                return;
            }
        }

        // Cancel on right-click if not over blocking UI
        if (Mouse.current.rightButton.wasPressedThisFrame && !IsPointerOverUI())
        {
            CancelPlacement();
        }
    }

    /// <summary>
    /// Begins placement by instantiating only the BuildingModel for a lightweight preview,
    /// resetting rotation to default.
    /// </summary>
    /// <param name="buildingData">The BuildingDataSO containing prefab and model references.</param>
    public void StartPlacement(BuildingDataSO buildingData)
    {
        if (_previewInstance != null)
            Destroy(_previewInstance);

        _currentBuildingData = buildingData;
        _currentYRotation = 0;
        CreatePreview();
    }

    /// <summary>
    /// Cancels the current building placement, destroys preview, and resets rotation.
    /// </summary>
    private void CancelPlacement()
    {
        if (_previewInstance != null)
            Destroy(_previewInstance);

        _previewInstance = null;
        _currentYRotation = 0;
    }

    /// <summary>
    /// Instantiates the real building prefab at world position,
    /// preserves rotation for next preview, and starts a new preview.
    /// </summary>
    /// <param name="worldPosition">Cursor world position on placement.</param>
    private void PlaceRealBuilding(Vector3 worldPosition)
    {
        var realGO = Instantiate(_currentBuildingData.BuildingPrefab);
        var realBase = realGO.GetComponent<BuildingBase>();
        realBase.buildingData = _currentBuildingData;
        realBase.team = Team.Player;
        realBase.ApplyTeamMaterial();

        realGO.transform.rotation = _previewInstance.transform.rotation;

        Vector2Int baseIndices = GetBaseIndices(worldPosition);
        Vector2Int footprint = GetRotatedSize(_currentBuildingData, _currentYRotation);

        if (realBase is BuildingGate gate)
            gate.InitializePlacement(baseIndices, footprint, _gridManager);

        Vector3 snapPos = CalculateSnapPosition(baseIndices, _currentBuildingData, _currentYRotation);
        realGO.transform.position = snapPos;
        MarkAreaOccupied(baseIndices, footprint, false);

        Destroy(_previewInstance);
        _previewInstance = null;
        CreatePreview();
    }

    /// <summary>
    /// Instantiates the BuildingModel preview and applies current rotation and invalid material.
    /// </summary>
    private void CreatePreview()
    {
        _previewInstance = Instantiate(_currentBuildingData.BuildingModel);
        _previewBaseRotation = _previewInstance.transform.rotation;
        ApplyRotation(_previewInstance.transform, _previewBaseRotation, _currentYRotation);
        ApplyGhostMaterial(_previewInstance, _ghostInvalidMaterial);
    }

    /// <summary>
    /// Determines if the pointer is over any UI element except those tagged 'UIIgnore'.
    /// </summary>
    /// <remarks>
    /// Tag UI element 'UIIgnore' to let clicks pass through if needed.
    /// </remarks>
    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
            return false;

        var pointerData = new PointerEventData(EventSystem.current)
        {
            position = Mouse.current.position.ReadValue()
        };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var res in results)
        {
            if (res.gameObject.CompareTag("UIIgnore"))
                continue;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Applies additional yaw rotation to an object's base rotation.
    /// </summary>
    private void ApplyRotation(Transform target, Quaternion baseRotation, int yRotation)
    {
        Vector3 e = baseRotation.eulerAngles;
        target.rotation = Quaternion.Euler(e.x, e.y + yRotation, e.z);
    }

    /// <summary>
    /// Applies a single ghost material across all child renderers.
    /// </summary>
    private void ApplyGhostMaterial(GameObject instance, Material ghostMaterial)
    {
        foreach (var r in instance.GetComponentsInChildren<Renderer>())
            r.material = ghostMaterial;
    }

    /// <summary>
    /// Raycasts from mouse to ground plane and returns hit point.
    /// </summary>
    private bool TryGetMouseWorldPosition(out Vector3 worldPos)
    {
        Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane plane = new(Vector3.up, Vector3.zero);
        if (plane.Raycast(ray, out float d))
        {
            worldPos = ray.GetPoint(d);
            return true;
        }
        worldPos = Vector3.zero;
        return false;
    }

    /// <summary>
    /// Checks if a footprint area is placeable and returns snapped position.
    /// </summary>
    private bool CanPlace(BuildingDataSO data, Vector3 worldPos, int rot, out Vector3 snap)
    {
        Vector2Int idx = GetBaseIndices(worldPos);
        snap = CalculateSnapPosition(idx, data, rot);
        return IsAreaWalkable(idx, GetRotatedSize(data, rot));
    }

    /// <summary>
    /// Converts world position to grid indices.
    /// </summary>
    private Vector2Int GetBaseIndices(Vector3 worldPos)
    {
        var node = _gridManager.getNodeFromWorldPosition(worldPos);
        float size = _gridManager.GridSettings.NodeSize;
        return new Vector2Int(
            Mathf.RoundToInt(node.worldPosition.x / size),
            Mathf.RoundToInt(node.worldPosition.z / size)
        );
    }

    /// <summary>
    /// Calculates center-aligned world position based on grid indices and footprint.
    /// </summary>
    private Vector3 CalculateSnapPosition(Vector2Int idx, BuildingDataSO data, int rot)
    {
        float size = _gridManager.GridSettings.NodeSize;
        var fp = GetRotatedSize(data, rot);
        float w = fp.x * size, d = fp.y * size;
        float y = _gridManager.getNodeFromWorldPosition(
            new Vector3(idx.x * size, 0, idx.y * size)
        ).worldPosition.y;
        return new Vector3(
            idx.x * size + (w - size) * 0.5f,
            y,
            idx.y * size + (d - size) * 0.5f
        );
    }

    /// <summary>
    /// Returns rotated footprint dimensions.
    /// </summary>
    private Vector2Int GetRotatedSize(BuildingDataSO data, int rot) =>
        rot % 180 == 0
            ? new Vector2Int(data.SizeX, data.SizeZ)
            : new Vector2Int(data.SizeZ, data.SizeX);

    /// <summary>
    /// Checks grid walkability for the given footprint.
    /// </summary>
    private bool IsAreaWalkable(Vector2Int idx, Vector2Int fp)
    {
        for (int dx = 0; dx < fp.x; dx++)
            for (int dy = 0; dy < fp.y; dy++)
            {
                var n = _gridManager.GetNode(idx.x + dx, idx.y + dy);
                if (n == null || !n.walkable)
                    return false;
            }
        return true;
    }

    /// <summary>
    /// Marks or unmarks grid cells as walkable.
    /// </summary>
    private void MarkAreaOccupied(Vector2Int idx, Vector2Int fp, bool walkable)
    {
        for (int dx = 0; dx < fp.x; dx++)
            for (int dy = 0; dy < fp.y; dy++)
                _gridManager.SetWalkable(idx.x + dx, idx.y + dy, walkable);
    }
}
