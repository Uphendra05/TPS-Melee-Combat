using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;

public class PlayerCombatSystem : MonoBehaviour
{
    public List<WeaponSO> weaponCombos;
    float lastClickedTime;
    float lastComboEnd;
    int comboCounter;
    private Animator m_Animator;



    void Start()
    {
        m_Animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        ExitAttack();
    }


    public void Attack()
    {
        if (Time.time - lastComboEnd > 0.5f && comboCounter <= weaponCombos.Count)
        {
            CancelInvoke("EndCombo");

            if(Time.time -lastClickedTime >= 0.2f)
            {
                m_Animator.runtimeAnimatorController = weaponCombos[comboCounter].attackAnimation;
                m_Animator.Play("LightAttack");
                comboCounter++;
                lastClickedTime = Time.time;

                if (comboCounter + 1 > weaponCombos.Count)
                {
                    comboCounter = 0;
                }
            }
        }
    }


    private void ExitAttack()
    {
        if(m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.5f && m_Animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
        {
            Invoke("EndCombo", 1);
        }
    }   
    
    private void EndCombo()
    {
        comboCounter = 0;
        lastComboEnd = Time.time;
    }
}
