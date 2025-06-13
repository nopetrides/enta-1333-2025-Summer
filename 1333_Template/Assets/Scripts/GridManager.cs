using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private GridSettings gridSettings;
    [SerializeField] private TerrainType defaultTerrainType;
    [SerializeField] private List<TerrainType> terrainTypes = new();

    public GridNode[,] gridNodes;
    private List<GridNode> allNodes = new();

    public List<GridNode> GetAllNodes() => allNodes;
    public bool IsInitialized { get; private set; }


    
    public void InitializeGrid()
    {
        gridNodes = new GridNode[gridSettings.GridSizeX, gridSettings.GridSizeY];
        allNodes.Clear();

        

        for (int x = 0; x < gridSettings.GridSizeX; x++)
        {
            for (int y = 0; y < gridSettings.GridSizeY; y++)
            {
                Vector3 worldPos = gridSettings.UseXZPlane
                    ? new Vector3(x, 0, y) * gridSettings.NodeSize
                    : new Vector3(x, y, 0) * gridSettings.NodeSize;

                TerrainType chosenTerrain = terrainTypes[Random.Range(0, terrainTypes.Count)];

                GridNode node = new GridNode
                {
                    Name = $"Cell_{x}_{y}",
                    WorldPosition = worldPos,
                    terrainType = chosenTerrain,
                    Walkable = chosenTerrain.IsWalkable
                };
               
                gridNodes[x, y] = node;
                allNodes.Add(node);

            }
        }

        IsInitialized = true;
        Debug.Log("Grid initialized with " + allNodes.Count + " nodes.");
    }
   



    public GridNode GetNode(int x, int y)//new
    {
        return gridNodes[x, y];
    }//new

    private void OnDrawGizmos()
    {
        if (gridNodes == null || gridSettings == null) return;

        for (int x = 0; x < gridSettings.GridSizeX; x++)
        {
            for (int y = 0; y < gridSettings.GridSizeY; y++)
            {
                GridNode node = gridNodes[x, y];
                Gizmos.color = node.Walkable ? node.terrainType.GizmoColor : Color.red;
                Gizmos.DrawWireCube(node.WorldPosition, Vector3.one * gridSettings.NodeSize * 0.9f);
            }
        }
    }
}