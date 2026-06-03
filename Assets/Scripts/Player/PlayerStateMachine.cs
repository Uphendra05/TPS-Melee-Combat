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

    public int blendTreeID { get; private set; } 
    public int dodgeAnimationID { get; private set; }
    public int backstepAnimationID { get; private set; }
    public int jumpAnimationID { get; private set; }
    public int groundedAnimationID { get; private set; }
    public int freefallAnimationID { get; private set; }
    public int jumpLandBlendTreeID { get; private set; }

    [HideInInspector] public float refVelocity;
    private Vector3 m_RootMotionDelta;


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

        // Always use root motion delta as horizontal velocity
        horizontalVelocity = new Vector3(m_RootMotionDelta.x, 0f, m_RootMotionDelta.z);
        
        Vector3 finalVelocity = new Vector3(m_RootMotionDelta.x, 0f, m_RootMotionDelta.z); 
        finalVelocity.y = verticalVelocity;

        Debug.Log($"horizontalVelocity: {horizontalVelocity} | finalVelocity: {finalVelocity}");

        controller.Move(finalVelocity * Time.deltaTime);






    }


    void OnAnimatorMove()
    {
        if (playerCombatSystem.isAttacking )
        {
            Vector3 delta = m_Animator.deltaPosition / Time.deltaTime;
            float forwardAmount = Vector3.Dot(delta, transform.forward);
            m_RootMotionDelta = transform.forward * Mathf.Max(0f, forwardAmount);

            if( playerCombatSystem.currentTarget != null)
            {
                Vector3 direction = playerCombatSystem.currentTarget.position - transform.position;
                direction.y = 0f;
                if (direction != Vector3.zero)
                {
                    Quaternion targetRot = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, playerCombatSystem.attackTurnSpeed * Time.deltaTime);
                }
            }
            else
            {
                transform.rotation *= m_Animator.deltaRotation;
            }

        }
        else
        {
            m_RootMotionDelta = (m_Animator.deltaPosition / Time.deltaTime) * moveSpeed;
            transform.rotation *= m_Animator.deltaRotation;
        }
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



