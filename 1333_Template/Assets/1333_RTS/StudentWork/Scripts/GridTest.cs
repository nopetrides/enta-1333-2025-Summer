using UnityEngine;

public class GridTest : MonoBehaviour
{
    [Header("Required References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private Pathfinder pathfinder;
    [SerializeField] private Transform startMarker;
    [SerializeField] private Transform endMarker;
    [SerializeField] private LineRenderer pathLine;

    [Header("Randomization Settings")]
    [SerializeField] private float markerHeight = 0.5f;
    [SerializeField] private TerrainType[] availableTerrainTypes;

    private void Start()
    {
        if (!ValidateReferences())
        {
            Debug.LogError("GridTest: Missing required references. Please assign all required components in the inspector.");
            enabled = false;
            return;
        }
        
        gridManager.InitializeGrid();
        RandomizeAndPathFind();
    }

    private bool ValidateReferences()
    {
        if (gridManager == null || pathfinder == null || startMarker == null || 
            endMarker == null || pathLine == null || availableTerrainTypes == null || 
            availableTerrainTypes.Length == 0)
        {
            return false;
        }
        return true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RandomizeAndPathFind();
        }
    }
    
    private void RandomizeAndPathFind()
    {
        RandomizeAll();
        // Find and visualize path
        var path = pathfinder.FindPath(startMarker.position, endMarker.position);
        
        if (path != null && path.Count > 0)
        {
            pathLine.positionCount = path.Count;
            pathLine.SetPositions(path.ToArray());
        }
    }

    private void RandomizeAll()
    {
        RandomizeTerrain();
        RandomizeMarkers();
    }

    private void RandomizeTerrain()
    {
        int gridSizeX = gridManager.GridSettings.GridSizeX;
        int gridSizeY = gridManager.GridSettings.GridSizeY;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                TerrainType randomTerrain = availableTerrainTypes[Random.Range(0, availableTerrainTypes.Length)];
                gridManager.SetTerrainType(x, y, randomTerrain);
            }
        }
    }

    private void RandomizeMarkers()
    {
        int gridSizeX = gridManager.GridSettings.GridSizeX;
        int gridSizeY = gridManager.GridSettings.GridSizeY;
        float nodeSize = gridManager.GridSettings.NodeSize;

        // Randomize start marker
        int startX = Random.Range(0, gridSizeX);
        int startY = Random.Range(0, gridSizeY);
        startMarker.position = new Vector3(startX * nodeSize, markerHeight, startY * nodeSize);

        // Randomize end marker (ensuring it's different from start)
        int endX, endY;
        do
        {
            endX = Random.Range(0, gridSizeX);
            endY = Random.Range(0, gridSizeY);
        } while (endX == startX && endY == startY);
        
        endMarker.position = new Vector3(endX * nodeSize, markerHeight, endY * nodeSize);
    }
} 