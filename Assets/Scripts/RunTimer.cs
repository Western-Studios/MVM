using UnityEngine;
using TMPro;

public class RunTimer : MonoBehaviour
{
    public static RunTimer Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private BossWizard boss;

    private float elapsed;
    private bool  running = true;

    public float  Elapsed       => elapsed;
    public string FormattedTime => FormatTime(elapsed);

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (boss != null)
            boss.onDeath += StopTimer;
    }

    private void OnDestroy()
    {
        if (boss != null)
            boss.onDeath -= StopTimer;
    }

    private void Update()
    {
        if (!running) return;
        elapsed += Time.deltaTime;
        if (timerText != null)
            timerText.text = FormattedTime;
    }

    private void StopTimer() => running = false;

    public static string FormatTime(float t)
    {
        int m  = (int)(t / 60f);
        int s  = (int)(t % 60f);
        int cs = (int)((t % 1f) * 100f);
        return $"{m:00}:{s:00}.{cs:00}";
    }
}
