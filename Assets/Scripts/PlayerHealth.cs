using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private float invincibilityDuration = 1f;

    [Header("Hit Flash")]
    [SerializeField] private Color hitFlashColor = new Color(1f, 0.2f, 0.2f, 1f);
    [SerializeField] private float hitFlashDuration = 0.15f;
    [SerializeField] private int   invincibilityBlinks = 4;

    [Header("Passive Healing")]
    [Tooltip("Seconds after last hit before healing begins.")]
    [SerializeField] private float healDelay    = 3f;
    [Tooltip("Seconds between each HP tick.")]
    [SerializeField] private float healInterval = 2f;
    [Tooltip("Colour the sprite briefly flashes when a heal tick fires.")]
    [SerializeField] private Color healFlashColor = new Color(0.2f, 1f, 0.3f, 1f);

    [Header("Death")]
    [Tooltip("Seconds the death flash plays before the respawn screen appears.")]
    [SerializeField] private float respawnDelay = 1f;

    public UnityEvent<int, int> onHealthChanged;   // (current, max) — wire to HealthBar
    public UnityEvent           onDeath;           // fires immediately — use for sound/effects
    public UnityEvent           onReadyToRespawn;  // fires after respawnDelay — wire to DeathScreen.Show
    public UnityEvent           onHealed;          // fires on each passive heal tick — use for sound

    // Enemies subscribe to this to reset themselves when the player respawns.
    public static event System.Action onPlayerRespawn;

    private int   currentHealth;
    private float invincibilityTimer;
    private bool  isDead;

    private float healDelayTimer;
    private float healIntervalTimer;

    private SpriteRenderer[] renderers;
    private Color[]          originalColors;
    private Coroutine        flashCoroutine;
    private Coroutine        healFlashCoroutine;

    // Cached so we can disable input/movement on death and restore on respawn.
    private PlayerController playerController;
    private Rigidbody2D      rb;

    // ── Lifecycle ──────────────────────────────────────────────────────
    private void Awake()
    {
        currentHealth      = maxHealth;
        playerController   = GetComponent<PlayerController>();
        rb                 = GetComponent<Rigidbody2D>();
        renderers          = GetComponentsInChildren<SpriteRenderer>();
        originalColors     = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            originalColors[i] = renderers[i].color;
    }

    private void Update()
    {
        if (invincibilityTimer > 0f)
            invincibilityTimer -= Time.deltaTime;

        HandlePassiveHeal();
    }

    // ── Public API ─────────────────────────────────────────────────────
    public void TakeDamage(int amount)
    {
        if (isDead || invincibilityTimer > 0f) return;

        healDelayTimer     = healDelay;   // reset heal delay on every hit
        currentHealth      = Mathf.Max(0, currentHealth - amount);
        invincibilityTimer = invincibilityDuration;
        onHealthChanged?.Invoke(currentHealth, maxHealth);

        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(
            currentHealth <= 0 ? DeathFlashRoutine() : HitFlashRoutine());

        if (currentHealth <= 0)
        {
            AudioManager.Instance?.PlayPlayerDeath();
            HandleDeath();
        }
        else
        {
            AudioManager.Instance?.PlayPlayerHurt();
        }
    }

    public void Heal(int amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void Kill()
    {
        if (isDead) return;
        currentHealth      = 0;
        invincibilityTimer = 0f;
        onHealthChanged?.Invoke(currentHealth, maxHealth);
        HandleDeath();
    }

    public void Respawn(Vector3 spawnPosition)
    {
        isDead             = false;
        currentHealth      = maxHealth;
        invincibilityTimer = invincibilityDuration;
        healDelayTimer     = 0f;
        healIntervalTimer  = 0f;

        transform.position = spawnPosition;
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // Stop any in-flight flash coroutines so they don't overwrite restored colors.
        if (flashCoroutine     != null) StopCoroutine(flashCoroutine);
        if (healFlashCoroutine != null) StopCoroutine(healFlashCoroutine);
        RestoreRenderersColor();

        if (playerController != null) playerController.enabled = true;

        onHealthChanged?.Invoke(currentHealth, maxHealth);
        onPlayerRespawn?.Invoke();
    }

    public int  CurrentHealth => currentHealth;
    public int  MaxHealth     => maxHealth;
    public bool IsDead        => isDead;

    // ── Internal ───────────────────────────────────────────────────────
    private void HandleDeath()
    {
        isDead = true;

        // Stop the player from moving or firing during the death screen.
        if (playerController != null) playerController.enabled = false;
        if (rb != null) rb.linearVelocity = Vector2.zero;

        onDeath?.Invoke();
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);
        onReadyToRespawn?.Invoke();
    }

    private void HandlePassiveHeal()
    {
        if (isDead || currentHealth >= maxHealth) return;

        if (healDelayTimer > 0f)
        {
            healDelayTimer -= Time.deltaTime;
            return;
        }

        healIntervalTimer -= Time.deltaTime;
        if (healIntervalTimer <= 0f)
        {
            currentHealth = Mathf.Min(maxHealth, currentHealth + 1);
            onHealthChanged?.Invoke(currentHealth, maxHealth);
            onHealed?.Invoke();
            healIntervalTimer = healInterval;

            if (healFlashCoroutine != null) StopCoroutine(healFlashCoroutine);
            healFlashCoroutine = StartCoroutine(HealFlashRoutine());
        }
    }

    // ── Flash routines ─────────────────────────────────────────────────
    private IEnumerator HitFlashRoutine()
    {
        SetRenderersColor(hitFlashColor);
        yield return new WaitForSeconds(hitFlashDuration);
        RestoreRenderersColor();

        float blinkInterval = (invincibilityDuration - hitFlashDuration) / (invincibilityBlinks * 2f);
        for (int i = 0; i < invincibilityBlinks; i++)
        {
            SetRenderersColor(new Color(1f, 1f, 1f, 0.2f));
            yield return new WaitForSeconds(blinkInterval);
            RestoreRenderersColor();
            yield return new WaitForSeconds(blinkInterval);
        }
    }

    private IEnumerator DeathFlashRoutine()
    {
        SetRenderersColor(hitFlashColor);
        yield return new WaitForSeconds(respawnDelay);
        RestoreRenderersColor();
    }

    private IEnumerator HealFlashRoutine()
    {
        SetRenderersColor(healFlashColor);
        yield return new WaitForSeconds(0.15f);
        RestoreRenderersColor();
    }

    private void SetRenderersColor(Color c)
    {
        foreach (var sr in renderers)
            if (sr != null) sr.color = c;
    }

    private void RestoreRenderersColor()
    {
        for (int i = 0; i < renderers.Length; i++)
            if (renderers[i] != null) renderers[i].color = originalColors[i];
    }
}
