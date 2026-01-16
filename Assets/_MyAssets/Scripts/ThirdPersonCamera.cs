using DG.Tweening;
using FStudio.MatchEngine.Cameras;
using FStudio.MatchEngine.Input;
using Unity.Cinemachine;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [SerializeField] private CinemachineCamera thirdPersonCamera;

    private CameraSystem defaultCameraSystem;

    private void Awake()
    {
        thirdPersonCamera = GetComponent<CinemachineCamera>();

        // find football Camera Script
        defaultCameraSystem = FindAnyObjectByType<CameraSystem>();
    }

    private void Start()
    {

    }



    private void OnEnable()
    {
        TeamInputListener.OnActivePlayerChanged += TeamInputListener_OnActivePlayerChanged;
    }

    private void OnDisable()
    {
        TeamInputListener.OnActivePlayerChanged -= TeamInputListener_OnActivePlayerChanged;
        defaultCameraSystem.UsingCinemachineCamera = false;
    }

    private void TeamInputListener_OnActivePlayerChanged(Transform newTarget)
    {
        CameraTarget newCinemachineTarget = new CameraTarget() { TrackingTarget = newTarget, LookAtTarget = newTarget };
        thirdPersonCamera.Target = newCinemachineTarget;

        // wait a frame and enable the camera
        DOVirtual.DelayedCall(Time.deltaTime, () =>
        {
            thirdPersonCamera.enabled = true;

            // stop using default camera
            defaultCameraSystem.UsingCinemachineCamera = true;
        });
    }

}