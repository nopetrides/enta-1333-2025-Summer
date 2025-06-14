using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI button for a building type that initiates placement mode when clicked.
/// </summary>
[RequireComponent(typeof(Button))]
public class BuildingButton : MonoBehaviour
{
    [SerializeField] private Image iconImage;      // UI image for the building icon
    [SerializeField] private TMP_Text nameText;    // UI text for the building name

    private BuildingDataSO _buildingData;
    private BuildingPlacementManager _placementManager;
    private Button _button;

    /// <summary>
    /// Initializes this button with its data and the placement manager.
    /// Must be called after instantiating the button.
    /// </summary>
    /// <param name="data">ScriptableObject containing building data.</param>
    /// <param name="placementManager">Reference to the BuildingPlacementManager.</param>
    public void Initialize(BuildingDataSO data, BuildingPlacementManager placementManager)
    {
        _buildingData = data;
        _placementManager = placementManager;

        iconImage.sprite = data.ButtonImage;
        nameText.text = data.BuildingName;

        // Cache the Button component and wire up the click handler
        _button = GetComponent<Button>();
        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(OnPlaceButtonClicked);
    }

    /// <summary>
    /// Called by the Button component when clicked.
    /// Starts placement mode for the assigned building type.
    /// </summary>
    public void OnPlaceButtonClicked()
    {
        if (_buildingData == null || _placementManager == null)
        {
            Debug.LogWarning("BuildingButton: Data or PlacementManager is missing.");
            return;
        }

        _placementManager.StartPlacement(_buildingData);
    }
}
