using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class Pathfinder : MonoBehaviour
{
    public enum PathfindingType
    {
        Unweighted,     // BFS - Fastest, ignores weights
        Weighted,       // Dijkstra's - Handles weights, optimal path
        BruteForce,     // Explores all paths - Demonstrates why we need better algorithms
        Naive          // "Draws" a line to target - Common beginner mistake
    }

    public enum VisualizationState
    {
        Idle,
        Exploring,
        Reconstructing,
        Paused
    }

    [Header("Required References")]
    [SerializeField] private GridManager gridManager;

    [Header("Pathfinding Settings")]
    [SerializeField] private PathfindingType pathfindingType = PathfindingType.Unweighted;
    [SerializeField, Range(0, 100)] private int framesPerStep = 10;
    [SerializeField] private bool visualizePathfinding = true;

    [Header("Visualization Colors")]
    [SerializeField] private Color startNodeColor = Color.green;
    [SerializeField] private Color endNodeColor = Color.red;
    [SerializeField] private Color currentPathColor = Color.yellow;
    [SerializeField] private Color visitedNodeColor = new Color(0.5f, 0.5f, 1f, 0.5f);
    [SerializeField] private Color unvisitedNodeColor = new Color(0.3f, 0.3f, 0.3f, 0.3f);
    [SerializeField] private Color finalPathColor = Color.cyan;
    [SerializeField] private Color currentNodeColor = Color.magenta;
    [SerializeField] private Color currentNeighborColor = new Color(1f, 0.5f, 0f, 0.5f); // Orange
    [SerializeField] private Color explorationLineColor = new Color(1f, 1f, 0f, 0.3f); // Semi-transparent yellow

    [Header("Visualization Settings")]
    [SerializeField] private int currentSeed = 0;
    [SerializeField] private bool useSeededRandom = true;
    [SerializeField] private float minWeight = 1f;
    [SerializeField] private float maxWeight = 10f;

    // Pathfinder instances
    private UnweightedPathfinder unweightedPathfinder;
    private WeightedPathfinder weightedPathfinder;
    private BruteForcePathfinder bruteForcePathfinder;
    private NaivePathfinder naivePathfinder;

    // Visualizer instances
    private UnweightedPathVisualizer unweightedVisualizer;
    private WeightedPathVisualizer weightedVisualizer;
    private BruteForcePathVisualizer bruteForceVisualizer;
    private NaivePathVisualizer naiveVisualizer;

    private System.Random seededRandom;

    // Visualization state
    private HashSet<Vector2Int> visitedNodes = new HashSet<Vector2Int>();
    private Dictionary<Vector2Int, int> nodeDistances = new Dictionary<Vector2Int, int>();  // Track distances from start
    private List<Vector2Int> currentPath = new List<Vector2Int>();
    private List<Vector2Int> reconstructionPath = new List<Vector2Int>();
    private Vector2Int? startNode;
    private Vector2Int? endNode;
    private VisualizationState visualizationState = VisualizationState.Idle;
    private Coroutine currentVisualization;
    private List<Vector3> finalPath;
    private bool shouldPause;
    private bool isStepMode;
    private List<Vector2Int> explorationOrder;
    private Vector2Int? currentNode;
    private List<Vector2Int> currentNeighbors;

    private void Update()
    {
        if (!visualizePathfinding) return;

        // Space to pause/resume
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (visualizationState == VisualizationState.Paused)
            {
                visualizationState = VisualizationState.Exploring;
                shouldPause = false;
                isStepMode = false;
            }
            else if (visualizationState == VisualizationState.Exploring || 
                     visualizationState == VisualizationState.Reconstructing)
            {
                visualizationState = VisualizationState.Paused;
                shouldPause = true;
                isStepMode = false;
            }
        }

        // Right arrow for next step
        if (Input.GetKeyDown(KeyCode.RightArrow) && visualizationState == VisualizationState.Paused)
        {
            isStepMode = true;
            shouldPause = false;
        }
    }

    private void OnDrawGizmos()
    {
        if (!visualizePathfinding || !Application.isPlaying) return;

        float nodeSize = gridManager.GridSettings.NodeSize;

        // Draw exploration history line
        if (explorationOrder != null && explorationOrder.Count > 1)
        {
            Gizmos.color = explorationLineColor;
            for (int i = 0; i < explorationOrder.Count - 1; i++)
            {
                Vector3 start = GridToWorld(explorationOrder[i]);
                Vector3 end = GridToWorld(explorationOrder[i + 1]);
                Gizmos.DrawLine(start, end);
            }
        }

        // Draw visited nodes
        Gizmos.color = visitedNodeColor;
        foreach (Vector2Int node in visitedNodes)
        {
            Vector3 worldPos = GridToWorld(node);
            Gizmos.DrawCube(worldPos, Vector3.one * nodeSize * 0.8f);
            
            // Draw distance number if available
            if (nodeDistances.TryGetValue(node, out int distance))
            {
                UnityEditor.Handles.Label(worldPos + Vector3.up * nodeSize * 0.5f, distance.ToString());
            }
        }

        // Draw current node being evaluated
        if (currentNode.HasValue)
        {
            Gizmos.color = currentNodeColor;
            Vector3 currentPos = GridToWorld(currentNode.Value);
            Gizmos.DrawCube(currentPos, Vector3.one * nodeSize * 0.9f);
        }

        // Draw current neighbors being evaluated
        if (currentNeighbors != null)
        {
            Gizmos.color = currentNeighborColor;
            foreach (Vector2Int neighbor in currentNeighbors)
            {
                Vector3 neighborPos = GridToWorld(neighbor);
                Gizmos.DrawCube(neighborPos, Vector3.one * nodeSize * 0.7f);
            }
        }

        // Draw current path
        Gizmos.color = currentPathColor;
        for (int i = 0; i < currentPath.Count - 1; i++)
        {
            Vector3 start = GridToWorld(currentPath[i]);
            Vector3 end = GridToWorld(currentPath[i + 1]);
            Gizmos.DrawLine(start, end);
        }

        // Draw final path
        if (finalPath != null && finalPath.Count > 1)
        {
            Gizmos.color = finalPathColor;
            for (int i = 0; i < finalPath.Count - 1; i++)
            {
                Gizmos.DrawLine(finalPath[i], finalPath[i + 1]);
            }
        }

        // Draw start and end nodes
        if (startNode.HasValue)
        {
            Gizmos.color = startNodeColor;
            Gizmos.DrawCube(GridToWorld(startNode.Value), Vector3.one * nodeSize);
        }
        if (endNode.HasValue)
        {
            Gizmos.color = endNodeColor;
            Gizmos.DrawCube(GridToWorld(endNode.Value), Vector3.one * nodeSize);
        }
    }

    private void Awake()
    {
        if (gridManager == null)
        {
            Debug.LogError("Pathfinder: GridManager reference is missing. Please assign it in the inspector.");
            enabled = false;
            return;
        }

        // Initialize pathfinders
        unweightedPathfinder = new UnweightedPathfinder(GetNeighbors, WaitForNextStep);
        weightedPathfinder = new WeightedPathfinder(GetNeighbors, GetMovementCost);
        bruteForcePathfinder = new BruteForcePathfinder(GetNeighbors);
        naivePathfinder = new NaivePathfinder(GetNeighbors, gridManager.GridSettings.AllowDiagonal);

        // Initialize visualizers
        unweightedVisualizer = new UnweightedPathVisualizer(
            unweightedPathfinder, gridManager, StartCoroutine);
        weightedVisualizer = new WeightedPathVisualizer(
            weightedPathfinder, gridManager, WaitForNextStep, StartCoroutine);
        bruteForceVisualizer = new BruteForcePathVisualizer(
            bruteForcePathfinder, gridManager, WaitForNextStep, StartCoroutine);
        naiveVisualizer = new NaivePathVisualizer(
            naivePathfinder, gridManager, WaitForNextStep, StartCoroutine);
    }

    private void Start()
    {
        if (useSeededRandom)
        {
            seededRandom = new System.Random(currentSeed);
        }
    }

    public void SetSeed(int seed)
    {
        currentSeed = seed;
        if (useSeededRandom)
        {
            seededRandom = new System.Random(currentSeed);
        }
    }

    private float GetRandomWeight()
    {
        if (useSeededRandom)
        {
            return (float)seededRandom.NextDouble() * (maxWeight - minWeight) + minWeight;
        }
        return Random.Range(minWeight, maxWeight);
    }

    public IEnumerator FindPath(Vector3 startPos, Vector3 endPos)
    {
        if (!enabled) yield break;

        // Stop any existing visualization
        if (currentVisualization != null)
        {
            StopCoroutine(currentVisualization);
        }

        // Reset visualization state
        visitedNodes.Clear();
        nodeDistances.Clear();  // Clear distances
        currentPath.Clear();
        reconstructionPath.Clear(); // Clear previous final path
        finalPath = null; // Clear previous world path
        explorationOrder = null;
        currentNode = null;
        currentNeighbors = null;
        startNode = WorldToGrid(startPos);
        endNode = WorldToGrid(endPos);
        visualizationState = VisualizationState.Exploring;
        shouldPause = false;

        // Convert world positions to grid coordinates
        Vector2Int startCoord = startNode.Value;
        Vector2Int endCoord = endNode.Value;

        // Validate coordinates
        if (!IsValidCoordinate(startCoord) || !IsValidCoordinate(endCoord))
        {
            Debug.LogWarning("Invalid start or end coordinates for pathfinding");
            yield break;
        }

        // Set initial distance for start node
        nodeDistances[startCoord] = 0;

        if (visualizePathfinding)
        {
            currentVisualization = StartCoroutine(VisualizePathfinding(startCoord, endCoord));
            yield break;
        }
        else
        {
            switch (pathfindingType)
            {
                case PathfindingType.Unweighted:
                    List<Vector2Int> finalUnweightedPath = null;
                    yield return StartCoroutine(unweightedPathfinder.FindPath(startCoord, endCoord, (path, cameFrom, visited, order, current, neighbors, distances) =>
                    {
                        visitedNodes = visited;
                        reconstructionPath = path;
                        explorationOrder = order;
                        currentNode = current;
                        currentNeighbors = neighbors;
                        nodeDistances = distances;
                        if (path.Count > 0 && path[0] == startCoord && path[path.Count - 1] == endCoord)
                        {
                            finalUnweightedPath = path;
                        }
                    }));
                    if (finalUnweightedPath != null)
                    {
                        finalPath = ConvertPathToWorldPositions(finalUnweightedPath);
                    }
                    break;
                case PathfindingType.Weighted:
                    var (weightedPath, _, _) = weightedPathfinder.FindPath(startCoord, endCoord);
                    finalPath = ConvertPathToWorldPositions(weightedPath);
                    break;
                case PathfindingType.BruteForce:
                    var (bruteForcePath, _) = bruteForcePathfinder.FindPath(startCoord, endCoord);
                    finalPath = ConvertPathToWorldPositions(bruteForcePath);
                    break;
                case PathfindingType.Naive:
                    var (naivePath, _) = naivePathfinder.FindPath(startCoord, endCoord);
                    finalPath = ConvertPathToWorldPositions(naivePath);
                    break;
            }
        }
    }

    private IEnumerator VisualizePathfinding(Vector2Int start, Vector2Int end)
    {
        switch (pathfindingType)
        {
            case PathfindingType.Unweighted:
                yield return StartCoroutine(unweightedPathfinder.FindPath(start, end, (path, cameFrom, visited, order, current, neighbors, distances) =>
                {
                    visitedNodes = visited;
                    reconstructionPath = path;
                    explorationOrder = order;
                    currentNode = current;
                    currentNeighbors = neighbors;
                    nodeDistances = distances;
                    if (path.Count > 0 && path[0] == start && path[path.Count - 1] == end)
                    {
                        finalPath = ConvertPathToWorldPositions(path);
                    }
                }));
                break;
            case PathfindingType.Weighted:
                yield return StartCoroutine(weightedVisualizer.VisualizePath(
                    start, end, visitedNodes, currentPath, OnPathFound));
                break;
            case PathfindingType.BruteForce:
                yield return StartCoroutine(bruteForceVisualizer.VisualizePath(
                    start, end, visitedNodes, currentPath, OnPathFound));
                break;
            case PathfindingType.Naive:
                yield return StartCoroutine(naiveVisualizer.VisualizePath(
                    start, end, visitedNodes, currentPath, OnPathFound));
                break;
        }
    }

    private void OnPathFound(List<Vector3> path)
    {
        finalPath = path;
        reconstructionPath.Clear();
        foreach (var pos in path)
        {
            reconstructionPath.Add(WorldToGrid(pos));
        }
        visualizationState = VisualizationState.Idle;
    }

    private IEnumerator WaitForNextStep()
    {
        if (isStepMode)
        {
            // In step mode, wait for the next step
            shouldPause = true;
            isStepMode = false;
            yield return new WaitUntil(() => !shouldPause);
        }
        else
        {
            // Normal mode, wait for frames
            for (int i = 0; i < framesPerStep; i++)
            {
                if (shouldPause)
                {
                    yield return new WaitUntil(() => !shouldPause);
                }
                yield return null;
            }
        }
    }

    private List<Vector3> ConvertPathToWorldPositions(List<Vector2Int> path)
    {
        List<Vector3> worldPath = new List<Vector3>();
        foreach (Vector2Int coord in path)
        {
            worldPath.Add(GridToWorld(coord));
        }
        return worldPath;
    }

    private Vector2Int WorldToGrid(Vector3 worldPos)
    {
        float nodeSize = gridManager.GridSettings.NodeSize;
        return new Vector2Int(
            Mathf.RoundToInt(worldPos.x / nodeSize),
            Mathf.RoundToInt(worldPos.z / nodeSize)
        );
    }

    private Vector3 GridToWorld(Vector2Int gridPos)
    {
        float nodeSize = gridManager.GridSettings.NodeSize;
        return new Vector3(gridPos.x * nodeSize, 0, gridPos.y * nodeSize);
    }

    private bool IsValidCoordinate(Vector2Int coord)
    {
        return coord.x >= 0 && coord.x < gridManager.GridSettings.GridSizeX &&
               coord.y >= 0 && coord.y < gridManager.GridSettings.GridSizeY;
    }

    private List<Vector2Int> GetNeighbors(Vector2Int coord)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        int[] dx = { -1, 0, 1, 0, -1, -1, 1, 1 };
        int[] dy = { 0, 1, 0, -1, -1, 1, -1, 1 };

        // Check cardinal directions first
        for (int i = 0; i < 4; i++)
        {
            Vector2Int neighbor = new Vector2Int(coord.x + dx[i], coord.y + dy[i]);
            if (IsValidCoordinate(neighbor) && gridManager.IsWalkable(neighbor))
            {
                neighbors.Add(neighbor);
            }
        }

        // Check diagonal directions if allowed
        if (gridManager.GridSettings.AllowDiagonal)
        {
            for (int i = 4; i < 8; i++)
            {
                Vector2Int neighbor = new Vector2Int(coord.x + dx[i], coord.y + dy[i]);
                if (IsValidCoordinate(neighbor) && gridManager.IsWalkable(neighbor))
                {
                    // Check if we can cut corners
                    Vector2Int cardinal1 = new Vector2Int(coord.x + dx[i - 4], coord.y + dy[i - 4]);
                    Vector2Int cardinal2 = new Vector2Int(coord.x + dx[(i - 3) % 4], coord.y + dy[(i - 3) % 4]);
                    if (gridManager.IsWalkable(cardinal1) && gridManager.IsWalkable(cardinal2))
                    {
                        neighbors.Add(neighbor);
                    }
                }
            }
        }

        return neighbors;
    }

    private float GetMovementCost(Vector2Int from, Vector2Int to)
    {
        float baseCost = Vector2Int.Distance(from, to);
        float weight = gridManager.GetNodeWeight(to);
        return baseCost * weight;
    }
} 