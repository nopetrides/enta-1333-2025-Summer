using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private UnitManager unitManager;

    private void Awake()
    {
        gridManager.InitializedGrid();
    }

    private void Start()
    {
        unitManager.SpawnDemoUnits(gridManager.GetGrid(), gridManager.GridSettings);
    }
}
