using UnityEngine;
using UnityEngine.InputSystem.XR;

public class PlayerDodgeState : BasePlayerStates
{
    public PlayerDodgeState(PlayerStateMachine currentContext, PlayerStateFactory stateFactory) : base(currentContext, stateFactory)
    {
        isRootState = true;
    }


    public override void InitState()
    {
        _ctx.ePlayerStates = PlayerStates.Dodge;

        if (_ctx.playerInput.isDashPressed)
        {
            _ctx.HandleEvade();
        }
    }

    public override void UpdateState()
    {

        Debug.Log("DODGE STATE");
    }
    public override void FixedUpdateState()
    {

    }

    public override void CheckSwitchStates()
    {

    }

    public override void ExitState()
    {
    }



    public override void HandleAnimationEvent(string eventName)
    {
        if (eventName == "DodgeFinished")
        {
            SwitchState(_stateFactory.Grounded());
        }
    }

}
