using UnityEngine;

public class EnemyAIAnimation : MonoBehaviour
{
    [Section("Animator")]
    [Tooltip("Leave empty to auto-find an Animator on this object or its children.")]
    [SerializeField] private Animator animator;
    [SerializeField] private string moveXParameter = "MoveX"; // left (-1) / right (+1)
    [SerializeField] private string moveYParameter = "MoveY"; // back (-1) / forward (+1)

    [Section("Tuning")]
   
    [SerializeField] private float dampTime = 0.1f;

    [SerializeField] private float forwardSpeed = 3.5f;   // chase speed
    [SerializeField] private float strafeSpeed = 2.3f;      // match your Strafe speed
    [SerializeField] private float backwardSpeed = 2f;

    private int moveXHash;
    private int moveYHash;
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
        lastPosition = transform.position;
    }

    private void LateUpdate()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        // Measure real movement, so it works whether the enemy moved by
        // SetDestination or by agent.Move.
        Vector3 velocity = (transform.position - lastPosition) / dt;
        lastPosition = transform.position;
        velocity.y = 0f;

        // World movement -> movement relative to where the enemy is facing.
        Vector3 local = transform.InverseTransformDirection(velocity);
        float x = local.x / strafeSpeed;
        float y = local.z >= 0f ? local.z / forwardSpeed : local.z / backwardSpeed;
        Vector2 blend = Vector2.ClampMagnitude(new Vector2(x, y), 1f);

        animator.SetFloat(moveXHash, blend.x, dampTime, dt);
        animator.SetFloat(moveYHash, blend.y, dampTime, dt);
    }
}
