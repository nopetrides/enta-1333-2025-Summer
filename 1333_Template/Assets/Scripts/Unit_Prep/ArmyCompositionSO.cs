using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject that defines a single army composition:
/// a name (e.g. PlayerArmy, EnemyArmy) and a list of UnitEntry.
/// Create new asset via: Assets → Create → Game → Army Composition
/// </summary>
[CreateAssetMenu(fileName = "ArmyComposition", menuName = "Game/Army Composition")]
public class ArmyCompositionSO : ScriptableObject
{
    [Header("Army Info")]
    [Tooltip("Name of this army (e.g. Player, Enemy).")]
    public string armyName = "New Army";

    [Header("Units to Spawn")]
    [Tooltip("List of unit entries: which UnitTypePrefab to spawn, and how many.")]
    public List<UnitEntry> unitEntries = new List<UnitEntry>();
}
