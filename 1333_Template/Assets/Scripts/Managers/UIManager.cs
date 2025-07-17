using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Collections;
public enum UIScreenType
{
    None,
    MainMenu,
    Settings,
    Pause,
    HowToPlay
}

/// <summary>
/// Central controller for UI screens (menu, pause, settings, etc).
/// Use ShowScreen() to activate one screen and hide all others.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("UI Screens")]
    [SerializeField] private GameObject _mainMenuScreen;
    [SerializeField] private GameObject _settingsScreen;
    [SerializeField] private GameObject _pauseScreen;
    [SerializeField] private GameObject _howToPlayScreen;

    [Header("UI-Sensitive Game Objects")]
    [SerializeField] private GameObject _buildingPlacementManagerGO;
    [SerializeField] private GameObject _selectionManagerGO;
    [SerializeField] private GameObject _resourcePanelUI;

    [Header("Wave Popup")]
    [SerializeField] private GameObject _wavePopupGO;   // root object (Image + TMP)
    [SerializeField] private TMP_Text _wavePopupText;   // TMP component for message
    [SerializeField] private float _popupDuration = 2f; // seconds shown
    private Coroutine _wavePopupRoutine;

    private Dictionary<UIScreenType, GameObject> _screenMap;

    private void Awake()
    {
        // Initialize map
        _screenMap = new Dictionary<UIScreenType, GameObject>
        {
            { UIScreenType.MainMenu, _mainMenuScreen },
            { UIScreenType.Settings, _settingsScreen },
            { UIScreenType.Pause, _pauseScreen },
            { UIScreenType.HowToPlay, _howToPlayScreen },
        };

        HideAll(); // Start with everything hidden
    }

    /// <summary>
    /// Hides all UI screens.
    /// </summary>
    public void HideAll()
    {
        if (_screenMap == null)
            return;

        foreach (var kvp in _screenMap)
            if (kvp.Value != null)
                kvp.Value.SetActive(false);
    }

    /// <summary>
    /// Shows only the specified UI screen.
    /// Automatically hides others.
    /// </summary>
    public void ShowScreen(UIScreenType type)
    {
        HideAll();

        if (_screenMap.TryGetValue(type, out var screen) && screen != null)
            screen.SetActive(true);

        HandleGameplayObjectsVisibility(type);
    }

    /// <summary>
    /// Enables gameplay objects only when UI is None (game in progress).
    /// </summary>
    private void HandleGameplayObjectsVisibility(UIScreenType screen)
    {
        bool enable = (screen == UIScreenType.None);

        if (_buildingPlacementManagerGO != null)
            _buildingPlacementManagerGO.SetActive(enable);

        if (_selectionManagerGO != null)
            _selectionManagerGO.SetActive(enable);

        if (_resourcePanelUI != null)
            _resourcePanelUI.SetActive(enable);
    }

    /// <summary>
    /// Returns true if the given screen is currently visible.
    /// </summary>
    public bool IsVisible(UIScreenType type)
    {
        return _screenMap.TryGetValue(type, out var screen) && screen.activeSelf;
    }

    public void ShowWavePopup(int waveNumber)
    {
        if (_wavePopupRoutine != null)
            StopCoroutine(_wavePopupRoutine);
        _wavePopupRoutine = StartCoroutine(WavePopupRoutine(waveNumber));
    }

    private IEnumerator WavePopupRoutine(int waveNumber)
    {
        if (_wavePopupGO == null || _wavePopupText == null)
            yield break;

        _wavePopupText.text = $"Wave {waveNumber} is coming!";
        _wavePopupGO.SetActive(true);
        yield return new WaitForSeconds(_popupDuration);
        _wavePopupGO.SetActive(false);
    }
}
