using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [SerializeField] private Transform target;
    [SerializeField] private float followSmoothing = 8f;

    private Camera cam;
    private Bounds activeBounds;
    private bool hasBounds;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        cam = GetComponent<Camera>();
    }

    public void SetRoom(Room room)
    {
        activeBounds = room.Bounds;
        hasBounds = true;

        // Snap camera immediately into the new room so the lerp doesn't fight the player
        if (target != null)
        {
            Vector3 snapped = ClampToRoom(new Vector3(target.position.x, target.position.y, transform.position.z), activeBounds);
            transform.position = snapped;
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = new Vector3(target.position.x, target.position.y, transform.position.z);

        if (hasBounds)
            desired = ClampToRoom(desired, activeBounds);

        transform.position = Vector3.Lerp(transform.position, desired, followSmoothing * Time.deltaTime);
    }

    private Vector3 ClampToRoom(Vector3 position, Bounds bounds)
    {
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;
        float x = Mathf.Clamp(position.x, bounds.min.x + halfW, bounds.max.x - halfW);
        float y = Mathf.Clamp(position.y, bounds.min.y + halfH, bounds.max.y - halfH);
        return new Vector3(x, y, position.z);
    }
}
