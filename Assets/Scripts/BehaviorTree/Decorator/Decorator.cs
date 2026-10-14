using UnityEngine;

// Base for nodes that wrap exactly one child (Repeater, Inverter, RandomChance...).
// Subclasses only need to implement OnUpdate.
public abstract class Decorator : Node
{
    protected readonly Node child;

    protected Decorator(Node child, string name = null) : base(name)
    {
        this.child = child;
        child.Parent = this;
    }

    // Every new run starts with a clean child. 
    protected override void OnEntry()
    {
        child.Reset();
    }

    public override void SetOwner(BehaviorTree owner)
    {
        base.SetOwner(owner);
        child.SetOwner(owner);
    }

    public override void Awaken()
    {
        base.Awaken();
        child.Awaken();
    }

    public override void Reset()
    {
        base.Reset();
        child.Reset();
    }
}
