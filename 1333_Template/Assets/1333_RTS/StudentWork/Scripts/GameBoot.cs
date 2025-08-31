using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;




public class GameBoot : MonoBehaviour
                                                // manages boot, scene loading, and fade transition
{
    public static GameBoot Instance { get; private set; }

    [Tooltip("Optional UI component to fade the screen.")]
    [SerializeField] private ScreenFader screenFader;
    [SerializeField] private GameObject howToPlay;

    void Awake()           // enforce singleton
    {
        
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    private void Start()
    {
        if (GameStateManager.Instance.CurrentState == GameState.MainMenu)
        {

            AudioManager.Instance.PlayMusic(AudioManager.Instance.menuMusic, true);

            
        }
    }


    
    
    public void StartGame(string sceneName)  // called by ui to start the game,  switches to playing state and loads the given scene
    {
        // Update state
        GameStateManager.Instance.SetState(GameState.Playing);

        if (screenFader != null)
            StartCoroutine(TransitionWithFade(sceneName));
        else
            SceneManager.LoadScene(sceneName);
    }

  
    private IEnumerator TransitionWithFade(string sceneName)    
                                                                 // fade out, load new scene, then fade in
                                                                
    {
        screenFader.FadeOut();
        yield return new WaitForSeconds(screenFader.FadeDuration);
        SceneManager.LoadScene(sceneName);
        yield return null;
        screenFader.FadeIn();
    }

   
    public void ReturnToMenu()
    {
        GameStateManager.Instance.SetState(GameState.MainMenu);
        SceneManager.LoadScene("MainMenu");
    }

    public void ShowHowToPlay()

    {

        howToPlay.SetActive(true);
    }
}
