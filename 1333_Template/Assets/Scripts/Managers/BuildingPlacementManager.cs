using UnityEngine;

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
    private BuildingInstance _previewInstance;
    private Quaternion _previewBaseRotation;
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
        if (_previewInstance == null) return;

        // Rotate preview on middle mouse click
        if (Input.GetMouseButtonDown(2))
        {
            _currentYRotation = (_currentYRotation + 90) % 360;
            ApplyRotation(_previewInstance.transform, _previewBaseRotation, _currentYRotation);
        }

        // Get world position under mouse cursor
        if (!TryGetMouseWorldPosition(out Vector3 hitPoint)) return;

        // Determine if placement is valid and get snapped position
        bool canPlace = CanPlace(_currentBuildingData, hitPoint, _currentYRotation, out Vector3 snapPos);
        _previewInstance.transform.position = snapPos;

        // Update ghost preview materials (supporting gates)
        ApplyGhostMaterial(_previewInstance, canPlace ? _ghostValidMaterial : _ghostInvalidMaterial);

        // Confirm placement on left click
        if (canPlace && Input.GetMouseButtonDown(0)) PlaceRealBuilding(hitPoint);

        // Cancel placement on right click
        if (Input.GetMouseButtonDown(1)) CancelPlacement();
    }

    public void StartPlacement(BuildingDataSO buildingData)
    {
        if (_previewInstance != null) Destroy(_previewInstance.gameObject);

        _currentBuildingData = buildingData;
        _currentYRotation = 0;

        var previewGO = Instantiate(buildingData.BuildingPrefab);
        _previewInstance = previewGO.GetComponent<BuildingInstance>();
        _previewInstance.buildingData = buildingData;

        _previewBaseRotation = previewGO.transform.rotation;
        ApplyRotation(previewGO.transform, _previewBaseRotation, _currentYRotation);

        // Set initial ghost material
        ApplyGhostMaterial(_previewInstance, _ghostInvalidMaterial);
    }

    private void PlaceRealBuilding(Vector3 worldPosition)
    {
        var realGO = Instantiate(_currentBuildingData.BuildingPrefab);
        var realInstance = realGO.GetComponent<BuildingInstance>();
        realInstance.buildingData = _currentBuildingData;

        var realBaseRotation = realGO.transform.rotation;
        ApplyRotation(realGO.transform, realBaseRotation, _currentYRotation);

        FinalizePlacement(realInstance, worldPosition);

        Destroy(_previewInstance.gameObject);
        _previewInstance = null;
    }

    private void CancelPlacement()
    {
        Destroy(_previewInstance.gameObject);
        _previewInstance = null;
    }

    private void ApplyRotation(Transform target, Quaternion baseRotation, int yRotation)
    {
        Vector3 baseEuler = baseRotation.eulerAngles;
        target.rotation = Quaternion.Euler(baseEuler.x, baseEuler.y + yRotation, baseEuler.z);
    }

    private void ApplyGhostMaterial(BuildingInstance instance, Material ghostMaterial)
    {
        // Apply to standard mesh renderer
        var meshRenderer = instance.GetComponentInChildren<Renderer>();
        if (meshRenderer != null)
            meshRenderer.material = ghostMaterial;

        // If gate, also apply to skinned mesh renderers
        if (instance is BuildingGate gate)
        {
            foreach (var smr in gate.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                smr.material = ghostMaterial;
            }
        }
    }

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

    private bool CanPlace(BuildingDataSO data, Vector3 worldPosition, int rotation, out Vector3 snapPosition)
    {
        snapPosition = Vector3.zero;
        Vector2Int baseIndices = GetBaseIndices(worldPosition);
        snapPosition = CalculateSnapPosition(baseIndices, data, rotation);
        Vector2Int footprint = GetRotatedSize(data, rotation);
        return IsAreaWalkable(baseIndices, footprint);
    }

    private void FinalizePlacement(BuildingInstance instance, Vector3 worldPosition)
    {
        if (CanPlace(_currentBuildingData, worldPosition, _currentYRotation, out Vector3 snapPos))
        {
            instance.transform.position = snapPos;
            Vector2Int baseIndices = GetBaseIndices(worldPosition);
            Vector2Int footprint = GetRotatedSize(_currentBuildingData, _currentYRotation);

            MarkAreaOccupied(baseIndices, footprint, false);
            instance.team = Team.Player;
            instance.ApplyTeamMaterial();
        }
    }

    private Vector2Int GetRotatedSize(BuildingDataSO data, int rotation)
    {
        return (rotation % 180 == 0)
            ? new Vector2Int(data.SizeX, data.SizeZ)
            : new Vector2Int(data.SizeZ, data.SizeX);
    }

    private Vector2Int GetBaseIndices(Vector3 worldPosition)
    {
        GridNode node = _gridManager.getNodeFromWorldPosition(worldPosition);
        float nodeSize = _gridManager.GridSettings.NodeSize;
        int x = Mathf.RoundToInt(node.worldPosition.x / nodeSize);
        int y = Mathf.RoundToInt(node.worldPosition.z / nodeSize);
        return new Vector2Int(x, y);
    }

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

    private bool IsAreaWalkable(Vector2Int baseIndices, Vector2Int footprint)
    {
        for (int dx = 0; dx < footprint.x; dx++)
        {
            for (int dy = 0; dy < footprint.y; dy++)
            {
                var node = _gridManager.GetNode(baseIndices.x + dx, baseIndices.y + dy);
                if (node == null || !node.walkable)
                    return false;
            }
        }
        return true;
    }

    private void MarkAreaOccupied(Vector2Int baseIndices, Vector2Int footprint, bool walkable)
    {
        for (int dx = 0; dx < footprint.x; dx++)
        {
            for (int dy = 0; dy < footprint.y; dy++)
            {
                _gridManager.SetWalkable(baseIndices.x + dx, baseIndices.y + dy, walkable);
            }
        }
    }
}