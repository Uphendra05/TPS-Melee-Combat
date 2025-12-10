using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStateMachine : MonoBehaviour
{

    BasePlayerStates _currentState;
    PlayerStateFactory _stateFactory;
    public PlayerStates ePlayerStates; // Visualize Which state the player is currently in

    public InputHandler playerInput { get; private set; }
    public BasePlayerStates CurrentState { get { return _currentState; } set { _currentState = value; } }


    public Vector3 horizontalVelocity; // XZ movement
    public float verticalVelocity;     // Y movement

    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float gravity = -9.81f;
    private float gravityMultiplier = 2.0f;
    public float jumpHeight = 1.5f;
    public int maxJumps = 2;
    public int jumpsRemaining;


    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    public CharacterController controller;
    public Vector3 velocity;

    [Header("Evade Settings")]
    [SerializeField] private float dodgeSpeed = 10f;
    [SerializeField] private float dodgeDuration = 0.25f;
    [SerializeField] private float dashCoolDown = 1.25f;
    public bool isDodging = false;


    [Header("Animation Properties")]
    public Animator m_Animator;
    public float _targetRotation = 0.0f;
    public float blendTreeVelocity;
    public int blendTreeID;
    public int dodgeAnimationID;
    public int backstepAnimationID;
    public int jumpAnimationID;
    public int groundedAnimationID;
    public int freefallAnimationID;
    public int jumpLandBlendTreeID;


    [Header("Camera Settings")]
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

    private void Awake()
    {
        playerInput = GetComponent<InputHandler>();
        cameraFollowTarget.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        controller = GetComponent<CharacterController>();
        _camera = Camera.main;
        _stateFactory = new PlayerStateFactory(this);
        CurrentState = _stateFactory.Grounded();
    }

    void Start()
    {
        blendTreeID = Animator.StringToHash("Velocity");
        jumpLandBlendTreeID = Animator.StringToHash("Speed");
        dodgeAnimationID = Animator.StringToHash("IsDodge");
        backstepAnimationID = Animator.StringToHash("Backstep");
        jumpAnimationID = Animator.StringToHash("IsJump");
        freefallAnimationID = Animator.StringToHash("IsFreefall");
        groundedAnimationID = Animator.StringToHash("IsGrounded");

        CurrentState.InitState();

    }

    void Update()
    {

        

        CurrentState.UpdateAllStates();


        Vector3 finalVelocity = horizontalVelocity;
        finalVelocity.y = verticalVelocity;
        controller.Move(finalVelocity * Time.deltaTime);


    }

    private void LateUpdate()
    {
        CameraMovement();

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



public enum PlayerStates
{
    Grounded = 0,
    Idle = 1,
    Jog = 2,
    Run = 3,
    Jump = 4
}