using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RTS_1333
{
    /// <summary>
    /// Tester script to create multiple armies, each with multiple unit types, and run a simple patrol/follow state machine.
    /// </summary>
    public class ArmyPathfindingTester : MonoBehaviour
    {
        // Reference to the grid manager for node lookups and placement.
        [SerializeField] private GridManager gridManager;
        // Reference to the pathfinder for path calculations.
        [SerializeField] private Pathfinder pathfinder;
        // List of army compositions to spawn (one per army).
        [SerializeField] private List<ArmyComposition> armyCompositions = new();
        // Patrol range in grid units.
        [SerializeField] private int patrolRange = 8;
        // Detection range for switching to follow state.
        [SerializeField] private float detectionRange = 4f;

        // List of all army managers in the scene.
        private readonly List<ArmyManager> _armies = new();
        // State for each unit.
        private enum UnitState { Patrol, Follow }
        // Dictionary to track each unit's state.
        private readonly Dictionary<UnitInstance, UnitState> _unitStates = new();
        // Dictionary to track each unit's patrol points.
        private readonly Dictionary<UnitInstance, Vector3[]> _patrolPoints = new();
        // Dictionary to track each unit's current patrol target index.
        private readonly Dictionary<UnitInstance, int> _patrolTargetIndex = new();
        // Dictionary to track each unit's follow target.
        private readonly Dictionary<UnitInstance, UnitInstance> _followTargets = new();
        // Dictionary to track last known enemy position for follow path recalculation.
        private readonly Dictionary<UnitInstance, Vector3> _lastKnownEnemyPos = new();

        // Called once at the start.
        private void Start()
        {
            // Clear any existing armies.
            _armies.Clear();
            // For each army composition, create an army and spawn its units.
            for (int i = 0; i < armyCompositions.Count; i++)
            {
                // Create a new army manager.
                ArmyManager army = new ArmyManager { ArmyID = i + 1, GridManager = gridManager };
                // Spawn all units for this army.
                SpawnArmyUnits(army, armyCompositions[i]);
                // Add to the list of armies.
                _armies.Add(army);
            }
        }

        // Spawns units for a given army, placing them at random valid positions.
        private void SpawnArmyUnits(ArmyManager army, ArmyComposition composition)
        {
            // Loop through each unit entry in the composition.
            foreach (var entry in composition.units)
            {
                // For the specified count, spawn that many units of this type.
                for (int i = 0; i < entry.count; i++)
                {
                    int attempts = 0;
                    int maxAttempts = 1000;
                    Vector3 spawnPos = Vector3.zero;
                    bool found = false;
                    int unitWidth = entry.unitTypePrefab.unitType.Width;
                    int unitHeight = entry.unitTypePrefab.unitType.Height;
                    // Try to find a valid spawn position.
                    while (!found && attempts < maxAttempts)
                    {
                        int x = Random.Range(0, gridManager.GridSettings.GridSizeX - unitWidth + 1);
                        int y = Random.Range(0, gridManager.GridSettings.GridSizeY - unitHeight + 1);
                        if (IsRegionWalkable(x, y, unitWidth, unitHeight))
                        {
                            spawnPos = gridManager.GetNode(x, y).WorldPosition;
                            found = true;
                        }
                        attempts++;
                    }
                    if (!found)
                    {
                        Debug.LogWarning($"Failed to find valid spawn position for unit {entry.unitTypePrefab.unitType.name}.");
                        continue;
                    }
                    // Instantiate the unit prefab at the spawn position.
                    GameObject go = Instantiate(entry.unitTypePrefab.prefab, spawnPos, Quaternion.identity);
                    // Get the UnitInstance component.
                    UnitInstance unit = go.GetComponent<UnitInstance>();
                    // Initialize the unit with its pathfinder and type.
                    unit.Initialize(pathfinder, entry.unitTypePrefab.unitType);
                    // Add to the army's unit list.
                    army.Units.Add(unit);
                    // Initialize state to Patrol.
                    _unitStates[unit] = UnitState.Patrol;
                    // Pick two random patrol points within patrolRange.
                    _patrolPoints[unit] = new Vector3[2] {
                        GetRandomPatrolPoint(spawnPos, unit.Width, unit.Height),
                        GetRandomPatrolPoint(spawnPos, unit.Width, unit.Height)
                    };
                    _patrolTargetIndex[unit] = 0;
                }
            }
        }

        // Returns true if the region (x, y) to (x+width-1, y+height-1) is walkable.
        private bool IsRegionWalkable(int x, int y, int width, int height)
        {
            for (int dx = 0; dx < width; dx++)
            {
                for (int dy = 0; dy < height; dy++)
                {
                    if (!gridManager.GetNode(x + dx, y + dy).Walkable)
                        return false;
                }
            }
            return true;
        }

        // Picks a random patrol point within patrolRange of a given position, for a given unit size.
        private Vector3 GetRandomPatrolPoint(Vector3 origin, int unitWidth, int unitHeight)
        {
            GridNode node = gridManager.GetNodeFromWorldPosition(origin);
            float nodeSize = gridManager.GridSettings.NodeSize;
            int nodeX = Mathf.RoundToInt(node.WorldPosition.x / nodeSize);
            int nodeY = Mathf.RoundToInt(node.WorldPosition.z / nodeSize);
            int x = Mathf.Clamp(Random.Range(nodeX - patrolRange, nodeX + patrolRange), 0, gridManager.GridSettings.GridSizeX - 1);
            int y = Mathf.Clamp(Random.Range(nodeY - patrolRange, nodeY + patrolRange), 0, gridManager.GridSettings.GridSizeY - 1);
            for (int tries = 0; tries < 20; tries++)
            {
                int tryX = Mathf.Clamp(x + Random.Range(-patrolRange, patrolRange), 0, gridManager.GridSettings.GridSizeX - unitWidth);
                int tryY = Mathf.Clamp(y + Random.Range(-patrolRange, patrolRange), 0, gridManager.GridSettings.GridSizeY - unitHeight);
                if (IsRegionWalkable(tryX, tryY, unitWidth, unitHeight))
                    return gridManager.GetNode(tryX, tryY).WorldPosition;
            }
            return node.WorldPosition;
        }

        // Called every frame to update unit states and behaviors.
        private void Update()
        {
            // For each army, update its units against all other armies.
            for (int i = 0; i < _armies.Count; i++)
            {
                ArmyManager ownArmy = _armies[i];
                // Build a list of all enemy units (from all other armies).
                List<UnitInstance> enemyUnits = new();
                for (int j = 0; j < _armies.Count; j++)
                {
                    if (i == j) continue;
                    enemyUnits.AddRange(_armies[j].Units.Select(x=> x as UnitInstance));
                }
                UpdateArmyUnits(ownArmy, enemyUnits);
            }
        }

        // Updates all units in one army, checking for enemy detection and state transitions.
        private void UpdateArmyUnits(ArmyManager ownArmy, List<UnitInstance> enemyUnits)
        {
            foreach (UnitInstance unit in ownArmy.Units)
            {
                if (unit == null) continue;
                UnitState state = _unitStates[unit];
                switch (state)
                {
                    case UnitState.Patrol:
                        UnitInstance enemy = FindNearestEnemy(unit, enemyUnits);
                        if (enemy != null)
                        {
                            _unitStates[unit] = UnitState.Follow;
                            _followTargets[unit] = enemy;
                            _lastKnownEnemyPos[unit] = enemy.transform.position;
                            unit.SetTarget(enemy.transform.position);
                        }
                        else
                        {
                            PatrolBehavior(unit);
                        }
                        break;
                    case UnitState.Follow:
                        if (!_followTargets.ContainsKey(unit) || _followTargets[unit] == null)
                        {
                            _unitStates[unit] = UnitState.Patrol;
                            break;
                        }
                        UnitInstance target = _followTargets[unit];
                        if (Vector3.Distance(_lastKnownEnemyPos[unit], target.transform.position) > 0.5f)
                        {
                            _lastKnownEnemyPos[unit] = target.transform.position;
                            unit.SetTarget(target.transform.position);
                        }
                        if (Vector3.Distance(unit.transform.position, target.transform.position) > detectionRange * 2)
                        {
                            _unitStates[unit] = UnitState.Patrol;
                            break;
                        }
                        break;
                }
            }
        }

        // Patrol behavior: move between two patrol points.
        private void PatrolBehavior(UnitInstance unit)
        {
            // Get patrol points and current target index for this unit.
            Vector3[] points = _patrolPoints[unit];
            int idx = _patrolTargetIndex[unit];
            // If the unit is close to the patrol point, switch to the next one and set a new target.
            if (Vector3.Distance(unit.transform.position, points[idx]) < 0.2f)
            {
                // Switch to the other patrol point.
                idx = 1 - idx;
                _patrolTargetIndex[unit] = idx;
                // Pick a new random patrol point.
                points[idx] = GetRandomPatrolPoint(unit.transform.position, unit.Width, unit.Height);
                // Set the new patrol target.
                unit.SetTarget(points[idx]);
            }
            // Only set a new target if the unit is not already moving (prevents resetting the path every frame).
            else if (!unit.IsMoving)
            {
                unit.SetTarget(points[idx]);
            }
        }

        // Finds the nearest enemy unit within detection range.
        private UnitInstance FindNearestEnemy(UnitInstance unit, List<UnitInstance> enemyUnits)
        {
            float minDist = detectionRange;
            UnitInstance nearest = null;
            foreach (UnitInstance enemy in enemyUnits)
            {
                if (enemy == null) continue;
                float dist = Vector3.Distance(unit.transform.position, enemy.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = enemy;
                }
            }
            return nearest;
        }
    }
}