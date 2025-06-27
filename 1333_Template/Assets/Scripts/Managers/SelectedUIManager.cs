// SelectedUIManager.cs
using UnityEngine;

/// <summary>
/// Central manager that toggles the correct “selected” UI panel
/// depending on what the player clicked (unit, barrack, resource building, …).
/// </summary>
public class SelectedUIManager : MonoBehaviour
{
    [Header("Unit UI")]
    [Tooltip("Panel that shows selected unit statistics.")]
    [SerializeField] private UnitSelectedUI _unitUI = null;

    [Header("Barrack UI")]
    [Tooltip("Panel that shows barrack spawn buttons.")]
    [SerializeField] private BarrackSelectedUI _barrackUI = null;

    [Header("Resource-building UI")]
    [Tooltip("Panel that shows resource building info.")]
    [SerializeField] private ResourceBuildingSelectedUI _resourceUI = null;

    /// <summary>
    /// Displays the appropriate panel for the selectable that was clicked.
    /// </summary>
    public void Show(ISelectable sel)
    {
        HideAll();   // close every panel first

        switch (sel)
        {
            case UnitBase unit:
                _unitUI.ShowUnitInfo(unit);
                break;

            case BuildingBarrack barrack:
                _barrackUI.GetBarrackInstance(barrack);
                _barrackUI.Show();
                break;

            case BuildingResource res:
                _resourceUI.Bind(res, res.buildingData);
                break;
        }
    }

    /// <summary>
    /// Hides every managed panel and clears stored references.
    /// </summary>
    public void HideAll()
    {
        if (_unitUI != null) _unitUI.Hide();
        if (_barrackUI != null) _barrackUI.Hide();
        if (_resourceUI != null) _resourceUI.Clear();
    }
}
