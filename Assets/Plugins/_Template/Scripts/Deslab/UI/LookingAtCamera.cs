using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookingAtCamera : MonoBehaviour
{
    [SerializeField] private bool updateLook;

    private Camera _mainCam;

    private void Awake()
    {
        _mainCam = Camera.main;
    }

    private void OnEnable()
    {
        LookAtCamera();
    }

    // Update is called once per frame
    void Update()
    {
        if (updateLook)
            LookAtCamera();
    }

    private void LookAtCamera()
    {
        transform.LookAt(transform.position + _mainCam.transform.forward);
    }
}