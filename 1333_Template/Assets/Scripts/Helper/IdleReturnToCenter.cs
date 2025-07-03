using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enemy unit watchdog: if this GameObject stays in Idle state for
/// longer than <see cref="_idleTimeout"/>, it receives a MoveTo command
/// toward the map center (or the closest free grid node).
///
/// • The script does nothing for non-enemy teams.
/// • It never cancels ongoing combat or movement – it simply injects
///   a new destination **once** when the timer expires.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(UnitBase), typeof(UnitMovement))]
public sealed class IdleReturnToCenter : MonoBehaviour
{
    [Header("Config")]
    [Tooltip("Seconds the unit may remain Idle before marching again.")]
    [SerializeField] private float _idleTimeout = 3f;

    // ---------- Cached refs ----------
    private UnitBase _core;
    private UnitMovement _move;

    // ---------- State ----------
    private float _idleTimer;
    private Vector2Int _centerIdx;
    private bool _ready;

    /* ================================================================== */
    /*  Unity                                                             */
    /* ================================================================== */
    private void Awake()
    {
        _core = GetComponent<UnitBase>();
        _move = GetComponent<UnitMovement>();
    }

    private void Start()
    {
        // Grid reference is assigned inside UnitMovement.Init()
        GridManager grid = _move.Grid;
        if (grid == null) return;

        _centerIdx = new Vector2Int(
            grid.GridSettings.GridSizeX / 2,
            grid.GridSettings.GridSizeY / 2);
        _ready = true;
    }

    private void Update()
    {
        if (!_ready) return;
        if (_core.UnitTeam != Team.Enemy) return;
        if (!_core.IsAlive) return;

        if (_core.CurrentState == UnitState.Idle)
        {
            _idleTimer += Time.deltaTime;
            if (_idleTimer >= _idleTimeout)
            {
                RecallToCenter();
                _idleTimer = 0f;
            }
        }
        else
        {
            _idleTimer = 0f; // reset on any non-idle state
        }
    }

    /* ================================================================== */
    /*  Helpers                                                           */
    /* ================================================================== */
    private void RecallToCenter()
    {
        GridManager grid = _move.Grid;
        GridNode center = grid.GetNode(_centerIdx.x, _centerIdx.y);
        List<GridNode> free = grid.FindNearestFreeNodes(center, 1);

        GridNode dst = (free.Count > 0) ? free[0] : center;

        _core.SetReservedDestination(dst);
        _core.MoveTo(dst);
    }
}
