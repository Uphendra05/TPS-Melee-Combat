using UnityEngine;

public class PlayerJogState : BasePlayerStates
{

    public PlayerJogState(PlayerStateMachine currentContext, PlayerStateFactory stateFactory) : base(currentContext, stateFactory)
    {

    }


    public override void InitState()
    {
        _ctx.ePlayerStates = PlayerStates.Jog;
        Debug.Log("Jogging time");
    }

    public override void CheckSwitchStates()
    {

    }

    public override void ExitState()
    {

    }

    public override void FixedUpdateState()
    {

    }

   

    public override void UpdateState()
    {
    }
}
