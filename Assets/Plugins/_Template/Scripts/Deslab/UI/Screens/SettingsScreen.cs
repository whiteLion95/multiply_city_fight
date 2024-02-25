using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Deslab.UI;

public class SettingsScreen : CanvasGroupWindow
{
    [SerializeField] private Image sFXDim;
    [SerializeField] private Image hapticDim;

    public void ToggleHaptics()
    {
        HapticsManager.Instance.ToggleVibration();
        hapticDim.gameObject.SetActive(!HapticsManager.Instance.VibrationEnabled);
    }

    public void ToggleSFX()
    {
        bool isMute = AudioManagerCore.Instance.ToggleMute();
        sFXDim.gameObject.SetActive(isMute);
    }

    public void ShowPrivacyPolicy()
    {
        Debug.Log("Showing privacy policy");
    }
}
