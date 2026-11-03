using UnityEngine;

public class EnemyAIAnimation : MonoBehaviour
{
    
    [Section("Blend")]
    [SerializeField] private float fullBlendSpeed = 0.5f;
    [SerializeField] private float deadZone = 0.01f;
    [SerializeField] private float dampTime = 0.1f;

    [Section("Playback speed")]
    [SerializeField] private float forwardAuthoredSpeed = 3.5f;
    [SerializeField] private float backwardAuthoredSpeed = 2f;
    [SerializeField] private float strafeAuthoredSpeed = 2.3f;
    [SerializeField] private float minPlayback = 0.6f;
    [SerializeField] private float maxPlayback = 1.6f;


    private Animator animator;
    private string moveXParameter = "MoveX";     // left (-1) / right (+1)
    private string moveYParameter = "MoveY";     // back (-1) / forward (+1)
    private string speedParameter = "AnimSpeed";


    private int moveXHash;
    private int moveYHash;
    private int speedHash;
    private bool hasSpeedParameter;
    private Vector3 lastPosition;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (animator == null)
        {
            Debug.LogError($"NPCAnimation on '{name}' found no Animator.");
            enabled = false;
            return;
        }

        if (animator.applyRootMotion)
        {
            Debug.LogWarning($"NPCAnimation on '{name}': Apply Root Motion is on and will fight the NavMeshAgent. Turn it off.");
        }

        moveXHash = Animator.StringToHash(moveXParameter);
        moveYHash = Animator.StringToHash(moveYParameter);
        speedHash = Animator.StringToHash(speedParameter);

        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.nameHash == speedHash && parameter.type == AnimatorControllerParameterType.Float)
            {
                hasSpeedParameter = true;
                break;
            }
        }

        lastPosition = transform.position;
    }

    private void LateUpdate()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        Vector3 velocity = (transform.position - lastPosition) / dt;
        lastPosition = transform.position;
        velocity.y = 0f;

        Vector3 local = transform.InverseTransformDirection(velocity);
        Vector2 move = new Vector2(local.x, local.z);
        float speedNow = move.magnitude;

        Vector2 blend = Vector2.zero;
        float playback = 1f;

        if (speedNow > 0.001f)
        {
            Vector2 direction = move / speedNow;

            blend = direction * Mathf.Clamp01(speedNow / fullBlendSpeed);

            float axisAuthored = direction.y >= 0f ? forwardAuthoredSpeed : backwardAuthoredSpeed;
            float authored = Mathf.Lerp(axisAuthored, strafeAuthoredSpeed, Mathf.Abs(direction.x));
            playback = Mathf.Clamp(speedNow / authored, minPlayback, maxPlayback);
        }

        if (Mathf.Abs(blend.x) < deadZone) blend.x = 0f;
        if (Mathf.Abs(blend.y) < deadZone) blend.y = 0f;

        animator.SetFloat(moveXHash, blend.x, dampTime, dt);
        animator.SetFloat(moveYHash, blend.y, dampTime, dt);

        if (hasSpeedParameter)
        {
            animator.SetFloat(speedHash, playback, dampTime, dt);
        }
    }
}
