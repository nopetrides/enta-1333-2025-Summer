using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GridManager _gridManager;
    [SerializeField] private UnitManager _unitManager;
    private void Awake()
    {
        _gridManager.InitializeGrid();
    }
}
