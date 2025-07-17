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
        // Clear existing buttons
        for (int i = _layoutGroupParent.childCount - 1; i >= 0; i--)
        {
            Destroy(_layoutGroupParent.GetChild(i).gameObject);
        }

        // Instantiate new buttons
        foreach (var data in _buildingType.Buildings)
        {
            var buttonObj = Instantiate(_buttonPrefab, _layoutGroupParent);
            var button = buttonObj.GetComponent<BuildingButton>();
            button.Initialize(data, _placementManager);
        }
    }
}
