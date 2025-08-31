using UnityEngine;

[CreateAssetMenu(fileName = "BuildingType", menuName = "Game/BuildingType")]
public class BuildingType : ScriptableObject
{
    public string BuildingName;
    public Sprite BuildingIcon;
    [SerializeField] private int width = 1;
    [SerializeField] private int height = 1;

    public int Width => width;
    public int Height => height;

    [Header("Resource Costs")]
    public int GoldCost = 10;
    public int StoneCost = 5;
    public int WoodCost = 0;
}
