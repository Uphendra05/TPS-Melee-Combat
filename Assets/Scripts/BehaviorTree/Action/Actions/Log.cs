using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class Log : Action
{
    private readonly string message;

    public Log(string message) : base("Log")
    {
        this.message = message;
    }

    protected override NodeState OnUpdate()
    {
        string owner = Owner != null ? Owner.name : "no owner";
        Debug.Log($"[{owner}] {message}");
        return NodeState.Success  ;
    }
}
