using UnityEngine;

// Throwaway test: waits 1 second, logs, waits 2 seconds, logs.
// Assumes ExternalBehaviorTree.CreateBehaviorTree() now returns Node.
[CreateAssetMenu(menuName = "Game/AI/BehaviorTrees/Test/Sequence Test")]
public class SequenceTest : ExternalBehaviorTree
{
    public override Node CreateBehaviorTree()
    {
        return new Sequence()
            .AddChild(new Wait(1f))
            .AddChild(new Log("Waited 1 second"))
            .AddChild(new Wait(2f))
            .AddChild(new Log("Waited 2 more seconds"));
    }
}
