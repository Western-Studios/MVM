using UnityEngine;

/// <summary>
/// Attach to any trigger collider to make it damage or instantly kill the player.
/// Works for spikes, lava pits, hazard floors, or anything else that hurts on contact.
/// </summary>
public class DamageZone : MonoBehaviour
{
    [Tooltip("Tick ON for instant-kill zones (pits, lava). " +
             "Tick OFF for spikes that deal damage over time.")]
    [SerializeField] private bool instantKill = false;

    [Tooltip("Damage per hit. Ignored when Instant Kill is ON.")]
    [SerializeField] private int damagePerHit = 1;

    [Tooltip("Seconds between damage ticks while the player stays inside.")]
    [SerializeField] private float damageInterval = 0.6f;

    private float damageTimer;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent<PlayerHealth>(out var health)) return;

        if (instantKill)
        {
            health.Kill();
            return;
        }

        // First contact deals damage immediately, then the interval timer takes over
        health.TakeDamage(damagePerHit);
        damageTimer = damageInterval;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (instantKill) return;
        if (!other.TryGetComponent<PlayerHealth>(out var health)) return;

        damageTimer -= Time.deltaTime;
        if (damageTimer <= 0f)
        {
            health.TakeDamage(damagePerHit);
            damageTimer = damageInterval;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Reset timer so next entry always deals damage immediately
        damageTimer = 0f;
    }
}
