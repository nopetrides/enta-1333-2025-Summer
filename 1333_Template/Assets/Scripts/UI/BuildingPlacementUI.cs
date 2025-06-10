using UnityEngine;

public class BuildingPlacementUI : MonoBehaviour
{
    [SerializeField] private RectTransform _layoutGroupParent;
    [SerializeField] private GameObject _buttonPrefab;
    [SerializeField] private BuildingTypeSO _buildingType;

    private void Start()
    {
        ShowBuildingPlacementUI();
    }

    private void ShowBuildingPlacementUI()
    {
        foreach (var data in _buildingType.Buildings)
        {
            var go = Instantiate(_buttonPrefab, _layoutGroupParent);
            var btn = go.GetComponent<BuildingButton>();
            btn.Initialize(data);
        }
    }
}