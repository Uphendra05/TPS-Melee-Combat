using UnityEngine;

public class MoveToTarget : NPCAction
{

    public float stoppingDistance;
    public SharedVariable<Transform> target;

    public MoveToTarget(float stoppingDistance)
    {
        this.stoppingDistance = stoppingDistance;
    }

    protected override void OnAwake()
    {
        base.OnAwake();

        target = Owner.Data.Get<Transform>(BTKeys.Target);
    }


     protected override NodeState OnUpdate()
    {
        if (target.Value == null)
        {
            return NodeState.Failure;
        }
        enemyAI.LookAtPlayer(target.Value.position);


        // flat direction and distance, so height differences don't matter
        Vector3 toTarget = target.Value.position - Owner.transform.position;
        toTarget.y = 0f;
        float distance = toTarget.magnitude;

        if (distance <= stoppingDistance)
        {
            enemyAI.StopMoving();              // cancel the path so it holds its position
            return NodeState.Success;
        }

        // aim at a point stoppingDistance short of the player
        Vector3 destination = target.Value.position - toTarget.normalized * stoppingDistance;
        enemyAI.MoveTo(destination);
        return NodeState.Running;
    }

    protected override void OnExit() { enemyAI.StopMoving(); }

}
