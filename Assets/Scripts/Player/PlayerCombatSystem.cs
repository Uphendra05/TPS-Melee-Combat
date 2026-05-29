using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.Timeline;

public class PlayerCombatSystem : MonoBehaviour
{
    [Section("Combos")]
    public List<WeaponDataSO> weaponCombos;

    [Section("Settings")]
    public float comboResetTime = 1f; 
    public float minAttackWindow; 
    private Animator m_Animator;
    private int comboCounter;
    private float lastClickTime;    
    [HideInInspector] public bool attackFinished;
    private AnimatorOverrideController overrideController;
    public float animationSpeed;
    private AnimationEventPlayer eventPlayer;
    public AnimationEventSO animationEventSO;

    [Section("Enemy Detection")]
    public float detectZone = 1f;
    public float attackTurnSpeed;
    //public float lungeDistance;
    private PlayerCameraController playerCameraController;
    private Transform currentTarget;
    private bool isAttacking;
    private CharacterController controller;

    public TriggerCollisionEvent triggerCollisionEvent;

    private void OnEnable()
    {
        triggerCollisionEvent.OnHit += DamageEnemy;
    }

    private void OnDisable()
    {
        triggerCollisionEvent.OnHit -= DamageEnemy;

    }

    private void Start()
    {
        m_Animator = GetComponent<Animator>();
        overrideController = new AnimatorOverrideController( m_Animator.runtimeAnimatorController );
        m_Animator.runtimeAnimatorController = overrideController;
        controller = GetComponent<CharacterController>();
        eventPlayer = GetComponent<AnimationEventPlayer>();
        playerCameraController = GetComponent<PlayerCameraController>();

        eventPlayer.Play(animationEventSO);
    }

    private void Update()
    {
        ResetComboIfIdle();
        AnimatorStateInfo state = m_Animator.GetCurrentAnimatorStateInfo(0);
        eventPlayer.Tick(state.normalizedTime % 1);
    }

    public void Attack()
    {
        AnimatorStateInfo state = m_Animator.GetCurrentAnimatorStateInfo(0);
        currentTarget = FindClosestEnemy();
        isAttacking = true;
        //if (currentTarget != null)
        //{
        //    if (state.IsTag("LightAttack"))
        //    {
        //        if (state.normalizedTime > 0.1f &&
        //            state.normalizedTime < 0.35f)
        //        {
        //            Debug.Log("Lunge Attack done");
        //            controller.Move(
        //                transform.forward *
        //                lungeDistance *
        //                Time.deltaTime
        //            );
        //        }
        //    }

        //}





        if (state.IsTag("LightAttack") && state.normalizedTime < minAttackWindow)
        {
            return;
        }
       


        if (comboCounter >= weaponCombos.Count)
        {
            comboCounter = 0;
        }

        lastClickTime = Time.time;
        overrideController["DummyClip"] = weaponCombos[comboCounter].attackAnimation;
        m_Animator.SetFloat("AttackSpeed",1.2f);
        m_Animator.Play("LightAttack", 0, 0f);
        comboCounter++;
    }

    private void ResetComboIfIdle()
    {
        if (Time.time - lastClickTime > comboResetTime)
        {
            comboCounter = 0;
            attackFinished = true;
            lastClickTime = 0;
            isAttacking = false;
            currentTarget = null;
            m_Animator.applyRootMotion = true;
        }
    }

    private Transform FindClosestEnemy()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectZone);

        Transform closestEnemy = null;
        float distance = Mathf.Infinity;

        foreach (Collider collider in colliders)
        {
            if (!collider.CompareTag("Enemy"))
                continue;

            float dist = Vector3.Distance(transform.position, collider.transform.position);

            if (dist < distance)
            {
                distance = dist;
                closestEnemy = collider.transform;
            }
        }


        return closestEnemy;
    }

    public void HandlePlayerRotation()
    {
        if (isAttacking && currentTarget != null)
        {
            Vector3 direction = currentTarget.position - transform.position;

            direction.y = 0f;

            if (direction != Vector3.zero)
            {
                m_Animator.applyRootMotion = false;
                Quaternion targetRot = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, attackTurnSpeed * Time.deltaTime);
            }
        }
        else if (currentTarget == null)
        {
            m_Animator.applyRootMotion = true;

        }


    }


    public void DamageEnemy(Collider collider)
    {
        if (collider == this.GetComponent<Collider>() || currentTarget == null) return;

        Debug.Log(collider.name);
        currentTarget.GetComponent<EnemyCombatSystem>().EnemyGetHit();
    }

    private void OnDrawGizmosSelected()
    {
       
      Gizmos.color = Color.blueViolet;
      Gizmos.DrawWireSphere(transform.position, detectZone);
        
    }




}
