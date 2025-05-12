using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;

    public void SpawnDummyUnit()
    {
        if (!gridManager.IsInitialized)
        {
            Debug.LogError("Grid not initialized!");
            return;
        }

        int randomX = Random.Range(0, gridManager.GridSettings.GridSizeX);
        int randomY = Random.Range(0, gridManager.GridSettings.GridSizeY);

        GridNode spawnNode = gridManager.GetNode(randomX, randomY);
        Debug.Log($"Dummy unit spawned at ({randomX}, {randomY}) - World Position: {spawnNode.WorldPosition}");

        // Instantiate dummy unit prefab here in future
    }
}
