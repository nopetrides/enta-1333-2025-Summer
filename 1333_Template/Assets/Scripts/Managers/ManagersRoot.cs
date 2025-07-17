using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Holds every manager in the game and persists across scene loads.
/// </summary>
public class ManagersRoot : MonoBehaviour
{
    private static ManagersRoot _instance;
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private GameObject _MainMenu;

    private void Awake()
    {
        // Ensure a single instance
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        // Re-inject scene-dependent refs every time a scene loads
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /// <summary>
    /// Refreshes references that depend on the active scene (camera, UI, etc.).
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Camera cam = Camera.main;
        _gameManager.GetMainCamera(cam);

        bool isInGame = scene.name == "InGame";
        bool isMainMenu = scene.name == "MainMenu";

        var bpm = GetComponentInChildren<BuildingPlacementManager>(true);
        if (bpm != null)
            bpm.ToggleUI(isInGame);

        var sel = GetComponentInChildren<SelectionManager>(true);
        if (sel != null)
            sel.gameObject.SetActive(isInGame);

        var rpu = GetComponentInChildren<ResourcePanelUI>(true);
        if (rpu != null)
            rpu.gameObject.SetActive(isInGame);

        if (_MainMenu != null) _MainMenu.SetActive(isMainMenu);
    }
}
