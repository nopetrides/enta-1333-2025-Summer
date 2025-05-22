using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// todo manage all units in an army
/// </summary>
public class ArmyManager
{
	public int ArmyID;
	// player is always army 0
	public bool IsPlayer => ArmyID == 0;
	public List<GameObject> Units;
}

