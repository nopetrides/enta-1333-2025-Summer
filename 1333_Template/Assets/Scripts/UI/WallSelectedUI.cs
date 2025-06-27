// WallSelectedUI.cs
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Panel that appears when a wall segment is selected.
/// Shows wall name / description and one button per nearby ranged unit.
/// Also has open slots for destroy or other buttons if needed.
/// </summary>
public class WallSelectedUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text _wallNameText = null;
    [SerializeField] private TMP_Text _wallDescText = null;
    [SerializeField] private Transform _buttonContainer = null;   // layout group
    [SerializeField] private Button _unitButtonPrefab = null;     // prefab
    [SerializeField] private Button _destroyButton = null;

    private BuildingWall _wall;
    private readonly List<Button> _dynamicButtons = new();

    /* -------------------------------------------------------- */
    /*  Life-cycle                                             */
    /* -------------------------------------------------------- */

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
    private void Awake() => Hide();

    /* -------------------------------------------------------- */
    /*  Public API                                             */
    /* -------------------------------------------------------- */

    public void Bind(BuildingWall wall, BuildingDataSO data, UnitManager manager)
    {
        _wall = wall;

        _wallNameText.text = data.BuildingName;
        _wallDescText.text = data.Description;

        foreach (Transform child in _buttonContainer)
            Destroy(child.gameObject);

        // wipe old buttons
        foreach (Button b in _dynamicButtons) Destroy(b.gameObject);
        _dynamicButtons.Clear();

        // show “already occupied” message or list ranged units
        if (_wall.HasGarrison)
        {
            // create a button that ungarrisons the unit
            Button btn = Instantiate(_unitButtonPrefab, _buttonContainer);
            TMP_Text txt = btn.GetComponentInChildren<TMP_Text>(true);

            txt.text = "Release Unit";
            txt.color = Color.blue;

            btn.onClick.AddListener(() =>
            {
                _wall.Ungarrison();   // call the new method
                Hide();               // close the panel afterwards
            });

            _dynamicButtons.Add(btn);
        }
        else
        {
            List<UnitBase> ranged = wall.GetNearbyRangedUnits(manager);
            foreach (UnitBase u in ranged)
            {
                Button btn = Instantiate(_unitButtonPrefab, _buttonContainer);
                TMP_Text label = btn.GetComponentInChildren<TMP_Text>(true);
                label.text = u.UnitType.TypeName;

                btn.onClick.AddListener(() =>
                {
                    _wall.Garrison(u);
                    Hide();
                });
                _dynamicButtons.Add(btn);
            }
            if (ranged.Count == 0)
            {
                TMP_Text msg = Instantiate(_unitButtonPrefab, _buttonContainer)
                               .GetComponentInChildren<TMP_Text>(true);
                msg.text = "No ranged unit nearby";
                msg.color = Color.gray;
                _dynamicButtons.Add(msg.GetComponentInParent<Button>());
            }
        }

        // destroy button
        _destroyButton.onClick.RemoveAllListeners();
        _destroyButton.onClick.AddListener(() =>
        {
            _wall.DestroySelf();
            Hide();
        });

        Show();
    }

    public void Clear()
    {
        for (int i = 0; i < _dynamicButtons.Count; i++)
        {
            Button b = _dynamicButtons[i];
            if (b != null)              
                Destroy(b.gameObject);
        }
        _dynamicButtons.Clear();
        foreach (Transform child in _buttonContainer)
            Destroy(child.gameObject);
        Hide();
    }
}
