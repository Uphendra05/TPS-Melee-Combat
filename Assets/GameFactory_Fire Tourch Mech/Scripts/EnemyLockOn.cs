using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyLockOn : MonoBehaviour
{
    

    

    [Section("Camera")]
    [SerializeField] Animator cinemachineAnimator;
    

    [Section("Settings")]
    [SerializeField] bool zeroVertLook;
    [SerializeField] float noticeZone = 10f;
    //[SerializeField] float lookAtSmoothing = 5f;
    [SerializeField] float maxNoticeAngle = 60f;
    [SerializeField] float crossHairScale = 0.1f;
    [SerializeField] Transform lockOnCanvas;
    [SerializeField] LayerMask targetLayers;
    [SerializeField] Transform enemyTargetLocator;

    private Transform cam;
    private bool enemyLocked;
    private float currentYOffset;
    private PlayerCameraController playerCameraController;
    private Transform currentTarget;

    void Start()
    {
        playerCameraController = GetComponent<PlayerCameraController>(); 
        cam = Camera.main.transform;
        lockOnCanvas.gameObject.SetActive(false);
    }

    void Update()
    {
        playerCameraController.camLockedToTarget = enemyLocked;

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (enemyLocked)
            {
                ResetTarget();
            }
            else
            {
                currentTarget = ScanNearBy();

                if (currentTarget != null)
                    FoundTarget();
            }
        }

        if (!enemyLocked)
            return;

        if (currentTarget == null)
        {
            ResetTarget();
            return;
        }

        if (!TargetOnRange())
        {
            ResetTarget();
            return;
        }

        LookAtTarget();
    }

    void FoundTarget()
    {
        enemyLocked = true;
        lockOnCanvas.gameObject.SetActive(true);
        cinemachineAnimator.Play("TargetCam");
    }

    void ResetTarget()
    {
        currentTarget = null;
        enemyLocked = false;
        lockOnCanvas.gameObject.SetActive(false);
        cinemachineAnimator.Play("FollowCam");


    }

    Transform ScanNearBy()
    {
        Collider[] nearbyTargets = Physics.OverlapSphere( transform.position, noticeZone, targetLayers);
        Transform closestTarget = null;
        float closestAngle = maxNoticeAngle;

        foreach (Collider target in nearbyTargets)
        {
            Vector3 dir =  target.transform.position - cam.position;

            dir.y = 0;

            float angle = Vector3.Angle(cam.forward, dir);

            if (angle < closestAngle)
            {
                Vector3 targetPos = target.transform.position + Vector3.up * 1.5f;

                if (Blocked(targetPos))
                    continue;

                closestAngle = angle;
                closestTarget = target.transform;
            }
        }

        if (closestTarget == null)
            return null;

        CapsuleCollider capsule = closestTarget.GetComponent<CapsuleCollider>();

        if (capsule != null)
        {
            float height = capsule.height * closestTarget.localScale.y;

            currentYOffset = height * 0.75f;
        }
        else
        {
            currentYOffset = 1.5f;
        }

        if (zeroVertLook && currentYOffset > 1.6f && currentYOffset < 4.8f)
        {
            currentYOffset = 1.6f;
        }

        return closestTarget;
    }

    bool Blocked(Vector3 targetPos)
    {
        Vector3 origin = transform.position + Vector3.up * 1.5f;

        if (Physics.Linecast(origin, targetPos, out RaycastHit hit))
        {
            if (!hit.transform.root.CompareTag("Enemy"))
                return true;
        }

        return false;
    }

    bool TargetOnRange()
    {
        if (currentTarget == null)
            return false;

        float distance = Vector3.Distance(transform.position,currentTarget.position);
        return distance <= noticeZone;
    }

    void LookAtTarget()
    {
        Vector3 targetPos = currentTarget.position + Vector3.up * currentYOffset;
        lockOnCanvas.position = targetPos;
        float scale = Vector3.Distance(cam.position, targetPos)  * crossHairScale;
        lockOnCanvas.localScale = Vector3.one * scale;
        enemyTargetLocator.position = targetPos;
        Vector3 dir = currentTarget.position - transform.position;
        dir.y = 0;

        if (dir.sqrMagnitude > 0.001f)
        {
           //Quaternion rot = Quaternion.LookRotation(dir);
           //transform.rotation = Quaternion.Slerp(transform.rotation,rot,Time.deltaTime * lookAtSmoothing);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position,noticeZone);
    }
}
