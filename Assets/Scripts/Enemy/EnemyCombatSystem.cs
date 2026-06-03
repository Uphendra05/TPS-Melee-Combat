using UnityEngine;

public class EnemyCombatSystem : MonoBehaviour
{
    private Animator m_Animator;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_Animator = GetComponent<Animator>();  
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    public void EnemyGetHit()
    {
        m_Animator.SetTrigger("OnHit");
        Debug.Log("Enemy Got Hit");

    }

}
