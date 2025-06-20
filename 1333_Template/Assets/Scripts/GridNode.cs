using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TerrainUtils;

public class GridNode
{
    public string Name;                            // Name of the node
    public bool walkable;                          // Can a unit walk over this node?
    public Vector3 WorldPosition;                  // World position in Unity space
    public TerrainType terrainType;               // Type of terrain on this node

<<<<<<< HEAD
 public class GridNode
 {
        public Vector3 cords;

    
        // public bool explored;
        // public bool path;


        public GridNode cameFrom;


=======
    
    public GridNode CameFromNode;                  // Reference to the previous node in the path
>>>>>>> 6217d9261501907b08ecf4bdfe194186b9dcd8a1

    
    public int MovementCost => terrainType != null ? terrainType.MovementCost : 1;   // Cost to move through this node (default 1 if no terrain type)

<<<<<<< HEAD
        public int gCost;
        public int hCost;
        public int fCost;

        public TerrainType terrainType;

        
    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }

   

 }

  
=======
    
    public int GCost;                              // Cost from start node to this node
    public int HCost;                              // Heuristic cost from this node to the target
    public int FCost => GCost + HCost;
}
>>>>>>> 6217d9261501907b08ecf4bdfe194186b9dcd8a1
