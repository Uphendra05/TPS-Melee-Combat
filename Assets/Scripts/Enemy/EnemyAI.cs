using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{

    NavMeshAgent agent;
    public Vector3 Position => transform.position;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false; // Disable automatic rotation
    }


   // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        
        
    }

    public void MoveTo(Vector3 point)
    {
        agent.SetDestination(point);
    }

    public bool HasArrived()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
           
            Debug.Log("Arrived at destination");
            return true;
        }

        return false;
    }

    public void StopMoving()
    {
        if(agent.hasPath)
            agent.ResetPath();
    }

    public void LookAtPlayer(Vector3 point)
    {
        Vector3 direction = point - transform.position;
        direction.y = 0; // Keep the y-axis rotation unchanged
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    public void Move(Vector3 direction, float speed)
    {
        agent.Move(direction * speed * Time.deltaTime);
    }

   
}
