using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingPlacementUI : MonoBehaviour
{

    [SerializeField] private RectTransform _layoutGroupParent;
    [SerializeField] private GameObject _buttonPrefab;
    [SerializeField] private BuildingTypeSO _buildingType;

    // Start is called before the first frame update
    void Start()
    {
        foreach(BuildingDataSO t in _buildingType.Buildings)
        {
            GameObject button = Instantiate(_buttonPrefab, _layoutGroupParent);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
