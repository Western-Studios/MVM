using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class SlimeEnemy : EnemyBase
{
    // ── Inspector ──────────────────────────────────────────────────────
    [Header("Patrol")]
    [SerializeField] private float moveSpeed      = 2f;
    [SerializeField] private float patrolRange    = 3f;
    [SerializeField] private float wallCheckDistance = 0.15f;
    [SerializeField] private float edgeCheckDistance = 0.5f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Chase")]
    [SerializeField] private float detectionRange  = 4f;
    [Tooltip("Must be larger than detectionRange to prevent flickering at the boundary.")]
    [SerializeField] private float loseAggroRange  = 6f;
    [SerializeField] private float chaseSpeed      = 4f;
    [Tooltip("How quickly the slime ramps up/down to its target speed (units/sec²). Lower = sluggier.")]
    [SerializeField] private float moveAcceleration = 10f;

    [Header("Knockback")]
    [SerializeField] private float knockbackForce    = 5f;
    [Tooltip("Seconds normal movement is suppressed so the impulse can carry the body.")]
    [SerializeField] private float knockbackDuration = 0.15f;

    // ── State ──────────────────────────────────────────────────────────
    private enum SlimeState { Patrol, Chase }

    [Header("Debug (read-only)")]
    [SerializeField] private SlimeState currentState = SlimeState.Patrol;
    [SerializeField] private Transform  player;

    private Collider2D  col;
    private SpriteRenderer spriteRenderer;

    private bool  movingRight  = true;
    private float flipCooldown;
    private float spawnX;
    private float knockbackTimer;

    // Reusable ContactFilter — excludes triggers (Room bounds, pressure plates).
    private ContactFilter2D groundFilter;

    // ── Lifecycle ──────────────────────────────────────────────────────
    protected override void Awake()
    {
        base.Awake();
        rb            = GetComponent<Rigidbody2D>();
        col           = GetComponent<Collider2D>() ?? GetComponentInChildren<Collider2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        rb.freezeRotation = true;
        spawnX        = transform.position.x;

        player = GameObject.FindWithTag("Player")?.transform;

        groundFilter = new ContactFilter2D();
        groundFilter.useLayerMask = true;
        groundFilter.SetLayerMask(groundLayer);
        groundFilter.useTriggers = false;

        if (groundLayer == 0)
            Debug.LogWarning("[SlimeEnemy] Ground Layer is not assigned — patrol turn " +
                             "detection will not work correctly.", this);
    }

    protected override void Update()
    {
        base.Update(); // ticks EnemyBase damage cooldown
        if (!IsInActiveRoom)
        {
            currentState = SlimeState.Patrol; // reset aggro so it re-patrols on room re-entry
            return;
        }
        if (flipCooldown > 0f) flipCooldown -= Time.deltaTime;
        UpdateAI();
    }

    public override void TakeDamage(int amount)
    {
        int healthBefore = currentHealth;
        base.TakeDamage(amount);

        // base returns early (no health change) when the damage cooldown is active.
        if (currentHealth == healthBefore) return;

        // Enemy died — Die() has already been called, skip knockback on a dead body.
        if (currentHealth <= 0) return;

        // Aggro immediately when hit — even if the player is outside detection range.
        if (player != null)
            currentState = SlimeState.Chase;

        if (rb == null || player == null) return;

        Vector2 dir = ((Vector2)transform.position - (Vector2)player.position).normalized;
        if (dir == Vector2.zero) dir = movingRight ? Vector2.left : Vector2.right;

        rb.linearVelocity = new Vector2(dir.x * knockbackForce, rb.linearVelocity.y);
        knockbackTimer = knockbackDuration;
    }

    private void FixedUpdate()
    {
        if (!IsInActiveRoom)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            return; // let the impulse carry the body; don't override velocity
        }

        float targetSpeed = (currentState == SlimeState.Chase ? chaseSpeed : moveSpeed)
                            * (movingRight ? 1f : -1f);
        float smoothX = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed,
                                          moveAcceleration * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(smoothX, rb.linearVelocity.y);
    }

    // ── AI ─────────────────────────────────────────────────────────────
    private void UpdateAI()
    {
        // Lazy-find: retries every frame if Awake ran before the player existed.
        if (player == null)
            player = GameObject.FindWithTag("Player")?.transform;
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        switch (currentState)
        {
            case SlimeState.Patrol:
                if (dist <= detectionRange)
                    currentState = SlimeState.Chase;
                else
                    CheckPatrolTurn();
                break;

            case SlimeState.Chase:
                if (dist > loseAggroRange)
                    currentState = SlimeState.Patrol;
                else
                    ChasePlayer();
                break;
        }
    }

    // ── Patrol ─────────────────────────────────────────────────────────
    private void CheckPatrolTurn()
    {
        if (flipCooldown > 0f) return;
        if (col == null) return;

        float dir = movingRight ? 1f : -1f;
        Bounds b  = col.bounds;
        var hits  = new RaycastHit2D[1];

        Vector2 wallOrigin = new Vector2(b.center.x + dir * b.extents.x, b.center.y);
        bool wallAhead = Physics2D.Raycast(wallOrigin, Vector2.right * dir,
                                           groundFilter, hits, wallCheckDistance) > 0;

        Vector2 edgeOrigin = new Vector2(b.center.x + dir * (b.extents.x + 0.05f), b.min.y + 0.1f);
        bool groundAhead = Physics2D.Raycast(edgeOrigin, Vector2.down,
                                             groundFilter, hits, edgeCheckDistance) > 0;

        bool pastRange = ( movingRight && transform.position.x >= spawnX + patrolRange)
                      || (!movingRight && transform.position.x <= spawnX - patrolRange);

        // Scene-view debug rays
        Debug.DrawRay(wallOrigin,  Vector2.right * dir * wallCheckDistance,
                      wallAhead   ? Color.red    : Color.green);
        Debug.DrawRay(edgeOrigin,  Vector2.down  * edgeCheckDistance,
                      groundAhead ? Color.yellow : Color.red);

        if (wallAhead || !groundAhead || pastRange)
        {
            Flip();
            flipCooldown = 0.25f;
        }
    }

    // ── Chase ──────────────────────────────────────────────────────────
    private void ChasePlayer()
    {
        if (col == null) return;

        // Turn to face the player; brief cooldown prevents jitter if player
        // is at almost the same X position as the slime.
        bool shouldMoveRight = player.position.x > transform.position.x;
        if (shouldMoveRight != movingRight && flipCooldown <= 0f)
        {
            movingRight = shouldMoveRight;
            if (spriteRenderer != null) spriteRenderer.flipX = !movingRight;
            flipCooldown = 0.1f;
        }

        // Edge check — slime won't chase the player off a cliff.
        if (flipCooldown > 0f) return;
        float dir = movingRight ? 1f : -1f;
        Bounds b  = col.bounds;
        var hits  = new RaycastHit2D[1];

        Vector2 edgeOrigin = new Vector2(b.center.x + dir * (b.extents.x + 0.05f), b.min.y + 0.1f);
        bool groundAhead = Physics2D.Raycast(edgeOrigin, Vector2.down,
                                             groundFilter, hits, edgeCheckDistance) > 0;
        if (!groundAhead)
        {
            // Stop at the cliff edge rather than falling.
            Flip();
            flipCooldown = 0.5f;
        }
    }

    private void Flip()
    {
        movingRight = !movingRight;
        if (spriteRenderer != null) spriteRenderer.flipX = !movingRight;
    }

    // ── Gizmos ─────────────────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        float originX = Application.isPlaying ? spawnX : transform.position.x;
        float y       = transform.position.y;

        // Patrol zone (green)
        Gizmos.color = new Color(0f, 1f, 0.5f, 0.5f);
        Gizmos.DrawLine(new Vector3(originX - patrolRange, y - 0.5f),
                        new Vector3(originX - patrolRange, y + 0.5f));
        Gizmos.DrawLine(new Vector3(originX + patrolRange, y - 0.5f),
                        new Vector3(originX + patrolRange, y + 0.5f));
        Gizmos.DrawLine(new Vector3(originX - patrolRange, y),
                        new Vector3(originX + patrolRange, y));

        // Detection range (yellow)
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Lose-aggro range (red, faint)
        Gizmos.color = new Color(1f, 0f, 0f, 0.15f);
        Gizmos.DrawWireSphere(transform.position, loseAggroRange);
    }
}
