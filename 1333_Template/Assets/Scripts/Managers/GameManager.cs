using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GridManager _gridManager;
    [SerializeField] private UnitManager _unitManager;         // Drag your UnitManager here
    [SerializeField] private ArmyManager _armyManager;
    [SerializeField] private SelectionManager _selectionManager;
    [SerializeField] private Camera _camera;

    private void Awake()
    {
        // 1) Initialize the grid
        _gridManager.InitializeGrid();

        // 2) Initialize ArmyManager, providing both GridManager and UnitManager
        _armyManager.Initialize(_gridManager, _unitManager);

        // 3) Initialize the SelectionManager as before
        _selectionManager.Initialize(_camera, _gridManager, _unitManager);
    }
}
