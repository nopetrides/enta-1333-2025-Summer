using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TerrainType", menuName = "Game/TerrainType")]
public class TerrainType : ScriptableObject
{
    [SerializeField] private string terrainName = "Default";   // Name of the terrain type
    [SerializeField] private Color gizmoColour = Color.green;  // Color used for gizmos
    [SerializeField] private bool walkable = true;             // Can units walk on the terrain?
    [SerializeField] private int movementCost = 1;            // Movement cost for the terrain types

    
    public string TerrainName => terrainName;
    public Color GizmoColour => gizmoColour;
    public bool Walkable => walkable;
    public int MovementCost => movementCost;
}
