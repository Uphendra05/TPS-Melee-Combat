using UnityEngine;

public class PlayerAttackState : BasePlayerStates
{
    public PlayerAttackState(PlayerStateMachine currentContext, PlayerStateLocator stateFactory) : base(currentContext, stateFactory)
    {
        isRootState = true;
    }

    public override void InitState()
    {
        _ctx.ePlayerStates = PlayerStates.Attack;


    }

    public override void UpdateState()
    {

        if (_ctx.playerInput.isDashPressed) // Also do dodge to interrupt attack
        {
            _ctx.HandleEvade();
        }

        if (_ctx.playerInput.isAttackPressed)
        {
            _ctx.playerCombatSystem.Attack();
            _ctx.playerCombatSystem.HandlePlayerRotation();
        }

        if (_ctx.playerCombatSystem.attackFinished)
        {
            SwitchState(_stateFactory.Grounded());            
           
            _ctx.playerCombatSystem.attackFinished = false;
        }



        


    }

    public override void FixedUpdateState()
    {
    }


    public override void ExitState()
    {
        _ctx.blendTreeVelocity = 0.0f;

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
