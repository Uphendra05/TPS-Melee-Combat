using UnityEngine;

public class PlayerCombatSystem : MonoBehaviour
{
    private Animator m_Animator;

    void Start()
    {
        m_Animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void SwordAttack()
    {
        m_Animator.SetTrigger("Attack");
    }
}
