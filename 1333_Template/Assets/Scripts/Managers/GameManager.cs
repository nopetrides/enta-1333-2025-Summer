using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Central controller for menu flow, fade transitions,
/// asynchronous scene loading, pause handling, and runtime manager
/// initialization. Designed for a single “InGame?scene.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private GridManager _gridManager;
    [SerializeField] private UnitManager _unitManager;
    [SerializeField] private ArmyManager _armyManager;
    [SerializeField] private SelectionManager _unitSelectionManager;
    [SerializeField] private BuildingPlacementManager _buildingPlacementManager;
    [SerializeField] private ResourceManager _resourceManager;
    [SerializeField] private UIManager _uiManager;

    [Header("Enemy Waves")]
    [SerializeField] private EnemyWaveSpawner _enemyWaveSpawner;

    private Camera _camera;
    private bool _isPaused;

    /* ================================================================== */
    /*  Unity lifecycle                                                   */
    /* ================================================================== */

    private void Start()
    {
        _uiManager.ShowScreen(UIScreenType.MainMenu);
        AudioManager.Instance.PlayMusic(FMODEvents.Instance.MenuMusic);
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name != "InGame") return;

        if (Input.GetKeyDown(KeyCode.Tab))
            TogglePause();
    }

    /* ================================================================== */
    /*  Public entry points                                               */
    /* ================================================================== */

    /// <summary>Begin the transition from main menu to gameplay.</summary>
    public void StartGame() => StartCoroutine(StartGameRoutine());
    /// <summary>
    /// Begin the transition from gameplay back to main menu.
    /// </summary>
    public void ReturnToMainMenu() => StartCoroutine(ReturnToMainMenuRoutine());

    public void OpenHowToPlay() => _uiManager.ShowScreen(UIScreenType.HowToPlay);
    public void OpenSettings() => _uiManager.ShowScreen(UIScreenType.Settings);

    public void OpenPause()
    {
        _uiManager.ShowScreen(UIScreenType.Pause);
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        _uiManager.ShowScreen(UIScreenType.None);
        Time.timeScale = 1f;
    }

    /// <summary>
    /// Show the Main Menu UI.
    /// </summary>
    public void OpenMainMenu()
    {
        _uiManager.ShowScreen(UIScreenType.MainMenu);
    }

    /// <summary>
    /// Quit the application (in Editor: stop play mode).
    /// </summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
    EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }

    public void GetMainCamera(Camera cam) => _camera = cam;

    /* ================================================================== */
    /*  Private helpers                                                   */
    /* ================================================================== */

    /// <summary>Toggles the pause state via Tab key.</summary>
    private void TogglePause()
    {
        _isPaused = !_isPaused;
        Time.timeScale = _isPaused ? 0f : 1f;
        _uiManager.ShowScreen(_isPaused ? UIScreenType.Pause : UIScreenType.None);
    }

    /// <summary>
    /// Fade out, load the “InGame?scene asynchronously, initialize managers
    /// over multiple frames, then fade-in.
    /// </summary>
    private IEnumerator StartGameRoutine()
    {
        /* 1. Fade to black */
        yield return ScreenFader.Instance.Fade(0f, 1f, 0.5f);

        /* 2. Stop menu music (fade out handled inside AudioManager) */
        AudioManager.Instance.StopMusic();

        /* 3. Load scene in background */
        AsyncOperation op = SceneManager.LoadSceneAsync("InGame");
        op.allowSceneActivation = false;
        while (op.progress < 0.9f)                      // almost done?
            yield return null;

        op.allowSceneActivation = true;                 // perform switch
        yield return null;                              // wait one frame

        /* 4. Initialize runtime managers */
        yield return InitializeManagersAsync();

        /* 5. Fade back to gameplay */
        yield return ScreenFader.Instance.Fade(1f, 0f, 0.5f);
    }

    /// <summary>
    /// Initializes heavy systems one per frame to avoid a single frame spike.
    /// </summary>
    private IEnumerator InitializeManagersAsync()
    {
        _camera = Camera.main;

        _gridManager.InitializeGrid();
        yield return null;

        _armyManager.Initialize(_gridManager, _unitManager);
        yield return null;

        _unitSelectionManager.Initialize(_camera, _gridManager, _unitManager);
        yield return null;

        _resourceManager.Initialize();
        yield return null;

        _buildingPlacementManager.Initialize(
            _resourceManager, _armyManager, _gridManager, _unitManager, _camera);
        yield return null;

        if (_enemyWaveSpawner != null)
            _enemyWaveSpawner.StartAutoWaves();
        else
            Debug.LogWarning("GameManager: EnemyWaveSpawner not assigned.");
        yield return null;

        _isPaused = false;
        Time.timeScale = 1f;
    }

    /// <summary>
    /// Fade out, stop game music, load MainMenu scene asynchronously,
    /// fade in, then show menu UI and play menu music.
    /// </summary>
    private IEnumerator ReturnToMainMenuRoutine()
    {
        // 1. Fade to black and reset all managers
        yield return ScreenFader.Instance.Fade(0f, 1f, 0.5f);
        ResetAllManagers();

        // 2. Stop gameplay music (fade-out handled inside AudioManager)
        AudioManager.Instance.StopMusic();

        _isPaused = false;
        Time.timeScale = 1f;
        _uiManager.ShowScreen(UIScreenType.None);

        // 3. Load MainMenu scene in background
        AsyncOperation op = SceneManager.LoadSceneAsync("MainMenu");
        op.allowSceneActivation = false;
        while (op.progress < 0.9f)
            yield return null;
        op.allowSceneActivation = true;
        yield return null; // wait one frame for activation

        // 4. Fade back in
        yield return ScreenFader.Instance.Fade(1f, 0f, 0.5f);

        // 5. Show main menu UI and play menu music
        _uiManager.ShowScreen(UIScreenType.MainMenu);
        AudioManager.Instance.PlayMusic(FMODEvents.Instance.MenuMusic);
    }

    /// <summary>
    /// Resets all runtime managers so InitializeManagersAsync() can run fresh.
    /// </summary>
    public void ResetAllManagers()
    {
        _gridManager.ResetGrid();
        _armyManager.ResetArmy();
        _unitSelectionManager.ResetSelection();
        _buildingPlacementManager.ResetPlacement();
        _resourceManager.ResetResources();

        if (_enemyWaveSpawner != null)
            _enemyWaveSpawner.ResetWaves();
        else
            Debug.LogWarning("GameManager: EnemyWaveSpawner not assigned for reset.");
    }
}
