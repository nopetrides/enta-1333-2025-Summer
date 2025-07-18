using UnityEngine;

public class BuildingPlacementController : MonoBehaviour   // handles  placement of buildings, grid snap, visual feedback and actual instantiation
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
    private Vector2Int occupiedOrigin;
    public void SetBuildingToPlace(BuildingTypePrefab typePrefab)     // called by ui to select a building for placement

    {
        currentToPlace = typePrefab;
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
            lastValid = gridManager.CanPlaceBuildingAt(overlayX, overlayY, currentToPlace.buildingType.Width, currentToPlace.buildingType.Height);

            if (lastValid && Input.GetMouseButtonDown(0))     // place building if valid and left mouse is clicked
            {
                float nodeSize = settings.NodeSize;
                int width = currentToPlace.buildingType.Width;
                int height = currentToPlace.buildingType.Height;

                Vector3 corner = settings.UseXZPlane
                    ? new Vector3(overlayX, 0, overlayY) * nodeSize
                    : new Vector3(overlayX, overlayY, 0) * nodeSize;

                Vector3 centerOffset = settings.UseXZPlane
                    ? new Vector3(width * 0.5f, 0, height * 0.5f) * nodeSize  // putting a slight offset like 0.5 to fit prefab into the overlayed grid node and to make it centered 
                    : new Vector3(width * 0.5f, height * 0.5f, 0) * nodeSize;

                Vector3 placePos = corner + centerOffset - new Vector3(nodeSize, 0, nodeSize) * 0.5f;

                GameObject obj = Instantiate(currentToPlace.prefab, placePos, currentToPlace.prefab.transform.rotation); // instantiating with -90 on x to place accordingly

                AudioManager.Instance.PlayBuildingPlaced();  //building placement sfx

                var instance = obj.GetComponent<BuildingInstance>();
                if (instance)
                {
                    instance.Initialize(currentToPlace.buildingType);
                    instance.SetOccupiedOrigin(new Vector2Int(overlayX, overlayY));  // track the corret orgin for destruction
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

    private void OnDrawGizmos()
    {
        if (!showOverlay || currentToPlace == null) return;
        var settings = gridManager.GridSettings;
        Color c = lastValid ? validColor : invalidColor;
        Gizmos.color = c;
        float s = settings.NodeSize;

        for (int dx = 0; dx < currentToPlace.buildingType.Width; dx++)
            for (int dy = 0; dy < currentToPlace.buildingType.Height; dy++)
            {
                Vector3 center = settings.UseXZPlane
                    ? new Vector3(overlayX + dx, 0, overlayY + dy) * s
                    : new Vector3(overlayX + dx, overlayY + dy, 0) * s;
                Gizmos.DrawCube(center, Vector3.one * s * 0.95f);
            }
    }
}
