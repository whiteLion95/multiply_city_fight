using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Deslab.UI;
using TMPro;

public class LevelLabel : MonoBehaviour
{
    private TextMeshProUGUI _levelText;

    private void Awake()
    {
        _levelText = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        CanvasGroupWindow.OnWindowShowed += OnWindowShowedHandler;
        CanvasGroupWindow.OnWindowShowing += OnWindowShowingHandler;
    }

    private void OnDisable()
    {
        CanvasGroupWindow.OnWindowShowed -= OnWindowShowedHandler;
        CanvasGroupWindow.OnWindowShowing -= OnWindowShowingHandler;
    }

    private void OnWindowShowedHandler(CanvasGroupWindow window)
    {
        if (window is WinScreen || window is LoseScreen)
        {
            SetLevelText("LEVEL <color=#FF1080>  " + StaticManager.levelID);
        }
    }

    private void OnWindowShowingHandler(CanvasGroupWindow window)
    {
        if (!(window is WinScreen || window is LoseScreen))
        {
            SetLevelText("Level " + StaticManager.levelID);
        }
    }

    private void SetLevelText(string value)
    {
        _levelText.text = value;
    }
}
