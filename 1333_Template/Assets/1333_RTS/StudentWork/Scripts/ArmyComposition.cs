using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "ArmyComposition", menuName = "Game/ArmyComposition")]  // scriptableObject that describes the types and counts of units in an army.
public class ArmyComposition : ScriptableObject
{
   
    [System.Serializable]
    public class UnitConfig
    {
        public UnitTypePrefab unitTypePrefab; // the pairing of type and prefab
        public int count = 1;              // number to spawn
    }

    
    public List<UnitConfig> units = new();
}
