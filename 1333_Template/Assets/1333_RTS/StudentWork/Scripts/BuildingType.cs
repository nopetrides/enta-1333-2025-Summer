using UnityEngine;

namespace RTS_1333
{
	[CreateAssetMenu(fileName = "BuildingType", menuName = "Game/Building Type")]
	public class BuildingType : ScriptableObject
	{
		/// <summary>
		/// The width of the building in grid cells.
		/// </summary>
		[SerializeField] private int _width = 1;
		/// <summary>
		/// The height of the building in grid cells.
		/// </summary>
		[SerializeField] private int _height = 1;

		/// <summary>
		/// Public property to get the width of the building.
		/// </summary>
		public int Width => _width;
		/// <summary>
		/// Public property to get the height of the building.
		/// </summary>
		public int Height => _height;
	}
}