using UnityEngine;

// A read-only "recipe" asset. It builds a tree and declares variables, and stores NOTHING.
// Every NPC calls these methods on its own BehaviorTree component, so each gets fresh nodes and data.
// Rules: never keep nodes/variables in fields here, never write to this asset at runtime.
public abstract class ExternalBehaviorTree : ScriptableObject
{
    /// <summary>
    /// Override to create a new tree and return its root. Always 'new' every node here.
    /// </summary>
    public abstract Node CreateBehaviorTree();

    /// <summary>
    /// Add shared variables here. 'data' belongs to the NPC that is calling.
    /// </summary>
    public virtual void CreateData(BehaviorTreeData data)
    {
    }
}
