using UnityEngine;
using System.Collections.Generic;

namespace RTS_1333
{
	/// <summary>
	/// Represents a specific unit instance in the game, derived from UnitBase.
	/// </summary>
	public class UnitInstance : UnitBase
	{
		[Header("Movement")]
		[SerializeField] private float moveSpeed = 3f; // Units per second.
		
		private Pathfinder _pathfinder; // Reference to the Pathfinder.
		private List<GridNode> _currentPath = new List<GridNode>(); // The current path to follow.
		private int _pathIndex = 0; // Current waypoint index.
		private Vector3? _targetWorldPosition = null; // The current target position.
		private bool _isMoving = false; // Is the unit currently moving?

		// Public property to check if the unit is currently moving.
		public bool IsMoving => _isMoving;

		public void Initialize(Pathfinder pathfinder, UnitType unitType)
		{
			_pathfinder = pathfinder;
			_unitType = unitType;
		}
		
		private void Update()
		{
			// If not moving or no path, do nothing.
			if (!_isMoving || _currentPath == null || _currentPath.Count == 0 || _pathIndex >= _currentPath.Count)
				return;

			// Get the next waypoint.
			Vector3 nextWaypoint = _currentPath[_pathIndex].WorldPosition;
			// Move towards the waypoint.
			Vector3 direction = (nextWaypoint - transform.position).normalized;
			float step = moveSpeed * Time.deltaTime;
			transform.position = Vector3.MoveTowards(transform.position, nextWaypoint, step);

			// Check if reached the waypoint.
			if (Vector3.Distance(transform.position, nextWaypoint) < 0.05f)
			{
				_pathIndex++;
				// If reached the end of the path, stop moving.
				if (_pathIndex >= _currentPath.Count)
		{
					_isMoving = false;
				}
			}
		}

		/// <summary>
		/// Sets a new movement target for the unit (world position).
		/// </summary>
		public void SetTarget(Vector3 worldPosition)
		{
			// Store the target.
			_targetWorldPosition = worldPosition;
			// Request a path from Pathfinder.
			_currentPath = _pathfinder.FindPath(transform.position, worldPosition, Width, Height);
			_pathIndex = 0;
			_isMoving = _currentPath != null && _currentPath.Count > 1;
		}

		/// <summary>
		/// Sets a new movement target for the unit (grid node).
		/// </summary>
		public void SetTarget(GridNode node)
		{
			SetTarget(node.WorldPosition);
		}

		/// <summary>
		/// Moves the unit to the specified grid node (required by base class).
		/// </summary>
		public override void MoveTo(GridNode targetNode)
		{
			SetTarget(targetNode);
		}
	}
}
