using UnityEngine;

// Enemies walk toward the target. Once inside the standoff band they circle it with Strafe.
// If the target moves away, Strafe fails and the chase node takes over again.
[CreateAssetMenu(menuName = "Game/AI/BehaviorTrees/Standoff Test")]
public class StandoffTest : ExternalBehaviorTree
{
    [Header("Standoff")]
    [SerializeField] private float standoffRadius = 6f;
    [SerializeField] private float band = 1.5f;
    [SerializeField] private float exitMargin = 1.5f;

    [Header("Strafe timing")]
    [SerializeField] private float minMoveTime = 0.8f;
    [SerializeField] private float maxMoveTime = 2f;
    [SerializeField, Range(0f, 1f)] private float pauseChance = 0.25f;
    [SerializeField] private float strafeSpeed = 2f;

    [Header("Separation (optional)")]
    [SerializeField] private LayerMask allyMask;
    [SerializeField] private float separationRadius = 1.5f;

    public override void CreateData(BehaviorTreeData data)
    {
        data.Add<Transform>(BTKeys.Target, null);
        data.Add<bool>(BTKeys.isStunned, false);
    }

    public override Node CreateBehaviorTree()
    {
        // Highest priority first: strafe when in range, otherwise chase.
        return new ReactiveSelector()
            .AddChild(new Strafe(standoffRadius, band, exitMargin, minMoveTime, maxMoveTime,
                                 pauseChance, strafeSpeed, separationRadius, allyMask))
            .AddChild(new MoveToTarget(standoffRadius)); // <-- your chase node and its constructor
    }
}
