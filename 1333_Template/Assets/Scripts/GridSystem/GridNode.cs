using UnityEngine;

[System.Serializable]
public struct GridNode
{
    public string name;
    public Vector3 worldPosition;
    public bool walkable;
    public int weight;
    public TerrainType terrainType;

    public Color GizmoColor => terrainType != null
                                ? terrainType.GizmoColor
                                : Color.white;
}


