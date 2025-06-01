using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the creation and querying of the grid of nodes.
/// </summary>
public class GridManager : MonoBehaviour
{
    [SerializeField] private GridSettings _gridSettings;
    [SerializeField] private TerrainType[] _terrainTypes;

    private GridNode[,] _gridNodes;
    private PathfindingManager _pathfindingManager;
    public bool isInitialized { get; private set; }

    /// <summary>
    /// Exposes the GridSettings so other classes (e.g., AStarPathfinder) can access it.
    /// </summary>
    public GridSettings GridSettings => _gridSettings;

    private void Awake()
    {
        InitializeGrid();
        _pathfindingManager = GetComponent<PathfindingManager>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            InitializeGrid();
            if (_pathfindingManager != null)
                _pathfindingManager.GridUpdated();
        }
    }

    /// <summary>
    /// Initializes the grid based on GridSettings and TerrainTypes.
    /// </summary>
    public void InitializeGrid()
    {
        int sizeX = _gridSettings.GridSizeX;
        int sizeY = _gridSettings.GridSizeY;
        _gridNodes = new GridNode[sizeX, sizeY];

        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                Vector3 worldPos = _gridSettings.UseXZPlane
                    ? new Vector3(x, 0, y) * _gridSettings.NodeSize
                    : new Vector3(x, y, 0) * _gridSettings.NodeSize;

                TerrainType terrain = _terrainTypes[Random.Range(0, _terrainTypes.Length)];

                GridNode node = new GridNode
                {
                    name = $"{terrain.TerrainName}_{x}_{y}",
                    worldPosition = worldPos,
                    terrainType = terrain,
                    walkable = terrain.Walkable,
                    weight = terrain.MovementCost
                };

                _gridNodes[x, y] = node;
            }
        }

        // Assign random start/goal to PathfindingManager if available
        if (_pathfindingManager != null)
        {
            var safeCoords = new List<Vector2Int>();
            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    if (_gridNodes[x, y].walkable)
                        safeCoords.Add(new Vector2Int(x, y));
                }
            }

            if (safeCoords.Count >= 2)
            {
                int idx = Random.Range(0, safeCoords.Count);
                var start = safeCoords[idx];
                safeCoords.RemoveAt(idx);
                int idx2 = Random.Range(0, safeCoords.Count);
                var goal = safeCoords[idx2];

                _pathfindingManager.startCoordinates = start;
                _pathfindingManager.goalCoordinates = goal;
            }
        }

        isInitialized = true;
    }

    /// <summary>
    /// Converts a world position to the corresponding GridNode.
    /// </summary>
    public GridNode getNodeFromWorldPosition(Vector3 position)
    {
        int x = _gridSettings.UseXZPlane
            ? Mathf.RoundToInt(position.x / _gridSettings.NodeSize)
            : Mathf.RoundToInt(position.x / _gridSettings.NodeSize);
        int y = _gridSettings.UseXZPlane
            ? Mathf.RoundToInt(position.z / _gridSettings.NodeSize)
            : Mathf.RoundToInt(position.y / _gridSettings.NodeSize);

        x = Mathf.Clamp(x, 0, _gridSettings.GridSizeX - 1);
        y = Mathf.Clamp(y, 0, _gridSettings.GridSizeY - 1);

        return GetNode(x, y);
    }

    /// <summary>
    /// Returns the GridNode at the given (x, y) coordinates.
    /// </summary>
    public GridNode GetNode(int x, int y)
    {
        if (!isInitialized) InitializeGrid();
        return _gridNodes[x, y];
    }

    private void OnDrawGizmos()
    {
        if (!isInitialized || _gridNodes == null) return;
        float half = _gridSettings.NodeSize * 0.5f;
        for (int x = 0; x < _gridSettings.GridSizeX; x++)
        {
            for (int y = 0; y < _gridSettings.GridSizeY; y++)
            {
                GridNode node = _gridNodes[x, y];
                Gizmos.color = node.GizmoColor;
                Gizmos.DrawWireCube(node.worldPosition, Vector3.one * (_gridSettings.NodeSize * 0.9f));
            }
        }
    }

    /// <summary>
    /// Finds and returns a random walkable GridNode in the grid.
    /// Returns null if no walkable nodes exist.
    /// </summary>
    public GridNode? GetRandomWalkableNode()
    {
        int gridWidth = _gridSettings.GridSizeX;
        int gridHeight = _gridSettings.GridSizeY;

        List<GridNode> walkableNodes = new List<GridNode>();

        // Collect all walkable nodes
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                GridNode node = _gridNodes[x, y];
                if (node.walkable)
                {
                    walkableNodes.Add(node);
                }
            }
        }

        if (walkableNodes.Count == 0)
        {
            return null; // no valid spawn node
        }

        // Pick one at random
        int index = Random.Range(0, walkableNodes.Count);
        return walkableNodes[index];
    }
}
