using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    [SerializeField] private BossWizard boss;
    [SerializeField] private GameObject panel;  // parent panel — hidden when boss dies

    private Image fillImage;

    private void Awake()
    {
        fillImage = GetComponent<Image>();

        if (boss == null)
        {
            Debug.LogWarning("[BossHealthBar] BossWizard reference not assigned.", this);
            return;
        }

        boss.onHealthChanged += OnHealthChanged;
        boss.onDeath         += OnBossDied;
    }

    private void Start()
    {
        if (boss != null)
            OnHealthChanged(boss.CurrentHealth, boss.MaxHealth);
    }

    private void OnDestroy()
    {
        if (boss != null)
        {
            boss.onHealthChanged -= OnHealthChanged;
            boss.onDeath         -= OnBossDied;
        }
    }

    private void OnHealthChanged(int current, int max)
    {
        if (fillImage != null)
            fillImage.fillAmount = max > 0 ? (float)current / max : 0f;
    }

    private void OnBossDied()
    {
        if (panel != null) panel.SetActive(false);
    }
}
