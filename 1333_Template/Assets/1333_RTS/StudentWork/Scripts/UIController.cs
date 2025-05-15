using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI controlsText;
    [SerializeField] private GridTest gridTest;

    private void Start()
    {
        if (controlsText == null)
        {
            Debug.LogError("UIController: Controls text reference is missing!");
            return;
        }

        if (gridTest == null)
        {
            Debug.LogError("UIController: GridTest reference is missing!");
            return;
        }

        // Set up the text component
        controlsText.fontSize = 24;
        controlsText.color = Color.white;
        controlsText.alignment = TextAlignmentOptions.TopLeft;
        
        // Position the text in the top-left corner
        RectTransform rectTransform = controlsText.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.pivot = new Vector2(0, 1);
        rectTransform.anchoredPosition = new Vector2(20, -20);
    }
} 