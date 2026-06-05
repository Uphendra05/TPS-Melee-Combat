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
    public int comboCounter;
    private float lastClickTime;    
    [HideInInspector] public bool attackFinished;
    private AnimatorOverrideController overrideController;
    public float animationSpeed;
    public AnimationEventSO animationEventSO;

    [Section("Enemy Detection")]
    public float detectZone = 1f;
    public float attackTurnSpeed;
    public float lungeDistance;
    private PlayerCameraController playerCameraController;
    public Transform currentTarget;
    public bool isAttacking;
    private CharacterController controller;

    public TriggerCollisionEvent triggerCollisionEvent;
    private WeaponManager weaponManager;
    private AnimationEventPlayer _eventPlayer;
   

    private void Start()
    {
        m_Animator = GetComponent<Animator>();
        overrideController = new AnimatorOverrideController(m_Animator.runtimeAnimatorController);
        m_Animator.runtimeAnimatorController = overrideController;
        controller = GetComponent<CharacterController>();
        playerCameraController = GetComponent<PlayerCameraController>();
        weaponManager = GetComponent<WeaponManager>();
      

    }

    private void Update()
    {

        ResetComboIfIdle();
       
    }


    public void Init(AnimationEventPlayer eventPlayer)
    {
        _eventPlayer = eventPlayer;
    }

    public void Attack()
    {
        AnimatorStateInfo state = m_Animator.GetCurrentAnimatorStateInfo(0);
        currentTarget = FindClosestEnemy();
        isAttacking = true;

        if (currentTarget != null)
        {
            if (state.IsTag("LightAttack"))
            {
                if (state.normalizedTime > 0.1f && state.normalizedTime < 0.35f)
                {
                    Debug.Log("Lunge Attack done");
                    controller.Move(transform.forward * lungeDistance * Time.deltaTime);
                }
            }
        }

        if (state.IsTag("LightAttack") && state.normalizedTime < minAttackWindow)
            return;

        lastClickTime = Time.time;
        overrideController["DummyClip"] = weaponCombos[comboCounter].clip;
        m_Animator.SetFloat("AttackSpeed", 1.2f);
        m_Animator.Play("LightAttack", 0, 0f);
        weaponManager.EquipByType(weaponCombos[comboCounter].weaponType);
        _eventPlayer.Play(weaponCombos[comboCounter]);

        comboCounter++;

        if (comboCounter >= weaponCombos.Count)
            comboCounter = 0;
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
            weaponManager.UnequipCurrent();
            _eventPlayer.Stop(); 

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
                Quaternion targetRot = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, attackTurnSpeed * Time.deltaTime);
            }
        }
       
    }



    public void DamageEnemy(Collider collider)
    {
        if (collider == this.GetComponent<Collider>() || currentTarget == null) return;

        Debug.Log(collider.name);
        currentTarget.GetComponent<EnemyCombatSystem>().EnemyGetHit();
        playerCameraController.ShakeCamera(2.5f, 0.1f);

    }

    private void OnDrawGizmosSelected()
    {
       
      Gizmos.color = Color.blueViolet;
      Gizmos.DrawWireSphere(transform.position, detectZone);
        
    }




}
