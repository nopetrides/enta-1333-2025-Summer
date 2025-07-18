using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Burst.Intrinsics;

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
    private ResourceManager _rm;

    /// <summary>
    /// Initializes this button with its data and the placement manager.
    /// Must be called after instantiating the button.
    /// </summary>
    /// <param name="data">ScriptableObject containing building data.</param>
    /// <param name="placementManager">Reference to the BuildingPlacementManager.</param>
    public void Initialize(BuildingDataSO data, BuildingPlacementManager placementManager, ResourceManager rm)
    {
        _buildingData = data;
        _placementManager = placementManager;
        _rm = rm;

        iconImage.sprite = data.ButtonImage;
        nameText.text = data.BuildingName;

        _button = GetComponent<Button>();
        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(OnPlaceButtonClicked);

        // subscribe to resource changes
        _rm.OnResourceChanged += HandleResourceChanged;
        // set initial interactable state
        UpdateInteractableState();
    }
    /// <summary>
    /// Update the button's interactable based on current resources.
    /// </summary>
    private void UpdateInteractableState()
    {
        _button.interactable = _rm.CanAffordCosts(_buildingData.Costs);
    }

    /// <summary>
    /// Called whenever any resource amount changes.
    /// </summary>
    private void HandleResourceChanged(ResourceList type, int newValue)
    {
        // simply refresh state whenever resources change
        UpdateInteractableState();
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

    private void OnDestroy()
    {
        // unsubscribe to avoid memory leaks
        if (_rm != null)
            _rm.OnResourceChanged -= HandleResourceChanged;
    }
}
