using UnityEngine;

public class AbilityPickup : MonoBehaviour
{
    public enum AbilityType { Teleport, Fireball, IceBolt }

    [SerializeField] private string pickupId;
    [SerializeField] private AbilityType abilityType;
    [SerializeField] private AbilityData abilityData;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        if (PersistenceManager.Instance.GetState(pickupId))
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        switch (abilityType)
        {
            case AbilityType.Teleport: abilityData.hasTeleport = true; break;
            case AbilityType.Fireball: abilityData.hasFireball = true; break;
            case AbilityType.IceBolt: abilityData.hasIceBolt = true; break;
        }

        PersistenceManager.Instance.SetState(pickupId, true);
        Destroy(gameObject);
    }
}
