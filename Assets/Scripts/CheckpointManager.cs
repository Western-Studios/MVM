using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    private Vector3 respawnPosition;
    private Room    respawnRoom;
    private bool    hasCheckpoint;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetCheckpoint(Vector3 position, Room room = null)
    {
        respawnPosition = position;
        respawnRoom     = room;
        hasCheckpoint   = true;
    }

    public Vector3 GetRespawnPosition(Vector3 fallback)
        => hasCheckpoint ? respawnPosition : fallback;

    public Room GetRespawnRoom() => respawnRoom;
}
