using System;
using UnityEngine;

/// <summary>
/// Serializable entry used in ArmyComposition.
/// One entry represents: “Spawn this unit type for this count units.”
/// </summary>
[Serializable]
public class UnitEntry
{
    [Tooltip("Pairs a UnitType and a prefab to spawn.")]
    public UnitTypePrefab unitTypePrefab = null;

    [Tooltip("How many instances of this unit type to spawn.")]
    public int count = 1;
}
