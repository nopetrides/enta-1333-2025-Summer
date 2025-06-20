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

<<<<<<< HEAD

    [SerializeField] private string terrainName = "Default";

    [SerializeField] private Color gizmoColor = Color.green;

    [SerializeField] private bool Walkable = true;

    public int movementCost = 1;
    // Start is called before the first frame update


    //new
=======
    
>>>>>>> 6217d9261501907b08ecf4bdfe194186b9dcd8a1
    public string TerrainName => terrainName;
    public Color GizmoColour => gizmoColour;
    public bool Walkable => walkable;
    public int MovementCost => movementCost;
}
