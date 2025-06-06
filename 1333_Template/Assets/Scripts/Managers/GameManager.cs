// GameManager.cs
using UnityEngine;

/// <summary>
/// Manages overall game initialization, including grid, army, and unit selection systems.
/// </summary>
public class GameManager : MonoBehaviour
{
    [SerializeField] private GridManager _gridManager;
    [SerializeField] private UnitManager _unitManager;
    [SerializeField] private ArmyManager _armyManager;
    [SerializeField] private SelectionManager _unitSelectionManager;
    [SerializeField] private Camera _camera;

    /// <summary>
    /// Called when the GameObject is first loaded.
    /// Initializes the grid, army manager, and unit selection manager with required dependencies.
    /// </summary>
    private void Awake()
    {
        _gridManager.InitializeGrid();
        _armyManager.Initialize(_gridManager, _unitManager);

        _unitSelectionManager.Initialize(_camera, _gridManager, _unitManager);
    }
}
