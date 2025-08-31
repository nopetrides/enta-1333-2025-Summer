using System.Collections.Generic;
using UnityEngine;


public class ArmyManager   // manages all units and buildings for a single army/team.
{
   
    public int ArmyId;    // id for this army; player army is always 0.

   
    public bool IsPlayer => ArmyId == 0;

  
    public List<UnitBase> Units = new();    // list of all active units 

  
    //public List<BuildingBase> Buildings = new();    // list of all active buildings (if used in the future).

 
    public GridManager GridManager;

   
    public void MoveAllUnitsTo(Vector3 destination)    // command all units to move to a specific world position.
    {
        foreach (var unit in Units)
        {
            var node = GridManager.GetNodeFromWorldPosition(destination);
            unit.MoveTo(node);
        }
    }

    
    public void MoveAllUnitsTo(GridNode node) 
    {
        foreach (var unit in Units)
        {
            unit.MoveTo(node);
        }
    }
}
