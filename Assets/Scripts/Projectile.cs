using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private bool canBurnDoors;
    [SerializeField] private bool destroyOnHit = true;
    [Tooltip("When enabled, the projectile passes through walls and ground — only a valid target (player or enemy) destroys it. Use for enemy projectiles.")]
    [SerializeField] private bool passThroughEnvironment = false;
    [Tooltip("Any collider with this tag is ignored. Leave empty to disable. Use to let boss projectiles pass through a specific dividing wall.")]
    [SerializeField] private string passThroughTag = "";

    [Header("VFX")]
    [SerializeField] private GameObject impactVFXPrefab;

    public bool CanBurnDoors => canBurnDoors;

    private Rigidbody2D rb;
    private Collider2D col;

    private void Awake()
    {
        rb  = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>() ?? GetComponentInChildren<Collider2D>();

        rb.gravityScale   = 0f;
        rb.linearDamping  = 0f;
    }

    public void Initialize(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        rb.linearVelocity  = direction * speed;

        // Disable the collider for one physics frame so the projectile
        // travels away from the spawn point before collision can fire.
        StartCoroutine(EnableColliderNextFrame());

        Destroy(gameObject, lifetime);
    }

    private IEnumerator EnableColliderNextFrame()
    {
        if (col != null) col.enabled = false;
        yield return new WaitForFixedUpdate();
        if (col != null) col.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
if (other.TryGetComponent<Room>(out _)) return;
        if (other.GetComponentInParent<BossWizard>() != null) return;
        if (!string.IsNullOrEmpty(passThroughTag) && other.CompareTag(passThroughTag)) return;

        var enemy = other.GetComponentInParent<EnemyBase>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            if (destroyOnHit) SpawnImpactAndDestroy();
            return;
        }

        var player = other.GetComponentInParent<PlayerHealth>();
        if (player != null)
        {
            player.TakeDamage(damage);
            if (destroyOnHit) SpawnImpactAndDestroy();
            return;
        }

        // Nothing meaningful hit — only destroy if this projectile doesn't pass through the environment.
        if (destroyOnHit && !passThroughEnvironment)
            SpawnImpactAndDestroy();
    }

    private void SpawnImpactAndDestroy()
    {
        if (impactVFXPrefab != null)
            Instantiate(impactVFXPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
