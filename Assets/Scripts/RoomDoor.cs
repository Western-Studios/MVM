using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class RoomDoor : MonoBehaviour
{
    [Tooltip("Room on the LEFT or BELOW the door")]
    [SerializeField] private Room roomA;
    [Tooltip("Room on the RIGHT or ABOVE the door")]
    [SerializeField] private Room roomB;

    private static float lastTransitionTime = -1f;
    private const float TransitionCooldown = 0.75f;

    private Collider2D col;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (Time.time - lastTransitionTime < TransitionCooldown) return;

        Rigidbody2D rb = other.attachedRigidbody;
        if (rb == null) return;

        Bounds b = col.bounds;
        bool isVerticalDoor = b.size.y > b.size.x;
        float velocity = isVerticalDoor ? rb.linearVelocity.x : rb.linearVelocity.y;

        if (Mathf.Abs(velocity) < 0.1f) return;

        bool towardsB = velocity > 0f;
        Room destination = towardsB ? roomB : roomA;
        if (destination == null || destination == Room.Active) return;

        destination.Activate();
        lastTransitionTime = Time.time;
    }
}
