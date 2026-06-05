

public abstract class BasePlayerStates 
{
    protected PlayerStateMachine _ctx;
    protected PlayerStateLocator _stateFactory;
    protected BasePlayerStates _currentSubState;
    protected BasePlayerStates _currentSuperState;
    protected bool isRootState = false;
    public BasePlayerStates(PlayerStateMachine currentContext, PlayerStateLocator stateFactory)
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

        if (isRootState)
        {
            _ctx.CurrentState = newState;

        }
        else if (_currentSuperState != null)
        {
            _currentSuperState.SetSubstate(newState);
        }

    }

    public virtual void InitializeSubState() { }

    public void UpdateAllStates() 
    {
        UpdateState();

        _currentSubState?.UpdateState();


    }
    protected void SetSuperState(BasePlayerStates superState) 
    { 
        _currentSuperState = superState;
    }
    protected void SetSubstate(BasePlayerStates subState) 
    {   
        _currentSubState = subState;
        subState.SetSuperState(this);
    }

    public virtual void HandleAnimationEvent(string eventName) { }
}
