using UnityEngine;

[CreateAssetMenu(fileName = "BuildingData", menuName = "Game/Building Data")]
public class BuildingDataSO : ScriptableObject
{
    [Header("Basic Info")]
    [SerializeField] private string _buildingName;               // Name of the building
    [SerializeField] private Sprite _buttonImage;                // Icon for the UI button

    [Header("Description")]
    [SerializeField] private string _description;                // Description shown in tooltip or detail panel
    
    [Header("Game Stats")]
    [SerializeField] private GameObject _buildingPrefab;        // Building prefab
    [SerializeField] private GameObject _buildingModel;         // Building Model    
    [SerializeField] private int _health;                       // Building health

    [Header("Grid Size")]
    [SerializeField] private int _sizeX;                // Width in grid cells
    [SerializeField] private int _sizeZ;                  // Height in grid cells


    /// <summary>
    /// Gets the building's display name.
    /// </summary>
    public string BuildingName => _buildingName;

    /// <summary>
    /// Gets the sprite used for the placement button.
    /// </summary>
    public Sprite ButtonImage => _buttonImage;

    /// <summary>
    /// Gets a longer description of the building.
    /// </summary>
    public string Description => _description;

    /// <summary>
    /// Gets game object prefab.
    /// </summary>
    public GameObject BuildingPrefab => _buildingPrefab;

    /// <summary>
    /// Gets building model.
    /// </summary>
    public GameObject BuildingModel => _buildingModel;

    // <summary>
    /// Gets building health.
    /// </summary>
    public int Health => _health;   

    /// <summary>
    /// Gets how many cells wide this building occupies.
    /// </summary>
    public int SizeX => _sizeX;

    /// <summary>
    /// Gets how many cells tall this building occupies.
    /// </summary>
    public int SizeZ => _sizeZ;
}
