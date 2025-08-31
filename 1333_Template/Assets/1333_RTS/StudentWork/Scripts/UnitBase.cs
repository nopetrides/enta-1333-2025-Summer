using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class UnitBase : MonoBehaviour
{
    [SerializeField] public UnitType _unitType;

    public int Width => _unitType.width;
    public int Height => _unitType.height;
    public float MaxHealth => _unitType.health;
    public float MoveSpeed => _unitType.moveSpeed;
    public float AttackRange => _unitType.attackRange;
    public float AttackDamage => _unitType.attackDamage;
    public string UnitName => _unitType.unitName;

    public abstract void MoveTo(GridNode targetNode);
}
