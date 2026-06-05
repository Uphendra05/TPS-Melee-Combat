using System.Collections.Generic;
using UnityEngine;

public class PlayerStateLocator
{

    Dictionary<string, BasePlayerStates> states = new Dictionary<string, BasePlayerStates>();

    public PlayerStateLocator(PlayerStateMachine currentContext)
    {

        states.Add("Grounded", new PlayerGroundedState(currentContext,this));
        states.Add("Idle", new PlayerIdleState(currentContext, this));
        states.Add("Jog", new PlayerJogState(currentContext, this));
        states.Add("Jump", new PlayerJumpState(currentContext, this));
        states.Add("Run", new PlayerRunState(currentContext, this));
        states.Add("Dodge", new PlayerDodgeState(currentContext, this));
        states.Add("Attack", new PlayerAttackState(currentContext, this));


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

    public BasePlayerStates Attack()
    {
        return states["Attack"];
    }
}