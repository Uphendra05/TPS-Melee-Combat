using UnityEngine;

public class Wait : Action
{
    private readonly float duration;
    private float elapsed;

    public Wait(float duration) : base("Wait")
    {
        this.duration = duration;
    }

    protected override void OnEntry()
    {
        elapsed = 0f;
    }

    protected override NodeState OnUpdate()
    {
        elapsed += Time.deltaTime;
        return elapsed >= duration ? NodeState.Success : NodeState.Running;
    }

}
