using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingType", menuName = "Game/Building Type")]
public class BuildingTypeSO : ScriptableObject
{
    public List<BuildingDataSO> Buildings = new();
}


