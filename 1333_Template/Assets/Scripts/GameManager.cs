using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;   // Manages the grid system
    [SerializeField] private UnitManager unitManager;   // Manages units on the grid
    [SerializeField] private VisualTargetPath pathFinder;   // Handles visual pathfinding

    private void Awake()
    {
        // Initialize grid and reset the pathfinder field when the game starts
        gridManager.InitializeGrid();
        pathFinder.ResetFeild();
    }

    private void Update()
    {
        // Listen for 'R' key press to reinitialize grid and reset pathfinder
        if (Input.GetKeyDown(KeyCode.R))
        {
            gridManager.InitializeGrid();
            pathFinder.ResetFeild();
        }
    }
}