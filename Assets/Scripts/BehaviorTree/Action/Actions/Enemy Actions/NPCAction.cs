using UnityEngine;

public class NPCAction : Action
{

    protected EnemyAI enemyAI;


    protected override void OnAwake()
    {
        base.OnAwake();
        enemyAI = Owner.GetComponentInParent<EnemyAI>();

        if (enemyAI == null)
        {
            Debug.LogError($"{Name} needs an NPC component, but none was found on '{Owner.name}'.");
        }
    }

    protected override NodeState OnUpdate()
    {
        throw new System.NotImplementedException();
    }

   
}
