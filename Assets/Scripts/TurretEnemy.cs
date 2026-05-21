using UnityEngine;

public class TurretEnemy : EnemyBase
{
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float fireRate = 2f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;

    private Transform player;
    private float fireTimer;

    protected override void Awake()
    {
        base.Awake();
        player = GameObject.FindWithTag("Player")?.transform;
    }

    private void Update()
    {
        if (player == null) return;
        if (Vector2.Distance(transform.position, player.position) > detectionRange) return;

        GetComponent<SpriteRenderer>().flipX = player.position.x < transform.position.x;

        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            Fire();
            fireTimer = fireRate;
        }
    }

    private void Fire()
    {
        Vector2 direction = (player.position - firePoint.position).normalized;
        Instantiate(projectilePrefab, firePoint.position, Quaternion.identity)
            .GetComponent<Projectile>()?.Initialize(direction);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}