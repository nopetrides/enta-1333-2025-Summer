using UnityEngine;

// Represents each node on our grid. Brutally efficient with struct usage and flags.
[System.Serializable]
public struct GridNode
{
    public string Name; // Grid Index
    public Vector3 WorldPosition;
    public TerrainType TerrainType;

    public bool Walkable => TerrainType != null && TerrainType.Walkable;
    public int Weight => TerrainType != null ? TerrainType.MovementCost : 1;
    public Color GizmoColor => TerrainType != null ? TerrainType.GizmoColor : Color.gray;

    // Future-proof: Add faction-based walkability or additional node metadata here
}