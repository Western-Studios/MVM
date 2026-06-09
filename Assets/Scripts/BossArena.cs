using UnityEngine;

public class BossArena : MonoBehaviour
{
    [SerializeField] private BossWizard boss;
    [Tooltip("Orthographic size to zoom to when the player enters. Tune this to fit your room.")]
    [SerializeField] private float arenaZoom = 8f;
    [SerializeField] private float zoomDuration = 0.8f;

    private float defaultZoom;
    private bool  entered;

    private void Start()
    {
        defaultZoom = Camera.main.orthographicSize;

        if (boss != null)
            boss.onDeath += OnBossDied;
    }

    private void OnDestroy()
    {
        if (boss != null)
            boss.onDeath -= OnBossDied;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (entered) return;
        if (other.GetComponentInParent<PlayerHealth>() == null) return;

        entered = true;
        CameraController.Instance?.ZoomTo(arenaZoom, zoomDuration);
    }

    private void OnBossDied()
    {
        CameraController.Instance?.ZoomTo(defaultZoom, zoomDuration);
    }
}
