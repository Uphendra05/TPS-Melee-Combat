using System.Collections.Generic;
using UnityEngine;

// The per-NPC "blackboard". Each BehaviorTree component owns its own instance.
// Storage only: add variables by name, get them back with a type check.
public class BehaviorTreeData
{
    private readonly Dictionary<string, SharedVariable> variables = new Dictionary<string, SharedVariable>();

    public SharedVariable<T> Add<T>(string name, T defaultValue = default)
    {
        if (variables.TryGetValue(name, out SharedVariable existing))
        {
            Debug.LogError($"BehaviorTreeData: variable '{name}' already exists.");
            return existing as SharedVariable<T>;
        }

        SharedVariable<T> variable = new SharedVariable<T>(name, defaultValue);
        variables.Add(name, variable);
        return variable;
    }

    // Returns null (and logs an error) if the name is missing or registered with another type.
    public SharedVariable<T> Get<T>(string name)
    {
        if (!variables.TryGetValue(name, out SharedVariable variable))
        {
            Debug.LogError($"BehaviorTreeData: variable '{name}' does not exist.");
            return null;
        }

        if (variable is SharedVariable<T> typed)
        {
            return typed;
        }

        Debug.LogError($"BehaviorTreeData: variable '{name}' is {variable.ValueType.Name}, not {typeof(T).Name}.");
        return null;
    }

    public bool Has(string name)
    {
        return variables.ContainsKey(name);
    }
}
