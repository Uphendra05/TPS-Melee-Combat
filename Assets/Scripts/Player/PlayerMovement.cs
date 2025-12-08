using System.Collections;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.Windows;

public class PlayerMovement : MonoBehaviour
{
    public InputHandler playerInput { get; private set; }


    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float gravity = -9.81f;
    private float gravityMultiplier = 2.0f;
    public float jumpHeight = 1.5f;
    public int maxJumps = 2;
    private int jumpsRemaining;


    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    private CharacterController controller;
    private Vector3 velocity;
    public bool isGrounded;

    [Header("Evade Settings")]
    [SerializeField] private float dodgeSpeed = 10f;
    [SerializeField] private float dodgeDuration = 0.25f;
    [SerializeField] private float dashCoolDown = 1.25f;
    public bool isDodging = false;


    [Header("Animation Properties")]
    private Animator m_Animator;
    private float _targetRotation = 0.0f;
    private float blendTreeVelocity;
    private int blendTreeID;
    private int dodgeAnimationID;
    private int backstepAnimationID;
    private int jumpAnimationID;
    private int groundedAnimationID;
    private int freefallAnimationID;
    private int jumpLandBlendTreeID;


    [Header("Camera Settings")]
    public float mouseSens;
    public Transform cameraFollowTarget;
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;
    public float CameraAngleOverride;
    public float TopClamp = 70.0f;
    public float BottomClamp = -30.0f;
    private const float _threshold = 0.01f;
    private Camera _camera;
    private float _rotationVelocity;






    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        playerInput = GetComponent<InputHandler>();

        m_Animator = GetComponent<Animator>();
        cameraFollowTarget.transform.rotation = Quaternion.Euler(0.0f,0.0f, 0.0f);
        controller = GetComponent<CharacterController>();
        _camera = Camera.main;

        blendTreeID = Animator.StringToHash("Velocity");
        jumpLandBlendTreeID = Animator.StringToHash("Speed");
        dodgeAnimationID = Animator.StringToHash("IsDodge");
        backstepAnimationID = Animator.StringToHash("Backstep");
        jumpAnimationID = Animator.StringToHash("IsJump");
        freefallAnimationID = Animator.StringToHash("IsFreefall");
        groundedAnimationID = Animator.StringToHash("IsGrounded");
    }

    // Update is called once per frame
    void Update()
    {



        HandleEvade();

        if (UnityEngine.Input.GetKey(KeyCode.LeftShift))
        {
            moveSpeed = 7f;
            blendTreeVelocity = 1;
        }
        else
        {
            moveSpeed = 2f;

        }



        Move();

    }

    private void LateUpdate()
    {
        CameraMovement();

    }



    private void Move()
    {
        

        

        Vector3 moveDir = new Vector3(playerInput.move.x, 0.0f, playerInput.move.y).normalized;

        m_Animator.SetFloat(blendTreeID, blendTreeVelocity, 0.1f, Time.deltaTime);
        m_Animator.SetFloat(jumpLandBlendTreeID, blendTreeVelocity, 0.1f, Time.deltaTime);

        Debug.Log(moveDir.magnitude);

        if (moveDir.magnitude > 0.1f)
        {

            blendTreeVelocity = 0.5f;
            _targetRotation = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg +
                              _camera.transform.eulerAngles.y;

            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, 0.1f);

            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);

            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            controller.Move(targetDirection.normalized * (moveSpeed * Time.deltaTime));

        }
        else
        {
            blendTreeVelocity = 0;

        }


        HandleJump();


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

    private void HandleEvade()
    {
        if (UnityEngine.Input.GetKeyDown(KeyCode.LeftAlt) && !isDodging)
        {
            isDodging = true;

            float x = UnityEngine.Input.GetAxisRaw("Horizontal");
            float z = UnityEngine.Input.GetAxisRaw("Vertical");

            Vector3 moveDir = new Vector3(x, 0.0f, z).normalized;
            Vector3 endPos;

            if (moveDir.magnitude > 0.1f)
            {
                Vector3 dashDirection = transform.forward;

                endPos = transform.position + dashDirection * dodgeSpeed;

                m_Animator.SetTrigger(dodgeAnimationID);
                m_Animator.SetBool(jumpAnimationID, false);
            }
            else
            {
                Vector3 dashDirection = -transform.forward;
                endPos = transform.position + dashDirection * dodgeSpeed;
                m_Animator.SetTrigger(backstepAnimationID);

            }

            StartCoroutine(DoDash(endPos));

        }
    }

    private IEnumerator DoDash(Vector3 endPos)
    {
        float elapsedTime = 0f;

        Vector3 startPos = transform.position;
        Vector3 dashVector = endPos - startPos;

        
        while (elapsedTime < dodgeDuration)
        {
            float t = elapsedTime / dodgeDuration;

            Vector3 targetPos = Vector3.Lerp(startPos, endPos, t);

            Vector3 delta = targetPos - controller.transform.position;

            controller.Move(delta);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Vector3 finalDelta = endPos - controller.transform.position;
        controller.Move(finalDelta);

        yield return new WaitForSeconds(dashCoolDown);
        isDodging = false;
    }

    private void HandleJump()
    {
        Debug.Log("Inside Jump");

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask, QueryTriggerInteraction.Ignore);
        m_Animator.SetBool(groundedAnimationID, isGrounded);

        // Reset jumps when grounded
        if (isGrounded)
        {
            jumpsRemaining = maxJumps;

            if (velocity.y < 0)
                velocity.y = -2f;
        }

        // JUMP INPUT (works in air now too)
        if (playerInput.isJumpPressed && jumpsRemaining > 0)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            if (jumpsRemaining == 1)
            {
                velocity.y *= 1.6f; // Try 1.2 to 1.5 — tweak this value
            }

            jumpsRemaining--;

            if (jumpsRemaining == maxJumps - 1)
            {
                // First jump
                m_Animator.SetBool(jumpAnimationID, true);
                m_Animator.SetBool(groundedAnimationID, false);
            }
            else
            {
                // Second jump
                Debug.Log("Second Jump");
                m_Animator.SetTrigger("DoubleJump");
            }
        }
        else
        {
            // If not jumping, make sure normal jump animation resets
            if (isGrounded)
                m_Animator.SetBool(jumpAnimationID, false);
        }
    }


    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }


}
