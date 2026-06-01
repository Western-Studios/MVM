using UnityEngine;

/// <summary>
/// Attach to any trigger collider to damage or instantly kill the player.
/// Works for spikes, lava pits, hazard floors, or anything else that hurts on contact.
///
/// Uses GetComponentInParent so it finds PlayerHealth whether it lives on the same
/// GameObject as the player's collider or on any parent in the hierarchy (e.g. SPUM rigs
/// where the Collider2D is on a child but PlayerHealth is on the root).
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
        // GetComponentInParent walks up the hierarchy, so it finds PlayerHealth
        // even when the collider is on a child object (common with SPUM rigs).
        var health = other.GetComponentInParent<PlayerHealth>();
        if (health == null) return;

        if (instantKill)
        {
            health.Kill();
            return;
        }

        // First contact deals damage immediately; interval timer handles repeated ticks
        health.TakeDamage(damagePerHit);
        damageTimer = damageInterval;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (instantKill) return;

        var health = other.GetComponentInParent<PlayerHealth>();
        if (health == null) return;

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
