using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Deslab.Level;
using Lean.Touch;

/// <summary>
/// Left-right movement of a car object in local space with finger slides
/// </summary>
public class CarFingerLocalMovement : MonoBehaviour
{
    private Camera _mainCam;
    private float _distanceFromCam;

    private void OnEnable()
    {
        LevelManager.OnLevelStarted += OnLevelStartedHandler;
    }

    private void OnDisable()
    {
        LeanTouch.OnFingerUpdate -= LeanTouch_OnFingerUpdate;
        LevelManager.OnLevelStarted -= OnLevelStartedHandler;
    }

    private void Init()
    {
        _mainCam = Camera.main;
        // There must be a VirtualCamerasCore component on the main camera
        _distanceFromCam = Vector3.Distance(_mainCam.transform.position, VirtualCamerasCore.Instance.CurrentCamera.m_Follow.position);
    }

    private void OnLevelStartedHandler()
    {
        Init();
        LeanTouch.OnFingerUpdate += LeanTouch_OnFingerUpdate;
    }

    private void LeanTouch_OnFingerUpdate(LeanFinger finger)
    {
        MoveLeftRight(finger);
    }

    private float prevDirection;
    private float curDirection;
    private void MoveLeftRight(LeanFinger finger)
    {
        if (finger.ScreenDelta.x != 0)
        {
            Vector3 prevFingerPos = _mainCam.ScreenToWorldPoint(new Vector3(finger.LastScreenPosition.x, 0, _distanceFromCam));
            Vector3 curFingerPos = _mainCam.ScreenToWorldPoint(new Vector3(finger.ScreenPosition.x, 0, _distanceFromCam));

            curDirection = Mathf.Sign(finger.ScreenPosition.x - finger.LastScreenPosition.x);
            float delta = (curFingerPos - prevFingerPos).magnitude;

            float absTargetLocalX = Mathf.Abs(transform.localPosition.x + (curDirection * delta));

            //if (absTargetLocalX <= range)
                
            //else if (Mathf.Abs(transform.localPosition.x) != range)
                
            //else
            //    return;
        }
    }
}
