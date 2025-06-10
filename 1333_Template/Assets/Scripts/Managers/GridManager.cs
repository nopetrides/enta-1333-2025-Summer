using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the creation and querying of a grid composed of GridNode instances.
/// Generates grid nodes based on settings and provides methods to access nodes by world position or random selection.
/// </summary>
public class GridManager : MonoBehaviour
{
    [SerializeField] private GridSettings _gridSettings;
    [SerializeField] private TerrainType[] _terrainTypes;

    private GridNode[,] _gridNodes;
    public bool isInitialized { get; private set; }

    /// <summary>
    /// Exposes the GridSettings so other classes can access configuration values.
    /// </summary>
    public GridSettings GridSettings => _gridSettings;

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Ensures that the grid is initialized on startup.
    /// </summary>
    private void Awake()
    {
        InitializeGrid();
    }

    /// <summary>
    /// Called once per frame.
    /// Allows reinitialization of the grid when the "O" key is pressed.
    /// </summary>
    private void Update()
    {
        // Reinitialize grid when pressing O
        if (Input.GetKeyDown(KeyCode.O))
        {
            InitializeGrid();
        }
    }

    /// <summary>
    /// Initializes or recreates the grid based on the current GridSettings and TerrainTypes.
    /// Fills the internal _gridNodes array with new GridNode instances.
    /// </summary>
    public void InitializeGrid()
    {
        int sizeX = _gridSettings.GridSizeX;
        int sizeY = _gridSettings.GridSizeY;
        _gridNodes = new GridNode[sizeX, sizeY];

        // Loop through each cell coordinate to create a GridNode
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                // Determine world position based on whether the grid uses XZ or XY plane
                Vector3 worldPos = _gridSettings.UseXZPlane
                    ? new Vector3(x, 0, y) * _gridSettings.NodeSize
                    : new Vector3(x, y, 0) * _gridSettings.NodeSize;

                // Select a random TerrainType from the provided array
                TerrainType terrain = _terrainTypes[Random.Range(0, _terrainTypes.Length)];

                // Create and configure a new GridNode
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

        // Mark the grid as initialized
        isInitialized = true;
    }

    /// <summary>
    /// Converts a world-space position to the nearest GridNode.
    /// Calculates grid indices by dividing by NodeSize, rounding to the nearest integer, and clamping to valid ranges.
    /// </summary>
    /// <param name="position">The world-space position to query.</param>
    /// <returns>The GridNode instance closest to the given position.</returns>
    public GridNode getNodeFromWorldPosition(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x / _gridSettings.NodeSize);
        int y = Mathf.RoundToInt(
            _gridSettings.UseXZPlane
                ? position.z / _gridSettings.NodeSize
                : position.y / _gridSettings.NodeSize
        );

        // Clamp indices to ensure they fall within the grid bounds
        x = Mathf.Clamp(x, 0, _gridSettings.GridSizeX - 1);
        y = Mathf.Clamp(y, 0, _gridSettings.GridSizeY - 1);

        return GetNode(x, y);
    }

    /// <summary>
    /// Returns the GridNode at the specified grid coordinates.
    /// If the grid is not yet initialized, this method will initialize it first.
    /// </summary>
    /// <param name="x">The x-coordinate index in the grid.</param>
    /// <param name="y">The y-coordinate index in the grid.</param>
    /// <returns>The GridNode located at (x, y).</returns>
    public GridNode GetNode(int x, int y)
    {
        if (!isInitialized) InitializeGrid();

        if (x < 0 || x >= _gridSettings.GridSizeX || y < 0 || y >= _gridSettings.GridSizeY)
            return null;

        return _gridNodes[x, y];
    }

    /// <summary>
    /// Marks a given cell as walkable or not. 
    /// </summary>
    public void SetWalkable(int x, int y, bool isWalkable)
    {
        if (!isInitialized) InitializeGrid();

        // guard against out of bound
        if (x < 0 || x >= _gridSettings.GridSizeX ||
            y < 0 || y >= _gridSettings.GridSizeY)
        {
            Debug.LogWarning($"SetWalkable: ({x},{y}) is outside grid bounds.");
            return;
        }

        _gridNodes[x, y].walkable = isWalkable;
    }

    /// <summary>
    /// Finds and returns a random walkable GridNode from the entire grid.
    /// Returns null if no walkable nodes are available.
    /// </summary>
    /// <returns>A randomly selected walkable GridNode, or null if none are walkable.</returns>
    public GridNode GetRandomWalkableNode()
    {
        int gridWidth = _gridSettings.GridSizeX;
        int gridHeight = _gridSettings.GridSizeY;

        List<GridNode> walkableNodes = new List<GridNode>();

        // Collect all walkable nodes into a list
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

        // If there are no walkable nodes, return null
        if (walkableNodes.Count == 0)
        {
            return null;
        }

        // Choose a random index from the list of walkable nodes
        int index = Random.Range(0, walkableNodes.Count);
        return walkableNodes[index];
    }

    /// <summary>
    /// Draws gizmos in the editor to visualize the grid and node colors.
    /// Only runs if the grid is initialized.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!isInitialized || _gridNodes == null) return;

        float half = _gridSettings.NodeSize * 0.5f;

        // Draw a wire cube for each grid node using the node's GizmoColor
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
}
