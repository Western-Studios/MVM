using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    private Vector3 respawnPosition;
    private bool    hasCheckpoint;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetCheckpoint(Vector3 position)
    {
        respawnPosition = position;
        hasCheckpoint   = true;
    }

    // Returns the saved checkpoint, or fallback (player's starting position) if none set yet.
    public Vector3 GetRespawnPosition(Vector3 fallback)
        => hasCheckpoint ? respawnPosition : fallback;
}
