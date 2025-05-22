using UnityEngine;

namespace RTS_1333
{
	/// <summary>
	/// Represents a specific building instance in the game, derived from BuildingBase.
	/// </summary>
	public class BuildingInstance : BuildingBase
	{
		public BuildingType BuildingConfig => _buildingType;
		/// <summary>
		/// Initializes the building with a given BuildingType.
		/// </summary>
		/// <param name="buildingType">The type of building to assign.</param>
		public void Initialize(BuildingType buildingType)
		{
			_buildingType = buildingType; // Assign the building type.
		}
		
		/// <summary>
		/// Places the building at the specified grid node.
		/// </summary>
		/// <param name="targetNode">The target node to place the building on.</param>
		public override void PlaceAt(GridNode targetNode)
		{
			// TODO: Implement placement logic here.
			// For now, just log the placement for demonstration.
			Debug.Log($"Building placed at node: {targetNode.Name} at {targetNode.WorldPosition}");
		}
	}
}
