using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    public List<UnitInstance> selectedUnits = new List<UnitInstance>();
    public BuildingInstance selectedBuilding;

    private Vector2 dragStart, dragEnd;
    private bool isDragging = false;

    [SerializeField] private BuildingUnitSpawnerUI buildingSpawnerUI;     // reference to UI panels 

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // lft mouse down
        {
            dragStart = Input.mousePosition;
            isDragging = true;
        }
        if (Input.GetMouseButtonUp(0))
        {
            dragEnd = Input.mousePosition;
            isDragging = false;
            if (Vector2.Distance(dragStart, dragEnd) < 10f)
                SelectAtMouse();
            else
                DragSelectUnits();
        }

        if (Input.GetMouseButtonDown(1)) // rght click for unit move
        {
            if (selectedUnits.Count > 0)
                MoveSelectedUnits();
        }
    }

    void SelectAtMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            var unit = hit.collider.GetComponent<UnitInstance>();
            var building = hit.collider.GetComponent<BuildingInstance>();

            selectedUnits.Clear();
            selectedBuilding = null;
          //  buildingSpawnerUI.Hide();

           
            if (unit && unit.teamId == 0)   // only select units with teamId == 0 (player units, e.g. King)
            {
                selectedUnits.Add(unit);
                Debug.Log("Unit selected");
            }
            else if (building)
            {
                selectedBuilding = building;
                Debug.Log("Building selected");
               // buildingSpawnerUI.ShowForBuilding(selectedBuilding); // show ui
            }
        }
    }

    void DragSelectUnits()
    {
        selectedUnits.Clear();
        selectedBuilding = null;
      //  buildingSpawnerUI.Hide();

        foreach (var unit in FindObjectsOfType<UnitInstance>())
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(unit.transform.position);
         
            if (unit.teamId == 0 && IsWithinSelectionBounds(screenPos))
                selectedUnits.Add(unit);
        }

        Debug.Log("Drag selected " + selectedUnits.Count + " units.");
    }

    bool IsWithinSelectionBounds(Vector3 screenPosition)
    {
        Rect rect = Utils.GetScreenRect(dragStart, dragEnd);
        return rect.Contains(screenPosition);
    }

    void MoveSelectedUnits()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            foreach (var unit in selectedUnits)     // only move player units (teamId == 0)
            {
             
                if (unit != null && unit.teamId == 0)
                    unit.SetDestination(hit.point);
            }
        }
    }

    public void DeselectAll()
    {
        selectedUnits.Clear();
        selectedBuilding = null;
      //  buildingSpawnerUI.Hide();
    }
}
