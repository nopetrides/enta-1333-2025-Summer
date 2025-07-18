using UnityEngine;
using UnityEngine.SceneManagement;

public class GameBoot : MonoBehaviour
{
    public static GameBoot Instance { get; private set; }  // game boot script instance

    [SerializeField] private ScreenFader screenFader; // fader

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartGame(string sceneName)
    {
        if (screenFader)
        {
            StartCoroutine(TransitionWithFade(sceneName));
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    private System.Collections.IEnumerator TransitionWithFade(string sceneName)
    {
        screenFader.FadeOut();
        yield return new WaitForSeconds(screenFader.FadeDuration);
        SceneManager.LoadScene(sceneName);
       
        yield return null;
        screenFader.FadeIn();
    }
}
