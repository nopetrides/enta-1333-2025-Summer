using UnityEngine;

/// <summary>
/// Handles displaying building placement UI and initializing building selection buttons.
/// </summary>
public class BuildingPlacementUI : MonoBehaviour
{
    [SerializeField] private RectTransform _layoutGroupParent;
    [SerializeField] private GameObject _buttonPrefab;
    [SerializeField] private BuildingTypeSO _buildingType;
    [SerializeField] private BuildingPlacementManager _placementManager;

    public void InitializeBuildingPlacementUI()
    {
        ShowBuildingPlacementUI();
    }

    /// <summary>
    /// Instantiates building selection buttons for each building type.
    /// </summary>
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
