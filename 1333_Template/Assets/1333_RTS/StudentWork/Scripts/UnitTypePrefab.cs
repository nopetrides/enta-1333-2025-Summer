using UnityEngine;


[System.Serializable]
public class UnitTypePrefab  // links a unit's scriptable object  with its in scene prefab
{
    public UnitType unitType;    // stats/config for this unit
    public GameObject prefab;    // prefab to instantiate for this unit type
}
