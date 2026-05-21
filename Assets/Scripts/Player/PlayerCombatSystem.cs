using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.Timeline;

public class PlayerCombatSystem : MonoBehaviour
{
    [Header("Combos")]
    public List<WeaponSO> weaponCombos;

    [Header("Settings")]
    public float comboResetTime = 1f; // time before combo resets
    public float minAttackWindow; // must be near end of animation

    private Animator m_Animator;
    private int comboCounter;
    private float lastClickTime;

    
    [HideInInspector] public bool attackFinished;

    private void Start()
    {
        m_Animator = GetComponent<Animator>();
    }

    private void Update()
    {
        ResetComboIfIdle();
    }

    public void Attack()
    {
        AnimatorStateInfo state = m_Animator.GetCurrentAnimatorStateInfo(0);

        if (state.IsTag("LightAttack") && state.normalizedTime < minAttackWindow)
        {
            return;
        }

        
        if (comboCounter >= weaponCombos.Count)
        {
            comboCounter = 0;
        }

        lastClickTime = Time.time;
        m_Animator.runtimeAnimatorController = weaponCombos[comboCounter].attackAnimation;
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
        }
    }
}
