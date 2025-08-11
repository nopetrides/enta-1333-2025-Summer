using System.Collections.Generic;
using UnityEngine;

// abstract base class for all units  in the game
public abstract class UnitBase : MonoBehaviour   
{
    [SerializeField] protected UnitType unitType;    // what type of unit is this (holds stats, prefab)

    public virtual int Width => unitType != null ? unitType.Width : 1;   // how many grid cells this unit occupies horizontally
    public virtual int Height => unitType != null ? unitType.Height : 1;  // how many grid cells this unit occupies vertically

    protected AStarPathfinder pathfinder;

    
    protected List<GridNode> path = new();    // the current path the unit is following
    protected int pathIndex = 0;    // index of the next waypoint in the path
    protected Vector3? movementTarget = null;
    protected bool moving = false;

    public bool IsMoving => moving;
    public List<GridNode> CurrentPath => path;

    protected UnitState state;     // the current state of this unit (movement, attacking)

    public abstract void MoveTo(GridNode node);     // function to call unit to move to a grid node

    public virtual void Tick()
    {
        switch (state)
        {
            case UnitState.Moving:
                MoveAlongPath();
                break;
            case UnitState.Attacking:
                break;
        }
    }

    public virtual void MoveAlongPath()       // move  unit along its current path
    {
        if (!moving || path == null || path.Count == 0 || pathIndex >= path.Count)
            return;

        Vector3 target = path[pathIndex].WorldPosition;
        Vector3 dir = (target - transform.position).normalized;
        float speed = unitType.MoveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target, speed);

        if (Vector3.Distance(transform.position, target) < 0.05f)
        {
            pathIndex++;
            if (pathIndex >= path.Count)
                moving = false;
        }
    }
}
