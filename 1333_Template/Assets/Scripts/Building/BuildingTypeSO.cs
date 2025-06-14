using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingType", menuName = "Game/Building Type")]
public class BuildingTypeSO : ScriptableObject
{
    [SerializeField] private List<BuildingDataSO> _buildings = new();  // All available building definitions

    /// <summary>
    /// List of building data objects this type holds.
    /// </summary>
    public List<BuildingDataSO> Buildings => _buildings;
}
