// Adds 1 to a shared int variable and succeeds immediately.
public class IncrementInt : Action
{
    private readonly string variableName;
    private SharedVariable<int> variable;

    public IncrementInt(string variableName) : base("IncrementInt")
    {
        this.variableName = variableName;
    }

    // Look the variable up ONCE and keep the reference; later ticks are plain field access.
    protected override void OnAwake()
    {
        variable = Owner.Data.Get<int>(variableName);
    }

    protected override NodeState OnUpdate()
    {
        if (variable == null) return NodeState.Failure;

        variable.Value++;
        return NodeState.Success;
    }
}
