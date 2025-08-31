using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private GridSettings gridSettings; // referances
    public GridSettings GridSettings => gridSettings;

    private GridNode[,] gridNodes;

#if UNITY_EDITOR
    [Header("Debug for editor playmode only")]
    [SerializeField] private List<GridNode> AllNodes = new();
    [SerializeField] private bool showGrid = true;
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
                    Walkable = true,
                    Weight = 1
                };

                gridNodes[x, y] = node;
            }
        }
        IsInitialized = true;
    }

   
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showGrid || gridNodes == null || gridSettings == null) return;

        for (int x = 0; x < gridSettings.GridSizeX; x++)
        {
            for (int y = 0; y < gridSettings.GridSizeY; y++)
            {
                GridNode node = gridNodes[x, y];
                if (node.TerrainType != null)
                    Gizmos.color = node.TerrainType.GizmoColor;
                else
                    Gizmos.color = node.Walkable ? Color.green : Color.red;

                Gizmos.DrawWireCube(node.WorldPosition, Vector3.one * gridSettings.NodeSize * 0.9f);
            }
        }
    }
#endif

    public GridNode GetNode(int x, int y)
    {
        return gridNodes[x, y];
    }

    public void SetNode(int x, int y, GridNode node)
    {
        gridNodes[x, y] = node;
    }

    public GridNode GetNodeFromWorldPosition(Vector3 worldPosition)
    {
        int x = Mathf.RoundToInt(worldPosition.x / gridSettings.NodeSize);
        int y = gridSettings.UseXZPlane
            ? Mathf.RoundToInt(worldPosition.z / gridSettings.NodeSize)
            : Mathf.RoundToInt(worldPosition.y / gridSettings.NodeSize);

        x = Mathf.Clamp(x, 0, gridSettings.GridSizeX - 1);
        y = Mathf.Clamp(y, 0, gridSettings.GridSizeY - 1);

        return gridNodes[x, y];
    }

    public bool IsInBounds(int x, int y)
    {
        return x >= 0 && x < gridSettings.GridSizeX && y >= 0 && y < gridSettings.GridSizeY;
    }

    public bool CanPlaceBuildingAt(int startX, int startY, int width, int height)
    {
        for (int dx = 0; dx < width; dx++)
        {
            for (int dy = 0; dy < height; dy++)
            {
                int x = startX + dx;
                int y = startY + dy;
                if (x < 0 || x >= gridSettings.GridSizeX || y < 0 || y >= gridSettings.GridSizeY)
                    return false;
                var node = gridNodes[x, y];
                if (node.Occupied || node.UnitPresent || !node.Walkable)
                    return false;
            }
        }
        return true;
    }

    public void SetBuildingOccupancy(int startX, int startY, int width, int height, bool occupied, BuildingType buildingType)
    {
        for (int dx = 0; dx < width; dx++)
        {
            for (int dy = 0; dy < height; dy++)
            {
                int x = startX + dx;
                int y = startY + dy;
                var node = gridNodes[x, y];
                node.Occupied = occupied;
                node.BuildingOnNode = occupied ? buildingType : null;
                gridNodes[x, y] = node;
            }
        }
    }

    public void SetUnitOccupancy(int x, int y, bool occupied, UnitInstance unit = null)
    {
        gridNodes[x, y].UnitPresent = occupied;
        gridNodes[x, y].OccupyingUnit = occupied ? unit : null;
    }

    public Vector2Int WorldToGridIndex(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt(worldPos.x / gridSettings.NodeSize);
        int y = gridSettings.UseXZPlane
            ? Mathf.RoundToInt(worldPos.z / gridSettings.NodeSize)
            : Mathf.RoundToInt(worldPos.y / gridSettings.NodeSize);
        return new Vector2Int(x, y);
    }
}
