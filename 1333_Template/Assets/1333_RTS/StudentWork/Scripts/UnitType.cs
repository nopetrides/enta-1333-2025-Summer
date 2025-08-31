using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitType", menuName = "Game/Unit Type")]
public class UnitType : ScriptableObject
{
    public string unitName;
    public float moveSpeed = 3f;
    public float attackRange = 1f;
    public float health = 100f;
    public float attackDamage = 10f;
    public Material teamMaterial;
    public int width = 1; // Grid footprint
    public int height = 1;
}
