using UnityEngine;

[CreateAssetMenu(fileName = "TerrainType", menuName = "Game/TerrainType")]
public class TerrainType : ScriptableObject
{
    [SerializeField] private string _terrainName = "Default";
    [SerializeField] private Color _gizmoColor = Color.green;
    [SerializeField] private bool _walkable = true;
    [SerializeField] private int _movementCost = 1;

    public string TerrainName => _terrainName;
    public Color GizmoColor => _gizmoColor;
    public bool Walkable => _walkable;
    public int MovementCost => _movementCost;
}
