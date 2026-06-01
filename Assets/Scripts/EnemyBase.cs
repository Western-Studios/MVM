using System.Collections;
using UnityEngine;

/// <summary>
/// Shared foundation for all enemies.
/// Handles health, contact damage, hit flash, and death.
/// Subclasses override Update() via the protected virtual pattern and call base.Update().
/// </summary>
public abstract class EnemyBase : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] protected int maxHealth = 3;
    [SerializeField] protected int contactDamage = 1;

    [Header("Hit Feedback")]
    [Tooltip("Colour the sprite flashes to when hit.")]
    [SerializeField] private Color hitFlashColor = new Color(1f, 0.2f, 0.2f, 1f);
    [Tooltip("How long the flash lasts in seconds.")]
    [SerializeField] private float hitFlashDuration = 0.12f;
    [Tooltip("Minimum seconds between damage hits. Prevents a single overlapping " +
             "collider from ticking damage every frame.")]
    [SerializeField] private float damageCooldown = 0.15f;

    protected int currentHealth;

    // All renderers cached so multi-part sprite rigs (e.g. SPUM) flash entirely.
    private SpriteRenderer[] renderers;
    private Color[] originalColors;
    private float hitCooldownTimer;
    private Coroutine flashCoroutine;

    private Room homeRoom;

    // True when this enemy's room is the currently active room.
    // Subclasses use this to pause AI and movement when the player isn't in this room.
    protected bool IsInActiveRoom => homeRoom == null || homeRoom == Room.Active;

    // Runs once at game start — sets global physics rules that apply to all enemies.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ConfigureEnemyPhysics()
    {
        int enemyLayer     = LayerMask.NameToLayer("Enemy");
        int enemyProjLayer = LayerMask.NameToLayer("EnemyProjectile");
        int playerLayer    = LayerMask.NameToLayer("Player");

        // Enemies pass through each other instead of stacking/blocking.
        Physics2D.IgnoreLayerCollision(enemyLayer, enemyLayer, true);

        // Enemy projectiles only collide with the Player — ignore every other layer.
        if (enemyProjLayer >= 0)
        {
            for (int i = 0; i < 32; i++)
                Physics2D.IgnoreLayerCollision(enemyProjLayer, i, true);

            // Re-enable the one interaction we actually want.
            Physics2D.IgnoreLayerCollision(enemyProjLayer, playerLayer, false);
        }
    }

    protected virtual void Awake()
    {
        currentHealth  = maxHealth;
        renderers      = GetComponentsInChildren<SpriteRenderer>();
        originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            originalColors[i] = renderers[i].color;
    }

    // Start runs after all Awake calls, so Room colliders are guaranteed to be
    // set up as triggers before we query them here.
    protected virtual void Start()
    {
        homeRoom = FindHomeRoom();
        homeRoom?.RegisterEnemy(this);
    }

    private Room FindHomeRoom()
    {
        // OverlapCircle with a generous radius handles enemies whose pivot sits at
        // floor level — below the bottom edge of the Room trigger collider.
        int roomMask = LayerMask.GetMask("Room");
        var col = Physics2D.OverlapCircle(transform.position, 2f, roomMask);
        if (col != null) return col.GetComponent<Room>();

        // Fallback: rooms are horizontally non-overlapping, so find the one whose
        // X bounds contain this enemy (robust even if room colliders are sized tightly).
        var allRooms = FindObjectsByType<Room>(FindObjectsInactive.Exclude);
        foreach (var room in allRooms)
        {
            Bounds b = room.Bounds;
            if (transform.position.x >= b.min.x && transform.position.x <= b.max.x)
                return room;
        }

#if UNITY_EDITOR
        Debug.LogWarning($"[EnemyBase] {name} could not find a home Room — it will always be active.", this);
#endif
        return null;
    }

    // Virtual so subclasses can override and add their own logic.
    // Any subclass that overrides Update MUST call base.Update() to keep
    // the damage cooldown timer ticking.
    protected virtual void Update()
    {
        hitCooldownTimer -= Time.deltaTime;
    }

    public virtual void TakeDamage(int amount)
    {
        // Brief cooldown prevents multi-hit from overlapping/sustained colliders.
        if (hitCooldownTimer > 0f) return;
        hitCooldownTimer = damageCooldown;

        currentHealth = Mathf.Max(0, currentHealth - amount);

        // Restart flash so rapid hits still show each one cleanly.
        if (renderers.Length > 0)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(HitFlashRoutine());
        }

        if (currentHealth <= 0)
            Die();
    }

    protected virtual void Die()
    {
        homeRoom?.NotifyEnemyDied();
        Destroy(gameObject);
    }

    // OnCollisionEnter2D fires when two solid (non-trigger) colliders make
    // physical contact — correct for a slime that hurts by walking into you.
    // OnTriggerEnter2D would never fire here because neither the enemy nor the
    // player's physics collider is marked Is Trigger.
    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        collision.collider.GetComponentInParent<PlayerHealth>()?.TakeDamage(contactDamage);
    }

    private IEnumerator HitFlashRoutine()
    {
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].color = hitFlashColor;

        yield return new WaitForSeconds(hitFlashDuration);

        for (int i = 0; i < renderers.Length; i++)
            if (renderers[i] != null) renderers[i].color = originalColors[i];
    }
}
