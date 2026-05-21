using UnityEngine;
using UnityEngine.Events;

public class TeleportSwitch : MonoBehaviour
{
    [SerializeField] private string switchId;
    [SerializeField] private bool oneShot = true;

    public UnityEvent onActivated;

    private bool triggered;

    private void Start()
    {
        if (PersistenceManager.Instance.GetState(switchId))
        {
            triggered = true;
            onActivated.Invoke();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.TryGetComponent<TeleportGhost>(out _)) return;

        triggered = oneShot;

        if (oneShot)
            PersistenceManager.Instance.SetState(switchId, true);

        onActivated.Invoke();
    }
}
