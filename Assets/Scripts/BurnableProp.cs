using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class BurnableProp : MonoBehaviour
{
    [Tooltip("Seconds before this prop burns after being triggered by a chain.")]
    [SerializeField] private float burnDelay = 0f;

    public UnityEvent onBurned;

    private bool burning;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<Projectile>(out var p) && p.CanBurnDoors)
            StartBurn();
    }

    // Call this from another BurnableProp's onBurned event to chain burns.
    public void StartBurn()
    {
        if (burning) return;
        burning = true;
        StartCoroutine(BurnRoutine());
    }

    private IEnumerator BurnRoutine()
    {
        if (burnDelay > 0f)
            yield return new WaitForSeconds(burnDelay);

        onBurned.Invoke();
        Destroy(gameObject);
    }
}
