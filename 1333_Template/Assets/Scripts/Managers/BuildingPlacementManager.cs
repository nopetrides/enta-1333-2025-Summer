using UnityEngine;

public class BuildingPlacementManager : MonoBehaviour
{
    [Header("Grid Reference")]
    [Tooltip("Drag your GridManager here")]
    [SerializeField] private GridManager _gridManager;

    [Header("Ghost Preview Materials")]
    [Tooltip("Semi-transparent green material for valid placement")]
    [SerializeField] private Material ghostValidMaterial;
    [Tooltip("Semi-transparent red material for invalid placement")]
    [SerializeField] private Material ghostInvalidMaterial;

    [Header("Ground Raycast")]
    [Tooltip("LayerMask for your terrain/ground")]
    [SerializeField] private LayerMask groundLayerMask;

    private Camera _mainCamera;
    private BuildingDataSO _currentData;
    private BuildingInstance _previewInstance;

    private void Awake()
    {
        _mainCamera = Camera.main;
        if (_gridManager == null) Debug.LogError("BuildingPlacementManager: GridManager is not assigned in the Inspector.");
        if (_mainCamera == null) Debug.LogError("BuildingPlacementManager: MainCamera is not assigned.");
    }

    private void Update()
    {
        if (_previewInstance == null) return;

        // 1) Raycast from mouse into world
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (!groundPlane.Raycast(ray, out float enter)) return;

        Vector3 hitPoint = ray.GetPoint(enter);

        // 2) Compute whether we *can* place here and get snapPos
        bool canPlace = _previewInstance.CanPlaceAt(hitPoint, out Vector3 snapPos);

        // 3) Move the ghost to that snapped position every frame
        _previewInstance.transform.position = snapPos;

        // 4) Tint ghost green or red
        var rend = _previewInstance.GetComponentInChildren<Renderer>();
        rend.material = canPlace ? ghostValidMaterial : ghostInvalidMaterial;

        // 5) On left‐click, commit placement
        if (canPlace && Input.GetMouseButtonDown(0))
            PlaceRealBuilding(hitPoint);

        // 6) On right‐click, cancel placement
        if (Input.GetMouseButtonDown(1))
            CancelPlacement();
    }

    /// <summary>
    /// Begins a new placement: spawns the ghost and wires it up.
    /// </summary>
    public void StartPlacement(BuildingDataSO data)
    {
        // Destroy any existing ghost
        if (_previewInstance != null)
            Destroy(_previewInstance.gameObject);

        _currentData = data;

        // Instantiate a new ghost
        _previewInstance = Instantiate(data.BuildingPrefab)
            .GetComponent<BuildingInstance>();

        // Assign building data of instance as passed building data
        _previewInstance.buildingData = data;

        // Give it the grid reference
        _previewInstance.InitializePlacement(_gridManager);

        // Start it tinted “invalid”
        var rend = _previewInstance.GetComponentInChildren<Renderer>();
        rend.material = ghostInvalidMaterial;
    }

    private void PlaceRealBuilding(Vector3 placementWorldPos)
    {
        var real = Instantiate(_currentData.BuildingPrefab)
            .GetComponent<BuildingInstance>();

        real.buildingData = _currentData;
        real.InitializePlacement(_gridManager);

        real.TryPlaceAt(placementWorldPos, Team.Player);

        Destroy(_previewInstance.gameObject);
        _previewInstance = null;
    }


    private void CancelPlacement()
    {
        Destroy(_previewInstance.gameObject);
        _previewInstance = null;
    }
}
