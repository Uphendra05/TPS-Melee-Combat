using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.InputSystem.XR;
using Unity.VisualScripting;

public class PlayerStateMachine : MonoBehaviour
{

    BasePlayerStates _currentState;
    PlayerStateFactory _stateFactory;
    public PlayerStates ePlayerStates; // Visualize Which state the player is currently in

    public InputHandler playerInput { get; private set; }
    public BasePlayerStates CurrentState { get { return _currentState; } set { _currentState = value; } }
    public Vector3 moveDir { get; private set; }

    public PlayerCombatSystem playerCombatSystem;
    public PlayerCameraController cameraController { get; private set; }


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


    private void Awake()
    {
        playerInput = GetComponent<InputHandler>();
        controller = GetComponent<CharacterController>();
        cameraController = GetComponent<PlayerCameraController>();

        _stateFactory = new PlayerStateFactory(this);
        CurrentState = _stateFactory.Grounded();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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


        moveDir = new Vector3(playerInput.move.x, 0.0f, playerInput.move.y).normalized;
        CurrentState.UpdateAllStates();

      
         Vector3 finalVelocity = horizontalVelocity;
         finalVelocity.y = verticalVelocity;
         controller.Move(finalVelocity * Time.deltaTime);
       

        

       


    }

   


    public void HandleEvade()
    {
        if (!isDodging)
        {
            isDodging = true;

            Vector3 moveDir = new Vector3(playerInput.move.x, 0, playerInput.move.y).normalized;
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

    public IEnumerator DoDash(Vector3 endPos)
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

        horizontalVelocity = Vector3.zero;
        yield return new WaitForSeconds(dashCoolDown);
        isDodging = false;
    }


    public void OnDodgeFinished() // Animation Event
    {
        Debug.Log("Inside Dodge finised");
        CurrentState?.HandleAnimationEvent("DodgeFinished");
    }

    public void OnAttackFinished() // Animation Event
    {
        CurrentState?.HandleAnimationEvent("AttackEnd");
    }

    
}



public enum PlayerStates
{
    Grounded = 0,
    Idle = 1,
    Jog = 2,
    Run = 3,
    Jump = 4,
    Dodge = 5,
    Attack = 6
}