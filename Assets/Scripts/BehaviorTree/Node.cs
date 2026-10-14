using System.Collections.Generic;
using UnityEngine;

public abstract class Node
{
    public string Name { get; protected set; }
    public NodeState CurrentState { get; protected set; } = NodeState.NotStarted;

    public BehaviorTree Owner { get; private set; }
    public Node Parent { get; internal set; }

    protected Node(string name = null)
    {
        Name = string.IsNullOrEmpty(name) ? GetType().Name : name;
    }

    
    public NodeState Evaluate()
    {
        if (CurrentState != NodeState.Running)
        {
            OnEntry();
        }

        CurrentState = OnUpdate();

        if (CurrentState != NodeState.Running)
        {
            OnExit();
        }

        return CurrentState;
    }

   
    public virtual void SetOwner(BehaviorTree owner)
    {
        Owner = owner;
    }

    public virtual void Awaken()
    {
        OnAwake();
    }


  
    public virtual void Reset()
    {
        CurrentState = NodeState.NotStarted;
    }

  
    protected virtual void OnAwake() { }   
    protected virtual void OnEntry() { }   
    protected abstract NodeState OnUpdate(); 
    protected virtual void OnExit() { }   
}
