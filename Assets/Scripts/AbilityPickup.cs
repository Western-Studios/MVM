using UnityEngine;

public class AbilityPickup : MonoBehaviour
{
    public enum AbilityType { Teleport, Fireball, IceBolt }

    [SerializeField] private string pickupId;
    [SerializeField] private AbilityType abilityType;
    [SerializeField] private AbilityData abilityData;

    [Header("VFX")]
    [SerializeField] private GameObject pickupVFXPrefab;

    private void Start()
    {
        // Validate required references
        if (abilityData == null)
            Debug.LogWarning($"[AbilityPickup] '{gameObject.name}' has no AbilityData assigned!", this);

        if (string.IsNullOrEmpty(pickupId))
            Debug.LogWarning($"[AbilityPickup] '{gameObject.name}' has no Pickup Id set — persistence will not work correctly.", this);

        var col = GetComponent<Collider2D>();
        if (col == null)
            Debug.LogWarning($"[AbilityPickup] '{gameObject.name}' has no Collider2D — player will never trigger it!", this);
        else if (!col.isTrigger)
            Debug.LogWarning($"[AbilityPickup] '{gameObject.name}' Collider2D is NOT set to Is Trigger — player will push it instead of picking it up.", this);

        if (PersistenceManager.Instance == null)
        {
            Debug.LogWarning("[AbilityPickup] PersistenceManager is not in the scene — pickup state cannot be saved.", this);
            return;
        }

        if (PersistenceManager.Instance.GetState(pickupId))
        {
            Debug.Log($"[AbilityPickup] '{pickupId}' already collected — destroying.");
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (abilityData != null)
        {
            switch (abilityType)
            {
                case AbilityType.Teleport: abilityData.hasTeleport = true; break;
                case AbilityType.Fireball: abilityData.hasFireball = true; break;
                case AbilityType.IceBolt:  abilityData.hasIceBolt  = true; break;
            }
        }

        AudioManager.Instance?.PlayAbilityPickup();

        if (pickupVFXPrefab != null)
            Instantiate(pickupVFXPrefab, transform.position, Quaternion.identity);

        if (PersistenceManager.Instance != null)
            PersistenceManager.Instance.SetState(pickupId, true);

        Destroy(gameObject);
    }
}
