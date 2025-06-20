// GameManager.cs
using UnityEngine;

/// <summary>
/// Manages overall game initialization and dependency injection.
/// </summary>
public class GameManager : MonoBehaviour
{
    [SerializeField] private GridManager _gridManager;
    [SerializeField] private UnitManager _unitManager;
    [SerializeField] private ArmyManager _armyManager;
    [SerializeField] private SelectionManager _unitSelectionManager;
    [SerializeField] private BuildingPlacementManager _buildingPlacementManager;
    [SerializeField] private ResourceManager _resourceManager;
    [SerializeField] private Camera _camera;

    /// <summary>
    /// Called at startup to wire up all dependencies.
    /// </summary>
    private void Awake()
    {
        // Core systems
        _gridManager.InitializeGrid();
        _armyManager.Initialize(_gridManager, _unitManager);
        _unitSelectionManager.Initialize(_camera, _gridManager, _unitManager);

        // Resources & Building placement DI
        _resourceManager.Initialize();
        _buildingPlacementManager.Initialize(_resourceManager, _armyManager, _gridManager);
    }

    public void StartGame(string name)
    {
        Debug.Log($"Starting game: {name}");
    }
}
