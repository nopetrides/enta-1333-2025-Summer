using UnityEngine;

public class BuildingPlacementController : MonoBehaviour   // handles placement, grid snap, visual feedback and actual instantiation
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private BuildingTypePrefab[] placeableBuildings;

    [Header("Overlay Visuals")]
    [SerializeField] private Color validColor = new Color(0, 1, 0, 0.4f);
    [SerializeField] private Color invalidColor = new Color(1, 0, 0, 0.4f);

    private BuildingTypePrefab currentToPlace;
    private int overlayX, overlayY;
    private bool showOverlay;
    private bool lastValid;
    private float buildingRotation = 0f; // Degrees (0, 90, 180, 270)

    public void SetBuildingToPlace(BuildingTypePrefab typePrefab)
    {
        currentToPlace = typePrefab;
        buildingRotation = 0f; // Reset rotation on new building selection
    }

    void Update()
    {
        if (currentToPlace == null) { showOverlay = false; return; }

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 gridPos = hit.point;
            var settings = gridManager.GridSettings;
            overlayX = Mathf.RoundToInt(gridPos.x / settings.NodeSize);
            overlayY = settings.UseXZPlane
                ? Mathf.RoundToInt(gridPos.z / settings.NodeSize)
                : Mathf.RoundToInt(gridPos.y / settings.NodeSize);

            showOverlay = true;

            lastValid = gridManager.CanPlaceBuildingAt(overlayX, overlayY, currentToPlace.buildingType.Width, currentToPlace.buildingType.Height)
                && HasAdjacentWalkableCell(overlayX, overlayY, currentToPlace.buildingType.Width, currentToPlace.buildingType.Height);

            // ---- Rotate with right mouse button ----
            if (Input.GetMouseButtonDown(1))
            {
                buildingRotation += 90f;
                if (buildingRotation >= 360f)
                    buildingRotation = 0f;
            }

            // ---- Place on left mouse button ----
            if (lastValid && Input.GetMouseButtonDown(0))
            {
                var type = currentToPlace.buildingType;
                if (!ResourceManager.Instance.SpendResources(type.GoldCost, type.StoneCost, type.WoodCost))
                {
                    // Optionally show UI feedback here
                    return;
                }

                float nodeSize = settings.NodeSize;
                int width = currentToPlace.buildingType.Width;
                int height = currentToPlace.buildingType.Height;

                Vector3 corner = settings.UseXZPlane
                    ? new Vector3(overlayX, 0, overlayY) * nodeSize
                    : new Vector3(overlayX, overlayY, 0) * nodeSize;

                Vector3 centerOffset = settings.UseXZPlane
                    ? new Vector3(width * 0.5f, 0, height * 0.5f) * nodeSize
                    : new Vector3(width * 0.5f, height * 0.5f, 0) * nodeSize;

                Vector3 placePos = corner + centerOffset - new Vector3(nodeSize, 0, nodeSize) * 0.5f;

                Quaternion rot = settings.UseXZPlane
                    ? Quaternion.Euler(-90, buildingRotation, 0) // Top-down grid: rotate around Y
                    : Quaternion.Euler(-90, 0, buildingRotation); // Side-view grid: rotate around Z

                GameObject obj = Instantiate(currentToPlace.prefab, placePos, rot);

                AudioManager.Instance.PlayBuildingPlaced();

                var instance = obj.GetComponent<BuildingInstance>();
                if (instance)
                {
                    instance.Initialize(currentToPlace.buildingType);
                    instance.SetOccupiedOrigin(new Vector2Int(overlayX, overlayY));
                }
                gridManager.SetBuildingOccupancy(overlayX, overlayY, width, height, true, currentToPlace.buildingType);

                currentToPlace = null;
                showOverlay = false;
            }
        }
        else
        {
            showOverlay = false;
        }
    }

    private bool HasAdjacentWalkableCell(int startX, int startY, int width, int height)
    {
        var settings = gridManager.GridSettings;

        for (int dx = -1; dx <= width; dx++)
        {
            for (int dy = -1; dy <= height; dy++)
            {
                if (dx >= 0 && dx < width && dy >= 0 && dy < height)
                    continue;

                int x = startX + dx;
                int y = startY + dy;

                if (gridManager.IsInBounds(x, y))
                {
                    var node = gridManager.GetNode(x, y);
                    if (node.Walkable && !node.Occupied)
                        return true;
                }
            }
        }
        return false;
    }

    // ---- Only draw overlay if building is being placed! ----
    private void OnDrawGizmos()
    {
        if (!showOverlay || currentToPlace == null) return;

        var settings = gridManager.GridSettings;
        Color c = lastValid ? validColor : invalidColor;
        Gizmos.color = c;
        float s = settings.NodeSize;

        // --- Visualize overlay rotation in Scene view ---
        Vector3 overlayCenter = settings.UseXZPlane
            ? new Vector3(overlayX + currentToPlace.buildingType.Width * 0.5f - 0.5f, 0, overlayY + currentToPlace.buildingType.Height * 0.5f - 0.5f) * s
            : new Vector3(overlayX + currentToPlace.buildingType.Width * 0.5f - 0.5f, overlayY + currentToPlace.buildingType.Height * 0.5f - 0.5f, 0) * s;

        Matrix4x4 rotationMatrix = settings.UseXZPlane
            ? Matrix4x4.TRS(overlayCenter, Quaternion.Euler(0, buildingRotation, 0), Vector3.one)
            : Matrix4x4.TRS(overlayCenter, Quaternion.Euler(0, 0, buildingRotation), Vector3.one);

        Gizmos.matrix = rotationMatrix;

        for (int dx = 0; dx < currentToPlace.buildingType.Width; dx++)
            for (int dy = 0; dy < currentToPlace.buildingType.Height; dy++)
            {
                Vector3 center = settings.UseXZPlane
                    ? new Vector3(dx - currentToPlace.buildingType.Width * 0.5f + 0.5f, 0, dy - currentToPlace.buildingType.Height * 0.5f + 0.5f) * s
                    : new Vector3(dx - currentToPlace.buildingType.Width * 0.5f + 0.5f, dy - currentToPlace.buildingType.Height * 0.5f + 0.5f, 0) * s;
                Gizmos.DrawCube(center, Vector3.one * s * 0.95f);
            }

        Gizmos.matrix = Matrix4x4.identity;
    }
}
