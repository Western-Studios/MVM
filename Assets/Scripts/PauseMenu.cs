using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Button     resumeButton;
    [SerializeField] private Button     settingsButton;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button     mainMenuButton;
    [SerializeField] private Button     quitButton;
    [SerializeField] private string     mainMenuSceneName = "MainMenu";

    private bool isPaused;

    private void Awake()
    {
        panel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        if (resumeButton   != null) resumeButton.onClick.AddListener(Resume);
        if (settingsButton != null) settingsButton.onClick.AddListener(OnSettings);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenu);
        if (quitButton     != null) quitButton.onClick.AddListener(OnQuit);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            SetPaused(!isPaused);
    }

    private void SetPaused(bool paused)
    {
        isPaused       = paused;
        Time.timeScale = paused ? 0f : 1f;
        panel.SetActive(paused);

        if (!paused && settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void Resume()
    {
        AudioManager.Instance?.PlayButtonClick();
        SetPaused(false);
    }

    private void OnSettings()
    {
        AudioManager.Instance?.PlayButtonClick();
        panel.SetActive(false);
        settingsPanel?.SetActive(true);
    }

    public void CloseSettings()
    {
        AudioManager.Instance?.PlayButtonClick();
        settingsPanel?.SetActive(false);
        panel.SetActive(true);
    }

    private void OnMainMenu()
    {
        AudioManager.Instance?.PlayButtonClick();
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void OnQuit()
    {
        AudioManager.Instance?.PlayButtonClick();
        Time.timeScale = 1f;
        Application.Quit();
    }
}
