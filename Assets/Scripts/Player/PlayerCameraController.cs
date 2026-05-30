using Cinemachine;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    [Section("Camera Settings")]
    public float mouseSens;
    public Transform cameraFollowTarget;
    public float _cinemachineTargetYaw;
    public float _cinemachineTargetPitch;
    public float CameraAngleOverride;
    public float TopClamp = 70.0f;
    public float BottomClamp = -30.0f;
    public const float _threshold = 0.01f;
    public Camera _camera;
    public float _rotationVelocity;
    public bool camLockedToTarget;

    [Section("Camera Shake")]
    public CinemachineVirtualCamera _cam;
    private float shakeTimer;





    private void Awake()
    {
        cameraFollowTarget.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        _camera = Camera.main;
        

    }


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        


    }

    private void LateUpdate()
    {
        if (!camLockedToTarget)
        {
            CameraMovement();
        }
        else
        {
            ResetCameraRotationAfterUnlockingTarget();
        }

        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;
            if(shakeTimer <= 0)
            {
                CinemachineBasicMultiChannelPerlin noise = _cam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
                noise.m_AmplitudeGain = 0f;

            }

        }
       

    }

    public void ShakeCamera(float intensity, float time)
    {
        CinemachineBasicMultiChannelPerlin noise = _cam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        noise.m_AmplitudeGain = intensity;
        shakeTimer = time;
    }

   
    private void CameraMovement()
    {
        float mouseX = UnityEngine.Input.GetAxis("Mouse X") * mouseSens * Time.deltaTime;
        float mouseY = UnityEngine.Input.GetAxis("Mouse Y") * mouseSens * Time.deltaTime;

        _cinemachineTargetYaw += mouseX;
        _cinemachineTargetPitch += -mouseY;

        _cinemachineTargetYaw =   ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        cameraFollowTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride, _cinemachineTargetYaw, 0.0f);

    }
     
    private void ResetCameraRotationAfterUnlockingTarget()
    {
        Vector3 forward = Camera.main.transform.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.001f)
            return;

        Quaternion rot = Quaternion.LookRotation(forward);

        _cinemachineTargetYaw = rot.eulerAngles.y;

        float pitch = rot.eulerAngles.x;

        if (pitch > 180) pitch -= 360;

        _cinemachineTargetPitch = pitch;
    }



    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

}
