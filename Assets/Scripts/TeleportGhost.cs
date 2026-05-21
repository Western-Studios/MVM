using UnityEngine;

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
