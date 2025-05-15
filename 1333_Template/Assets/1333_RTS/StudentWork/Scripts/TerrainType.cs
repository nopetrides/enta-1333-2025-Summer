using UnityEngine;

[CreateAssetMenu(fileName = "TerrainType", menuName = "Game/Terrain Type")]
public class TerrainType : ScriptableObject
{
    [SerializeField] private string terrainName = "Default";
    [SerializeField] private Color gizmoColor = Color.green;
    [SerializeField] private bool walkable = true;
    [SerializeField] private int movementCost = 1;
    [SerializeField] private Texture2D terrainTexture;

    public string TerrainName => terrainName;
    public Color GizmoColor => gizmoColor;
    public bool Walkable => walkable;
    public int MovementCost => movementCost;
    public Texture2D TerrainTexture => terrainTexture;
} 