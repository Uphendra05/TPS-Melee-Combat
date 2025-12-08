using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.InputSystem;

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
        if (_ctx.playerInput.move.sqrMagnitude == 0)
        {
            SwitchState(_stateFactory.Idle());
        }
        else if (_ctx.playerInput.move.sqrMagnitude > 0.1f && _ctx.playerInput.isSprintPressed)
        {
            SwitchState(_stateFactory.Run());

        }
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

        HandleMove();
        Debug.Log("JOG STATE");

    }

    public override void InitializeSubState()
    {
    }
    void HandleMove()
    {
        Vector3 moveDir = new Vector3(_ctx.playerInput.move.x, 0.0f, _ctx.playerInput.move.y).normalized;

        _ctx.m_Animator.SetFloat(_ctx.blendTreeID, _ctx.blendTreeVelocity, 0.1f, Time.deltaTime);
        _ctx.m_Animator.SetFloat(_ctx.jumpLandBlendTreeID, _ctx.blendTreeVelocity, 0.1f, Time.deltaTime);


        if (moveDir.magnitude > 0.1f)
        {

            _ctx.blendTreeVelocity = 0.5f;
            _ctx._targetRotation = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg +
                             _ctx._camera.transform.eulerAngles.y;

            float rotation = Mathf.SmoothDampAngle(_ctx.transform.eulerAngles.y, _ctx._targetRotation, ref _ctx._rotationVelocity, 0.1f);

            _ctx.transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);

            Vector3 targetDirection = Quaternion.Euler(0.0f, _ctx._targetRotation, 0.0f) * Vector3.forward;

            _ctx.controller.Move(targetDirection.normalized * (_ctx.moveSpeed * Time.deltaTime));

        }
        else
        {
            _ctx.blendTreeVelocity = 0;

        }
    }
}
