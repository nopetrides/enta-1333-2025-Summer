using System.Collections.Generic;
using UnityEngine;

namespace RTS_1333
{
	/// <summary>
	/// Manages all units and buildings belonging to a single army.
	/// </summary>
	public class ArmyManager
	{
		/// <summary>
		/// The unique ID of this army. Player is always army 0.
		/// </summary>
		public int ArmyID;

		/// <summary>
		/// Returns true if this army is the player army (ID 0).
		/// </summary>
		public bool IsPlayer => ArmyID == 0;

		/// <summary>
		/// List of all units in this army. Uses UnitBase for polymorphism.
		/// </summary>
		public List<UnitBase> Units = new List<UnitBase>();

		/// <summary>
		/// List of all buildings in this army. Uses BuildingBase for polymorphism.
		/// </summary>
		public List<BuildingBase> Buildings = new List<BuildingBase>();

		/// <summary>
		/// Reference to the grid manager for node lookups.
		/// </summary>
		public GridManager GridManager;

		/// <summary>
		/// Commands all units in the army to move to a target world position.
		/// </summary>
		public void MoveAllUnitsTo(Vector3 worldPosition)
		{
			// Loop through all units in the army.
			foreach (var unit in Units)
			{
				// Command each unit to move to the target position.
				unit.MoveTo(GridManager.GetNodeFromWorldPosition(worldPosition));
			}
		}

		/// <summary>
		/// Commands all units in the army to move to a target grid node.
		/// </summary>
		public void MoveAllUnitsTo(GridNode node)
		{
			// Loop through all units in the army.
			foreach (var unit in Units)
			{
				// Command each unit to move to the target node.
				unit.MoveTo(node);
			}
		}
	}
}