using System.Collections.Generic;
using UnityEngine;


public class UnitManager : MonoBehaviour     // handles spawning and management of all units in the game.
{
    [SerializeField] private GridManager gridManager;


   private Dictionary<int, ArmyManager> armies;

    
    public ArmyManager PlayerArmy => armies?[0];

  
    public void SpawnDummyUnit(Transform marker)   // spawn a test/dummy unit at a random grid location.
    {
        if (!gridManager.IsInitialized)
        {
            Debug.LogError("UnitManager: Grid not initialized!");
            return;
        }

        int x = Random.Range(0, gridManager.GridSettings.GridSizeX);
        int y = Random.Range(0, gridManager.GridSettings.GridSizeY);

        GridNode node = gridManager.GetNode(x, y);
      
        
    }
}