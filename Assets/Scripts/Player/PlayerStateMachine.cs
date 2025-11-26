using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStateMachine : MonoBehaviour
{

    BasePlayerStates _currentState;
    PlayerStateFactory _stateFactory;
    public PlayerStates ePlayerStates; // Visualize Which state the player is currently in

    public InputHandler playerInput { get; private set; }
    public BasePlayerStates CurrentState { get { return _currentState; } set { _currentState = value; } }



    private void Awake()
    {
        playerInput = GetComponent<InputHandler>();
        _stateFactory = new PlayerStateFactory(this);
        CurrentState = _stateFactory.Grounded();
    }

    void Start()
    {
        CurrentState.InitState();

    }

    void Update()
    {
        CurrentState.UpdateState();
    }



}



public enum PlayerStates
{
    Grounded = 0,
    Idle = 1,
    Jog = 2,
    Run = 3,
}