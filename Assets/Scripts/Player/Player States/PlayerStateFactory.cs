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
}