using System;
using UnityEngine;

public class ResourceManager : MonoBehaviour // resource manager that works for in game resource system
{
    public static ResourceManager Instance { get; private set; }

    [Header("Current Resources")]
    public int gold = 50;
    public int stone = 50;
    public int wood = 50;

    [Header("Passive Income")]
    public int goldPerTick = 10;
    public int stonePerTick = 8;
    public int woodPerTick = 5;
    public float incomeInterval = 5f; // seconds

    public event Action OnResourcesChanged;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        InvokeRepeating(nameof(GivePassiveIncome), incomeInterval, incomeInterval);
    }

    void GivePassiveIncome()
    {
        gold += goldPerTick;
        stone += stonePerTick;
        wood += woodPerTick;
        OnResourcesChanged?.Invoke();
    }

    public bool CanAfford(int goldCost, int stoneCost, int woodCost)
    {
        return gold >= goldCost && stone >= stoneCost && wood >= woodCost;
    }

    public bool SpendResources(int goldCost, int stoneCost, int woodCost)
    {
        if (!CanAfford(goldCost, stoneCost, woodCost))
            return false;
        gold -= goldCost;
        stone -= stoneCost;
        wood -= woodCost;
        OnResourcesChanged?.Invoke();
        return true;
    }

    public void AddResources(int addGold, int addStone, int addWood)
    {
        gold += addGold;
        stone += addStone;
        wood += addWood;
        OnResourcesChanged?.Invoke();
    }
}
