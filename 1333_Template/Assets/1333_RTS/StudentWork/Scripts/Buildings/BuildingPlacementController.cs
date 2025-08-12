using UnityEngine;

public class BuildingPlacementController : MonoBehaviour   // handles placement, grid snap, visual feedback and actual instantiation
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private BuildingTypePrefab[] placeableBuildings;

    [Header("Overlay Visuals (runtime)")]
    [SerializeField] private Material overlayMaterial;                    
    [SerializeField] private Color validColor = new Color(0f, 1f, 0f, 0.35f);
    [SerializeField] private Color invalidColor = new Color(1f, 0f, 0f, 0.35f);

    private BuildingTypePrefab currentToPlace;
    private int overlayX, overlayY;
    private bool showOverlay;
    private bool lastValid;

    private float buildingRotation = 0f;

    // runtime overlay objects
    private GameObject overlayGO;          // a single quad  move/scale/rotate
    private MeshRenderer overlayRenderer;

    public void SetBuildingToPlace(BuildingTypePrefab typePrefab)     // called by UI to select a building for placement
    {
        currentToPlace = typePrefab;
        buildingRotation = 0f; // reset rotation when a new type is chosen
        EnsureOverlay();
        SetOverlayActive(true);
    }

    void Update()
    {
        if (currentToPlace == null) { SetOverlayActive(false); showOverlay = false; return; }

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
            SetOverlayActive(true);

            int width = currentToPlace.buildingType.Width;
            int height = currentToPlace.buildingType.Height;

            lastValid = gridManager.CanPlaceBuildingAt(overlayX, overlayY, width, height)
                        && HasAdjacentWalkableCell(overlayX, overlayY, width, height);

            // rotate with right mouse button (90 degree)
            if (Input.GetMouseButtonDown(1))
            {
                buildingRotation += 90f;
                if (buildingRotation >= 360f) buildingRotation = 0f;
            }

           
            UpdateOverlayTransformAndColor(lastValid);

           
            if (lastValid && Input.GetMouseButtonDown(0))   //  place on left mouse button
            {
                var type = currentToPlace.buildingType;
                if (!ResourceManager.Instance.SpendResources(type.GoldCost, type.StoneCost, type.WoodCost))
                {
                    
                    return;
                }

                float nodeSize = settings.NodeSize;

                Vector3 corner = settings.UseXZPlane
                    ? new Vector3(overlayX, 0, overlayY) * nodeSize
                    : new Vector3(overlayX, overlayY, 0) * nodeSize;

                Vector3 centerOffset = settings.UseXZPlane
                    ? new Vector3(width * 0.5f, 0, height * 0.5f) * nodeSize
                    : new Vector3(width * 0.5f, height * 0.5f, 0) * nodeSize;

                Vector3 placePos = corner + centerOffset - new Vector3(nodeSize, 0, nodeSize) * 0.5f;

                
                Quaternion rot = settings.UseXZPlane
                    ? Quaternion.Euler(currentToPlace.prefab.transform.eulerAngles.x, buildingRotation, currentToPlace.prefab.transform.eulerAngles.z)
                    : Quaternion.Euler(currentToPlace.prefab.transform.eulerAngles.x, currentToPlace.prefab.transform.eulerAngles.y, buildingRotation);

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
                SetOverlayActive(false);   // hide overlay after placement
            }
        }
        else
        {
            showOverlay = false;
            SetOverlayActive(false);
        }
    }

    // helper that checks if there's at least one walkable & unoccupied cell adjacent to the proposed building area
    private bool HasAdjacentWalkableCell(int startX, int startY, int width, int height)
    {
        var settings = gridManager.GridSettings;

        for (int dx = -1; dx <= width; dx++)
        {
            for (int dy = -1; dy <= height; dy++)
            {
                // skip cells that are inside the building area
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

 
    private void EnsureOverlay()
    {
        if (overlayGO != null) return;

     
        overlayGO = GameObject.CreatePrimitive(PrimitiveType.Quad);
        overlayGO.name = "[PlacementOverlay]";
        overlayGO.layer = gameObject.layer; 
        var col = overlayGO.GetComponent<Collider>();
        if (col) Destroy(col);

        overlayRenderer = overlayGO.GetComponent<MeshRenderer>();
        overlayRenderer.sharedMaterial = new Material(overlayMaterial); 

        
        overlayGO.SetActive(false);
    }

    private void SetOverlayActive(bool active)
    {
        if (overlayGO != null && overlayGO.activeSelf != active)
            overlayGO.SetActive(active);
    }

    private void UpdateOverlayTransformAndColor(bool isValid)
    {
        if (overlayGO == null) return;

        var settings = gridManager.GridSettings;
        float s = settings.NodeSize;
        int w = currentToPlace.buildingType.Width;
        int h = currentToPlace.buildingType.Height;

       
        Vector3 corner = settings.UseXZPlane
            ? new Vector3(overlayX, 0, overlayY) * s
            : new Vector3(overlayX, overlayY, 0) * s;

        Vector3 centerOffset = settings.UseXZPlane
            ? new Vector3(w * 0.5f, 0, h * 0.5f) * s
            : new Vector3(w * 0.5f, h * 0.5f, 0) * s;

        Vector3 pos = corner + centerOffset - new Vector3(s, 0, s) * 0.5f;

      
        Vector3 scale = new Vector3(w * s, h * s, 1f);

        
        Quaternion rot = settings.UseXZPlane
            ? Quaternion.Euler(90f, buildingRotation, 0f)
            : Quaternion.Euler(0f, 0f, buildingRotation);

        overlayGO.transform.SetPositionAndRotation(pos, rot);
        overlayGO.transform.localScale = scale;

       
        if (overlayRenderer != null)
        {
            overlayRenderer.sharedMaterial.color = isValid ? validColor : invalidColor;
        }
    }

    
}
