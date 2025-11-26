

public abstract class BasePlayerStates 
{
    protected PlayerStateMachine _ctx;
    protected PlayerStateFactory _stateFactory;


    public BasePlayerStates(PlayerStateMachine currentContext, PlayerStateFactory stateFactory)
    {
        _ctx = currentContext;
        _stateFactory = stateFactory;

    }

    public abstract void InitState();
    public abstract void UpdateState();
    public abstract void FixedUpdateState();
    // TODO: Add Late update if needed
    public abstract void ExitState();

    public abstract void CheckSwitchStates();
   

    public void SwitchState(BasePlayerStates newState)
    {
        ExitState();

        newState.InitState();

        _ctx.CurrentState = newState;

    }

}
