using UnityEngine;

/// <summary>
/// Place in the world as a trigger. When the player walks through it, saves this
/// position as the respawn point. Attach a SpriteRenderer as the indicator to
/// show active/inactive state visually.
/// </summary>
public class Checkpoint : MonoBehaviour
{
    [SerializeField] private SpriteRenderer indicator;
    [Tooltip("Colour the indicator changes to when this checkpoint is activated.")]
    [SerializeField] private Color activeColor = new Color(1f, 0.85f, 0f, 1f);

    private Color inactiveColor;
    private bool  activated;
    private Room  homeRoom;

    private void Awake()
    {
        if (indicator != null)
            inactiveColor = indicator.color;
    }

    private void Start()
    {
        var col = Physics2D.OverlapCircle(transform.position, 1f, LayerMask.GetMask("Room"));
        homeRoom = col?.GetComponent<Room>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated) return;
        if (other.GetComponentInParent<PlayerHealth>() == null) return;

        activated = true;
        CheckpointManager.Instance?.SetCheckpoint(transform.position, homeRoom);
        AudioManager.Instance?.PlayCheckpoint();

        if (indicator != null)
            indicator.color = activeColor;
    }

    // Lets other checkpoints visually deactivate this one if you want only one active at a time.
    public void Deactivate()
    {
        activated = false;
        if (indicator != null)
            indicator.color = inactiveColor;
    }
}
