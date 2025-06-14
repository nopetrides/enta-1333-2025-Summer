// BuildingPlacementManager.cs
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// Manages building placement: preview, rotation, validation,
/// final instantiation, and injection of ResourceManager into new buildings.
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

    // Injected by GameManager
    private ResourceManager _resourceManager;

    /// <summary>
    /// Called by GameManager to inject dependencies.
    /// </summary>
    public void Initialize(ResourceManager resourceManager)
    {
        _resourceManager = resourceManager;
    }

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

    public void StartPlacement(BuildingDataSO buildingData)
    {
        if (_previewInstance != null)
            Destroy(_previewInstance);

        _currentBuildingData = buildingData;
        _currentYRotation = 0;
        CreatePreview();
    }

    private void CancelPlacement()
    {
        if (_previewInstance != null)
            Destroy(_previewInstance);

        _previewInstance = null;
        _currentYRotation = 0;
    }

    private void PlaceRealBuilding(Vector3 worldPosition)
    {
        var realGO = Instantiate(_currentBuildingData.BuildingPrefab);
        var realBase = realGO.GetComponent<BuildingBase>();
        realBase.buildingData = _currentBuildingData;
        realBase.team = Team.Player;
        realBase.ApplyTeamMaterial();

        realGO.transform.rotation = _previewInstance.transform.rotation;

        Vector2Int baseIdx = GetBaseIndices(worldPosition);
        Vector2Int footprint = GetRotatedSize(_currentBuildingData, _currentYRotation);

        if (realBase is BuildingGate gate)
            gate.InitializePlacement(baseIdx, footprint, _gridManager);

        // If it produces resources, inject manager
        if (realBase is BuildingResource br && _resourceManager != null)
            br.Initialize(_resourceManager);

        Vector3 snapPos = CalculateSnapPosition(baseIdx, _currentBuildingData, _currentYRotation);
        realGO.transform.position = snapPos;
        MarkAreaOccupied(baseIdx, footprint, false);

        Destroy(_previewInstance);
        _previewInstance = null;
        CreatePreview();
    }

    private void CreatePreview()
    {
        _previewInstance = Instantiate(_currentBuildingData.BuildingModel);
        _previewBaseRotation = _previewInstance.transform.rotation;
        ApplyRotation(_previewInstance.transform, _previewBaseRotation, _currentYRotation);
        ApplyGhostMaterial(_previewInstance, _ghostInvalidMaterial);
    }

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
            if (!res.gameObject.CompareTag("UIIgnore"))
                return true;

        return false;
    }

    private void ApplyRotation(Transform target, Quaternion baseRot, int yRot)
    {
        Vector3 e = baseRot.eulerAngles;
        target.rotation = Quaternion.Euler(e.x, e.y + yRot, e.z);
    }

    private void ApplyGhostMaterial(GameObject instance, Material mat)
    {
        foreach (var r in instance.GetComponentsInChildren<Renderer>())
            r.material = mat;
    }

    private bool TryGetMouseWorldPosition(out Vector3 pos)
    {
        Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane groundPlane = new(Vector3.up, Vector3.zero);
        if (groundPlane.Raycast(ray, out float d))
        {
            pos = ray.GetPoint(d);
            return true;
        }
        pos = Vector3.zero;
        return false;
    }

    private bool CanPlace(BuildingDataSO data, Vector3 worldPos, int rot, out Vector3 snap)
    {
        Vector2Int idx = GetBaseIndices(worldPos);
        snap = CalculateSnapPosition(idx, data, rot);
        return IsAreaWalkable(idx, GetRotatedSize(data, rot));
    }

    private Vector2Int GetBaseIndices(Vector3 worldPos)
    {
        var node = _gridManager.getNodeFromWorldPosition(worldPos);
        float sz = _gridManager.GridSettings.NodeSize;
        return new Vector2Int(
            Mathf.RoundToInt(node.worldPosition.x / sz),
            Mathf.RoundToInt(node.worldPosition.z / sz)
        );
    }

    private Vector3 CalculateSnapPosition(Vector2Int idx, BuildingDataSO data, int rot)
    {
        float sz = _gridManager.GridSettings.NodeSize;
        var fp = GetRotatedSize(data, rot);
        float w = fp.x * sz, d = fp.y * sz;
        float y = _gridManager.getNodeFromWorldPosition(
            new Vector3(idx.x * sz, 0, idx.y * sz)
        ).worldPosition.y;
        return new Vector3(
            idx.x * sz + (w - sz) * 0.5f,
            y,
            idx.y * sz + (d - sz) * 0.5f
        );
    }

    private Vector2Int GetRotatedSize(BuildingDataSO data, int rot) =>
        (rot % 180 == 0)
            ? new Vector2Int(data.SizeX, data.SizeZ)
            : new Vector2Int(data.SizeZ, data.SizeX);

    private bool IsAreaWalkable(Vector2Int idx, Vector2Int fp)
    {
        for (int x = 0; x < fp.x; x++)
            for (int y = 0; y < fp.y; y++)
            {
                var n = _gridManager.GetNode(idx.x + x, idx.y + y);
                if (n == null || !n.walkable)
                    return false;
            }
        return true;
    }

    private void MarkAreaOccupied(Vector2Int idx, Vector2Int fp, bool walkable)
    {
        for (int x = 0; x < fp.x; x++)
            for (int y = 0; y < fp.y; y++)
                _gridManager.SetWalkable(idx.x + x, idx.y + y, walkable);
    }
}
