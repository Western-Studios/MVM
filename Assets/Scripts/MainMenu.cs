using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button     playButton;
    [SerializeField] private Button     settingsButton;
    [SerializeField] private Button     quitButton;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private string     gameSceneName = "Game";

    private void Awake()
    {
        if (playButton     != null) playButton.onClick.AddListener(OnPlay);
        if (settingsButton != null) settingsButton.onClick.AddListener(OnSettings);
        if (quitButton     != null) quitButton.onClick.AddListener(OnQuit);

        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    private void OnPlay()     { AudioManager.Instance?.PlayButtonClick(); SceneManager.LoadScene(gameSceneName); }
    private void OnSettings() { AudioManager.Instance?.PlayButtonClick(); settingsPanel?.SetActive(!settingsPanel.activeSelf); }
    private void OnQuit()     { AudioManager.Instance?.PlayButtonClick(); Application.Quit(); }

    public void CloseSettings() { AudioManager.Instance?.PlayButtonClick(); settingsPanel?.SetActive(false); }
}
