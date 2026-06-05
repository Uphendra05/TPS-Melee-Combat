using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRunState : BasePlayerStates
{
    public PlayerRunState(PlayerStateMachine currentContext, PlayerStateLocator stateFactory) : base(currentContext, stateFactory)
    {

    }

    public override void InitState()
    {
        _ctx.ePlayerStates = PlayerStates.Run;

    }


    public override void UpdateState()
    {
        CheckSwitchStates();


        if (_ctx.moveDir.sqrMagnitude > 0.1f)
        {


            _ctx._targetRotation = Mathf.Atan2(_ctx.moveDir.x, _ctx.moveDir.z) * Mathf.Rad2Deg +
                             _ctx.cameraController._camera.transform.eulerAngles.y;

            float rotation = Mathf.SmoothDampAngle(_ctx.transform.eulerAngles.y, _ctx._targetRotation, ref _ctx.cameraController._rotationVelocity, 0.1f);

            _ctx.transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);

            Vector3 targetDirection = Quaternion.Euler(0.0f, _ctx._targetRotation, 0.0f) * Vector3.forward;

            HandleRunTurn(targetDirection);


            _ctx.horizontalVelocity = targetDirection.normalized * _ctx.moveSpeed;

           

        }
        else
        {
            _ctx.horizontalVelocity = Vector3.zero;
        }


        Debug.Log("RUN STATE");

    }

    public override void FixedUpdateState()
    {
    }


    public override void ExitState()
    {

    }

    public override void CheckSwitchStates()
    {
        if (_ctx.playerInput.move.sqrMagnitude == 0)
        {
            SwitchState(_stateFactory.Idle());
        }
        else if (_ctx.playerInput.move.sqrMagnitude > 0.1f && !_ctx.playerInput.isSprintPressed)
        {
            SwitchState(_stateFactory.Jog());

        }
    }


    private void HandleRunTurn(Vector3 targetDirection)
    {

        float turnAngle = Mathf.Abs(Vector3.SignedAngle(_ctx.transform.forward, targetDirection, Vector3.up));

        if (turnAngle >= 165f && _ctx.playerInput.isSprintPressed)
        {
            _ctx.m_Animator.CrossFade("RunTurn", 0.3f);
        }

    }
}
