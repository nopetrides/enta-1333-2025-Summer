using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectBuildingButton : MonoBehaviour
{
    [SerializeField] private Image buttonImage;
    [SerializeField] private TMP_Text buttonText;
    [SerializeField] private Button button;

    
    public void Setup(string buildingName, Sprite buildingIcon, int goldCost, int stoneCost, int woodCost) // passing the resource cost
    {
        buttonText.text =
            $"{buildingName}\n" +
            $"<size=14><color=#FFD700>Gold: {goldCost}</color>  " +
            $"<color=#B0B0B0>Stone: {stoneCost}</color>  " +
            $"<color=#8B5A2B>Wood: {woodCost}</color></size>";
        buttonImage.sprite = buildingIcon;
    }
}
