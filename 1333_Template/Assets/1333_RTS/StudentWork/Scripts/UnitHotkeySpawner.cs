using System.Collections.Generic;
using UnityEngine;

public class UnitHotkeySpawner : MonoBehaviour
{
    [SerializeField] private ArmyPathfindingTester pathfindingTester;
    [SerializeField] private List<UnitTypePrefab> spawnableUnitPrefabs;

   
    [SerializeField] private int playerArmyIndex = 0;
    [SerializeField] private int enemyArmyIndex = 1;
    [SerializeField] private bool spawnForPlayer = true;

    void Update()  // spawning new units tied to number keys (currently 1-4) 
    {
        for (int i = 0; i < spawnableUnitPrefabs.Count && i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                int armyIdx = spawnForPlayer ? playerArmyIndex : enemyArmyIndex;
                pathfindingTester.SpawnUnitInArmy(armyIdx, spawnableUnitPrefabs[i]);
            }
        }

        if (Input.GetKeyDown(KeyCode.Tab))          // toggle target army with tab key
            spawnForPlayer = !spawnForPlayer;
    }
}