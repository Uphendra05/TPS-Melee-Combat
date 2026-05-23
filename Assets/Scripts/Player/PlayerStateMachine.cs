using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.InputSystem.XR;
using Unity.VisualScripting;

public class PlayerStateMachine : MonoBehaviour
{

    BasePlayerStates _currentState;
    PlayerStateFactory _stateFactory;

    [Section("Player Settings")]
    public PlayerStates ePlayerStates;
    public InputHandler playerInput { get; private set; }
    public BasePlayerStates CurrentState { get { return _currentState; } set { _currentState = value; } }
    public Vector3 moveDir { get; private set; }
    public PlayerCameraController cameraController { get; private set; }
    public PlayerCombatSystem playerCombatSystem;
    public Vector3 horizontalVelocity;
    public float verticalVelocity;     

    [Section("Movement Settings")]
    public float moveSpeed = 6f;
    public float gravity = -9.81f;
   // private float gravityMultiplier = 2.0f;
    public float jumpHeight = 1.5f;
    public int maxJumps = 2;
    public int jumpsRemaining;


    [Section("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    public CharacterController controller;
    public Vector3 velocity;

    [Section("Evade Settings")]
    [SerializeField] private float dodgeSpeed = 10f;
    [SerializeField] private float dodgeDuration = 0.25f;
    [SerializeField] private float dashCoolDown = 1.25f;
    public bool isDodging = false;


    [Section("Animation Properties")]
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
    [HideInInspector] public float refVelocity;


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
        if (isDodging)
            return;

        isDodging = true;

        Vector3 moveDir =
            new Vector3(playerInput.move.x, 0, playerInput.move.y);

        Vector3 endPos;

        // NO INPUT = BACKSTEP
        if (moveDir.magnitude < 0.1f)
        {
            Vector3 dashDirection = -transform.forward;

            endPos =
                transform.position +
                dashDirection * dodgeSpeed;

            m_Animator.SetTrigger(backstepAnimationID);
        }
        else
        {
            // CAMERA RELATIVE DIRECTION
            Vector3 camForward = cameraController._camera.transform.forward;
            Vector3 camRight = cameraController._camera.transform.right;

            camForward.y = 0;
            camRight.y = 0;

            camForward.Normalize();
            camRight.Normalize();

            Vector3 dashDirection =
                camForward * moveDir.z +
                camRight * moveDir.x;

            dashDirection.Normalize();

            // ROTATE PLAYER
            transform.rotation =
                Quaternion.LookRotation(dashDirection);

            endPos =
                transform.position +
                dashDirection * dodgeSpeed;

            m_Animator.SetTrigger(dodgeAnimationID);
            m_Animator.SetBool(jumpAnimationID, false);
        }

        StartCoroutine(DoDash(endPos));
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



