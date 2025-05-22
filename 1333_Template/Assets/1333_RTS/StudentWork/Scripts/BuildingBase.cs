using UnityEngine;

namespace RTS_1333
{
    /// <summary>
    /// Abstract base class for all buildings in the game.
    /// </summary>
    public abstract class BuildingBase : MonoBehaviour
    {
        /// <summary>
        /// The type of this building (ScriptableObject containing building data).
        /// </summary>
        [SerializeField] protected BuildingType _buildingType;

        /// <summary>
        /// The width of the building in grid cells.
        /// </summary>
        public virtual int Width => _buildingType != null ? _buildingType.Width : 1;

        /// <summary>
        /// The height of the building in grid cells.
        /// </summary>
        public virtual int Height => _buildingType != null ? _buildingType.Height : 1;

        /// <summary>
        /// Places the building at the specified grid node.
        /// </summary>
        /// <param name="targetNode">The target node to place the building on.</param>
        public abstract void PlaceAt(GridNode targetNode);
    }
} 