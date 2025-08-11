
using UnityEngine;
using UnityEngine.UI;

public class BuildingPlacementUI : MonoBehaviour   // displays ui buttons for each building and tells controller what to place.
{
    [SerializeField] private RectTransform layoutGroupParent;
    [SerializeField] private SelectBuildingButton buttonPrefab;
    [SerializeField] private BuildingTypePrefab[] buildingTypePrefabs;
    [SerializeField] private BuildingPlacementController placementController;

    void Start()
    {
        foreach (var typePrefab in buildingTypePrefabs)
        {
            var button = Instantiate(buttonPrefab, layoutGroupParent);
            var type = typePrefab.buildingType;
            button.Setup(
                type.BuildingName,
                type.BuildingIcon,
                type.GoldCost,
                type.StoneCost,
                type.WoodCost
            );
            button.GetComponent<Button>().onClick.AddListener(() =>
            {
                placementController.SetBuildingToPlace(typePrefab);
            });
        }

    }
}
