using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour // the main controller that ties together grid, units, pathfinding, and armies
{
    [Header("Main System References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private UnitManager unitManager;
    [SerializeField] private AStarPathfinder pathfinder;
    [SerializeField] private Transform startMarker;
    [SerializeField] private Transform endMarker;
    [SerializeField] private LineRenderer pathLine;
    [SerializeField] private WaveManager waveManager;

    [Header("Marker & Army Settings")]
    [SerializeField] private float markerHeight = 0.5f;
    [SerializeField] private AvailableUnits defaultUnits;

    [Header("Terrain Settings")]
    [SerializeField] private List<TerrainType> terrains; 

    [Header("Mask Settings")]
    [SerializeField] private Texture2D pathMask;                   // png mask to create 2d grıd accordıngly
    [SerializeField] private TerrainType grassTerrainType;       
    [SerializeField] private TerrainType dangerTerrainType;       
    [SerializeField] private Color grassColor = new Color(0.8f, 1f, 0.1f); 
    [SerializeField] private Color dangerColor = new Color(1f, 0f, 0f);  
    [SerializeField, Range(0, 0.5f)] private float colorTolerance = 0.25f;

    public static GameManager Instance;

    private TeamArmies allTeams = new TeamArmies();
    private bool _initializedForGameplay = false; 

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        if (!AreReferencesSet())
        {
            Debug.LogError("Missing references!");
            enabled = false;
            return;
        }
        // gamesceneinitializer  calls InitializeForGameplay()
    }

   
    // entry point for gameplay setup. called by gamesceneinitializer when game state is playing
 
    public void InitializeForGameplay()
    {
        if (_initializedForGameplay) return;
        _initializedForGameplay = true;

        gridManager.InitializeGrid();
        ApplyTerrainMask();

        if (waveManager != null)
            waveManager.Initialize();

        Debug.Log("GameManager: Gameplay initialized.");
    }

    private bool AreReferencesSet()
    {
        return gridManager && unitManager && pathfinder && startMarker && endMarker && pathLine && waveManager;
    }

    public void ApplyTerrainMask()
    {
        if (pathMask == null || grassTerrainType == null || dangerTerrainType == null)
        {
            Debug.LogError("Assign pathMask, grassTerrainType, and dangerTerrainType in inspector!");
            return;
        }

        int width = gridManager.GridSettings.GridSizeX;
        int height = gridManager.GridSettings.GridSizeY;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                
                int texX = Mathf.Clamp(Mathf.RoundToInt((float)x / (width - 1) * (pathMask.width - 1)), 0, pathMask.width - 1);
                int texY = Mathf.Clamp(Mathf.RoundToInt((float)y / (height - 1) * (pathMask.height - 1)), 0, pathMask.height - 1);

              

                Color pixel = pathMask.GetPixel(texX, texY);

                GridNode node = gridManager.GetNode(x, y);

                if (ColorDistance(pixel, grassColor) < colorTolerance)
                {
                    node.Walkable = grassTerrainType.IsWalkable;
                    node.Weight = grassTerrainType.MovementCost;
                    node.TerrainType = grassTerrainType;
                }
                else if (ColorDistance(pixel, dangerColor) < colorTolerance)
                {
                    node.Walkable = dangerTerrainType.IsWalkable;
                    node.Weight = dangerTerrainType.MovementCost;
                    node.TerrainType = dangerTerrainType;
                }
                else
                {
                   
                    node.Walkable = dangerTerrainType.IsWalkable;
                    node.Weight = dangerTerrainType.MovementCost;
                    node.TerrainType = dangerTerrainType;
                }

                gridManager.SetNode(x, y, node);
            }
        }
        Debug.Log("Applied terrain mask!");
    }

   
    float ColorDistance(Color a, Color b)
    {
        return Mathf.Sqrt(
            Mathf.Pow(a.r - b.r, 2) +
            Mathf.Pow(a.g - b.g, 2) +
            Mathf.Pow(a.b - b.b, 2)
        );
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ApplyTerrainMask();
        }
    }

    public void OnWin()
    {
        
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.SetState(GameState.Win);

        //UIManager.Instance?.ShowWin();
    }

    public void OnLose()
    {
       
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.SetState(GameState.Lose);

        UIManager.Instance?.ShowLosePanel();
    }

    public void GoToMain()
    {
        if (GameBoot.Instance != null)
        {
            GameBoot.Instance.ReturnToMenu();
        }
        else
        {
            SceneManager.LoadScene("MainMenu");

        }
    }
}
