using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
<<<<<<< HEAD

    [SerializeField] private GridManager gridManager;
    [SerializeField] private UnitManager unitManager;
    [SerializeField] private VisualsDrawing visualsDrawing;

    //private void Awake()

=======
    [SerializeField] private GridManager gridManager;   // Manages the grid system
    [SerializeField] private UnitManager unitManager;   // Manages units on the grid
    [SerializeField] private VisualTargetPath pathFinder;   // Handles visual pathfinding
>>>>>>> 6217d9261501907b08ecf4bdfe194186b9dcd8a1

    private void Awake()
    {
        // Initialize grid and reset the pathfinder field when the game starts
        gridManager.InitializeGrid();
<<<<<<< HEAD
        visualsDrawing.ResetFeild();
=======
        pathFinder.ResetFeild();
>>>>>>> 6217d9261501907b08ecf4bdfe194186b9dcd8a1
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