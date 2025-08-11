using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceUI : MonoBehaviour
{
    [Header("Resource Texts")]
    public TMP_Text goldText;
    public TMP_Text stoneText;
    public TMP_Text woodText;

    [Header("Resource Images")]
    public Image goldImage;
    public Image stoneImage;
    public Image woodImage;

    void Start()
    {
        ResourceManager.Instance.OnResourcesChanged += UpdateUI;
        UpdateUI();
    }

    void OnDestroy()
    {
        if (ResourceManager.Instance != null)
            ResourceManager.Instance.OnResourcesChanged -= UpdateUI;
    }

    void UpdateUI()
    {
        goldText.text = $" {ResourceManager.Instance.gold}";
        stoneText.text = $" {ResourceManager.Instance.stone}";
        woodText.text = $" {ResourceManager.Instance.wood}";
       
    }
}
