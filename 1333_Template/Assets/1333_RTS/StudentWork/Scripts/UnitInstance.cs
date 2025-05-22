using UnityEngine;

namespace RTS_1333
{
	/// <summary>
	/// Represents a specific unit instance in the game, derived from UnitBase.
	/// </summary>
	public class UnitInstance : UnitBase
	{
		public UnitType UnitConfig => _unitType;
		/// <summary>
		/// Initializes the unit with a given UnitType.
		/// </summary>
		/// <param name="unitType">The type of unit to assign.</param>
		public void Initialize(UnitType unitType)
		{
			_unitType = unitType; // Assign the unit type.
		}

		/// <summary>
		/// Moves the unit to the specified grid node.
		/// </summary>
		/// <param name="targetNode">The target node to move to.</param>
		public override void MoveTo(GridNode targetNode)
		{
			// TODO: Implement movement logic here.
			// For now, just log the move for demonstration.
			Debug.Log($"Unit moving to node: {targetNode.Name} at {targetNode.WorldPosition}");
		}
	}
}
