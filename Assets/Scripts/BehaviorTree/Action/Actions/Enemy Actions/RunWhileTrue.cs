using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class RunWhileTrue : NPCAction
{
    private readonly string key;
    private SharedVariable<bool> flag;

    public RunWhileTrue(string key)
    {
        this.key = key;
    }

    protected override void OnAwake()
    {
        base.OnAwake();
        flag = Owner.Data.Get<bool>(key);
    }

    protected override NodeState OnUpdate()
    {
        if (flag == null || !flag.Value)
        {
            return NodeState.Failure;
        }

        // Done every tick while locked, so nothing can leave the agent walking.
        //enemyAI.Freeze();
        return NodeState.Running;
    }
}
