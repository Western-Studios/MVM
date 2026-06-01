using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Slides a door open while the player OR a teleport ghost is standing on it.
/// The door moves between its original (closed) position and an offset (open) position,
/// so it always remains an active GameObject and smoothly returns when the plate is released.
///
/// Ghost detection uses a bounds check — the ghost needs no Rigidbody2D or Collider2D.
/// Player detection uses OnTriggerEnter/Exit (player already has a Rigidbody2D).
/// </summary>
public class PressurePlate : MonoBehaviour
{
    [Tooltip("The door GameObject to slide. It must be active in the scene — do NOT deactivate it manually.")]
    [SerializeField] private GameObject door;

    [Header("Door Slide")]
    [Tooltip("How far and in which direction the door slides when the plate is pressed (world units). " +
             "Positive Y slides up, negative Y slides down, X slides sideways.")]
    [SerializeField] private Vector2 openOffset = new Vector2(0f, 3f);

    [Tooltip("Units per second the door travels between open and closed positions.")]
    [SerializeField] private float doorMoveSpeed = 5f;

    [Header("Behaviour")]
    [Tooltip("OFF = door returns to closed when the plate empties (default, reactive). " +
             "ON  = door stays open permanently once triggered (one-shot mechanic).")]
    [SerializeField] private bool stayOpenWhenTriggered = false;

    [Space]
    public UnityEvent onPressed;
    public UnityEvent onReleased;

    // ── Private state ────────────────────────────────────────────────────
    private readonly HashSet<Collider2D> playerActivators = new HashSet<Collider2D>();
    private Collider2D plateCollider;
    private bool isOpen = false;
    private bool permanentlyOpen = false;

    private Vector2 doorClosedPos;
    private Vector2 doorOpenPos;

    // ── Lifecycle ────────────────────────────────────────────────────────
    private void Awake()
    {
        plateCollider = GetComponent<Collider2D>();

        if (door != null)
        {
            // Capture positions at scene start so they are stable regardless of
            // when Awake fires relative to other scripts.
            doorClosedPos = door.transform.position;
            doorOpenPos   = doorClosedPos + openOffset;

            // Make sure the door is visible and collidable at startup.
            door.SetActive(true);
        }
    }

    private void Update()
    {
        // ── Determine whether the plate should be active ─────────────────
        bool playerOn = playerActivators.Count > 0;
        bool ghostOn  = CheckForGhost();
        bool shouldBeOpen = permanentlyOpen || playerOn || ghostOn;

        // Fire events only on state *change*
        if (shouldBeOpen != isOpen)
        {
            isOpen = shouldBeOpen;

            if (isOpen)
            {
                if (stayOpenWhenTriggered) permanentlyOpen = true;
                onPressed?.Invoke();
            }
            else
            {
                onReleased?.Invoke();
            }
        }

        // ── Slide the door every frame toward the current target ──────────
        // Using MoveTowards instead of Lerp so the speed is constant (units/sec)
        // and the door always reaches its target exactly — no floating-point drift.
        if (door != null)
        {
            Vector2 target = isOpen ? doorOpenPos : doorClosedPos;
            door.transform.position = Vector2.MoveTowards(
                door.transform.position, target, doorMoveSpeed * Time.deltaTime);
        }
    }

    // ── Player — detected via physics trigger (player has Rigidbody2D) ───
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerActivators.Add(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        playerActivators.Remove(other);
    }

    // ── Ghost — detected via bounds check (no physics on ghost needed) ───
    private bool CheckForGhost()
    {
        if (plateCollider == null) return false;

        // Expand the plate bounds slightly so a ghost whose pivot sits exactly
        // at the edge (very common when both the ghost and the plate are at floor level)
        // still registers as "on" the plate rather than "just outside" it.
        Bounds plateBounds = plateCollider.bounds;
        plateBounds.Expand(0.2f);

        var ghosts = FindObjectsByType<TeleportGhost>(FindObjectsInactive.Exclude);

        foreach (var ghost in ghosts)
        {
            if (plateBounds.Contains(ghost.transform.position))
                return true;
        }
        return false;
    }

#if UNITY_EDITOR
    // Draw the open position so designers can visualise the slide in the Scene view.
    private void OnDrawGizmosSelected()
    {
        if (door == null) return;
        Vector2 start = Application.isPlaying ? doorClosedPos : (Vector2)door.transform.position;
        Vector2 end   = start + openOffset;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(start, end);
        Gizmos.DrawWireCube(end, door.transform.localScale);
    }
#endif
}
