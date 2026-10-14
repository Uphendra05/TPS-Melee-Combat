using UnityEngine;

public class Selector : Composite
{
    public Selector(string name = null) : base(name) { }

    protected override NodeState OnUpdate()
    {
        while (currentChild < children.Count)
        {
            Node child = children[currentChild];

            switch (child.Evaluate())
            {
                case NodeState.Running:
                    return NodeState.Running;   // stay on this child, resume next tick

                case NodeState.Success:
                    return NodeState.Success;   // one child worked, so the selector succeeds
                case NodeState.Failure:
                    currentChild++;             // try the next child in the same tick
                    break;                      // leaves the switch, the while loop continues

                default:
                    // Unexpected result (e.g. NotStarted): fail loudly instead of looping forever
                    Debug.LogError($"Selector child '{child.Name}' returned {child.CurrentState}");
                    return NodeState.Failure;
            }
        }

        return NodeState.Failure; // every child failed
    }
}
