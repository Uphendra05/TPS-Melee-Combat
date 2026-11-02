using UnityEngine;


[CreateAssetMenu(menuName = "Game/AI/BehaviorTrees/Test/Move To Player Test")]
public class MoveToPlayerTest : ExternalBehaviorTree
{
    public override void CreateData(BehaviorTreeData data)
    {
        data.Add<Transform>(BTKeys.Target, null);
    }

    public override Node CreateBehaviorTree()
    {
        return new Sequence().AddChild(new MoveToTarget(2f));
           
    }
}
