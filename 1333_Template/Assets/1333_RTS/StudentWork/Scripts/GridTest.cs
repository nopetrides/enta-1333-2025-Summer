using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

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
    [SerializeField] private int currentSeed = 0;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI controlsText;

    private System.Random seededRandom;

    private void Start()
    {
        if (!ValidateReferences())
        {
            Debug.LogError("GridTest: Missing required references. Please assign all required components in the inspector.");
            enabled = false;
            return;
        }
        
        gridManager.InitializeGrid();
        GenerateNewSeed();
        StartCoroutine(RandomizeAndPathFind());
        UpdateControlsText();
    }

    private void UpdateControlsText()
    {
        if (controlsText != null)
        {
            controlsText.text = "Controls:\n" +
                              "R - Generate new random seed and reset\n" +
                              "T - Reset using current seed\n" +
                              "Space - Pause/Resume pathfinding\n" +
                              "Right Arrow - Step forward (when paused)\n" +
                              $"Current Seed: {currentSeed}";
        }
    }

    private void GenerateNewSeed()
    {
        currentSeed = Random.Range(int.MinValue, int.MaxValue);
        seededRandom = new System.Random(currentSeed);
        pathfinder.SetSeed(currentSeed);
        Debug.Log($"Generated new seed: {currentSeed}");
        UpdateControlsText();
    }

    private void ResetWithCurrentSeed()
    {
        seededRandom = new System.Random(currentSeed);
        pathfinder.SetSeed(currentSeed);
        StartCoroutine(RandomizeAndPathFind());
        Debug.Log($"Reset with seed: {currentSeed}");
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
        if (Input.GetKeyDown(KeyCode.R))
        {
            GenerateNewSeed();
            StartCoroutine(RandomizeAndPathFind());
        }
        else if (Input.GetKeyDown(KeyCode.T))
        {
            ResetWithCurrentSeed();
        }
    }
    
    private IEnumerator RandomizeAndPathFind()
    {
        RandomizeAll();
        // Find and visualize path
        yield return StartCoroutine(pathfinder.FindPath(startMarker.position, endMarker.position));
        
        // The pathfinder will update the path visualization internally
        // We don't need to manually set the line renderer positions anymore
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
                TerrainType randomTerrain = availableTerrainTypes[seededRandom.Next(0, availableTerrainTypes.Length)];
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
        int startX = seededRandom.Next(0, gridSizeX);
        int startY = seededRandom.Next(0, gridSizeY);
        startMarker.position = new Vector3(startX * nodeSize, markerHeight, startY * nodeSize);

        // Randomize end marker (ensuring it's different from start)
        int endX, endY;
        do
        {
            endX = seededRandom.Next(0, gridSizeX);
            endY = seededRandom.Next(0, gridSizeY);
        } while (endX == startX && endY == startY);
        
        endMarker.position = new Vector3(endX * nodeSize, markerHeight, endY * nodeSize);
    }
} 