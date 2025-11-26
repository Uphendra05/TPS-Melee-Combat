using UnityEngine;

public class PlayerIdleState : BasePlayerStates
{


    public PlayerIdleState(PlayerStateMachine currentContext, PlayerStateFactory stateFactory) : base (currentContext, stateFactory)
    {

    }
   
   
    public override void InitState()
    {
        _ctx.ePlayerStates = PlayerStates.Idle;

    }

    public override void UpdateState()
    {

    }

    public override void FixedUpdateState()
    {

    }


    public override void ExitState()
    {

    }

    public override void CheckSwitchStates()
    {
       

    }
}
