using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private UnitManager unitManager;

    private void Awake()
    {
        gridManager.InitializeGrid();
        unitManager.SpawnDummyUnit();
    }
}