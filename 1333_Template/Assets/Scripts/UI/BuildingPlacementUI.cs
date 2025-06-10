using UnityEngine;

public class BuildingPlacementUI : MonoBehaviour
{
    [SerializeField] private RectTransform _layoutGroupParent;
    [SerializeField] private GameObject _buttonPrefab;
    [SerializeField] private BuildingTypeSO _buildingType;
    [SerializeField] private BuildingPlacementManager _placementManager;

    private void Start()
    {
        ShowBuildingPlacementUI();
    }

    private void ShowBuildingPlacementUI()
    {
        foreach (var data in _buildingType.Buildings)
        {
            var buttonPrefab = Instantiate(_buttonPrefab, _layoutGroupParent);
            var button = buttonPrefab.GetComponent<BuildingButton>();
            button.Initialize(data, _placementManager);
        }
    }
}