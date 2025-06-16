// SelectedUIManager.cs
using UnityEngine;

/// <summary>
/// Central manager for all “selected” UI panels (units, barracks, etc.).
/// </summary>
public class SelectedUIManager : MonoBehaviour
{
    [Header("Unit UI")]
    [Tooltip("Panel for showing selected unit info.")]
    [SerializeField] private UnitSelectedUI _unitUI = null;

    [Header("Barrack UI")]
    [Tooltip("Panel for showing selected barrack actions.")]
    [SerializeField] private BarrackSelectedUI _barrackUI = null;

    /// <summary>
    /// Show the appropriate panel for this selectable.
    /// </summary>
    /// <param name="sel">The object that was just selected.</param>
    public void Show(ISelectable sel)
    {
        HideAll();

        if (sel is UnitBase unit)
        {
            // Display unit stats
            _unitUI.ShowUnitInfo(unit);
        }
        else if (sel is BuildingBarrack barrack)
        {
            // Pass the barrack instance into its UI, then show it
            _barrackUI.GetBarrackInstance(barrack);
            _barrackUI.Show();
        }
    }

    /// <summary>
    /// Hide every panel managed here.
    /// </summary>
    public void HideAll()
    {
        if (_unitUI != null)
            _unitUI.Hide();

        if (_barrackUI != null)
            _barrackUI.Hide();
    }
}
