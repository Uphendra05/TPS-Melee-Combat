using System.Collections.Generic;

// Base for nodes that own several children (Sequence, Selector, Parallel...).
// Subclasses only need to implement OnUpdate.
public abstract class Composite : Node
{
    protected readonly List<Node> children = new List<Node>();
    protected int currentChild;

    protected Composite(string name = null) : base(name) { }

    public Composite AddChild(Node child)
    {
        child.Parent = this;
        children.Add(child);
        return this; // allows chaining: new Sequence().AddChild(a).AddChild(b)
    }

    // Every new run starts from the first child with clean children.
    protected override void OnEntry()
    {
        currentChild = 0;
        ResetChildren();
    }

    public override void SetOwner(BehaviorTree owner)
    {
        base.SetOwner(owner);
        foreach (Node child in children)
        {
            child.SetOwner(owner);
        }
    }

    public override void Awaken()
    {
        base.Awaken();
        foreach (Node child in children)
        {
            child.Awaken();
        }
    }

    public override void Reset()
    {
        base.Reset();
        currentChild = 0;
        ResetChildren();
    }

    protected void ResetChildren()
    {
        foreach (Node child in children)
        {
            child.Reset();
        }
    }
}