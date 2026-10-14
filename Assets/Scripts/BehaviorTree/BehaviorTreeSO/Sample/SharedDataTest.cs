using UnityEngine;

// Throwaway test for shared data.
// Each loop: increment "counter", wait 1 second, log "counter".
[CreateAssetMenu(menuName = "Game/AI/BehaviorTrees/Test/Shared Data Test")]
public class SharedDataTest : ExternalBehaviorTree
{
    public override void CreateData(BehaviorTreeData data)
    {
        data.Add<int>("counter", 0);
    }

    public override Node CreateBehaviorTree()
    {
        return new Sequence()
            .AddChild(new IncrementInt("counter"))
            .AddChild(new Wait(1f))
            .AddChild(new LogInt("counter"));
    }
}
