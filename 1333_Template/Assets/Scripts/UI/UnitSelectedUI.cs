using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays the selected unit's information in a UI panel.
/// </summary>
public class UnitSelectedUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("TMP component displaying the unit type name.")]
    [SerializeField] private TMP_Text _typeNameText = null;

    [Tooltip("TMP component displaying the grid size (width x height).")]
    [SerializeField] private TMP_Text _gridSizeText = null;

    [Tooltip("TMP component displaying current and max HP.")]
    [SerializeField] private TMP_Text _hpText = null;

    [Tooltip("TMP component displaying movement speed.")]
    [SerializeField] private TMP_Text _moveSpeedText = null;

    [Tooltip("TMP component displaying damage value.")]
    [SerializeField] private TMP_Text _damageText = null;

    [Tooltip("TMP component displaying defense value.")]
    [SerializeField] private TMP_Text _defenseText = null;

    [Tooltip("TMP component displaying attack type.")]
    [SerializeField] private TMP_Text _attackTypeText = null;

    [Tooltip("TMP component displaying attack range.")]
    [SerializeField] private TMP_Text _attackRangeText = null;

    [Tooltip("TMP component displaying attack cooldown.")]
    [SerializeField] private TMP_Text _attackCoolDownText = null;   

    [Tooltip("TMP component displaying vision range.")]
    [SerializeField] private TMP_Text _visionRangeText = null;      

    [Tooltip("TMP component displaying team name.")]
    [SerializeField] private TMP_Text _teamText = null;

    /// <summary>
    /// Populates the UI with the given unit's stats and shows the panel.
    /// </summary>
    /// <param name="unit">Unit whose information to display.</param>
    public void ShowUnitInfo(UnitBase unit)
    {
        if (unit == null || unit.UnitType == null)
            return;

        var type = unit.UnitType;

        _typeNameText.text = $"Unit Name: {type.TypeName}";
        _gridSizeText.text = $"Grid Size: {type.Width} x {type.Height}";
        _hpText.text = $"HP: {unit.CurrentHp:F0} / {type.MaxHp}";
        _moveSpeedText.text = $"Move Speed: {type.MoveSpeed:F1}";
        _damageText.text = $"Damage: {type.Damage}";
        _defenseText.text = $"Defense: {type.Defense}";
        _attackTypeText.text = $"Attack Type: {type.AttackType}";
        _attackRangeText.text = $"Attack Range: {type.AttackRange}";
        _attackCoolDownText.text = $"Attack Cooldown: {type.AttackCooldown:F1}s"; 
        _visionRangeText.text = $"Vision Range: {type.VisionRange}";          
        _teamText.text = $"Team: {unit.UnitTeam}";

        gameObject.SetActive(true);
    }

    /// <summary>
    /// Hides the UI panel.
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
