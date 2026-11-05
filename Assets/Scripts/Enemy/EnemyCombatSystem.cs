using UnityEngine;

public class EnemyCombatSystem : MonoBehaviour, IDamageable
{
    private Animator m_Animator;
    private float angle;
    private AudioSource audioSource;

    [Header("Random Pitch Settings")]
    [SerializeField] private float minPitch = 0.85f;
    [SerializeField] private float maxPitch = 1.15f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_Animator = GetComponent<Animator>();  
        audioSource = GetComponent<AudioSource>();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeHit(in DamageInfo hit)
    {
        EnemyGetHit(hit.attacker.position);

        if(audioSource != null)
            PlayGetHitSound();
    }



    public void EnemyGetHit(Vector3 attackerPosition)
    {

        float angle = GetHitAngle(attackerPosition);
        string animToPlay = GetHitAnimation(angle);
        m_Animator.SetTrigger(animToPlay);
        Debug.Log("Enemy Got Hit");

    }



    private float GetHitAngle(Vector3 attackerPosition)
    {
        Vector3 directionToAttacker = (attackerPosition - transform.position).normalized;
        directionToAttacker.y = 0f; 

        return Vector3.SignedAngle(transform.forward, directionToAttacker, Vector3.up);
    }


    private string GetHitAnimation(float angle)
    {
        float abs = Mathf.Abs(angle);

        if (abs < 45f)
            return "OnHitFront";
        else if (abs > 135f)
            return "OnHitBack";
        else if (angle > 0)
            return "OnHitFront";
        else
            return "OnHitBack";
    }

    public void PlayGetHitSound()
    {
        audioSource.pitch = Random.Range(minPitch, maxPitch);
        audioSource.Play();
    }
}
