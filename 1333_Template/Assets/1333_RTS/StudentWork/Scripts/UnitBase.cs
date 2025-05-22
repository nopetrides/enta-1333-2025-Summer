using UnityEngine;

namespace RTS_1333
{
    /// <summary>
    /// Abstract base class for all units in the game.
    /// </summary>
    public abstract class UnitBase : MonoBehaviour
    {
        /// <summary>
        /// The type of this unit (ScriptableObject containing unit data).
        /// </summary>
        [SerializeField] protected UnitType _unitType;

        /// <summary>
        /// The width of the unit in grid cells (for large units).
        /// </summary>
        public virtual int Width => _unitType != null ? _unitType.Width : 1;

        /// <summary>
        /// The height of the unit in grid cells (for large units).
        /// </summary>
        public virtual int Height => _unitType != null ? _unitType.Height : 1;

        /// <summary>
        /// Moves the unit to the specified grid node.
        /// </summary>
        /// <param name="targetNode">The target node to move to.</param>
        public abstract void MoveTo(GridNode targetNode);
    }
} 