using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Manages the grid data
public class GridManager : MonoBehaviour
{
    [SerializeField] private GridSettings gridSettings;
    public GridSettings GridSettings => gridSettings;

    private GridNode[,] gridNodes;

#if UNITY_EDITOR
    [Header("Debug for editor playmode only")]
    [SerializeField] private List<GridNode> AllNodes = new();
    [SerializeField] private bool showGrid = true;
    [SerializeField] private bool showNodeInfo = false;
#endif

    public bool IsInitialized { get; private set; } = false;

    public void InitializeGrid()
    {
        gridNodes = new GridNode[gridSettings.GridSizeX, gridSettings.GridSizeY];

        for (int x = 0; x < gridSettings.GridSizeX; x++)
        {
            for (int y = 0; y < gridSettings.GridSizeY; y++)
            {
                Vector3 worldPos = gridSettings.UseXZPlane
                    ? new Vector3(x, 0, y) * gridSettings.NodeSize
                    : new Vector3(x, y, 0) * gridSettings.NodeSize;

                GridNode node = new GridNode
                {
                    Name = $"Cell_{x}_{y}",
                    WorldPosition = worldPos,
                    TerrainType = TerrainTypes.Grass // Default to grass
                };
                gridNodes[x, y] = node;
            }
        }
        IsInitialized = true;
    }

    public void SetTerrainType(int x, int y, TerrainType terrainType)
    {
        if (!IsValidCoordinate(x, y)) return;
        
        GridNode node = gridNodes[x, y];
        node.TerrainType = terrainType;
        gridNodes[x, y] = node;
    }

    private bool IsValidCoordinate(int x, int y)
    {
        return x >= 0 && x < gridSettings.GridSizeX && y >= 0 && y < gridSettings.GridSizeY;
    }

#if UNITY_EDITOR
    private void PopulateDebugList()
    {
        AllNodes.Clear();
        for (int x = 0; x < gridSettings.GridSizeX; x++)
        {
            for (int y = 0; y < gridSettings.GridSizeY; y++)
            {
                AllNodes.Add(gridNodes[x, y]);
            }
        }
    }
#endif

    // Retrieve node data efficiently
    public GridNode GetNode(int x, int y)
    {
        if (!IsValidCoordinate(x, y))
            throw new System.IndexOutOfRangeException("Grid node indices out of range.");

        return gridNodes[x, y];
    }

    // Efficient visualization using Gizmos, toggleable through Unity editor
    private void OnDrawGizmos()
    {
        if (!showGrid || gridNodes == null || gridSettings == null) return;

        for (int x = 0; x < gridSettings.GridSizeX; x++)
        {
            for (int y = 0; y < gridSettings.GridSizeY; y++)
            {
                GridNode node = gridNodes[x, y];
                Gizmos.color = node.GizmoColor;
                Gizmos.DrawWireCube(node.WorldPosition, Vector3.one * gridSettings.NodeSize * 0.9f);

#if UNITY_EDITOR
                if (showNodeInfo)
                {
                    Handles.Label(node.WorldPosition + Vector3.up * 0.1f, $"{x},{y}");
                }
#endif
            }
        }
    }

    [CustomEditor(typeof(GridManager))]
    public class GridManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GridManager grid = (GridManager)target;
            if (grid.IsInitialized)
            {
                if (GUILayout.Button("Refresh Grid Debug View"))
                {
                    grid.PopulateDebugList();
                }
            }
        }
    }
}
