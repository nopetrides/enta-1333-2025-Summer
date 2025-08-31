using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "AvailableUnits", menuName = "Game/AvailableUnits")]  // scriptable object  that  defines which unit types an army can spawn.
public class AvailableUnits : ScriptableObject
{
   
    public List<UnitType> spawnableUnits = new();
}
