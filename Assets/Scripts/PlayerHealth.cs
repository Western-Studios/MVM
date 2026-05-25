using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private float invincibilityDuration = 1f;

    public UnityEvent<int, int> onHealthChanged;
    public UnityEvent onDeath;

    private int currentHealth;
    private float invincibilityTimer;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (invincibilityTimer > 0f)
            invincibilityTimer -= Time.deltaTime;
    }

    public void TakeDamage(int amount)
    {
        if (invincibilityTimer > 0f) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        invincibilityTimer = invincibilityDuration;
        onHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
            onDeath?.Invoke();
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>Instantly kills the player, bypassing invincibility frames.</summary>
    public void Kill()
    {
        currentHealth = 0;
        invincibilityTimer = 0f;
        onHealthChanged?.Invoke(currentHealth, maxHealth);
        onDeath?.Invoke();
    }
}