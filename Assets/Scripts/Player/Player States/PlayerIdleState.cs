using UnityEngine;

public class PlayerIdleState : BasePlayerStates
{


    public PlayerIdleState(PlayerStateMachine currentContext, PlayerStateLocator stateFactory) : base (currentContext, stateFactory)
    {

    }
   
   
    public override void InitState()
    {
        _ctx.ePlayerStates = PlayerStates.Idle;

    }

    public override void UpdateState()
    {
        CheckSwitchStates();
        Debug.Log("IDLE STATE");
    }

    public override void FixedUpdateState()
    {

    }


    public override void ExitState()
    {

    }

    public override void CheckSwitchStates()
    {
        if (_ctx.playerInput.move.sqrMagnitude > 0.1f && _ctx.playerInput.isSprintPressed)
        {
            SwitchState(_stateFactory.Run());

        }
        else if (_ctx.playerInput.move.sqrMagnitude > 0.1f)
        {
            SwitchState(_stateFactory.Jog());

        }
        if (_ctx.playerInput.move.sqrMagnitude == 0)
        {
            SwitchState(_stateFactory.Idle());
        }

    }

   
}
