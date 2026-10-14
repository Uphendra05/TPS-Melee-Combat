using UnityEngine;

// Lives on the NPC. Owns this NPC's Data and Root, builds them from the ExternalBehaviorTree
// recipe, and ticks the tree every frame.
public class BehaviorTree : MonoBehaviour
{
    [Section("Behavior Tree Settings")]
    [SerializeField]
    bool RestartWhenDone = true;

    [SerializeField]
    ExternalBehaviorTree externalBehaviorTree;

    public BehaviorTreeData Data { get; private set; } = new BehaviorTreeData();
    public Node Root { get; private set; }
    public int Restarts { get; private set; }

    bool hasAwaken;
    bool finished;

    void Awake()
    {
        EnsureInitialized();
    }

    void Update()
    {
        TickTree();
    }

    // Safe to call from anywhere; only runs once.
    public void EnsureInitialized()
    {
        if (hasAwaken) return;
        hasAwaken = true;

        if (externalBehaviorTree == null)
        {
            Debug.LogError($"Behavior Tree in object '{gameObject.name}' has no ExternalBehaviorTree assigned.");
            return;
        }

        // Data first, so it exists before any node looks up a variable in OnAwake.
        externalBehaviorTree.CreateData(Data);
        Root = externalBehaviorTree.CreateBehaviorTree();

        if (Root == null)
        {
            Debug.LogError($"Behavior Tree in object '{gameObject.name}' has a null root node.");
            return;
        }

        Root.SetOwner(this); // give every node its owner
        Root.Awaken();       // call OnAwake on every node
    }

    // Kept in one small method so a tick interval can be added here later.
    void TickTree()
    {
        if (Root == null || finished) return;

        NodeState state = Root.Evaluate();

        if (state == NodeState.Running) return;

        if (RestartWhenDone)
        {
            Root.Reset();
            Restarts++;
        }
        else
        {
            finished = true;
        }
    }
}
