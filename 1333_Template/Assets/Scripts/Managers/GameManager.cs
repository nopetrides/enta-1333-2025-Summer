using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Central controller for menu flow, fade transitions,
/// asynchronous scene loading, pause handling, and runtime manager
/// initialization. Designed for a single “InGame” scene.
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
    /// Fade out, load the “InGame” scene asynchronously, initialize managers
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
        while (op.progress < 0.9f)                      // “almost done”
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

        _isPaused = false;
        Time.timeScale = 1f;
        _uiManager.ShowScreen(UIScreenType.None);
    }
}
