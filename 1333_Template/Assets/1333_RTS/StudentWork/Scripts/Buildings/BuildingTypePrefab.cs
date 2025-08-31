
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingTypePrefab", menuName = "Game/BuildingTypePrefab")]
public class BuildingTypePrefab : ScriptableObject
{
    public BuildingType buildingType;  // the config/stats
    public GameObject prefab;          //  in scene prefab to place

    public UnitType unitType { get; internal set; }
}
