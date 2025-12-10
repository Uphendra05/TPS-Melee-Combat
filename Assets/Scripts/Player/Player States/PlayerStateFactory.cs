using System.Collections.Generic;
using UnityEngine;

public class PlayerStateFactory
{

    Dictionary<string, BasePlayerStates> states = new Dictionary<string, BasePlayerStates>();

    public PlayerStateFactory(PlayerStateMachine currentContext)
    {

        states.Add("Grounded", new PlayerGroundedState(currentContext,this));
        states.Add("Idle", new PlayerIdleState(currentContext, this));
        states.Add("Jog", new PlayerJogState(currentContext, this));
        states.Add("Jump", new PlayerJumpState(currentContext, this));
        states.Add("Run", new PlayerRunState(currentContext, this));
        states.Add("Dodge", new PlayerDodgeState(currentContext, this));


    }


    public BasePlayerStates Grounded()
    {
        return states["Grounded"];
    }

    public BasePlayerStates Idle()
    {
        return states["Idle"];
    }

    public BasePlayerStates Jog()
    {
        return states["Jog"];
    }

    public BasePlayerStates Jump()
    {
        return states["Jump"];
    }
    public BasePlayerStates Run()
    {
        return states["Run"];
    }

    public BasePlayerStates Dodge()
    {
        return states["Dodge"];
    }
}