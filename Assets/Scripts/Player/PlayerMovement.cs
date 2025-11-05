using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.Windows;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    private CharacterController controller;
    private Vector3 velocity;
    public bool isGrounded;


    private Animator m_Animator;
    public float mouseSens;
    public Transform cameraFollowTarget;
    private float _targetRotation = 0.0f;


    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;
    public float CameraAngleOverride;

    public float TopClamp = 70.0f;

    public float BottomClamp = -30.0f;

    private const float _threshold = 0.01f;

    private Camera _camera;
    private float _rotationVelocity;
    private float _verticalVelocity;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_Animator = GetComponent<Animator>();
        cameraFollowTarget.transform.rotation = Quaternion.Euler(0.0f,0.0f, 0.0f);
        controller = GetComponent<CharacterController>();
        _camera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {

       
        

       
        if(UnityEngine.Input.GetKeyDown(KeyCode.LeftAlt)) 
        {
            m_Animator.SetTrigger("Dodge");

        }

        if (UnityEngine.Input.GetKey(KeyCode.LeftShift))
        {
            moveSpeed = 7f;

            m_Animator.SetBool("IsRun", true);
        }
        else
        {
            moveSpeed = 2f;

            m_Animator.SetBool("IsRun", false);

        }

        Move();
    }

    private void LateUpdate()
    {
        CameraMovement();

    }



    private void Move()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        float x = UnityEngine.Input.GetAxis("Horizontal");
        float z = UnityEngine.Input.GetAxis("Vertical");

        Vector3 moveDir = new Vector3(x, 0.0f, z).normalized;

        if (moveDir.magnitude > 0.1f)
        {
            _targetRotation = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg +
                              _camera.transform.eulerAngles.y;

            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, 0.1f);

            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);

            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            controller.Move(targetDirection.normalized * (moveSpeed * Time.deltaTime));
            m_Animator.SetBool("IsWalk", true);

        }
        else
        {
            m_Animator.SetBool("IsWalk", false);
            m_Animator.SetTrigger("Idle");
        }

        if (UnityEngine.Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            m_Animator.SetTrigger("Jump");
        }
       

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void CameraMovement()
    {
        float mouseX = UnityEngine.Input.GetAxis("Mouse X") * mouseSens * Time.deltaTime;
        float mouseY = UnityEngine.Input.GetAxis("Mouse Y") * mouseSens * Time.deltaTime;

        _cinemachineTargetYaw += mouseX;
        _cinemachineTargetPitch += -mouseY;

        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        cameraFollowTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride, _cinemachineTargetYaw, 0.0f);
    }


    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
}
