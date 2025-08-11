using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private GameObject losePanel;


    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }



    public void UpdateWave(int wave) // wave ui
    {
        waveText.text = "Wave: " + wave;
    }

    public void ShowCountdown(float time) // countdown between waves
    {
        countdownText.gameObject.SetActive(true);
        countdownText.text = $"Next Wave In: {Mathf.CeilToInt(time)}";
    }

    public void HideCountdown()
    {
        countdownText.gameObject.SetActive(false);
    }

    public void ShowLosePanel()
    {
        losePanel.SetActive(true);
        //  winPanel.SetActive(false);
    }

   
}
