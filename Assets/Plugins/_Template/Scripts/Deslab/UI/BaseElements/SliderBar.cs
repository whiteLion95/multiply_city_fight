using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System;
using Sirenix.OdinInspector;
using UnityEngine.Animations;

public class SliderBar : MonoBehaviour
{
    [SerializeField] private float changeSmoothness;
    [SerializeField] private bool lookAtCamera;
    [ShowIf("lookAtCamera")] [SerializeField] private bool updateLook;

    public Action OnZero = delegate { };

    private Slider _slider;
    private Camera _mainCam;

    private void Awake()
    {
        Init();

        _slider = GetComponent<Slider>();

        if (lookAtCamera)
        {
            _mainCam = Camera.main;
            LookAtCamera();
        }  
    }

    protected virtual void Init() { }

    private void Update()
    {
        if (updateLook)
        {
            LookAtCamera();
        }
    }

    public void SetMaxValue(float value)
    {
        if (_slider != null)
        {
            _slider.maxValue = value;
            SetValue(value);
        }
    }

    public void SetValue(float value)
    {
        _slider.value = value;
    }

    public void ChangeValue(float value)
    {
        _slider.DOValue(value, changeSmoothness).SetUpdate(true).onComplete +=
            () => { if (value <= 0) OnZero?.Invoke(); };
    }

    private void LookAtCamera()
    {
        transform.parent.LookAt(transform.position + _mainCam.transform.rotation * Vector3.back);
    }
}
