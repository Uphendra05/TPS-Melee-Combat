using UnityEngine;
using UnityEngine.InputSystem;

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

        if (_ctx.playerInput.move.sqrMagnitude > 0 && _ctx.playerInput.isSprintPressed)
        {

            _ctx.blendTreeVelocity = 1.0f;
        }
        else if (_ctx.playerInput.move.sqrMagnitude > 0.1f)
        {
            _ctx.blendTreeVelocity = 0.5f;
        }
        else
        {
            _ctx.blendTreeVelocity = 0.0f;

        }

        _ctx.m_Animator.SetFloat(_ctx.blendTreeID, _ctx.blendTreeVelocity, 0.1f, Time.deltaTime);
        _ctx.m_Animator.SetFloat(_ctx.jumpLandBlendTreeID, _ctx.blendTreeVelocity, 0.1f, Time.deltaTime);
    }
    public override void CheckSwitchStates()
    {
        if (_ctx.playerInput.isJumpPressed)
        {
            SwitchState(_stateFactory.Jump());

        }
        if (_ctx.playerInput.isDashPressed)
        {
            _ctx.HandleEvade();
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
