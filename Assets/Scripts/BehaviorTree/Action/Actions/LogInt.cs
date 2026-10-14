using UnityEngine;

// Logs a shared int variable (with the owner's name) and succeeds immediately.
public class LogInt : Action
{
    private readonly string variableName;
    private SharedVariable<int> variable;

    public LogInt(string variableName) : base("LogInt")
    {
        this.variableName = variableName;
    }

    protected override void OnAwake()
    {
        variable = Owner.Data.Get<int>(variableName);
    }

    protected override NodeState OnUpdate()
    {
        if (variable == null) return NodeState.Failure;

        Debug.Log($"[{Owner.name}] {variableName} = {variable.Value}");
        return NodeState.Success;
    }
}
