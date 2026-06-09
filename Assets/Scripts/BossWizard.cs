using System.Collections;
using UnityEngine;

public class BossWizard : MonoBehaviour
{
    [Header("Positions & Traps")]
    [Tooltip("Boss teleports through these in order. Index matches Traps array.")]
    [SerializeField] private Transform[] teleportPositions;
    [Tooltip("One HazardTrap per position — same index.")]
    [SerializeField] private HazardTrap[] traps;

    [Header("Combat")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform  firePoint;
    [SerializeField] private float attackInterval       = 2f;
    [SerializeField] private float phase2AttackInterval = 1f;
    [SerializeField] private int   phase2BurstCount     = 3;
    [SerializeField] private float burstSpreadAngle     = 25f;

    [Header("Health")]
    [SerializeField] private int maxHealth = 6;

    [Header("Timing")]
    [SerializeField] private float appearDelay      = 0.8f;
    [SerializeField] private float teleportOutDelay = 0.5f;

    [Header("VFX")]
    [SerializeField] private GameObject teleportVFXPrefab;
    [Tooltip("Tint applied to the boss sprite when phase 2 begins.")]
    [SerializeField] private Color phase2Tint = new Color(1f, 0.35f, 0.1f, 1f);

    // Subscribe to these for HUD updates.
    public event System.Action<int, int> onHealthChanged;
    public event System.Action           onDeath;

    public int MaxHealth     => maxHealth;
    public int CurrentHealth => currentHealth;

    private int       currentHealth;
    private int       currentPositionIndex;
    private int       hitsThisCycle;
    private bool      isPhase2;
    private bool      isDead;
    private Transform player;
    private SpriteRenderer[] renderers;
    private Animator  animator;

    private enum BossState { Appearing, Attacking, TeleportingOut, Dead }
    private BossState state;
    private Coroutine attackCoroutine;

    private void Awake()
    {
        currentHealth = maxHealth;
        renderers     = GetComponentsInChildren<SpriteRenderer>();
        animator      = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        player = FindAnyObjectByType<PlayerController>()?.transform;
        StartCoroutine(BossRoutine());
    }

    // Called by HazardTrap.OnTriggerEnter2D when a trap lands on the boss.
    public void TakeDamage(int amount)
    {
        if (isDead || state != BossState.Attacking) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        onHealthChanged?.Invoke(currentHealth, maxHealth);
        animator?.SetTrigger("Hit");
        AudioManager.Instance?.PlayBossHurt();
        hitsThisCycle++;

        if (currentHealth <= 0)
        {
            StartCoroutine(DieRoutine());
            return;
        }

        if (!isPhase2 && currentHealth <= maxHealth / 2)
            EnterPhase2();

        // Full cycle complete — reset all traps so the player can use them again.
        if (hitsThisCycle >= teleportPositions.Length)
        {
            hitsThisCycle = 0;
            foreach (var trap in traps)
                trap?.ResetTrap();
        }

        state = BossState.TeleportingOut;
        animator?.SetTrigger("TeleportOut");
    }

    private IEnumerator BossRoutine()
    {
        if (teleportPositions == null || teleportPositions.Length == 0) yield break;

        TeleportTo(currentPositionIndex);

        while (!isDead)
        {
            state = BossState.Appearing;
            yield return new WaitForSeconds(appearDelay);

            state           = BossState.Attacking;
            attackCoroutine = StartCoroutine(AttackRoutine());

            // Wait here until TakeDamage() changes state or the boss dies.
            yield return new WaitUntil(() => state != BossState.Attacking || isDead);

            if (attackCoroutine != null) StopCoroutine(attackCoroutine);
            if (isDead) break;

            yield return new WaitForSeconds(teleportOutDelay);

            currentPositionIndex = (currentPositionIndex + 1) % teleportPositions.Length;
            TeleportTo(currentPositionIndex);
        }
    }

    private IEnumerator AttackRoutine()
    {
        while (state == BossState.Attacking)
        {
            yield return new WaitForSeconds(isPhase2 ? phase2AttackInterval : attackInterval);

            if (state != BossState.Attacking) break;

            if (isPhase2) FireBurst();
            else          FireSingle();
        }
    }

    private void FireSingle()
    {
        if (projectilePrefab == null || player == null || firePoint == null) return;
        animator?.SetTrigger("Attack1");
        AudioManager.Instance?.PlayBossAttack();
        Vector2 dir = ((Vector2)player.position - (Vector2)firePoint.position).normalized;
        SpawnProjectile(dir);
    }

    private void FireBurst()
    {
        if (projectilePrefab == null || player == null || firePoint == null) return;
        animator?.SetTrigger("Attack2");
        AudioManager.Instance?.PlayBossAttack();

        Vector2 baseDir   = ((Vector2)player.position - (Vector2)firePoint.position).normalized;
        float   baseAngle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;
        float   start     = baseAngle - burstSpreadAngle;
        float   step      = phase2BurstCount > 1
                            ? (burstSpreadAngle * 2f) / (phase2BurstCount - 1)
                            : 0f;

        for (int i = 0; i < phase2BurstCount; i++)
        {
            float rad = (start + step * i) * Mathf.Deg2Rad;
            SpawnProjectile(new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)));
        }
    }

    private void SpawnProjectile(Vector2 direction)
    {
        var go   = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        var proj = go.GetComponent<Projectile>() ?? go.GetComponentInChildren<Projectile>();
        proj?.Initialize(direction);
    }

    private void TeleportTo(int index)
    {
        if (teleportVFXPrefab != null)
            Instantiate(teleportVFXPrefab, transform.position, Quaternion.identity);

        transform.position = teleportPositions[index].position;
        animator?.SetTrigger("TeleportIn");

        if (teleportVFXPrefab != null)
            Instantiate(teleportVFXPrefab, transform.position, Quaternion.identity);
    }

    private void EnterPhase2()
    {
        isPhase2 = true;
        AudioManager.Instance?.PlayBossPhase2();
        foreach (var sr in renderers)
            if (sr != null) sr.color = phase2Tint;
    }

    private IEnumerator DieRoutine()
    {
        isDead = true;
        state  = BossState.Dead;
        animator?.SetTrigger("Die");
        AudioManager.Instance?.PlayBossDeath();
        onDeath?.Invoke();
        yield return new WaitForSeconds(1.5f);
        gameObject.SetActive(false);
    }
}
