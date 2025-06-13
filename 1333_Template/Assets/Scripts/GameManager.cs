using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour


{

    [SerializeField] private GridManager gridManager;
    [SerializeField] private UnitManager unitManager;
    [SerializeField] private VisualsDrawing visualsDrawing;

    //private void Awake()


    private void Awake()
    {
        gridManager.InitializeGrid();
        visualsDrawing.ResetFeild();
    }

    // Start is called before the first frame update
   

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Awake();

        }
    }
}
