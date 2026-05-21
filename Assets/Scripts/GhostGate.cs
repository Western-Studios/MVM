using UnityEngine;

public class GhostGate : MonoBehaviour
{
    [SerializeField] private GameObject gate;

    private int ghostCount;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent<TeleportGhost>(out _)) return;
        ghostCount++;
        gate.SetActive(ghostCount == 0);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.TryGetComponent<TeleportGhost>(out _))
            gate.SetActive(false);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.TryGetComponent<TeleportGhost>(out _)) return;
        ghostCount = Mathf.Max(0, ghostCount - 1);
        if (ghostCount == 0)
            gate.SetActive(true);
    }
}
