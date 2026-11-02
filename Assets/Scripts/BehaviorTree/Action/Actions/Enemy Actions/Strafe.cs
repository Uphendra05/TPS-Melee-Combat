using UnityEngine;

// Combat movement around the target. The enemy keeps facing the target and moves in short
// random bursts: forward, backward, left, right, or a pause. It stays inside a band around
// standoffRadius. It never finishes by itself: it returns Running while the target is in range
// and Failure when the target is too far, so a ReactiveSelector can hand over to the chase node.
//
// Assumes NPC has:  LookAt(Vector3 worldPoint)  and  Move(Vector3 worldDirection, float speed).
// If your methods are named differently, change the two calls marked below.
public class Strafe : NPCAction
{
    private enum Dir { Idle, Forward, Backward, Left, Right }

    private readonly float standoffRadius;
    private readonly float band;
    private readonly float exitMargin;
    private readonly float minMoveTime;
    private readonly float maxMoveTime;
    private readonly float pauseChance;
    private readonly float speed;
    private readonly float separationRadius;
    private readonly LayerMask allyMask;

    private readonly float speedScale;                       // per-enemy variation
    private readonly Collider[] allyBuffer = new Collider[8];

    private SharedVariable<Transform> target;
    private Dir current = Dir.Idle;
    private Dir last = Dir.Idle;
    private float timer;
    private bool engaged;

    public Strafe(float standoffRadius,
                  float band = 1.5f,
                  float exitMargin = 1.5f,
                  float minMoveTime = 0.8f,
                  float maxMoveTime = 2f,
                  float pauseChance = 0.25f,
                  float speed = 2f,
                  float separationRadius = 1.5f,
                  LayerMask allyMask = default)
    {
        this.standoffRadius = standoffRadius;
        this.band = band;
        this.exitMargin = exitMargin;
        this.minMoveTime = minMoveTime;
        this.maxMoveTime = maxMoveTime;
        this.pauseChance = pauseChance;
        this.speed = speed;
        this.separationRadius = separationRadius;
        this.allyMask = allyMask;

        speedScale = Random.Range(0.85f, 1.15f);
    }

    protected override void OnAwake()
    {
        base.OnAwake();
        target = Owner.Data.Get<Transform>(BTKeys.Target);
    }

    protected override void OnEntry()
    {
        engaged = false;
        current = Dir.Idle;
        last = Dir.Idle;
        timer = 0f; // pick a move on the first tick
    }

    protected override NodeState OnUpdate()
    {
        if (enemyAI == null || target == null || target.Value == null)
        {
            return NodeState.Failure;
        }

        Vector3 toTarget = target.Value.position - enemyAI.transform.position;
        toTarget.y = 0f;
        float distance = toTarget.magnitude;

        // Hysteresis: easier to stay engaged than to become engaged, so it doesn't flicker at the edge.
        float limit = standoffRadius + band + (engaged ? exitMargin : 0f);
        if (distance > limit)
        {
            return NodeState.Failure;
        }
        engaged = true;

        if (distance < 0.01f)
        {
            return NodeState.Running; // standing on the target, avoid a zero direction
        }

        Vector3 forward = toTarget / distance;

        enemyAI.LookAtPlayer(target.Value.position); // <-- your NPC facing method

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            PickMove(distance);
        }

        // Safety limits while a move is running: cancel it and re-pick next tick.
        Dir move = current;
        if ((move == Dir.Forward && distance < standoffRadius - band) ||
            (move == Dir.Backward && distance > standoffRadius + band))
        {
            move = Dir.Idle;
            timer = 0f;
        }

        Vector3 right = Vector3.Cross(Vector3.up, forward);
        Vector3 direction = Vector3.zero;

        switch (move)
        {
            case Dir.Forward:  direction = forward;  break;
            case Dir.Backward: direction = -forward; break;
            case Dir.Left:     direction = -right;   break;
            case Dir.Right:    direction = right;    break;
        }

        if (direction != Vector3.zero)
        {
            enemyAI.Move(direction, speed * speedScale); // <-- your NPC movement method
        }

        return NodeState.Running;
    }

    private void PickMove(float distance)
    {
        timer = Random.Range(minMoveTime, maxMoveTime);

        if (Random.value < pauseChance)
        {
            current = Dir.Idle;                  // 'last' is kept so the next move still avoids a reversal
            timer = Random.Range(0.3f, 1f);
            return;
        }

        // Weights: sideways is most common, forward/back less so.
        float forward = distance < standoffRadius - band ? 0f : 15f;   // too close: no forward
        float backward = distance > standoffRadius + band ? 0f : 15f;  // too far: no backward
        float left = 35f;
        float right = 35f;

        // No instant reversal of the previous move.
        switch (last)
        {
            case Dir.Left:     right = 0f;    break;
            case Dir.Right:    left = 0f;     break;
            case Dir.Forward:  backward = 0f; break;
            case Dir.Backward: forward = 0f;  break;
        }

        ApplySeparation(ref left, ref right);

        float total = forward + backward + left + right;
        if (total <= 0f)
        {
            current = Dir.Idle;
            timer = 0.3f;
            return;
        }

        float roll = Random.value * total;

        if ((roll -= forward) < 0f) current = Dir.Forward;
        else if ((roll -= backward) < 0f) current = Dir.Backward;
        else if ((roll -= left) < 0f) current = Dir.Left;
        else current = Dir.Right;

        last = current;
    }

    // If an ally stands close on one side, lean toward the other side. Skipped if allyMask is empty.
    private void ApplySeparation(ref float left, ref float right)
    {
        if (allyMask.value == 0) return;

        int count = Physics.OverlapSphereNonAlloc(enemyAI.transform.position, separationRadius, allyBuffer, allyMask);

        for (int i = 0; i < count; i++)
        {
            Transform other = allyBuffer[i].transform;
            if (other.IsChildOf(enemyAI.transform)) continue; // that's me

            Vector3 local = enemyAI.transform.InverseTransformPoint(other.position);

            if (local.x > 0f) { right = 0f; left *= 2f; }  // ally on my right: go left
            else { left = 0f; right *= 2f; }               // ally on my left: go right

            return;
        }
    }

   
}
