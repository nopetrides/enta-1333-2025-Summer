using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GridManager : MonoBehaviour
{
    // Reference to the grid configuration ScriptableObject
    [SerializeField] private GridSettings gridSettings;

    // Array of possible terrain types (ScriptableObjects like Grass, Rock, Water)
    [SerializeField] private TerrainType[] terrainTypes;

    // Expose the grid settings for external access
    public GridSettings GridSettings => gridSettings;

    // 2D array representing the grid nodes
    private GridNode[,] gridNodes;

    // List of all grid nodes for easy access or iteration
    [SerializeField] private List<GridNode> AllNodes = new();

    // Flag to check if the grid was initialized
    public bool IsInitialized { get; private set; } = false;

    // Creates the grid based on the size and fills it with randomized terrain
    public void InitializedGrid()
    {
        Debug.Log("Grid initialized!");
        gridNodes = new GridNode[gridSettings.GridSizeX, gridSettings.GridSizeY];

        for (int x = 0; x < gridSettings.GridSizeX; x++)
        {
            for (int y = 0; y < gridSettings.GridSizeY; y++)
            {
                // Calculate the world position of this tile
                Vector3 worldPos = gridSettings.UseXZPlane
                    ? new Vector3(x, 0, y) * gridSettings.NodeSize  // For 3D top-down
                    : new Vector3(x, y, 0) * gridSettings.NodeSize; // For 2D side-scroller

                // Pick a random terrain type for this node
                TerrainType terrain = terrainTypes[Random.Range(0, terrainTypes.Length)];

                // Create and configure the new grid node
                GridNode node = new GridNode
                {
                    Name = $"{terrain.TerrainName}_{x}_{y}",
                    WorldPosition = worldPos,
                    terrainType = terrain,
                    walkable = terrain.Walkable,
                    Weight = terrain.MovementCost
                };

                AllNodes.Add(node);        // Store in the list
                gridNodes[x, y] = node;    // Store in the grid array

                // Optional: Spawn a visual prefab here
                // SpawnVisualTile(node);
            }
        }
    }

    // Returns the full 2D grid array
    public GridNode[,] GetGrid() => gridNodes;

    // Visual debug: draws a wireframe box for each tile in the grid
    private void OnDrawGizmos()
    {
        if (gridNodes == null || gridSettings == null) return;

        for (int x = 0; x < gridSettings.GridSizeX; x++)
        {
            for (int y = 0; y < gridSettings.GridSizeY; y++)
            {
                GridNode node = gridNodes[x, y];
                Gizmos.color = node.walkable ? node.GizmoColor : Color.red;
                Gizmos.DrawWireCube(node.WorldPosition, Vector3.one * gridSettings.NodeSize * 0.9f);
            }
        }
    }
}