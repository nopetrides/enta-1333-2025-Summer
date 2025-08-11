using UnityEngine;
using System.Collections;




public class GameSceneInitializer : MonoBehaviour   // in the gamescene, this ensures music starts, core systems reset, and win/lose ui appear at the right time
{
    [Header("Optional Delay Before Starting")]
    [Tooltip("Seconds to wait before kicking off gameplay systems.")]
    [SerializeField] private float initialDelay = 0.5f;

    [Header("References to Core Managers (optional to assign)")]
    [Tooltip("Passive resource generator (optional)")]
    [SerializeField] private ResourceManager resourceManager;
    [Tooltip("Grid for pathfinding/buildings (optional if handled by GameManager)")]
    [SerializeField] private GridManager gridManager;
    [Tooltip("Reference to GameManager (optional—will use Instance if null)")]
    [SerializeField] private GameManager gameManager;

    void Awake()
    {
        
        GameStateManager.Instance.OnGameStateChanged += HandleStateChange;
    }

    void OnDestroy()
    {
        GameStateManager.Instance.OnGameStateChanged -= HandleStateChange;
    }

    void Start()
    {
        
        if (GameStateManager.Instance.CurrentState == GameState.Playing) // only run gameplay init if  arrived via the proper flow
        {
            StartCoroutine(InitializeAfterDelay());

        }
        else
        {
            // if  loaded gamescene directly in editor, bounce back to menu to keep flow consistent
            GameBoot.Instance.ReturnToMenu();
        }
    }

   
    
    private IEnumerator InitializeAfterDelay()   // wait a moment, then reset all core systems for a fresh play session
    {
        yield return new WaitForSeconds(initialDelay);

        AudioManager.Instance.PlayMusic(AudioManager.Instance.gameAmbience, true);

        //  initialize gameplay via gamemanager (grid init, terrain mask, waves)
        var gm = gameManager != null ? gameManager : GameManager.Instance;
        if (gm != null)
        {
            gm.InitializeForGameplay();
        }
        else
        {
            Debug.LogError("GameSceneInitializer: No GameManager found to initialize gameplay!");
        }

       


        UIManager.Instance.HideCountdown();
        //UIManager.Instance.HideWin();
        //UIManager.Instance.HideLose();
    }

  

    private void HandleStateChange(GameState newState)
    {
        switch (newState)
        {
            //case GameState.Win:
            //    UIManager.Instance.ShowWin();
            //    break;

            case GameState.Lose:
                UIManager.Instance.ShowLosePanel();
                break;
        }
    }
}
