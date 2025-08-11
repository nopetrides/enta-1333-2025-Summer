using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "GameScene"; 

    public void OnStartGamePressed()
    {
        AudioManager.Instance.PlayMenuSelect();
        GameBoot.Instance.StartGame(gameSceneName);
        Debug.Log("yes"); 
    }

    public void OnQuitPressed()
    {
        AudioManager.Instance.PlayMenuSelect();
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
