using UnityEngine;

/// <summary>
/// Visual marker left at the player's last teleport origin.
/// Purely cosmetic — no physics interaction. Fades out over its lifetime.
/// Pressure plate detection is handled by PressurePlate.CheckForGhost() via bounds overlap.
/// </summary>
public class TeleportGhost : MonoBehaviour
{
    [SerializeField] private float lifetime = 4f;
    [SerializeField] private float fadeDuration = 1f;

    private SpriteRenderer sr;
    private float timer;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        timer = lifetime;

        // Ghost is a pure visual marker — it must never move on its own.
        // If the prefab has a Rigidbody2D (e.g., for sprite sorting or layer reasons),
        // switch it to Kinematic so gravity cannot pull the ghost through the floor.
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity  = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (sr != null && timer < fadeDuration)
        {
            Color c = sr.color;
            sr.color = new Color(c.r, c.g, c.b, timer / fadeDuration);
        }

        if (timer <= 0f)
            Destroy(gameObject);
    }
}
