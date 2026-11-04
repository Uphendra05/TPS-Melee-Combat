using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Analytics.IAnalytic;

public struct SwingData
{
    public float damage;
    public float knockbackForce;
    public float stunDuration;
    public int hitReactionType;
}

public class WeaponTracer : MonoBehaviour
{

    private struct SweepRecord
    {
        public Vector3 from, to;
        public float time;
        public int frame;
        public int socket;
    }

    private struct HitRecord
    {
        public Vector3 point;
        public float time;
    }

    public event System.Action<DamageInfo, IDamageable> OnHitLanded;

#if UNITY_EDITOR
    [Section("Debug Settings")]
    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private float trailLifetime = 0.5f;

   

    private const int MaxSweeps = 256;
    private const int MaxHitRecords = 32;
    private readonly SweepRecord[] _sweepLog = new SweepRecord[MaxSweeps];
    private readonly HitRecord[] _hitLog = new HitRecord[MaxHitRecords];
    private int _sweepHead;
    private int _hitHead;
#endif


    [Section("Tracer Settings")]
    [SerializeField] private Transform[] sockets;
    [SerializeField] private float radius = 0.08f;
    [SerializeField] private LayerMask hitMask;          // enemies only
    [SerializeField] private float minSweepDistance = 0.001f;

    private const int MaxHits = 16;
    private readonly RaycastHit[] _castHits = new RaycastHit[MaxHits];
    private readonly Collider[] _overlapHits = new Collider[MaxHits];
    private readonly HashSet<IDamageable> _alreadyHit = new();

    private Vector3[] _prev;
    private SwingData _swing;
    private Transform _attacker;

    private void Awake()
    {
        _prev = new Vector3[sockets.Length];
        enabled = false; // no LateUpdate cost while idle

       
    }

    public void Begin(in SwingData swing, Transform attacker)
    {
        _swing = swing;
        _attacker = attacker;
        _alreadyHit.Clear();

        for (int i = 0; i < sockets.Length; i++)
            _prev[i] = sockets[i].position;

        enabled = true;
    }

    public void End()
    {
        if (enabled) Sweep(); // cover the last partial frame
        enabled = false;
    }

    private void OnDisable() => _alreadyHit.Clear();

    // LateUpdate so socket bones are already posed by the Animator this frame
    private void LateUpdate() => Sweep();

    private void Sweep()
    {
        for (int i = 0; i < sockets.Length; i++)
        {
            Vector3 cur = sockets[i].position;
            Vector3 delta = cur - _prev[i];
            float dist = delta.magnitude;

            LogSweep(i, _prev[i], cur);

            if (dist < minSweepDistance)
            {
                int n = Physics.OverlapSphereNonAlloc(cur, radius, _overlapHits, hitMask, QueryTriggerInteraction.Ignore);
                for (int h = 0; h < n; h++)
                {
                    Vector3 closest = _overlapHits[h].ClosestPoint(cur);
                    Register(_overlapHits[h], closest, (cur - closest).normalized);
                }
            }
            else
            {
                int n = Physics.SphereCastNonAlloc(_prev[i], radius, delta / dist, _castHits, dist, hitMask, QueryTriggerInteraction.Ignore);
                for (int h = 0; h < n; h++)
                    Register(_castHits[h].collider, _castHits[h].point, _castHits[h].normal);
            }

            _prev[i] = cur;
        }
    }

    private void Register(Collider col, Vector3 point, Vector3 normal)
    {

        if (!col.TryGetComponent(out IDamageable target))
            target = col.GetComponentInParent<IDamageable>();

        if (target == null)
        {
            return;
        }
        if (!_alreadyHit.Add(target))
        {
            return;
        }

        LogHit(point);

        Vector3 dir = col.transform.position - _attacker.position;
        dir.y = 0f;

        var data = new DamageInfo
        {
            damage = _swing.damage,
            knockbackDirection = dir.normalized,
            knockbackForce = _swing.knockbackForce,
            stunDuration = _swing.stunDuration,
            hitReactionType = _swing.hitReactionType,
            point = point,
            normal = normal,
            attacker = _attacker
        };

        target.TakeHit(data);
        OnHitLanded?.Invoke(data, target);



    }


    #region Debug Logging

   


    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void LogSweep(int socket, Vector3 from, Vector3 to)
    {
#if UNITY_EDITOR
        _sweepLog[_sweepHead] = new SweepRecord
        {
            from = from,
            to = to,
            socket = socket,
            time = Time.time,
            frame = Time.frameCount
        };
        _sweepHead = (_sweepHead + 1) % MaxSweeps;
#endif
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void LogHit(Vector3 point)
    {
#if UNITY_EDITOR
        _hitLog[_hitHead] = new HitRecord { point = point, time = Time.time };
        _hitHead = (_hitHead + 1) % MaxHitRecords;
#endif
    }



#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!drawGizmos || sockets == null || sockets.Length == 0) return;

        // Setup view: always visible. Green = idle, orange = tracing.
        Gizmos.color = enabled && Application.isPlaying
            ? new Color(1f, 0.6f, 0.1f, 1f)
            : new Color(0.2f, 0.9f, 0.6f, 0.9f);

        for (int i = 0; i < sockets.Length; i++)
        {
            if (sockets[i] == null) continue;
            Gizmos.DrawWireSphere(sockets[i].position, radius);
            if (i > 0 && sockets[i - 1] != null)
                Gizmos.DrawLine(sockets[i - 1].position, sockets[i].position);
        }

        if (!Application.isPlaying) return;

        float now = Time.time;

        // Sweep trails: per-socket path plus the sheet between neighbours
        for (int n = 0; n < MaxSweeps; n++)
        {
            SweepRecord r = _sweepLog[n];
            float age = now - r.time;
            if (r.time <= 0f || age > trailLifetime) continue;

            float a = 1f - age / trailLifetime;
            Gizmos.color = new Color(0.2f, 0.9f, 0.6f, a);
            Gizmos.DrawLine(r.from, r.to);
            Gizmos.DrawWireSphere(r.to, radius);

            // Previous slot in the ring buffer = the lower-numbered socket, same frame
            SweepRecord p = _sweepLog[(n - 1 + MaxSweeps) % MaxSweeps];
            if (r.socket > 0 && p.frame == r.frame && p.socket == r.socket - 1)
            {
                Gizmos.color = new Color(0.2f, 0.9f, 0.6f, a * 0.5f);
                Gizmos.DrawLine(p.from, r.from);
                Gizmos.DrawLine(p.to, r.to);
            }
        }

        // Hit points
        for (int n = 0; n < MaxHitRecords; n++)
        {
            HitRecord h = _hitLog[n];
            float age = now - h.time;
            if (h.time <= 0f || age > trailLifetime * 2f) continue;

            Gizmos.color = new Color(1f, 0.25f, 0.2f, 1f - age / (trailLifetime * 2f));
            Gizmos.DrawSphere(h.point, 0.06f);
        }
    }
#endif

    #endregion
}
