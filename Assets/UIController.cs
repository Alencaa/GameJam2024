using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
    public static UIController instance;
    [SerializeField] private TextMeshProUGUI medalText;
    [SerializeField] private TextMeshProUGUI heightClimbedText;

    private void Start()
    {
        instance = this;
    }
    public void ChangeMedalText(int number)
    {
        medalText.text = number.ToString();
    }
    public void ChangeHeightClimbedText(int number)
    {
        heightClimbedText.text = "Height Scaled " + number;
    }
}
