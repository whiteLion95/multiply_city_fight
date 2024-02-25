using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.NiceVibrations;

public class HapticsManager : Singleton<HapticsManager>
{
    [SerializeField] private bool vibrationEnabled = true;

    public bool VibrationEnabled { get => vibrationEnabled; set => vibrationEnabled = value; }

    private void Start()
    {
#if !UNITY_EDITOR
        CheckHapticsSupport();
#endif
    }

    private void CheckHapticsSupport()
    {
        bool testUniversal = MMVibrationManager.HapticsSupported();

        if (testUniversal)
            Debug.Log("Haptcis are supported on this device");
        else
            Debug.Log("Haptics are not supported on this device!");
    }

    public void LightVibrate()
    {
        if (vibrationEnabled)
        {
            MMVibrationManager.Haptic(HapticTypes.LightImpact);
            Debug.Log("Light vibration");
        }
    }

    public void SuccessVibrate()
    {
        if (vibrationEnabled)
        {
            MMVibrationManager.Haptic(HapticTypes.Success);
            Debug.Log("Succes vibration");
        }
    }

    public void FailureVibrate()
    {
        if (vibrationEnabled)
            MMVibrationManager.Haptic(HapticTypes.Failure);
    }

    public void ToggleVibration()
    {
        vibrationEnabled = (vibrationEnabled == true) ? vibrationEnabled = false : vibrationEnabled = true;
    }

    public void SelectionVibrate()
    {
        if (vibrationEnabled)
        {
#if UNITY_ANDROID
            MMNVAndroid.AndroidVibrate(30);
#endif
#if UNITY_IOS
                MMVibrationManager.Haptic(HapticTypes.Selection);
#endif
        }
    }
}
