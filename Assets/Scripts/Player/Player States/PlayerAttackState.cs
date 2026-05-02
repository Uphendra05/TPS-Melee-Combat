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

        if (_ctx.playerInput.isAttackPressed)
        {
            _ctx.playerCombatSystem.SwordAttack();
        }
    }

    public override void UpdateState()
    {
        _ctx.horizontalVelocity = Vector3.zero;

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
            SwitchState(_stateFactory.Grounded());
        }
    }


}
