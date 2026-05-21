using UnityEngine;

public class Projectile : MonoBehaviour
{

    [SerializeField] private float speed = 12f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private bool canBurnDoors;
    [SerializeField] private bool destroyOnHit = true;
    public bool CanBurnDoors => canBurnDoors;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Initialize(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        Destroy(gameObject, lifetime);
    }

    // Update is called once per frame
    public void Update()
    {
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<EnemyBase>(out var enemy))
            enemy.TakeDamage(damage);
        else if (other.TryGetComponent<PlayerHealth>(out var player))
            player.TakeDamage(damage);

        if (destroyOnHit)
            Destroy(gameObject);
    }
}
