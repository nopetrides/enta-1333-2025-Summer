// ===== GridManager.cs =====
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private GridSettings _gridSettings;
    public GridSettings GridSettings => _gridSettings;

    [Header("Terrain Types (ScriptableObjects)")]
    [SerializeField] private TerrainType[] _terrainTypes;

    private GridNode[,] _gridNodes;
    private PathfindingManager _pathfindingManager;
    public bool isInitialized { get; private set; }

    private void Awake()
    {
        InitializeGrid();
        _pathfindingManager = GetComponent<PathfindingManager>();
    }

    private void Update()
    {
        // Regenerate grid and notify pathfinder when Space is pressed
        if (Input.GetKeyDown(KeyCode.Space))
        {
            InitializeGrid();
            // Notify pathfinding manager to recalc
            if (_pathfindingManager != null)
                _pathfindingManager.GridUpdated();
        }
    }

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

                TerrainType terrain = _terrainTypes[
                    Random.Range(0, _terrainTypes.Length)
                ];


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

        // Assign random start and goal on safe (walkable) terrain
        if (_pathfindingManager != null)
        {
            var safeCoords = new List<Vector2Int>();
            for (int x = 0; x < sizeX; x++)
                for (int y = 0; y < sizeY; y++)
                    if (_gridNodes[x, y].walkable)
                        safeCoords.Add(new Vector2Int(x, y));

            if (safeCoords.Count >= 2)
            {
                // pick start
                int idx = Random.Range(0, safeCoords.Count);
                var start = safeCoords[idx];
                safeCoords.RemoveAt(idx);
                // pick goal
                int idx2 = Random.Range(0, safeCoords.Count);
                var goal = safeCoords[idx2];

                _pathfindingManager.startCoordinates = start;
                _pathfindingManager.goalCoordinates = goal;
            }
        }

        isInitialized = true;
    }
    /// <summary>
    /// Convert a world position to the nearest valid grid node.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public GridNode getNodeFromWorldPosition(Vector3 position)
    {
        int x = _gridSettings.UseXZPlane ? Mathf.RoundToInt(position.x / _gridSettings.NodeSize) : Mathf.RoundToInt(position.x / _gridSettings.NodeSize);
        int y = _gridSettings.UseXZPlane ? Mathf.RoundToInt(position.z / _gridSettings.NodeSize) : Mathf.RoundToInt(position.y / _gridSettings.NodeSize);
        x = Mathf.Clamp(x, 0, _gridSettings.GridSizeX - 1);
        y = Mathf.Clamp(y, 0, _gridSettings.GridSizeY - 1);

        return GetNode(x, y);
    }

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
            for (int y = 0; y < _gridSettings.GridSizeY; y++)
            {
                GridNode node = _gridNodes[x, y];
                Gizmos.color = node.GizmoColor;
                Gizmos.DrawWireCube(
                    node.worldPosition,
                    Vector3.one * (_gridSettings.NodeSize * 0.9f)
                );
            }
    }
}
