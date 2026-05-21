using UnityEngine;

public class DestructibleDoor : MonoBehaviour
{
    [SerializeField] private string doorId;
    

    private void Start()
    {
        if (PersistenceManager.Instance.GetState(doorId))
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<Projectile>(out var projectile) && projectile.CanBurnDoors)
            Burn();
    }

    private void Burn()
    {
        PersistenceManager.Instance.SetState(doorId, true);
        Destroy(gameObject);
    }
}
