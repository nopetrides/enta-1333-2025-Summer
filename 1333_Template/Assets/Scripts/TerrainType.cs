using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "TerrainType", menuName = "Game/TerrainType")]
public class TerrainType : ScriptableObject
{


    [SerializeField] private string terrainName = "Default";

    [SerializeField] private Color gizmoColor = Color.green;

    [SerializeField] private bool Walkable = true;

    public int movementCost = 1;
    // Start is called before the first frame update


    //new
    public string TerrainName => terrainName;
    public Color GizmoColor => gizmoColor;
    public bool IsWalkable => Walkable;
    public int MovementCost => movementCost;
}
