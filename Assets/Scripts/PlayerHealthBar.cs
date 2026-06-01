using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach to a UI Image set to Image Type: Filled, Fill Method: Horizontal.
/// Drag your PlayerHealth component into the Inspector field and it will
/// update automatically via the onHealthChanged event — no per-frame polling.
/// </summary>
public class PlayerHealthBar : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;

    private Image fillImage;

    private void Awake()
    {
        fillImage = GetComponent<Image>();

        if (playerHealth == null)
        {
            Debug.LogWarning("[PlayerHealthBar] PlayerHealth reference not assigned.", this);
            return;
        }

        // Subscribe to the event so the bar updates whenever health changes.
        playerHealth.onHealthChanged.AddListener(OnHealthChanged);
    }

    private void Start()
    {
        // Force an initial sync in case Awake order puts us ahead of PlayerHealth.
        if (playerHealth != null)
            OnHealthChanged(playerHealth.CurrentHealth, playerHealth.MaxHealth);
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.onHealthChanged.RemoveListener(OnHealthChanged);
    }

    private void OnHealthChanged(int current, int max)
    {
        if (fillImage != null)
            fillImage.fillAmount = max > 0 ? (float)current / max : 0f;
    }
}
