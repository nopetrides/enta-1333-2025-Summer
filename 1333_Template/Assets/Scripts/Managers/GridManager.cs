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
    private bool _showGizmos = false; // Toggle flag for drawing gizmos
    public bool isInitialized { get; private set; }

    private HashSet<GridNode> _reservedNodes = new HashSet<GridNode>(); // Reserved node

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
        // Press 'X' to toggle grid gizmos on/off
        if (Input.GetKeyDown(KeyCode.X))
        {
            _showGizmos = !_showGizmos;
        }
        if (!_showGizmos || !isInitialized) return;
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
    /// Add node to reservedNodes harshset
    /// </summary>
    /// <param name="node"></param>
    public void ReserveNode(GridNode node)
    {
        if (node != null && !_reservedNodes.Contains(node))
            _reservedNodes.Add(node);
    }

    /// <summary>
    /// Remove node to reservedNodes harshset
    /// </summary>
    /// <param name="node"></param>
    public void UnreserveNode(GridNode node)
    {
        if (node != null)
            _reservedNodes.Remove(node);
    }

    /// <summary>
    /// Check if the node is reserved
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public bool IsNodeReserved(GridNode node)
    {
        return _reservedNodes.Contains(node);
    }

    /// <summary>
    /// Clear all reserved nodes
    /// </summary>
    public void ClearAllReservations()
    {
        _reservedNodes.Clear();
    }

    /// <summary>
    /// Returns the four direct neighbors (up, down, left, right) of a given node within grid bounds.
    /// </summary>
    /// <param name="node">The GridNode whose neighbors you want to retrieve.</param>
    /// <returns>An IEnumerable of adjacent GridNode objects.</returns>
    public IEnumerable<GridNode> GetNeighbors(GridNode node)
    {
        // Convert world position to grid indices
        int x = Mathf.RoundToInt(node.worldPosition.x / _gridSettings.NodeSize);
        int y = Mathf.RoundToInt(node.worldPosition.z / _gridSettings.NodeSize);

        // Yield the node above if within bounds
        if (y + 1 < _gridSettings.GridSizeY)
            yield return GetNode(x, y + 1);

        // Yield the node below if within bounds
        if (y - 1 >= 0)
            yield return GetNode(x, y - 1);

        // Yield the node to the right if within bounds
        if (x + 1 < _gridSettings.GridSizeX)
            yield return GetNode(x + 1, y);

        // Yield the node to the left if within bounds
        if (x - 1 >= 0)
            yield return GetNode(x - 1, y);
    }

    /// <summary>
    /// Finds up to a specified number of free nodes (walkable and not reserved) 
    /// starting from a center node, using breadth-first search.
    /// </summary>
    /// <param name="center">The starting GridNode for the search.</param>
    /// <param name="count">The maximum number of free nodes to return.</param>
    /// <returns>A list of GridNode objects that are walkable and not reserved.</returns>
    public List<GridNode> FindNearestFreeNodes(GridNode center, int count)
    {
        List<GridNode> result = new List<GridNode>();
        HashSet<GridNode> checkedNodes = new HashSet<GridNode>();
        Queue<GridNode> queue = new Queue<GridNode>();

        // Begin BFS from the center node
        queue.Enqueue(center);
        checkedNodes.Add(center);

        // Continue until queue is empty or desired count is reached
        while (queue.Count > 0 && result.Count < count)
        {
            GridNode node = queue.Dequeue();

            // If this node is walkable and not reserved, add to results
            if (node.walkable && !IsNodeReserved(node))
                result.Add(node);

            // Enqueue each neighbor that has not yet been checked
            foreach (GridNode neighbor in GetNeighbors(node))
            {
                if (!checkedNodes.Contains(neighbor))
                {
                    checkedNodes.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }

        return result;
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
    /// Converts a grid index (tile origin) to world-space position.
    /// If <paramref name="center"/> is true, returns the cell center,
    /// otherwise the bottom-left (XZ) or bottom-left-front (XY) corner.
    /// </summary>
    public Vector3 IdxToWorld(Vector2Int idx, bool center = false)
    {
        float s = _gridSettings.NodeSize;
        float hs = center ? s * 0.5f : 0f;

        if (_gridSettings.UseXZPlane)
            return new Vector3(idx.x * s + hs, 0f, idx.y * s + hs);

        // XY plane
        return new Vector3(idx.x * s + hs, idx.y * s + hs, 0f);
    }

    /// <summary>
    /// Draws gizmos in the editor to visualize the grid and node colors.
    /// Only runs if the grid is initialized and showGizmos is true.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!isInitialized || _gridNodes == null || !_showGizmos) return;

        float size = _gridSettings.NodeSize * 0.9f;
        Vector3 halfOffset = Vector3.one * (_gridSettings.NodeSize * 0.5f);

        for (int x = 0; x < _gridSettings.GridSizeX; x++)
            for (int y = 0; y < _gridSettings.GridSizeY; y++)
            {
                var node = _gridNodes[x, y];
                Vector3 center = node.worldPosition;

                if (!node.walkable)
                {
                    // draw solid red cube for blocked nodes
                    Gizmos.color = Color.red;
                    Gizmos.DrawCube(center, Vector3.one * size);
                }
                else if (_showGizmos)
                {
                    // draw your normal wireframe for walkable nodes
                    Gizmos.color = node.GizmoColor;
                    Gizmos.DrawWireCube(center, Vector3.one * size);
                }
            }
    }
}
