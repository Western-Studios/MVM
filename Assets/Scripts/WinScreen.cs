using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class WinScreen : MonoBehaviour
{
    [SerializeField] private GameObject      panel;
    [SerializeField] private BossWizard      boss;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private Leaderboard    leaderboard;
    [SerializeField] private Button          replayButton;
    [SerializeField] private Button          mainMenuButton;
    [SerializeField] private Button          quitButton;
    [SerializeField] private string          gameSceneName     = "Game";
    [SerializeField] private string          mainMenuSceneName = "MainMenu";
    [Tooltip("Seconds after boss death before the win screen appears — lets the death animation finish.")]
    [SerializeField] private float           showDelay = 2f;

    private void Awake()
    {
        panel.SetActive(false);

        if (replayButton   != null) replayButton.onClick.AddListener(OnReplay);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenu);
        if (quitButton     != null) quitButton.onClick.AddListener(OnQuit);
    }

    private void Start()
    {
        if (boss != null)
            boss.onDeath += OnBossDied;
    }

    private void OnDestroy()
    {
        if (boss != null)
            boss.onDeath -= OnBossDied;
    }

    private void OnBossDied() => StartCoroutine(ShowRoutine());

    private IEnumerator ShowRoutine()
    {
        yield return new WaitForSeconds(showDelay);

        if (RunTimer.Instance != null)
        {
            if (timeText != null)
                timeText.text = $"Time: {RunTimer.Instance.FormattedTime}";
            leaderboard?.SubmitTime(RunTimer.Instance.Elapsed);
        }

        panel.SetActive(true);
        Time.timeScale = 0f;
    }

    private void OnReplay()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }

    private void OnMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void OnQuit()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}
