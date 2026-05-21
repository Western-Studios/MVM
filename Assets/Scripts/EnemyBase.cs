using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    [SerializeField] protected int maxHealth = 3;
    [SerializeField] protected int contactDamage = 1;

    protected int currentHealth;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }

    public virtual void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
            Die();
    }

    protected virtual void Die()
    {
        GetComponentInParent<Room>()?.NotifyEnemyDied();
        Destroy(gameObject);
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        other.GetComponent<PlayerHealth>()?.TakeDamage(contactDamage);
    }
}