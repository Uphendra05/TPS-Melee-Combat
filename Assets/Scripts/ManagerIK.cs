using UnityEngine;

public class ManagerIK : MonoBehaviour
{

    Animator animator;
    public bool ikActive = false;
    public Transform target; 





    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void OnAnimatorIK(int layerIndex)
    {
        if(animator)
        {
            if (ikActive)
            {
                if (target != null)
                {
                    animator.SetLookAtWeight(1);
                    animator.SetLookAtPosition(target.position);



                }
            }
            else
            {
                animator.SetLookAtWeight(0);

            }
        }

    }
}
