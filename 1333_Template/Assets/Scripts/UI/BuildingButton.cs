using UnityEngine;
using UnityEngine.UI;
using TMPro;  // If you’re using TextMeshPro; otherwise use UnityEngine.UI.Text

/// <summary>
/// Binds a BuildingDataSO to a UI Button: sets icon, text, and click behavior.
/// </summary>
[RequireComponent(typeof(Button))]
public class BuildingButton : MonoBehaviour
{
    [SerializeField] private Image _iconImage;    // Reference to the Image component for the icon
    [SerializeField] private TMP_Text _nameText;  // Reference to the TextMeshProUGUI for the label

    private BuildingDataSO _data;

    /// <summary>
    /// Initializes this button with the given building data.
    /// </summary>
    public void Initialize(BuildingDataSO data)
    {
        _data = data;

        // populate UI
        _iconImage.sprite = data.ButtonImage;
        _nameText.text = data.BuildingName;
    }

    /// <summary>
    /// Called when this button is clicked.
    /// </summary>
    public void BuildingImageClicked()
    {
        // e.g. start placement mode for this building:
        // BuildingPlacementManager.Instance.StartPlacement(_data);

        Debug.Log($"Clicked place-{_data.BuildingName}");
    }
}
