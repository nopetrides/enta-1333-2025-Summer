using UnityEngine.SceneManagement;
using UnityEngine;

public class BootManager : Singleton<BootManager>
{
    [Header("Scene Names")]
    [SerializeField] private string bootSceneName = "BootScene";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private void Start()
    {
        Debug.Log("Boot manager starts");
        var active = SceneManager.GetActiveScene().name;
        if (active == bootSceneName)
            SceneManager.LoadScene(mainMenuSceneName);
        Debug.Log("Boot manager loaded main menu scene");
    }
}
