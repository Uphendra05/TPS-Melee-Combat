using UnityEngine;

public class PlayerRunState : BasePlayerStates
{
    public PlayerRunState(PlayerStateMachine currentContext, PlayerStateFactory stateFactory) : base(currentContext, stateFactory)
    {

    }

    public override void CheckSwitchStates()
    {
        if (_ctx.playerInput.move.sqrMagnitude == 0)
        {
            SwitchState(_stateFactory.Idle());
        }
        else if (_ctx.playerInput.move.sqrMagnitude > 0.1f)
        {
            SwitchState(_stateFactory.Jog());

        }
    }

    public override void ExitState()
    {
    }

    public override void FixedUpdateState()
    {
    }

    public override void InitializeSubState()
    {
    }

    public override void InitState()
    {
        _ctx.ePlayerStates = PlayerStates.Run;
    }

    public override void UpdateState()
    {
        CheckSwitchStates();
        Debug.Log("RUN STATE");

    }
}
