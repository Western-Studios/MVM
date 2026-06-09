using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartHUD : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private GameObject heartPrefab;        // UI Image prefab with your full heart sprite already assigned
    [SerializeField] private Sprite emptyHeartSprite;       // Optional: if null, depleted hearts are hidden

    private readonly List<Image> hearts = new List<Image>();
    private Sprite fullHeartSprite;

    private void Awake()
    {
        if (playerHealth == null)
        {
            Debug.LogWarning("[HeartHUD] PlayerHealth reference not assigned.", this);
            return;
        }
        playerHealth.onHealthChanged.AddListener(OnHealthChanged);
    }

    private void Start()
    {
        if (playerHealth == null) return;
        if (heartPrefab == null)
        {
            Debug.LogWarning("[HeartHUD] Heart Prefab is not assigned — no hearts will spawn.", this);
            return;
        }
        BuildHearts(playerHealth.MaxHealth);
        OnHealthChanged(playerHealth.CurrentHealth, playerHealth.MaxHealth);
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.onHealthChanged.RemoveListener(OnHealthChanged);
    }

    private void OnHealthChanged(int current, int max)
    {
        if (hearts.Count != max)
            BuildHearts(max);

        for (int i = 0; i < hearts.Count; i++)
        {
            bool full = i < current;

            if (emptyHeartSprite != null)
            {
                hearts[i].sprite = full ? fullHeartSprite : emptyHeartSprite;
                hearts[i].gameObject.SetActive(true);
            }
            else
            {
                // No empty sprite supplied — just hide depleted hearts
                hearts[i].gameObject.SetActive(full);
            }
        }
    }

    private void BuildHearts(int count)
    {
        foreach (var h in hearts)
            if (h != null) Destroy(h.gameObject);
        hearts.Clear();

        for (int i = 0; i < count; i++)
        {
            var go = Instantiate(heartPrefab, transform);
            var img = go.GetComponent<Image>();
            if (img == null) continue;
            // Capture the full heart sprite from the prefab on first build
            if (fullHeartSprite == null) fullHeartSprite = img.sprite;
            hearts.Add(img);
        }
    }
}
