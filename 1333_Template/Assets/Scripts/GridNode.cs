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

    
    public GridNode CameFromNode;                  // Reference to the previous node in the path

    
    public int MovementCost => terrainType != null ? terrainType.MovementCost : 1;   // Cost to move through this node (default 1 if no terrain type)

    
    public int GCost;                              // Cost from start node to this node
    public int HCost;                              // Heuristic cost from this node to the target
    public int FCost => GCost + HCost;
}
