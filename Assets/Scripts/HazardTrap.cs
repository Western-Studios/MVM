using System.Collections;
using UnityEngine;

public class HazardTrap : MonoBehaviour
{
    [SerializeField] private float dropSpeed       = 14f;
    [SerializeField] private float maxDropDistance = 25f;

    private Vector3  startPosition;
    private bool     dropped;
    private Coroutine dropCoroutine;

    private void Awake()
    {
        startPosition = transform.position;
    }

    // Called by BurnableProp.onBurned, PressurePlate.onPressed, or TeleportSwitch.onActivated
    public void Trigger()
    {
        if (dropped) return;
        dropped      = true;
        dropCoroutine = StartCoroutine(DropRoutine());
    }

    // Called by BossWizard after a full cycle completes
    public void ResetTrap()
    {
        if (dropCoroutine != null) StopCoroutine(dropCoroutine);
        dropped            = false;
        transform.position = startPosition;
    }

    private IEnumerator DropRoutine()
    {
        AudioManager.Instance?.PlayTrapDrop();
        float fallen = 0f;
        while (fallen < maxDropDistance)
        {
            float step = dropSpeed * Time.deltaTime;
            transform.position += Vector3.down * step;
            fallen             += step;
            yield return null;
        }
    }

    [Header("Hit FX")]
    [SerializeField] private GameObject hitVFXPrefab;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!dropped) return;
        var boss = other.GetComponent<BossWizard>()
                ?? other.GetComponentInParent<BossWizard>();
        if (boss == null) return;

        if (dropCoroutine != null) StopCoroutine(dropCoroutine);
        boss.TakeDamage(2);

        AudioManager.Instance?.PlayTrapImpact();

        if (hitVFXPrefab != null)
            Instantiate(hitVFXPrefab, transform.position, Quaternion.identity);

        gameObject.SetActive(false);
    }
}
