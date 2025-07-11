using UnityEngine;
using UnityEngine.SceneManagement;

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
    private string _pendingGameName;
    private bool _isPaused = false;

    private void Start()
    {
        _uiManager.ShowScreen(UIScreenType.MainMenu);
        AudioManager.Instance.PlayMusic(FMODEvents.Instance.MenuMusic);
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name != "InGame")
            return;

        if (Input.GetKeyDown(KeyCode.Tab))
            TogglePause();
    }

    private void TogglePause()
    {
        _isPaused = !_isPaused;
        Time.timeScale = _isPaused ? 0f : 1f;

        if (_isPaused)
            _uiManager.ShowScreen(UIScreenType.Pause);
        else
            _uiManager.ShowScreen(UIScreenType.None);
    }

    public void StartGame(string name)
    {
        AudioManager.Instance.StopMusic();
        _pendingGameName = name;
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        SceneManager.LoadScene("InGame", LoadSceneMode.Single);
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "InGame") return;
        SceneManager.sceneLoaded -= HandleSceneLoaded;

        Debug.Log($"Starting game: {_pendingGameName}");
        _camera = Camera.main;

        _gridManager.InitializeGrid();
        _armyManager.Initialize(_gridManager, _unitManager);
        _unitSelectionManager.Initialize(_camera, _gridManager, _unitManager);
        _resourceManager.Initialize();
        _buildingPlacementManager.Initialize(_resourceManager, _armyManager, _gridManager, _unitManager, _camera);

        _isPaused = false;
        Time.timeScale = 1f;

        _uiManager.ShowScreen(UIScreenType.None);
    }

    public void GetMainCamera(Camera camera) => _camera = camera;

    // UI public methods
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
}
