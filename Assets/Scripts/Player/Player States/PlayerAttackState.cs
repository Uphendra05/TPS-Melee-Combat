using UnityEngine;

public class PlayerAttackState : BasePlayerStates
{
    public PlayerAttackState(PlayerStateMachine currentContext, PlayerStateFactory stateFactory) : base(currentContext, stateFactory)
    {
        isRootState = true;
    }

    public override void InitState()
    {
        _ctx.ePlayerStates = PlayerStates.Attack;


    }

    public override void UpdateState()
    {

        if (_ctx.playerInput.isAttackPressed)
        {
            _ctx.playerCombatSystem.Attack();
        }

        if (_ctx.playerCombatSystem.attackFinished)
        {
            SwitchState(_stateFactory.Grounded());
            _ctx.playerCombatSystem.attackFinished = false;
        }


       _ctx.horizontalVelocity = Vector3.zero;
       _ctx.controller.Move(_ctx.m_Animator.deltaPosition);


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


    public override void HandleAnimationEvent(string eventName)
    {
        if (eventName == "AttackEnd")
        {
            //SwitchState(_stateFactory.Grounded());
        }
    }

   


}
