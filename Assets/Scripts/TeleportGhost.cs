using System.Collections;
using UnityEngine;

public class TeleportGhost : MonoBehaviour
{
    [SerializeField] private float lifetime = 4f;
    [SerializeField] private float fadeDuration = 1f;
    [Tooltip("Max seconds to wait for gravity to land the ghost before freezing it in place.")]
    [SerializeField] private float settleTimeout = 0.5f;

    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private float timer;

    private void Awake()
    {
        sr    = GetComponent<SpriteRenderer>();
        rb    = GetComponent<Rigidbody2D>();
        timer = lifetime;
    }

    private void Start()
    {
        if (rb != null)
            StartCoroutine(SettleRoutine());
    }

    // Let gravity drop the ghost to the floor, then freeze it in place.
    private IEnumerator SettleRoutine()
    {
        yield return new WaitForSeconds(settleTimeout);
        rb.bodyType        = RigidbodyType2D.Kinematic;
        rb.linearVelocity  = Vector2.zero;
        rb.angularVelocity = 0f;
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
