using UnityEngine;
using UnityEngine.UI;

// UI panel for spawning units from a selected building
public class BuildingUnitSpawnerUI : MonoBehaviour
{
    //[SerializeField] private RectTransform layoutGroupParent;
    //[SerializeField] private SelectUnitButton buttonPrefab;          // a ui  button prefab for each unit type
    //[SerializeField] private UnitTypePrefab[] unitTypePrefabs;       // assign all possible spawnable units here
    //[SerializeField] private AStarPathfinder pathfinder;

    //private BuildingInstance targetBuilding;  // the currently selected building 

    //void Start()
    //{
     
    //    foreach (var unitPrefab in unitTypePrefabs)     // populate buttons for all units
    //    {
    //        var button = Instantiate(buttonPrefab, layoutGroupParent);
    //        button.Setup(unitPrefab.unitType.UnitName, unitPrefab.unitType.UnitIcon); 
    //        button.GetComponent<Button>().onClick.AddListener(() =>
    //        {
    //            if (targetBuilding != null)
    //                targetBuilding.SpawnUnit(unitPrefab.prefab, pathfinder); 
    //        });
    //    }
    //    gameObject.SetActive(false); // hiding it default enable only when building is selected
    //}

   
    //public void ShowForBuilding(BuildingInstance building)   // call this from selection system
    //{
    //    targetBuilding = building;
    //    gameObject.SetActive(true);
    //}

    //public void Hide()
    //{
    //    targetBuilding = null;
    //    gameObject.SetActive(false);
    //}
}
