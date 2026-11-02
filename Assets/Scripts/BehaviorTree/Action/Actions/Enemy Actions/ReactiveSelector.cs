using UnityEngine;

// Like Selector, but every tick it re-checks the higher-priority children (earlier in the list).
// If one of them starts running or succeeds, the lower-priority child that was running is aborted.
// Order the children from highest to lowest priority.
public class ReactiveSelector : Composite
{
    private int runningChild = -1;

    public ReactiveSelector(string name = null) : base(name) { }

    protected override void OnEntry()
    {
        base.OnEntry();
        runningChild = -1;
    }

    protected override NodeState OnUpdate()
    {
        for (int i = 0; i < children.Count; i++)
        {
            Node child = children[i];

            switch (child.Evaluate())
            {
                case NodeState.Running:
                    AbortPrevious(i);
                    runningChild = i;
                    return NodeState.Running;

                case NodeState.Success:
                    AbortPrevious(i);
                    runningChild = -1;
                    return NodeState.Success;

                case NodeState.Failure:
                    if (runningChild == i) runningChild = -1; // it was running and has just failed
                    break;                                    // try the next child

                default:
                    Debug.LogError($"ReactiveSelector child '{child.Name}' returned {child.CurrentState}");
                    return NodeState.Failure;
            }
        }

        runningChild = -1;
        return NodeState.Failure;
    }

    // Cuts off the child that was running before a higher-priority one took over.
    private void AbortPrevious(int newIndex)
    {
        if (runningChild >= 0 && runningChild != newIndex)
        {
            children[runningChild].Abort();
        }
    }
}
