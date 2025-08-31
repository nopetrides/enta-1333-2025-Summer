using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectUnitButton : MonoBehaviour
{

    [SerializeField] private Image buttonImage;
    [SerializeField] private TMP_Text buttonText;
    [SerializeField] private Button button;




    public void Setup(string unitName, Sprite unitIcon)
    {
        buttonText.text = unitName;
        buttonImage.sprite = unitIcon;
    }
}
