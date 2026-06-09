using System.Collections;
using UnityEngine;

public class BossDoor : MonoBehaviour
{
    [SerializeField] private GameObject gate;
    [SerializeField] private BossWizard boss;

    [Header("Camera Zoom")]
    [Tooltip("Orthographic size to zoom to when the player enters. Tune to fit your room.")]
    [SerializeField] private float arenaZoom    = 8f;
    [SerializeField] private float zoomDuration = 0.8f;

    private float defaultZoom;
    private bool  entered;

    private void Start()
    {
        defaultZoom = Camera.main.orthographicSize;

        if (boss != null)
            boss.onDeath += OnBossDied;

        if (gate != null)
            gate.SetActive(false);
    }

    private void OnDestroy()
    {
        if (boss != null)
            boss.onDeath -= OnBossDied;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<PlayerHealth>() == null) return;

        if (gate != null) gate.SetActive(true);

        if (!entered)
        {
            entered = true;
            CameraController.Instance?.ZoomTo(arenaZoom, zoomDuration);
            AudioManager.Instance?.PlayBossMusic();
        }
    }

    private void OnBossDied()
    {
        if (gate != null) gate.SetActive(false);
        CameraController.Instance?.ZoomTo(defaultZoom, zoomDuration);
    }
}
