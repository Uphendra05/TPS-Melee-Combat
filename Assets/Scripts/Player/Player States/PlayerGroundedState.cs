using UnityEngine;

public class PlayerGroundedState : BasePlayerStates
{

    public PlayerGroundedState(PlayerStateMachine currentContext, PlayerStateFactory stateFactory) : base(currentContext, stateFactory)
    {

    }


    public override void InitState()
    {

        _ctx.ePlayerStates = PlayerStates.Grounded;


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
            SwitchState(_stateFactory.Jog());

        }
    }



}
