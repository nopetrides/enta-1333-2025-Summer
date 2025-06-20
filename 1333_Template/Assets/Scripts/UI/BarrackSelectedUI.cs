using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI panel that appears when a Barrack is selected. Generates one button per ArmyType.
/// </summary>
public class BarrackSelectedUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform _buttonContainer = null;   // Parent with Layout Group
    [SerializeField] private Button _buttonPrefab = null;         // Button prefab that contains a TMP_Text child

    private BuildingBarrack _barrack;
    private readonly List<Button> _spawnButtons = new();

    /// <summary>Show this panel.</summary>
    public void Show() => gameObject.SetActive(true);

    /// <summary>Hide this panel.</summary>
    public void Hide() => gameObject.SetActive(false);

    private void Awake() => Hide();

    /// <summary>Bind the selected barrack and rebuild the button list.</summary>
    public void GetBarrackInstance(BuildingBarrack barrack)
    {
        _barrack = barrack;

        // Guard clause: check required references
        if (_barrack == null || _buttonPrefab == null || _buttonContainer == null)
        {
            Debug.LogError("BarrackSelectedUI: Missing barrack reference or UI assignments");
            return;
        }

        RebuildButtons();
    }

    /// <summary>Clear barrack reference and destroy existing buttons.</summary>
    public void ClearBarrackInstance()
    {
        _barrack = null;

        foreach (Button btn in _spawnButtons)
            Destroy(btn.gameObject);

        _spawnButtons.Clear();
    }

    // -------------------- Internal helpers --------------------

    /// <summary>Create one button for each ArmyType the barrack can spawn.</summary>
    private void RebuildButtons()
    {
        // Remove old buttons
        foreach (Button btn in _spawnButtons)
            Destroy(btn.gameObject);

        _spawnButtons.Clear();

        // Create new buttons
        foreach (ArmyType type in _barrack.SpawnableTypes)
        {
            Button btn = Instantiate(_buttonPrefab, _buttonContainer);

            TMP_Text label = btn.GetComponentInChildren<TMP_Text>(true);
            if (label == null)
            {
                Debug.LogError("BarrackSelectedUI: TMP_Text not found in Button prefab");
                continue;
            }

            label.text = type.ToString();
            btn.onClick.AddListener(() => _barrack.SpawnUnit(type));
            _spawnButtons.Add(btn);
        }
    }

    public void DestroyBuilding()
    {
        _barrack.DestroySelf();
    }
}
