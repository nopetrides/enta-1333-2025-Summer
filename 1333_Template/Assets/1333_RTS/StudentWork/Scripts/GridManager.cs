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
                    Name = $"Cell_{(x + gridSettings.GridSizeX * x + y)}",
                    WorldPosition = worldPos,
                    Walkable = true, // Default all nodes walkable, modified later
                    Weight = 1 // Default weight, useful for varied terrain costs
                };
                gridNodes[x, y] = node;
            }
        }
        IsInitialized = true;
    }


#if UNITY_EDITOR
    private void PopulateDebugList()
    {
        AllNodes.Clear();
        for (int x = 0; x < gridSettings.GridSizeX; x++)
        {
            for (int y = 0; y < gridSettings.GridSizeY; y++)
            {
                GridNode node = gridNodes[x, y];
                AllNodes.Add(new GridNode
                {
                    Name = $"Cell_{x}_{y}",
                    WorldPosition = node.WorldPosition,
                    Walkable = node.Walkable,
                    Weight = node.Weight
                });
            }
        }
    }
#endif

    // Retrieve node data efficiently
    public GridNode GetNode(int x, int y)
    {
        if (x < 0 || x >= gridSettings.GridSizeX || y < 0 || y >= gridSettings.GridSizeY)
            throw new System.IndexOutOfRangeException("Grid node indices out of range.");

        return gridNodes[x, y];
    }

    // Example setter for walkability, expandable for future logic
    public void SetWalkable(int x, int y, bool walkable)
    {
        gridNodes[x, y].Walkable = walkable;
    }

    // Efficient visualization using Gizmos, toggleable through Unity editor
    private void OnDrawGizmos()
    {
        if (gridNodes == null || gridSettings == null) return;

        Gizmos.color = Color.green;

        for (int x = 0; x < gridSettings.GridSizeX; x++)
        {
            for (int y = 0; y < gridSettings.GridSizeY; y++)
            {
                GridNode node = gridNodes[x, y];
                Gizmos.color = node.Walkable ? Color.green : Color.red;
                Gizmos.DrawWireCube(node.WorldPosition, Vector3.one * gridSettings.NodeSize * 0.9f);
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
