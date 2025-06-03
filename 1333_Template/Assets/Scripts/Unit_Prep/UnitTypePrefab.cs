using System;
using UnityEngine;

/// <summary>
/// Serializable class that pairs a UnitType ScriptableObject with its corresponding prefab.
/// Typically used inside an ArmyComposition to specify which unit types to spawn.
/// </summary>
[Serializable]
public class UnitTypePrefab
{
    [Tooltip("The UnitType ScriptableObject defining stats for this unit.")]
    public UnitType unitType = null;

    [Tooltip("The actual prefab (GameObject) to instantiate for this unit type.")]
    public GameObject unitPrefab = null;
}
