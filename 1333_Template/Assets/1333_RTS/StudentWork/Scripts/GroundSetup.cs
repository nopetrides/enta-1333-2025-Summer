using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundSetup : MonoBehaviour
{
    /*[SerializeField] private GridManager gridManager;
    [SerializeField] private GameObject grassPrefab;
    [SerializeField] private GameObject rockPrefab;
    [SerializeField] private GameObject waterPrefab;

    public void InstantiateGroundTiles()
    {
        GridNode[,] grid = gridManager.GetGrid();
        int width = grid.GetLength(0);
        int height = grid.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GridNode node = grid[x, y];
                Vector3 spawnPosition = node.WorldPosition;

                GameObject prefabToUse = null;

                switch (node.terrainType.TerrainName)
                {
                    case "Grass": prefabToUse = grassPrefab; break;
                    case "Rock": prefabToUse = rockPrefab; break;
                    case "Water": prefabToUse = waterPrefab; break;
                }

                if (prefabToUse != null)
                    Instantiate(prefabToUse, spawnPosition, Quaternion.identity, transform);
            }
        }
    }*/
}
