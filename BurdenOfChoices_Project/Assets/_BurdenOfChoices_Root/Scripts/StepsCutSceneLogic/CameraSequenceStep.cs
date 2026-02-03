using System;
using Unity.Cinemachine;
using UnityEngine;

public class CameraSequenceStep : MonoBehaviour, ISequenceStep
{
    [SerializeField] CinemachineCamera cameraToActive;
    [SerializeField] int priorityOverride = -1;

    public void Play(Action onFinished)
    {
        CameraManager.Instance.ActivateCamera(cameraToActive, priorityOverride);
        onFinished?.Invoke();
    }
}
