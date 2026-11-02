using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Basic spawner. Two separate jobs, so waves can be added later without touching the first:
//   1) HOW to spawn one enemy  -> SpawnEnemy / InitializeEnemy
//   2) WHEN and how many       -> SpawnRoutine (replace this with wave data later)
public class EnemySpawner : MonoBehaviour
{
    [Section("References")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform player;
    [SerializeField] private Transform[] spawnPoints;

    [Section("Spawning")]
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private int count = 5;
    [SerializeField] private float interval = 1f;
    [SerializeField] private float navMeshSampleRadius = 2f;

    private readonly List<GameObject> enemies = new List<GameObject>();

    // Number of enemies still alive (destroyed ones are dropped from the list).
    public int AliveCount
    {
        get
        {
            enemies.RemoveAll(e => e == null);
            return enemies.Count;
        }
    }

    private void Start()
    {
        if (!Validate()) return;

        if (spawnOnStart)
        {
            StartCoroutine(SpawnRoutine());
        }
    }

    private bool Validate()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError($"EnemySpawner '{name}' has no enemy prefab assigned.");
            return false;
        }

        if (player == null)
        {
            Debug.LogError($"EnemySpawner '{name}' has no player assigned.");
            return false;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError($"EnemySpawner '{name}' has no spawn points.");
            return false;
        }

        return true;
    }

    // ---------- When and how many (replace with waves later) ----------

    private IEnumerator SpawnRoutine()
    {
        for (int i = 0; i < count; i++)
        {
            Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
            SpawnEnemy(point.position);
            yield return new WaitForSeconds(interval);
        }
    }

    // ---------- How to spawn one enemy ----------

    public GameObject SpawnEnemy(Vector3 point)
    {
        // The agent must start on the NavMesh, so snap the point onto it first.
        if (!NavMesh.SamplePosition(point, out NavMeshHit hit, navMeshSampleRadius, NavMesh.AllAreas))
        {
            Debug.LogWarning($"EnemySpawner '{name}': no NavMesh near {point}, enemy not spawned.");
            return null;
        }

        // Instantiate runs the enemy's Awake right away, so its BehaviorTree already has its Data.
        GameObject enemy = Instantiate(enemyPrefab, hit.position, Quaternion.identity);

        InitializeEnemy(enemy);
        enemies.Add(enemy);
        return enemy;
    }

    // Everything an enemy needs at spawn goes here (target now; routes, squads, difficulty later).
    private void InitializeEnemy(GameObject enemy)
    {
        BehaviorTree tree = enemy.GetComponent<BehaviorTree>();

        if (tree == null)
        {
            Debug.LogError($"Enemy prefab '{enemyPrefab.name}' has no BehaviorTree component.");
            return;
        }

        tree.EnsureInitialized(); // no-op if Awake already did it

        // Trees that don't declare a target (e.g. a pure patrol tree) are simply skipped.
        if (tree.Data.Has(BTKeys.Target))
        {
            tree.Data.Get<Transform>(BTKeys.Target).Value = player;
        }
    }
}
