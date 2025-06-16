using UnityEngine;

public class BarrackSelectedUI : MonoBehaviour
{
    private BuildingBarrack _barrack;

    /// <summary>Show the selection UI.</summary>
    public void Show() => gameObject.SetActive(true);

    /// <summary>Hide the selection UI.</summary>
    public void Hide() => gameObject.SetActive(false);

    private void Awake()
    {
        // start hidden
        Hide();
    }

    /// <summary>Inject the barrack this UI controls.</summary>
    public void GetBarrackInstance(BuildingBarrack barrack)
    {
        _barrack = barrack;
        if (_barrack == null)
        {
            Debug.LogWarning("BarrackSelectedUI: barrack prefab is null");
        }
    }

    public void ClearBarrackInstance()
    {
        _barrack = null;
    }

    public void OnSpawnButtonClicked()
    {
        // Delegate the actual spawn call back to the barrack
        _barrack.SpawnUnit();
    }
}
