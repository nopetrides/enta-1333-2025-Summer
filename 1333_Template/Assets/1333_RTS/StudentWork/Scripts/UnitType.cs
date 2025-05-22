using UnityEngine;

namespace RTS_1333
{
	[CreateAssetMenu(fileName = "UnitType", menuName = "Game/Unit Type")]
	public class UnitType : ScriptableObject
	{
		/// <summary>
		/// The width of the unit in grid cells.
		/// </summary>
		[SerializeField] private int _width = 1;
		/// <summary>
		/// The height of the unit in grid cells.
		/// </summary>
		[SerializeField] private int _height = 1;

		/// <summary>
		/// Public property to get the width of the unit.
		/// </summary>
		public int Width => _width;
		/// <summary>
		/// Public property to get the height of the unit.
		/// </summary>
		public int Height => _height;
	}
}