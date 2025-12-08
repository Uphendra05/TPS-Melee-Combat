using UnityEngine;

public class PlayerGroundedState : BasePlayerStates
{

    public PlayerGroundedState(PlayerStateMachine currentContext, PlayerStateFactory stateFactory) : base(currentContext, stateFactory)
    {
        isRootState = true;
    }


    public override void InitState()
    {
        _ctx.ePlayerStates = PlayerStates.Grounded;
        InitializeSubState();

    }


    public override void ExitState()
    {
    }

    public override void FixedUpdateState()
    {
    }

   
    public override void UpdateState()
    {
        CheckSwitchStates();
    }
    public override void CheckSwitchStates()
    {
        if (_ctx.playerInput.isJumpPressed)
        {
            SwitchState(_stateFactory.Jump());

        }
    }

    public override void InitializeSubState()
    {
        if( _ctx.playerInput.move.sqrMagnitude == 0)
        {
            SetSubstate(_stateFactory.Idle());
        }
        else if(_ctx.playerInput.move.sqrMagnitude == 1)
        {
            SetSubstate(_stateFactory.Jog());

        }
        else if(_ctx.playerInput.move.sqrMagnitude > 0 && _ctx.playerInput.isSprintPressed)
        {
            SetSubstate(_stateFactory.Run());

        }

    }
}
