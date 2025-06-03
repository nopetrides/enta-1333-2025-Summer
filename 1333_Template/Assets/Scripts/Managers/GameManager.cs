// GameManager.cs
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GridManager _gridManager;
    [SerializeField] private UnitManager _unitManager;
    [SerializeField] private ArmyManager _armyManager;
    [SerializeField] private SelectionManager _unitSelectionManager;
    [SerializeField] private Camera _camera;

    private void Awake()
    {
        _gridManager.InitializeGrid();
        _armyManager.Initialize(_gridManager, _unitManager);

        _unitSelectionManager.Initialize(_camera, _gridManager, _unitManager);
    }

}