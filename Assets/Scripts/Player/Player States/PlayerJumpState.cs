using Unity.Android.Gradle.Manifest;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class PlayerJumpState : BasePlayerStates
{
    public PlayerJumpState(PlayerStateMachine currentContext, PlayerStateFactory stateFactory) : base(currentContext, stateFactory)
    {
        isRootState = true;

    }

    public override void InitState()
    {
        _ctx.ePlayerStates = PlayerStates.Jump;

    }


    public override void UpdateState()
    {
        CheckSwitchStates();
        HandleJump();
        _ctx.verticalVelocity += _ctx.gravity * Time.deltaTime;
    }


    public override void FixedUpdateState()
    {

    }


    public override void ExitState()
    {
        _ctx.m_Animator.SetBool(_ctx.jumpAnimationID, false);

    }

    public override void CheckSwitchStates()
    {
        if (_ctx.controller.isGrounded)
        {
            SwitchState(_stateFactory.Grounded());

        }
    }

    void HandleJump() // TODO: Find a way to implement double jump ( maybe have it as a different state ? )
    {
        _ctx.m_Animator.SetBool(_ctx.groundedAnimationID, _ctx.controller.isGrounded);

        if (_ctx.controller.isGrounded)
        {
            _ctx.jumpsRemaining = _ctx.maxJumps;

        }


        if(_ctx.jumpsRemaining > 0)
        {
            if (_ctx.playerInput.isJumpPressed && _ctx.controller.isGrounded)
            {
                Debug.Log("Inside Jump");

                _ctx.verticalVelocity = Mathf.Sqrt(_ctx.jumpHeight * -2f * _ctx.gravity );

                _ctx.jumpsRemaining--;

                if (_ctx.jumpsRemaining == 1)
                {
                    _ctx.m_Animator.SetBool(_ctx.jumpAnimationID, true);
                    _ctx.m_Animator.SetBool(_ctx.groundedAnimationID, false);

                }


            }
            else
            {
                if (_ctx.controller.isGrounded)
                    _ctx.m_Animator.SetBool(_ctx.jumpAnimationID, false);
            }
        }
       
    }

    public override void InitializeSubState()
    {
        throw new System.NotImplementedException();
    }
}
