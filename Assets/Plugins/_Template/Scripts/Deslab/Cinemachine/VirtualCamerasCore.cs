using UnityEngine;
using Cinemachine;
using System.Linq;
using System;

/// <summary>
/// A base class for virtual cameras managers
/// </summary>
public class VirtualCamerasCore : MonoBehaviour
{
    protected CinemachineVirtualCamera[] virtualCameras;
    protected CinemachineVirtualCamera currentCamera;

    public static VirtualCamerasCore Instance { get; private set; }
    public CinemachineVirtualCamera CurrentCamera { get { return currentCamera; } }

    protected virtual void Awake()
    {
        Instance = this;

        virtualCameras = GetComponentsInChildren<CinemachineVirtualCamera>();
        SetCurrentCamera();
    }

    public virtual void SwitchCamera(string targetCamName)
    {
        CinemachineVirtualCamera targetCam = virtualCameras.Single(cam => cam.gameObject.name == targetCamName);

        if (!targetCam.Equals(currentCamera))
        {
            if (targetCam == null)
            {
                Debug.LogError("There is no virtual cameras with gameobject's name: " + targetCamName);
            }
            else
            {
                int lowerPriority = targetCam.Priority;
                targetCam.Priority = currentCamera.Priority;
                currentCamera.Priority = lowerPriority;
                SetCurrentCamera();
            }
        }
    }

    protected void SetCurrentCamera()
    {
        CinemachineVirtualCamera camWithHighestPriority = virtualCameras[0];

        for (int i = 1; i < virtualCameras.Length; i++)
        {
            camWithHighestPriority = (virtualCameras[i].Priority > camWithHighestPriority.Priority) ? virtualCameras[i] : camWithHighestPriority;
        }

        currentCamera = camWithHighestPriority;
    }

    protected CinemachineVirtualCamera GetCam(string name)
    {
        foreach (var cam in virtualCameras)
        {
            if (cam.gameObject.name == name)
            {
                return cam;
            }
        }

        return null;
    }

    public virtual void SetTarget(Transform target)
    {
        currentCamera.Follow = target;
        currentCamera.LookAt = target;
    }
}