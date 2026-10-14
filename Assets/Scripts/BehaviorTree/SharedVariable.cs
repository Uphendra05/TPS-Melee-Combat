// Non-generic base so BehaviorTreeData can store different types in one dictionary.
public abstract class SharedVariable
{
    public string Name { get; }
    public abstract System.Type ValueType { get; }

    protected SharedVariable(string name)
    {
        Name = name;
    }
}

// A value shared between the nodes of ONE behavior tree (one NPC).
// Nodes look it up once (e.g. in OnAwake), keep the reference, and read/write Value directly.
public class SharedVariable<T> : SharedVariable
{
    public T Value;

    public SharedVariable(string name, T value = default) : base(name)
    {
        Value = value;
    }

    public override System.Type ValueType => typeof(T);
}
