using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Opens a door while the player OR a teleport ghost is standing on it.
/// Ghost detection uses a bounds check — the ghost needs no Rigidbody2D or Collider2D.
/// Player detection uses OnTriggerEnter/Exit (player already has a Rigidbody2D).
/// </summary>
public class PressurePlate : MonoBehaviour
{
    [Tooltip("The door GameObject to control. Active = blocking. Inactive = open.")]
    [SerializeField] private GameObject door;

    [Tooltip("OFF = door closes when plate empties (timed ghost mechanic). " +
             "ON  = door stays open permanently once triggered.")]
    [SerializeField] private bool stayOpenWhenTriggered = false;

    [Space]
    public UnityEvent onPressed;
    public UnityEvent onReleased;

    private readonly HashSet<Collider2D> playerActivators = new HashSet<Collider2D>();
    private Collider2D plateCollider;
    private bool isOpen = false;
    private bool permanentlyOpen = false;

    private void Awake()
    {
        plateCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        bool playerOn = playerActivators.Count > 0;
        bool ghostOn  = CheckForGhost();
        bool shouldBeOpen = permanentlyOpen || playerOn || ghostOn;

        if (shouldBeOpen == isOpen) return;

        isOpen = shouldBeOpen;

        if (isOpen)
        {
            if (stayOpenWhenTriggered) permanentlyOpen = true;
            onPressed?.Invoke();
            SetDoor(open: true);
        }
        else
        {
            onReleased?.Invoke();
            SetDoor(open: false);
        }
    }

    // ── Player — detected via physics trigger (player has Rigidbody2D) ──
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerActivators.Add(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        playerActivators.Remove(other);
    }

    // ── Ghost — detected via bounds check (no Rigidbody2D on ghost needed) ──
    private bool CheckForGhost()
    {
        if (plateCollider == null) return false;

        Bounds bounds = plateCollider.bounds;
        var ghosts = FindObjectsByType<TeleportGhost>(FindObjectsSortMode.None);

        foreach (var ghost in ghosts)
        {
            if (bounds.Contains(ghost.transform.position))
                return true;
        }
        return false;
    }

    private void SetDoor(bool open)
    {
        if (door != null)
            door.SetActive(!open);  // active = blocking, inactive = open passage
    }
}
