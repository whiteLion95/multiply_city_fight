using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;
using DG.Tweening;
using Deslab.Level;

/// <summary>
/// Left-right movement of an object in local space with finger slides
/// </summary>
public class FingerMovementLocal : MonoBehaviour
{
    [Header("Side to side movement")]
    [SerializeField] private float sideSmoothness = 0.05f;
    [SerializeField] private float range = 1.5f;

    private Camera _mainCam;
    private float _distanceFromCam;
    private bool _isMovingToSide;

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

    private void Start()
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

            if (absTargetLocalX <= range)
                transform.DOLocalMoveX(curDirection * delta, sideSmoothness).SetRelative(true);
            else if (Mathf.Abs(transform.localPosition.x) != range)
                transform.DOLocalMoveX(curDirection * range, sideSmoothness);
            else
                return;

            if ((!_isMovingToSide || prevDirection != curDirection))
            {
                prevDirection = curDirection;
                _isMovingToSide = true;
                StartCoroutine(CheckIfReadyToTurn(finger));
                StartCoroutine(CheckIfNotMovingToSide());
            }
        }
    }

    [Header("Turns while moving from side to side")]
    [SerializeField] [Tooltip("Greater this number, more significantly object will turn from side to side")]private float turnValue = 10f;
    [SerializeField] private float turnSmoothness = 0.5f;
    [SerializeField] [Tooltip("How often to check if object is moving to side")] private float turnCheckRate = 0.01f;
    private void SlightlyTurnToSide(float direction)
    {
        transform.DOLocalRotate(new Vector3(0f, direction * turnValue, 0f), turnSmoothness);
    }

    private float _prevLocalPosX;
    [SerializeField] private float inaccuracy = 0.05f;
    private IEnumerator CheckIfNotMovingToSide()
    {
        while (_isMovingToSide)
        {
            _prevLocalPosX = transform.localPosition.x;
            yield return new WaitForSeconds(turnCheckRate);

            if (transform.localPosition.x >= (_prevLocalPosX - inaccuracy) && transform.localPosition.x <= (_prevLocalPosX + inaccuracy))
            {
                _isMovingToSide = false;
                SlightlyTurnToSide(0f);
                _readyToTurn = false;
            }
        }
    }

    private bool _readyToTurn;
    private IEnumerator CheckIfReadyToTurn(LeanFinger finger)
    {
        while (!_readyToTurn && finger.ScreenDelta.x != 0 && LeanTouch.Instance.UseTouch)
        {
            _prevLocalPosX = transform.localPosition.x;
            yield return new WaitForSeconds(turnCheckRate);

            if (transform.localPosition.x >= (_prevLocalPosX + inaccuracy) || transform.localPosition.x <= (_prevLocalPosX - inaccuracy))
            {
                _readyToTurn = true;
                SlightlyTurnToSide(curDirection);
            }
        }
    }
}
