using UnityEngine;

/// <summary>
/// Stationary enemy that detects the player within a range and fires projectiles at them.
/// Flips to face the player while in range.
/// </summary>
public class TurretEnemy : EnemyBase
{
    [Header("Turret")]
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float fireRate = 2f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [Tooltip("Layers that block line-of-sight (typically your Ground / Solid layer). " +
             "Triggers are always ignored regardless of this mask.")]
    [SerializeField] private LayerMask sightBlockLayer;

    private Transform player;
    private SpriteRenderer sr;
    private ContactFilter2D sightFilter;
    private float fireTimer;

    protected override void Awake()
    {
        base.Awake();
        player    = GameObject.FindWithTag("Player")?.transform;
        sr        = GetComponentInChildren<SpriteRenderer>();
        fireTimer = fireRate;

        sightFilter = new ContactFilter2D();
        sightFilter.useLayerMask = true;
        sightFilter.SetLayerMask(sightBlockLayer);
        sightFilter.useTriggers = false;
    }

    protected override void Update()
    {
        base.Update(); // ticks EnemyBase damage cooldown timer

        // Lazy-find: retries if Awake ran before the player existed in the scene.
        if (player == null)
            player = GameObject.FindWithTag("Player")?.transform;
        if (player == null) return;
        if (!IsInActiveRoom) return;
        if (Vector2.Distance(transform.position, player.position) > detectionRange) return;

        // Face the player
        if (sr != null)
            sr.flipX = player.position.x < transform.position.x;

        // Timer always counts down so the turret fires on a predictable interval
        // the moment it regains sight — not immediately the instant LoS clears.
        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            if (HasLineOfSight())
                Fire();

            fireTimer = fireRate;
        }
    }

    private bool HasLineOfSight()
    {
        Vector2 origin  = firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position;
        var     results = new RaycastHit2D[1];
        return Physics2D.Linecast(origin, player.position, sightFilter, results) == 0;
    }

    private void Fire()
    {
        if (projectilePrefab == null || firePoint == null) return;

        Vector2 direction = (player.position - firePoint.position).normalized;
        var go = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        // Ignore collision between the projectile and every collider on this turret
        // so the projectile can't trigger against its own spawner.
        var projCol = go.GetComponent<Collider2D>() ?? go.GetComponentInChildren<Collider2D>();
        if (projCol != null)
        {
            foreach (var col in GetComponentsInChildren<Collider2D>())
                Physics2D.IgnoreCollision(projCol, col);
        }

        go.GetComponent<Projectile>()?.Initialize(direction);
    }

    private void OnDrawGizmosSelected()
    {
        if (player == null) return;

        // Red sphere = detection range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Green line = current LoS ray (only visible when selected in editor)
        Vector2 origin = firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position;
        Gizmos.color = HasLineOfSight() ? Color.green : Color.grey;
        Gizmos.DrawLine(origin, player.position);
    }
}
