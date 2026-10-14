using UnityEngine;

// Runs children in order. Fails as soon as one fails, succeeds only if all succeed.
public class Sequence : Composite
{
    public Sequence(string name = null) : base(name) { }

    protected override NodeState OnUpdate()
    {
        while (currentChild < children.Count)
        {
            Node child = children[currentChild];

            switch (child.Evaluate())
            {

                case NodeState.Running:
                    return NodeState.Running;

                case NodeState.Failure:
                    return NodeState.Failure    ;

                case NodeState.Success:
                    currentChild++;   // move on in the same tick
                    break;            // leaves the switch, the while loop continues

                default:
                    Debug.LogError($"{children[currentChild].Name} returned {children[currentChild].CurrentState}");
                    return NodeState.Failure;
            }
        }

        return NodeState.Success;
    }
}